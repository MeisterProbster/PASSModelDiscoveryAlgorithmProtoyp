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
    class AddMessage
    {
        public static void NewMessage(string name, PASSProcessModel model, FullySpecifiedSubject subjectSource, FullySpecifiedSubject subjectTarget, ISendTransition sendTransition, string transitionName)
        {
            
            IMessageExchange Message = new MessageExchange(layer: model.getBaseLayer(), additionalLabel: name);

            Message.setSender(subjectSource);
            Message.setReceiver(subjectTarget);

            Message.setMessageType(new MessageSpecification(model.getBaseLayer(), transitionName, null, null, transitionName));
            ISendTransitionCondition sendReviewCompletedMessage = new SendTransitionCondition(sendTransition, transitionName, null, Message, 3, 1, null, subjectSource, Message.getMessageType());
            
        }
    }
}
