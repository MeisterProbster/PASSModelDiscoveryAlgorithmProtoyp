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
    class SupportFunctionsToAddStates
    {
        public static IDoState AddDoState(string eventName, ISubjectBehavior behavior)
        {
            string doName = eventName;
            IDoState doState = new DoState(behavior, additionalLabel: doName);
            doState.addComment(doName);
            return doState;
        }

        public static ISendState SendState(string eventName, ISubjectBehavior behavior)
        {
            string sendName = "send " + eventName + " completed";
            ISendState sendState = new SendState(behavior, additionalLabel: sendName);
            sendState.addComment(sendName);
            return sendState;
        }

        public static IReceiveState AddReceiveState(string eventName, string vorgängerEvent ,ISubjectBehavior behavior)
        {
            string receiveName = "wait for " + vorgängerEvent;
            IReceiveState receiveState = new ReceiveState(behavior, additionalLabel: receiveName);
            return receiveState;
        }
        
    }
}
