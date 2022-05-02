using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using RimWorld;
using CombatExtended;
using HarmonyMod;
using HarmonyLib;
using UnityEngine;

namespace CEMechs
{
    [StaticConstructorOnStartup]
    public class HarmonyBloater
    {
        static HarmonyBloater()
        {
            var harmony = new Harmony("CaulasCEBloat");

            harmony.PatchAll();

            DamageDefOf.Blunt.workerClass = typeof(BetterBluntWorker);

            foreach (var dmgDef in DefDatabase<DamageDef>.AllDefs.Where(
                x => x.workerClass == typeof(DamageWorker_Blunt)
                )
                )
            {
                Log.Message(dmgDef.label.Colorize(Color.yellow));
                dmgDef.workerClass = typeof(BetterBluntWorker);
            }

            DamageDefOf.Bomb.workerClass = typeof(OverPressureExplosionWorker);
        }
    }
    public class ExtLoot : DefModExtension
    {
        public List<ThingDef> lootsDefs;

        public FloatRange amount;

        public FloatRange itemAm;
    }

    public class CompLoot : ThingComp
    {
        public Pawn dad => this.parent as Pawn;

        public ExtLoot ext => dad?.kindDef?.GetModExtension<ExtLoot>();
        public void AddLoot()
        {
            Log.Message("1");
            if (ext != null)
            {
                Log.Message("2");
                for (int i = (int)ext.itemAm.RandomInRange; i >= 0; i--)
                {
                    Log.Message("3" + i);
                    var thing = ThingMaker.MakeThing(ext.lootsDefs.RandomElement());
                    thing.stackCount = (int)ext.amount.RandomInRange;
                    dad.inventory.TryAddItemNotForSale(thing);
                }
                
            }
        }
    }
    [HarmonyPatch(typeof(PawnInventoryGenerator), "GenerateInventoryFor")]
    static class PostFixCaulaBloat
    {
        public static void Postfix(Pawn p, PawnGenerationRequest request)
        {
            var ext = p.kindDef.GetModExtension<ExtLoot>();
            if (ext != null)
            {
                var comp = p.TryGetComp<CompLoot>();

                if (comp != null)
                {
                    comp.AddLoot();
                }
            }
        }
    }
}
