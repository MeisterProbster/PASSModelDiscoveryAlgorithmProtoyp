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
    class AddTransition
    {
        public static IDoTransition AddDoTransitionTo(IDoState sourceState, IState targetState)
        {
            string transitionName = sourceState + " completed"; //Label oder ID? 
            IDoTransition transition = new DoTransition(sourceState, targetState, additionalLabel: transitionName);

            return transition;
        }

        public static ISendTransition AddSendTransitionTo(ISendState sourceState, IState targetState)
        {
            string transitionName = sourceState + " completed"; //Label oder ID? 
            ISendTransition transition = new SendTransition(sourceState, targetState, additionalLabel: transitionName);

            return transition;
        }

        public static IReceiveTransition AddReceiveTransitionTo(IReceiveState sourceState, IState targetState)
        {
            string transitionName = "receive " + sourceState;
            IReceiveTransition transition = new ReceiveTransition(sourceState, targetState, additionalLabel: transitionName);

            return transition;
        }
    }
}
