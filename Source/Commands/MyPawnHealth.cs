using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchToolkit;
using TwitchToolkit.IRC;
using TwitchToolkit.PawnQueue;
using UnityEngine;
using Verse;

namespace TwitchToolkitMoreMyPawnCommands.Commands
{
    public class MyPawnHealth : MyPawnBase
    {

        public override void RunCommand(IRCMessage message)
        {
            Viewer viewer = Viewers.GetViewer(message.User);
            Pawn pawn = GetPawnIfAllowed(message);
            if (pawn == null)
            {
                return;
            }

            string[] healthArgs = message.Message.Split(' ').Skip(1).ToArray();
            for (int i = 0; i < healthArgs.Length; ++i)
            {
                healthArgs[i] = healthArgs[i].ToLower();
            }
            string output;
            if (healthArgs.Length > 0)
            {
                PawnCapacityDef capacityDef = DefDatabase<PawnCapacityDef>.AllDefs
                    .FirstOrDefault(def => healthArgs.All(arg => def.defName.ToLower().Contains(arg.ToLower()) || def.label.ToLower().Contains(arg.ToLower())));
                if (capacityDef != null)
                {
                    output = MyPawnHealthCapacity(viewer, pawn, capacityDef);
                }
                else
                {
                    output = $"@{viewer.username} Could not find a pawn capacity for \"{healthArgs.Aggregate((s1, s2) => s1 + " " + s2)}\"";
                }
            }
            else
            {
                output = MyPawnHealthSummary(viewer, pawn);
            }

            SendWrappedOutputText(output, message);
        }

        private static string MyPawnHealthSummary(Viewer viewer, Pawn pawn)
        {
            string healthPercent = (pawn.health.summaryHealth.SummaryHealthPercent * 100.0f).ToString("n1") + "%";
            string output = $"@{viewer.username} {pawn.Name.ToStringShort.CapitalizeFirst()}'s health: {healthPercent} ";
            if (pawn.health.State != PawnHealthState.Mobile)
            {
                output += $"({HealthStateForPawn(pawn)}) ";
            }

            if (pawn.health.hediffSet.BleedRateTotal > 0.01f)
            {
                int ticksUntilDeath = HealthUtility.TicksUntilDeathDueToBloodLoss(pawn);
                if (ticksUntilDeath < 60000)
                {
                    output += "- " + "TimeToDeath".Translate(ticksUntilDeath.ToStringTicksToPeriod()) + " ";
                }
                else
                {
                    output += "- " + "WontBleedOutSoon".Translate() + " ";
                }
            }

            output += " | ";

            IEnumerable<PawnCapacityDef> capacityDefs;
            if (pawn.def.race.Humanlike)
            {
                capacityDefs = from x in DefDatabase<PawnCapacityDef>.AllDefs
                               where x.showOnHumanlikes
                               select x;
            }
            else if (pawn.def.race.Animal)
            {
                capacityDefs = from x in DefDatabase<PawnCapacityDef>.AllDefs
                               where x.showOnAnimals
                               select x;
            }
            else if (pawn.def.race.IsMechanoid)
            {
                capacityDefs = from x in DefDatabase<PawnCapacityDef>.AllDefs
                               where x.showOnMechanoids
                               select x;
            }
            else
            {
                capacityDefs = new List<PawnCapacityDef>();
                output += "(can't show capacities for this race) ";
            }
            foreach (PawnCapacityDef pawnCapacityDef in from def in capacityDefs
                                                        orderby def.listOrder
                                                        select def)
            {
                if (PawnCapacityUtility.BodyCanEverDoCapacity(pawn.RaceProps.body, pawnCapacityDef))
                {
                    Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, pawnCapacityDef);
                    output += $"{pawnCapacityDef}: {efficiencyLabel.First} | ";
                }
            }
            if (capacityDefs.Count() > 0)
            {
                output = output.Substring(0, output.Length - 2);
            }

            if (ModSettings.Singleton.MyPawnHealthShowSurgeries)
            {
                output += " | Queued Surgeries - ";
                if (pawn.health.surgeryBills?.Count > 0)
                {
                    foreach (var surgeryBill in pawn.health.surgeryBills)
                    {
                        output += $"{surgeryBill.LabelCap}, ";
                    }
                    output = output.Substring(0, output.Length - 2);
                }
                else
                {
                    output += $"none";
                }
            }
            return output;
        }

        private static string MyPawnHealthCapacity(Viewer viewer, Pawn pawn, PawnCapacityDef capacityDef)
        {
            if (PawnCapacityUtility.BodyCanEverDoCapacity(pawn.RaceProps.body, capacityDef))
            {
                Pair<string, Color> efficiencyLabel = HealthCardUtility.GetEfficiencyLabel(pawn, capacityDef);

                List<PawnCapacityUtility.CapacityImpactor> impactorList = new List<PawnCapacityUtility.CapacityImpactor>();
                float fLevel = PawnCapacityUtility.CalculateCapacityLevel(pawn.health.hediffSet, capacityDef, impactorList);
                string sLevel = (fLevel * 100.0f).ToString("F0") + "%";

                string output = $"@{viewer.username} {pawn.Name.ToStringShort.CapitalizeFirst()}'s {capacityDef.LabelCap}: {efficiencyLabel.First} ({sLevel})";
                output += " - " + "AffectedBy".Translate() + " ";
                if (impactorList.Count > 0)
                {
                    for (int i = 0; i < impactorList.Count; i++)
                    {
                        if (impactorList[i] is PawnCapacityUtility.CapacityImpactorHediff)
                        {
                            output += impactorList[i].Readable(pawn) + ", ";
                        }
                    }
                    for (int i = 0; i < impactorList.Count; i++)
                    {
                        if (impactorList[i] is PawnCapacityUtility.CapacityImpactorBodyPartHealth)
                        {
                            output += impactorList[i].Readable(pawn) + ", ";
                        }
                    }
                    for (int i = 0; i < impactorList.Count; i++)
                    {
                        if (impactorList[i] is PawnCapacityUtility.CapacityImpactorCapacity)
                        {
                            output += impactorList[i].Readable(pawn) + ", ";
                        }
                    }
                    for (int i = 0; i < impactorList.Count; i++)
                    {
                        if (impactorList[i] is PawnCapacityUtility.CapacityImpactorPain)
                        {
                            output += impactorList[i].Readable(pawn) + ", ";
                        }
                    }
                    output = output.Substring(0, output.Length - 2);
                }
                else
                {
                    output += $"nothing ";
                }
                return output;
            }
            else
            {
                return $"@{viewer.username} {pawn.Name.ToStringShort.CapitalizeFirst()} Can not have {capacityDef.LabelCap}.";
            }
        }

        private static string HealthStateForPawn(Pawn pawn)
        {
            switch (pawn.health.State)
            {
                case PawnHealthState.Mobile:
                    return "Alive";
                case PawnHealthState.Down:
                    return "Downed";
                case PawnHealthState.Dead:
                    return "DEAD";
                default:
                    return pawn.health.State.ToStringSafe();
            }
        }
    }
}
