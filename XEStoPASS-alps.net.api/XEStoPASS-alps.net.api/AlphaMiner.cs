using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XEStoPASS_alps.net.api
{
    public static class AlphaMiner
    {
        public static IEnumerable<Tuple<string[], string[]>> MinePetriNet(EventLog log)
        {
            // Pre-processing (Alpha+)
            var lengthOneLoops = FindLoopsOfLengthOne(log);
            var transitions = GetActivitiesFromEvents(log.Traces.SelectMany(trace => trace.Events))
                .Except(lengthOneLoops.Select(loop => loop.Activity))
                .ToList();
            
            // Alpha algorithm
            var maximalSet = FindMaximalSet(log, transitions).ToArray(); //Hier werden alle Vorgänge aufgelistet, falls zu zweit in einer Liste sind xor Events nope

            var newSet = maximalSetMerger(maximalSet);

            return newSet;
            

            var arcs = new List<Arc>(); //Transitionen
            var places = new List<string>(); //Stellen

            for (int i = 0; i < maximalSet.Length; i++)
            {
                var pair = maximalSet[i];
                var place = i.ToString();
                places.Add(place);

                arcs.AddRange(pair.Item1.Select(transition => new Arc(transition, place)));
                arcs.AddRange(pair.Item2.Select(transition => new Arc(place, transition)));
            }

            const string inPlace = "start";
            places.Add(inPlace);
            var initialTransitions = GetActivitiesFromEvents(log.Traces.Select(trace => trace.Events.First()));
            arcs.AddRange(initialTransitions.Select(transition => new Arc(inPlace, transition)));

            const string outPlace = "end";
            places.Add(outPlace);
            var endTransitions = GetActivitiesFromEvents(log.Traces.Select(trace => trace.Events.Last()));
            arcs.AddRange(endTransitions.Select(transition => new Arc(transition, outPlace)));

            // Post-processing (Alpha+)
            foreach (var loop in lengthOneLoops)
            {
                var pairIndex = Array.FindIndex(maximalSet, pair =>
                    pair.Item1.Contains(loop.PrecedingActivity) &&
                    pair.Item2.Contains(loop.FollowingActivity));

                if (pairIndex == -1)
                {
                    continue;
                }

                var place = pairIndex.ToString();

                if (!transitions.Contains(loop.Activity))
                {
                    transitions.Add(loop.Activity);

                    arcs.Add(new Arc(place, loop.Activity));
                    arcs.Add(new Arc(loop.Activity, place));
                }
            }

            //return new PetriNet(places, transitions, arcs);
        }



        public static IEnumerable<Tuple<string[], string[]>> maximalSetMerger(Tuple<string[], string[]>[] set)
        {
            var maximalSet = set;

            for (int i = 0; i < set.Count(); i++)
            {
                for (int j = 0; j < set[i].Item1.Count(); j++)
                {
                    if (set[i].Item1[j].Contains("+start"))
                    {
                        string input = maximalSet[i].Item1[j];
                        string output = input.Replace("+start", "+complete");

                        maximalSet[i].Item1[j] = output;
                    }
                }
                for (int j = 0; j < set[i].Item2.Count(); j++)
                {
                    if (set[i].Item2[j].Contains("+start"))
                    {
                        string input = maximalSet[i].Item2[j];
                        string output = input.Replace("+start", "+complete");

                        maximalSet[i].Item2[j] = output;
                    }
                }
            }

            for (int i = 0; i < maximalSet.Count(); i++)
            {
                for (int j = 0; j < maximalSet[i].Item1.Count(); j++)
                {
                    for (int l = 0; l < maximalSet[i].Item2.Count(); l++)
                    {
                        if (maximalSet[i].Item1[j] == maximalSet[i].Item2[l])
                        {
                            var anotherSet = maximalSet.Any(p => maximalSet[i].Item1[j] != maximalSet[i].Item2[l]);
                        }
                    }
                }
            }
            return maximalSet;
        }



        private static LoopOfLengthOne[] FindLoopsOfLengthOne(EventLog log)
        {
            var result = new List<LoopOfLengthOne>();

            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 1; i++)
                {
                    if (events[i].Activity == events[i + 1].Activity)
                    {
                        int loopStart = i;
                        int loopEnd = events.Count - 1;
                        for (int j = i; j < events.Count - 1; j++)
                        {
                            if (events[j].Activity != events[j + 1].Activity)
                            {
                                loopEnd = j;
                                break;
                            }
                        }

                        result.Add(new LoopOfLengthOne(
                            activity: events[i].Activity,
                            precedingActivity: events[loopStart - 1].Activity,
                            followingActivity: events[loopEnd + 1].Activity));

                        i = loopEnd;
                    }
                }
            }

            return result.Distinct().ToArray();
        }

        private static IEnumerable<string> GetActivitiesFromEvents(IEnumerable<Event> events)
        {
            return events.Select(@event => @event.Activity).Distinct();
        }



        private static IEnumerable<Tuple<string[], string[]>> FindMaximalSet(EventLog log, IEnumerable<string> transitions)
        {
            var footprintTable = new FootprintTable(log);
            var powerSet = transitions.Select(footprintTable.ActivityToIndex).PowerSet();

            var pairsSequence =
                from setA in powerSet
                from setB in powerSet
                where setA.Length > 0 && setB.Length > 0
                where AreActivitiesConnected(setA, setB, footprintTable)
                select new { setA, setB };

            // To prevent multiple enumeration
            var pairs = pairsSequence.ToArray();

            var nonMaximalPlaces =
                from place1 in pairs
                from place2 in pairs
                where place1 != place2
                where place1.setA.ContainsAll(place2.setA)
                    && place1.setB.ContainsAll(place2.setB)
                select place2;

            return pairs
                .Except(nonMaximalPlaces)
                .Select(pair => new Tuple<string[], string[]>(
                    pair.setA.Select(index => footprintTable.IndexToActivity(index)).ToArray(),
                    pair.setB.Select(index => footprintTable.IndexToActivity(index)).ToArray()));
        }

        private static bool AreActivitiesConnected(int[] inputActivityIndices, int[] outputActivityIndices, FootprintTable footprintTable)
        {
            // For every a1,a2 in A => a1#a2
            for (int i = 0; i < inputActivityIndices.Length - 1; i++)
            {
                for (int j = i + 1; j < inputActivityIndices.Length; j++)
                {
                    if (footprintTable[inputActivityIndices[i], inputActivityIndices[j]] != RelationType.NotConnected)
                    {
                        return false;
                    }
                }
            }

            // For every b1, b2 in B => b1#b2
            for (int i = 0; i < outputActivityIndices.Length - 1; i++)
            {
                for (int j = i + 1; j < outputActivityIndices.Length; j++)
                {
                    if (footprintTable[outputActivityIndices[i], outputActivityIndices[j]] != RelationType.NotConnected)
                    {
                        return false;
                    }
                }
            }

            // For every a in A and b in B => a > b in f
            return inputActivityIndices.All(first => outputActivityIndices.All(second => footprintTable[first, second] == RelationType.Precedes));
        }
    }
}