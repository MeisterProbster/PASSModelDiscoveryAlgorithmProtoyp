using System.Collections.Generic;

namespace XEStoPASS_alps.net.api
{
    public class PetriNet
    {
        public IReadOnlyList<string> Places { get; }

        public IReadOnlyList<string> Transitions { get; }

        public IReadOnlyList<Arc> Arcs { get; }

        public PetriNet(IReadOnlyList<string> places, IReadOnlyList<string> transitions, IReadOnlyList<Arc> arcs)
        {
            Places = places;
            Transitions = transitions;
            Arcs = arcs;
        }
    }
}