using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using UnityEngine;
using RimWorld;
using CombatExtended;
using HarmonyLib;
using HarmonyMod;
using CombatExtended.Utilities;

namespace CEMechs
{
    public class CEMech_SmokePopper : ThingComp
    {
        public PropsSmoker Props => this.props as PropsSmoker;

        public void PopSmoke()
        {
            GenExplosion.DoExplosion
                (
                this.parent.Position,
                this.parent.Map,
                Props.smokeRadius,
                DamageDefOf.Smoke,
                this.parent,
                postExplosionSpawnThingDef: ThingDefOf.Gas_Smoke,
                postExplosionSpawnChance: 1f
                , postExplosionSpawnThingCount: 1
                );
        } 
    }

    public class PropsSmoker : CompProperties
    {
        public PropsSmoker() : base()
        {
            this.compClass = typeof(CEMech_SmokePopper);
        }

        public float smokeRadius;
    }

    public class SmartShooterModExt : DefModExtension
    {
        public float detectionRadius;

        public bool switchGun;

        public string targettingTag;
    }

    public class SmartShooterVerb : Verb_ShootCE
    {
        public Pawn launcherBoy;

        public ThingDef projectile2;

        public SmartShooterModExt controller => this.Caster.def.GetModExtension<SmartShooterModExt>();

        public ProjectileChangeExt projExt => this.Caster.def.GetModExtension<ProjectileChangeExt>();

        public override ThingDef Projectile
        {
            get
            {
                if (projectile2 != null)
                {
                    return projectile2;
                }
                return base.Projectile;
            }
        }

        public override bool TryCastShot()
        {
            var result = base.TryCastShot();

            if (result)
            {
                var system = projExt?.additionalEquipment.Find(x => x.projectile == projectile2) ?? null;
                if (system != null)
                {
                    system.uses--;   
                }

                projectile2 = null;
            }

            return result;
        }

        public override bool TryStartCastOn(LocalTargetInfo castTarg, LocalTargetInfo destTarg, bool surpriseAttack = false, bool canHitNonTargetPawns = true, bool preventFriendlyFire = false)
        {
            if (castTarg.Thing is Pawn p && (p != launcherBoy))
            {
                var pawns = p.Position.PawnsInRange(p.Map, 8).ToList();

                var launchyBoy = pawns.Find(x => x.equipment?.Primary?.def.weaponTags?.Contains(controller.targettingTag) ?? false);

                if (launchyBoy != null && this.CanHitTargetFrom(Caster.Position, launchyBoy))
                {
                    launcherBoy = launchyBoy;

                    var smoker = this.Caster.TryGetComp<CEMech_SmokePopper>();

                    if (smoker != null)
                    {
                        smoker.PopSmoke();
                    }

                    if (projExt != null && controller.switchGun)
                    {
                        projectile2 = projExt.additionalEquipment.Where(x => x.uses > 0).RandomElementByWeight(y => y.chanceToUse).projectile;
                    }

                    var infoLaunchyBoy = new LocalTargetInfo(launchyBoy);

                    this.OrderForceTarget(infoLaunchyBoy);
                    return false;
                }
            }
            return base.TryStartCastOn(castTarg, destTarg, surpriseAttack, canHitNonTargetPawns, preventFriendlyFire);
        }
    }

    [HarmonyPatch(typeof(CompSuppressable), "AddSuppression")]

    static class PostFixSmoker
    {
        public static void Postfix(CompSuppressable __instance, float amount)
        {
            var dad = __instance.parent as Pawn;

            var smoker = dad.TryGetComp<CEMech_SmokePopper>();

            if (smoker != null)
            {
                //Log.Message(amount.ToString().Colorize(Color.red));

                //For Bor, that is 0.1 supressability, it's ~270 for LAW, ~35 for explosion from LAW, ~7 from frags
                if (amount >= 40f)
                {
                    smoker.PopSmoke();
                }
            }

        }
    }
}
