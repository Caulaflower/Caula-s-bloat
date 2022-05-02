using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CombatExtended;
using Verse;
using RimWorld;

namespace CEMechs
{
    public class PartDensityModExt : DefModExtension
    {
        public float density;
    }

    public class BetterBluntWorker : DamageWorker_Blunt
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            var result = base.Apply(dinfo, thing);

            if (thing is Pawn p)
            {
                var result2 = result.parts.Find(x => x.depth == BodyPartDepth.Outside);

                Log.Message(result2?.Label ?? "noen");

                if (result != null)
                {
                    float hp = p.health.hediffSet.GetPartHealth(result2);

                    float hpP = hp / result2.def.GetMaxHealth(p);

                    Log.Message((hpP).ToStringPercent());

                    float pen = dinfo.ArmorPenetrationInt;

                    if (!Rand.Chance(hpP))
                    {
                        result.parts.Clear();

                        var newHit = result2.GetDirectChildParts().Where(x => x.depth == BodyPartDepth.Inside)
                            .RandomElementByWeightWithFallback(x => x.coverage);

                        result.parts.Add(newHit);

                        pen -= p.GetStatValueForPawn(StatDefOf.ArmorRating_Blunt, p);

                        //crashes the game rn
                        #region
                        /*while (pen >= 0)
                        {
                            BodyPartRecord newPartHit = newHit;

                            var newpartPool = newHit.GetDirectChildParts().Where(x => x.depth == BodyPartDepth.Inside);

                            if (newpartPool.EnumerableNullOrEmpty())
                            {
                                newPartHit = result2.GetDirectChildParts().Where(x => x.depth == BodyPartDepth.Inside).RandomElementByWeightWithFallback(x => x.coverage);
                            }
                            else
                            {
                                newPartHit = newpartPool.RandomElementByWeight(x => x.coverage);
                            }

                            pen -= p.GetStatValueForPawn(StatDefOf.ArmorRating_Blunt, p);
                        }
                        
                        */
                        #endregion
                    }
                }
            }

            return result;
        }

        protected override BodyPartRecord ChooseHitPart(DamageInfo dinfo, Pawn pawn)
        {
            //cope
            var p = pawn;

            var result = base.ChooseHitPart(dinfo, pawn);

            return result;
        }
    }

    public class OverPressureExplosionWorker : DamageWorker_AddInjury
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            var result = base.Apply(dinfo, thing);

            if (Rand.Chance(result.totalDamageDealt / 15f))
            {
                if (thing is Pawn p)
                {
                    if (p.RaceProps.IsFlesh)
                    {
                        var parts = p.health.hediffSet.GetNotMissingParts().Where(x => x.def.defName == "Lung" || x.def.defName == "Stomach" || x.def.label.ToLower().Contains("intestine"));

                        if (result.parts == null)
                        {
                            result.parts = new List<BodyPartRecord>();
                        }

                        result.parts.AddRange(parts);
                    }
                }
            }

            return result;
        }
    }
}
