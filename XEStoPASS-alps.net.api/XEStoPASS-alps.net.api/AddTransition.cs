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
       

        public static void AddDoTransitionTo(IDoState sourceState, IState targetState)
        {
            string transitionName = sourceState + " completed"; //Label oder ID? 
            IDoTransition transition = new DoTransition(sourceState, targetState, additionalLabel: transitionName);
        }

        public static void AddSendTransitionTo(ISendState sourceState, IState targetState)
        {
            string transitionName = sourceState + " completed"; //Label oder ID? 
            IDoTransition transition = new DoTransition(sourceState, targetState, additionalLabel: transitionName);
        }

        public static void AddReceiveTransitionTo(IReceiveState sourceState, IState targetState)
        {
            string transitionName = "receive " + sourceState;
            IReceiveTransition transition = new ReceiveTransition(sourceState, targetState, additionalLabel: transitionName);
        }



        /*public static void AddDoToReceiveTransition(DoState sourceEvent, ReceiveState targetEvent)
        {
            string transitionName = sourceEvent.getModelComponentLabels().First() + " completed";
            IDoTransition transition = new DoTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddSendToReceiveTransition(SendState sourceEvent, ReceiveState targetEvent)
        {
            string transitionName = sourceEvent + " send";
            ISendTransition transition = new SendTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddSendToDoTransition(SendState sourceEvent, DoState targetEvent)
        {
            string transitionName = sourceEvent + " send";
            ISendTransition transition = new SendTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddSendToSendTransition(SendState sourceEvent, SendState targetEvent)
        {
            string transitionName = sourceEvent + " send";
            ISendTransition transition = new SendTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddReceiveToDoTransition(ReceiveState sourceEvent, DoState targetEvent)
        {
            string transitionName = "receive " + sourceEvent;
            IReceiveTransition transition = new ReceiveTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddReceiveToSendTransition(ReceiveState sourceEvent, SendState targetEvent)
        {
            string transitionName = "receive " + sourceEvent;
            IReceiveTransition transition = new ReceiveTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }

        public static void AddReceiveToReceiveTransition(ReceiveState sourceEvent, ReceiveState targetEvent)
        {
            string transitionName = "receive " + sourceEvent;
            IReceiveTransition transition = new ReceiveTransition(sourceEvent, targetEvent, additionalLabel: transitionName);
        }*/
    }
}
