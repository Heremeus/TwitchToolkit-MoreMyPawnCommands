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
    public class MyPawnGear : MyPawnBase
    {
        public override void RunCommand(IRCMessage message)
        {
            Viewer viewer = Viewers.GetViewer(message.User);
            Pawn pawn = GetPawnIfAllowed(message);
            if(pawn == null)
            {
                return;
            }

            string output = $"@{viewer.username} {pawn.Name.ToStringShort.CapitalizeFirst()}'s gear: ";

            if (ModSettings.Singleton.MyPawnGearShowComfyTemp)
            {
                float comfyTempMin = pawn.GetStatValue(StatDefOf.ComfyTemperatureMin, true);
                float comfyTempMax = pawn.GetStatValue(StatDefOf.ComfyTemperatureMax, true);
                output += "ComfyTemperatureRange".Translate() + " " + comfyTempMin.ToStringTemperature("F0") + " ~ " + comfyTempMax.ToStringTemperature("F0") + " | ";
            }

            if (ModSettings.Singleton.MyPawnGearShowArmorValues)
            {
                output += "Armor - ";
                output += "ArmorSharp".Translate() + " " + (CalcOverallArmor(pawn, StatDefOf.ArmorRating_Sharp) * 100).ToString("n1") + "%, ";
                output += "ArmorBlunt".Translate() + " " + (CalcOverallArmor(pawn, StatDefOf.ArmorRating_Blunt) * 100).ToString("n1") + "%, ";
                output += "ArmorHeat".Translate() + " " + (CalcOverallArmor(pawn, StatDefOf.ArmorRating_Heat) * 100).ToString("n1") + "% | ";
            }

            if (ModSettings.Singleton.MyPawnGearShowWeapons)
            {
                // TODO: Support simple sidearms
                output += "Equipment".Translate() + " - ";
                if (pawn.equipment?.AllEquipmentListForReading?.Count > 0)
                {
                    foreach (ThingWithComps thing in pawn.equipment.AllEquipmentListForReading)
                    {
                        output += $"{thing.LabelCap}, ";
                    }
                    output = output.Substring(0, output.Length - 2);
                }
                else
                {
                    output += $"none";
                }
                output += " | ";
            }

            if (ModSettings.Singleton.MyPawnGearShowApparel)
            {
                // Worn apparel
                string nudistInfo = "";
                if (pawn.story.traits.HasTrait(TraitDefOf.Nudist))
                {
                    nudistInfo = pawn.apparel.PsychologicallyNude ? "(nudist) " : "(unhappy nudist)";
                }
                output += $"{"Apparel".Translate()} {nudistInfo}- ";
                if (pawn.apparel?.WornApparelCount > 0)
                {
                    foreach (Apparel apparel in from ap in pawn.apparel.WornApparel
                                                orderby ap.def.apparel.bodyPartGroups[0].listOrder descending
                                                select ap)
                    {
                        output += $"{apparel.LabelCap}, ";
                    }
                    output = output.Substring(0, output.Length - 2);
                }
                else
                {
                    output += $"none";
                }
                output += " | ";
            }
            if(output.EndsWith(" | "))
            {
                output = output.Substring(0, output.Length - 3);
            }

            SendWrappedOutputText(output, message);
        }

        private static float CalcOverallArmor(Pawn pawn, StatDef stat)
        {
            float num = 0f;
            float num2 = Mathf.Clamp01(pawn.GetStatValue(stat, true) / 2f);
            List<BodyPartRecord> allParts = pawn.RaceProps.body.AllParts;
            List<Apparel> list = pawn.apparel?.WornApparel;
            for (int i = 0; i < allParts.Count; i++)
            {
                float num3 = 1f - num2;
                if (list != null)
                {
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (list[j].def.apparel.CoversBodyPart(allParts[i]))
                        {
                            float num4 = Mathf.Clamp01(list[j].GetStatValue(stat, true) / 2f);
                            num3 *= 1f - num4;
                        }
                    }
                }
                num += allParts[i].coverageAbs * (1f - num3);
            }
            num = Mathf.Clamp(num * 2f, 0f, 2f);
            return num;
        }
    }
}
