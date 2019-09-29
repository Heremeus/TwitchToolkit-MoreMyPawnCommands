using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchToolkit;
using TwitchToolkit.IRC;
using TwitchToolkit.PawnQueue;
using Verse;

namespace TwitchToolkitMoreMyPawnCommands.Commands
{
    public class MyPawnBody : MyPawnBase
    {
        public override void RunCommand(IRCMessage message)
        {
            Viewer viewer = Viewers.GetViewer(message.User);
            Pawn pawn = GetPawnIfAllowed(message);
            if (pawn == null)
            {
                return;
            }

            string output = $"@{viewer.username} {pawn.Name.ToStringShort.CapitalizeFirst()}'s body: ";
            if (pawn.health.hediffSet.hediffs?.Count > 0)
            {
                IEnumerable<IGrouping<BodyPartRecord, Hediff>> visibleHediffGroupsInOrder = VisibleHediffGroupsInOrder(pawn);
                foreach (IGrouping<BodyPartRecord, Hediff> diffs in visibleHediffGroupsInOrder)
                {
                    if (diffs.Key == null)
                    {
                        output += $"{"WholeBody".Translate()} - ";
                    }
                    else
                    {
                        output += $"{diffs.Key.LabelCap} - ";
                    }
                    foreach (IGrouping<int, Hediff> hediffGroup in diffs.GroupBy(h => h.UIGroupKey))
                    {
                        int numBleeding = 0;
                        foreach (Hediff hediff in hediffGroup)
                        {
                            if (hediff.Bleeding)
                            {
                                ++numBleeding;
                            }
                        }
                        int hediffCount = hediffGroup.Count();
                        output += hediffGroup.First().LabelCap;
                        if (hediffCount != 1)
                        {
                            output += " x" + hediffCount.ToString();
                        }
                        if (numBleeding > 0)
                        {
                            output += " (bleeding x" + numBleeding + ")";
                        }
                        output += ", ";
                    }
                    output = output.Substring(0, output.Length - 2);
                    output += " | ";
                }
                if (visibleHediffGroupsInOrder.Count() > 1)
                {
                    output = output.Substring(0, output.Length - 3);
                }
            }

            SendWrappedOutputText(output, message);
        }

        // Copied from HealthCardUtility
        private static IEnumerable<IGrouping<BodyPartRecord, Hediff>> VisibleHediffGroupsInOrder(Pawn pawn)
        {
            foreach (IGrouping<BodyPartRecord, Hediff> group in from x in VisibleHediffs(pawn)
                                                                group x by x.Part into x
                                                                orderby GetListPriority(x.First().Part) descending
                                                                select x)
            {
                yield return group;
            }
            yield break;
        }

        // Copied from HealthCardUtility
        private static IEnumerable<Hediff> VisibleHediffs(Pawn pawn)
        {
            List<Hediff_MissingPart> mpca = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
            for (int i = 0; i < mpca.Count; i++)
            {
                yield return mpca[i];
            }
            IEnumerable<Hediff> visibleDiffs = from d in pawn.health.hediffSet.hediffs
                                               where !(d is Hediff_MissingPart) && d.Visible
                                               select d;
            foreach (Hediff diff in visibleDiffs)
            {
                yield return diff;
            }
            yield break;
        }

        // Copied from HealthCardUtility
        private static float GetListPriority(BodyPartRecord rec)
        {
            if (rec == null)
            {
                return 9999999f;
            }
            return ((int)rec.height * 10000) + rec.coverageAbsWithChildren;
        }
    }
}
