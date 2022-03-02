using alps.net.api.StandardPASS;
using System;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using System.Linq;
using alps.net.api;
using System.Collections.Generic;
using alps.net.api.parsing;
using alps.net.api.ALPS.ALPSModelElements;

namespace XEStoPASS_alps.net.api
{
    class AddSBD
    {
        public static ISubjectBehavior NewSBD(IFullySpecifiedSubject subject)
        {
            ISubjectBehavior behavior = subject.getBehaviors().Values.First();
            return behavior;
        }
    }
}
