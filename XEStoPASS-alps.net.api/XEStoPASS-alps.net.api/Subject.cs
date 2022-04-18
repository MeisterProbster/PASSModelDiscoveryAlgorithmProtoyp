using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using alps.net.api.StandardPASS;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using alps.net.api;
using alps.net.api.parsing;
using alps.net.api.ALPS.ALPSModelElements;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace XEStoPASS_alps.net.api
{
    class Subject
    {
        public string Name { get; set; }
        public List<string> EventNames { get; set; }
        public List<Ressource> Ressources { get; set; }
        public List<Event> Events { get; set; }
        public List<List<Event>> Vorgänger { get; set; }
        public List<List<Event>> Nachfolger { get; set; }
        public IFullySpecifiedSubject fullySpecified { get; set; }
        public ISubjectBehavior subjectBehavior { get; set; }
        public List<IDoState> DoStates { get; set; }
        public List<List<List<IDoTransition>>> DoTransitions { get; set; }
        public List<List<ISendState>> SendStates { get; set; }
        public List<List<List<ISendTransition>>> SendTransitions { get; set; }
        public List<List<IReceiveState>> ReceiveStates { get; set; }
        public List<List<List<IReceiveTransition>>> ReceiveTransitions { get; set; }
        public List<List<IDoState>> HilfsDoStates { get; set; }
        public List<List<List<IDoTransition>>> HilfsDoTransitions { get; set; }
        public List<List<IReceiveState>> ReceiveEndStates { get; set; }
        public List<List<List<IReceiveTransition>>> ReceiveEndTransitions { get; set; }
        public List<List<IDoState>> DoEndStates { get; set; }
        public List<bool> Start { get; set; }
        public List<bool> WiederholungsEnds { get; set; }
        public List<bool> End { get; set; }
        public bool StartSubject { get; set; }
        public List<List<IMessageExchange>> MessageExchanges { get; set; }
        public List<List<string>> OrderList { get; set; }
        public List<List<string>> Next { get; set; }
        public List<List<string>> Previous { get; set; }
        public List<List<IState>> EndStates { get; set; }
        public List<List<IState>> StartStates { get; set; }


        public Subject(List<Ressource> ressources) //1
        {
            //Namen zusammenführen
            string name = null;
            foreach (Ressource r in ressources)
            {
                name = name + r.Name;
            }
            Name = name;

            //EventNamen zusammentragen
            List<string> eventNames = new List<string>();
            foreach (Ressource r in ressources)
            {
                foreach (string eventName in r.EventNameList)
                {
                    if (!eventNames.Contains(eventName))
                    {
                        eventNames.Add(eventName);
                    }
                    
                }

            }
            EventNames = eventNames;

            //Ressourcen zusammentragen
            List<Ressource> ressourcesInSubject = new List<Ressource>();
            foreach (Ressource r in ressources)
            {
                ressourcesInSubject.Add(r);
            }
            Ressources = ressourcesInSubject;

            //Events zusammentragen / bringt das überhaupt was? Um die States zu bilden brauche ich nur die Eventnamen/mögliche Nachfolger und Vorgänger (immer mit Event!)
            List<Event> events = new List<Event>();
            foreach (Ressource r in ressources)
            {
                foreach (Event @event in r.Events)
                {
                    events.Add(@event);
                }
            }
            Events = events;

            
            //Vorgänger zusammentragen / Vorgänger so zusammentragen, dass die vorgänger der gleichen eventnames abgeglichen (Kombinationen von Ressource mit Event) und zusammengefasst werden
            List<List<Event>> vorgänger = new List<List<Event>>();
            foreach (Ressource r in ressources)
            {
                foreach (List<Event> vorgängerList in r.Vorgänger)
                {
                        vorgänger.Add(vorgängerList);
                }
            }
            Vorgänger = vorgänger; 

            //Nachfolger zusammentragen
            List<List<Event>> nachfolger = new List<List<Event>>();
            foreach (Ressource r in ressources)
            {
                foreach (List<Event> nachfolgerList in r.Nachfolger)
                {
                    nachfolger.Add(nachfolgerList);
                }
            }
            Nachfolger = nachfolger; 
        }

        public void EventReihenfolge(XDocument document1) //2
        {

            List<List<string>> orderList = ConfirmOrder(document1, this.EventNames);
            OrderList = orderList;
        }

        public List<List<string>> ConfirmOrder(XDocument document, List<string> essentielleEvents) //3
        {
            var traces = document.Root
                .Elements("trace")
                .Select(traceElement => traceElement
                    .Elements("event")
                    .Select(eventElement => new Event(
                        tracename: traceElement.GetXesTracename("concept:name"),
                        name: eventElement.GetXesAttribute("concept:name"),
                        transition: eventElement.GetXesAttribute("lifecycle:transition"),
                        timestamp: Convert.ToDateTime(eventElement.GetXesAttribute("time:timestamp")),
                        resource: eventElement.GetXesAttribute("org:resource"))));

            var traces2 = new EventLog(traces.Select(trace => new Trace(trace.ToArray())).ToArray()).Traces;

            List<List<string>> orderList = new List<List<string>>();
            //orderList.Add(essentielleEvents);

            int q = 0;
            foreach (Trace trace in traces2)
            {
                IReadOnlyList<Event> ev = trace.Events;
                List<string> order = new List<string>();
                int eventIndex = 0;
                foreach (Event e in ev)
                {
                    if (essentielleEvents.Contains(e.Name))
                    {
                        if (!(e.Activity.Contains("+start")))
                        {
                            order.Add(e.Name);
                        }
                    }

                    //Falls eines deiner Events das Erste im Trace ist, bist du ein Startsubject
                    if (essentielleEvents.Contains(e.Name))
                    {
                        if (!(e.Activity.Contains("+start")))
                        {
                            if (eventIndex == 0)
                            {
                                StartSubject = true;
                            }
                        }
                    }

                    eventIndex++;
                }
                bool test = true;


                for (int i = 0; i < orderList.Count(); i++)
                {
                    if (orderList[i].SequenceEqual(order))
                    {
                        test = false;
                        break;
                    }
                }

                if (test)
                {
                    orderList.Add(order);
                }


                q++;
            }
            return orderList;
        }

        //Auch EventNameLsite durchlaufen
        //Laufe die Eventlisten ab, falls Namen gleich, führe zusammen und zwar so, dass Kombination von Subjekt + Event bei Nach/Vor nur einmal vorkommt
        public void MergeSameEvents() //4
        {
            List<Event> eventList = new List<Event>();
            List<List<Event>> vorgänger = new List<List<Event>>();
            List<List<Event>> nachfolger = new List<List<Event>>();

            int i = 0;
            foreach (Event e in this.Events)
            {
                if (!eventList.Any(p => p.Name == e.Name))
                {
                    eventList.Add(e);
                    vorgänger.Add(this.Vorgänger[i]);
                    nachfolger.Add(this.Nachfolger[i]);
                }
                else
                {
                    int j = eventList.FindIndex(p => p.Name == e.Name);
                    vorgänger[j] = vorgänger[j].Concat(this.Vorgänger[i]).ToList();
                    nachfolger[j] = vorgänger[j].Concat(this.Nachfolger[i]).ToList();
                }
                i++;
            }

            for (int m = 0; m < vorgänger.Count(); m++)
            {
                vorgänger[m] = vorgänger[m].GroupBy(x => x.Name).Select(x => x.First()).ToList();
            }

            for (int m = 0; m < nachfolger.Count(); m++)
            {
                nachfolger[m] = nachfolger[m].GroupBy(x => x.Name).Select(x => x.First()).ToList();
            }


            Vorgänger = vorgänger;
            Nachfolger = nachfolger;
            Events = eventList;
        }

        public void PreviousNext(List<string> allEvents, List<List<string>> directlyPreviousToThisEvent, List<List<string>> directlyNextToThisEvent) //5
        {
            List<List<string>> previous = new List<List<string>>();
            List<List<string>> next = new List<List<string>>();

            foreach (Event e in this.Events)
            {
                next.Add(new List<string> { });
                previous.Add(new List<string> { });
            }

            int i = 0;
            foreach (Event e in this.Events)
            {
                if (allEvents.Contains(e.Name + "+complete"))
                {

                    int index = allEvents.IndexOf(e.Name + "+complete");
                    if (!(index > directlyPreviousToThisEvent.Count()))
                    {
                        if (directlyPreviousToThisEvent[index].Count() > 0)
                        {
                            for (int j = 0; j < directlyPreviousToThisEvent[index].Count(); j++)
                            {
                                previous[i].Add(directlyPreviousToThisEvent[index][j]);
                            }
                        }
                    }

                    if (!(index == directlyNextToThisEvent.Count()))
                    {
                        if (directlyNextToThisEvent[index].Count() > 0)
                        {
                            for (int j = 0; j < directlyNextToThisEvent[index].Count(); j++)
                            {
                                next[i].Add(directlyNextToThisEvent[index][j]);
                            }
                        }
                    }


                }
                i++;
            }
            Next = next;
            Previous = previous;
        }

        public void CreateStates() //6
        {
            List<IDoState> doStates = new List<IDoState>();
            List<List<IReceiveState>> receiveStates = new List<List<IReceiveState>>();
            List<List<ISendState>> sendStates = new List<List<ISendState>>();
            List<List<IDoState>> hilfsDoStates = new List<List<IDoState>>();

            Start = new List<bool>();
            End = new List<bool>();
            ReceiveEndStates = new List<List<IReceiveState>>();
            DoEndStates = new List<List<IDoState>>();
            HilfsDoStates = new List<List<IDoState>>();

            DoTransitions = new List<List<List<IDoTransition>>>();
            SendTransitions = new List<List<List<ISendTransition>>>();
            ReceiveTransitions = new List<List<List<IReceiveTransition>>>();
            ReceiveEndTransitions = new List<List<List<IReceiveTransition>>>();
            HilfsDoTransitions = new List<List<List<IDoTransition>>>();

            int r = 0;
            foreach (string s in this.EventNames)
            {
                this.DoEndStates.Add(new List<IDoState>());
                this.ReceiveEndStates.Add(new List<IReceiveState>());
                this.HilfsDoStates.Add(new List<IDoState>());

                this.SendTransitions.Add(new List<List<ISendTransition>>());
                this.DoTransitions.Add(new List<List<IDoTransition>>());
                this.ReceiveTransitions.Add(new List<List<IReceiveTransition>>());
                this.ReceiveEndTransitions.Add(new List<List<IReceiveTransition>>());
                this.HilfsDoTransitions.Add(new List<List<IDoTransition>>());

                r++;
            }


            //Durchlaufe alle Events eines Subjektes
            int i = 0;
            foreach (Event e in Events)
            {
                //Erstelle für jedes Event einen Do-State
                IDoState doState = SupportFunctionsToAddStates.AddDoState(e.Name, subjectBehavior);
                doStates.Add(doState);

                //Lege für dieses Event eine neue Liste SendStates an, in der alle SendStates von diesem Event gespeichert werden
                List<ISendState> sends = new List<ISendState>();
                sendStates.Add(sends);

                //Lege für dieses Event eine neue Liste HilfsDoStates an, in der alle HilfsDoStates von diesem Event gespeichert werden
                List<IDoState> hilfs = new List<IDoState>();
                hilfsDoStates.Add(hilfs);

                //falls das Event Nachfolger hat, durchlaufe diese
                if (Next[i].Count() > 0)
                {
                    for (int j = 0; j < Next[i].Count(); j++)
                    {
                        string überprüfeName = Next[i][j].Replace("+complete", "");
                        //Falls das nächste Event ein anderes Subjekt ist, erstelle SendState, ansonsten erstelle kein SendState
                        if (!this.EventNames.Contains(überprüfeName))
                        {
                            //Falls das nächste Element nicht mit Xor startet und entweder das letze Element ist oder danach kein Xor kommt, erstelle ein "normales Send"
                            if (!Next[i][j].StartsWith("xor+"))
                            {
                                if (j == Next[i].Count() - 1)
                                {
                                    //Falls SendStates vorhanden, verbinde das letzte mit mir, ansonsten verbinde den DoState mit mir
                                    if (sends.Count() > 0)
                                    {
                                        ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);

                                        //SendStates vorhanden und letztes ist kein xor --> verbinde das letzte mit dem neuen SendState
                                        if (!sends.Last().getComments().Contains("xor@en"))
                                        {
                                            int letztes = sends.Count() - 1;
                                            ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[letztes], sendState);

                                            SendTransitions[i].Add(new List<ISendTransition>());

                                            SendTransitions[i].Last().Add(sendTransition);

                                        }
                                        //SendStates vorhanden und letztes ist xor --> verbinde alle letzten mit dem SendState
                                        else if (sends.Last().getComments().Contains("xor@en"))
                                        {
                                            for (int f = sends.Count() - 1; f < SendStates[i].Count(); f--)
                                            {
                                                if (sends[f].getComments().Contains("xor@en"))
                                                {
                                                    ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], sendState);

                                                    SendTransitions[i].Add(new List<ISendTransition>());

                                                    SendTransitions[i].Last().Add(sendTransition);
                                                }
                                                else
                                                {
                                                    ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], sendState);

                                                    SendTransitions[i].Add(new List<ISendTransition>());

                                                    SendTransitions[i].Last().Add(sendTransition);
                                                    break;
                                                }
                                            }
                                        }
                                        sends.Add(sendState);
                                    }
                                    else
                                    {
                                        ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);
                                        sends.Add(sendState);

                                        IDoTransition doTransition = AddTransition.AddDoTransitionTo(doState, sendState);

                                        DoTransitions[i].Add(new List<IDoTransition>());

                                        DoTransitions[i].Last().Add(doTransition);
                                    }
                                }
                                else if (!Next[i][j + 1].Contains("xor+"))
                                {
                                    //Falls SendStates vorhanden, verbinde das letzte mit mir, ansonsten verbinde den DoState mit mir
                                    if (sends.Count() > 0)
                                    {
                                        ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);
                                        sends.Add(sendState);

                                        int vorletztes = sends.Count() - 1;
                                        ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[vorletztes], sendState);

                                        SendTransitions[i].Add(new List<ISendTransition>());

                                        SendTransitions[i].Last().Add(sendTransition);
                                    }
                                    else
                                    {
                                        ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);
                                        sends.Add(sendState);

                                        IDoTransition doTransition = AddTransition.AddDoTransitionTo(doState, sendState);

                                        DoTransitions[i].Add(new List<IDoTransition>());

                                        DoTransitions[i].Last().Add(doTransition);
                                    }
                                }
                            }

                            if (j + 1 < Next[i].Count())
                            {
                                if (!Next[i][j].StartsWith("xor+") && Next[i][j + 1].StartsWith("xor+"))
                                {
                                    //Füge HilfsState ein
                                    IDoState supportDoState = SupportFunctionsToAddStates.AddDoState("supportDoState", subjectBehavior);

                                    HilfsDoStates[i].Add(supportDoState);

                                    //Falls SendStates vorhanden, verbinde das letzte mit mir, ansonsten verbinde den DoState mit mir
                                    if (sends.Count() > 0)
                                    {
                                        //SendStates vorhanden und letztes ist kein xor --> verbinde das letzte mit dem neuen supportState
                                        if (!sends.Last().getComments().Contains("xor@en"))
                                        {
                                            int letztes = sends.Count() - 1;
                                            ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[letztes], supportDoState);

                                            SendTransitions[i].Add(new List<ISendTransition>());

                                            SendTransitions[i].Last().Add(sendTransition);
                                        }
                                        //SendStates vorhanden und letztes ist xor --> verbinde alle letzten mit dem supportState
                                        else if (sends.Last().getComments().Contains("xor@en"))
                                        {
                                            for (int f = sends.Count() - 1; f < SendStates[i].Count(); f--)
                                            {
                                                if (sends[f].getComments().Contains("xor@en"))
                                                {
                                                    ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], supportDoState);

                                                    SendTransitions[i].Add(new List<ISendTransition>());

                                                    SendTransitions[i].Last().Add(sendTransition);
                                                }
                                                else
                                                {
                                                    ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], supportDoState);

                                                    SendTransitions[i].Add(new List<ISendTransition>());

                                                    SendTransitions[i].Last().Add(sendTransition); ;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        IDoTransition doTransition = AddTransition.AddDoTransitionTo(doState, supportDoState);

                                        DoTransitions[i].Add(new List<IDoTransition>());

                                        DoTransitions[i].Last().Add(doTransition);
                                    }

                                    //Erstelle einen SendState + Transiition von HilfsState zu SendState
                                    ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);

                                    sends.Add(sendState);

                                    IDoTransition doTransition1 = AddTransition.AddDoTransitionTo(supportDoState, sendState);

                                    HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                    HilfsDoTransitions[i].Last().Add(doTransition1);

                                    int k;
                                    //Für jedes folgende XorElement, erstelle einen SendState + Transiition von HilfsState zu SendState
                                    for (k = j + 1; k < Next[i].Count(); k++)
                                    {
                                        if (Next[i][k].StartsWith("xor+"))
                                        {
                                            ISendState sendState1 = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);
                                            sendState1.addComment("xor");
                                            sends.Add(sendState1);

                                            IDoTransition doTransition = AddTransition.AddDoTransitionTo(supportDoState, sendState1);

                                            HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                            HilfsDoTransitions[i].Last().Add(doTransition);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }

                                    //Setze den Counter von j ans Ende der XorListe
                                    j = k;
                                }
                            }
                        }
                    }
                }

                /*foreach (ISendState send in sends)
                {
                    SendTransitions[i].Add(new List<ISendTransition>());
                }*/



                if (sends.Count() > 0)
                {
                    //Falls es SendStates gibt und der letzte ein xor ist --> füge eine Hilfsvariable ein
                    if (sends.Last().getComments().Contains("xor@en"))
                    {
                        //Füge HilfsState ein
                        IDoState supportDoState = SupportFunctionsToAddStates.AddDoState("supportDoState", subjectBehavior);
                        hilfs.Add(supportDoState);

                        HilfsDoStates[i].Add(supportDoState);

                        for (int f = sends.Count() - 1; f < sends.Count(); f--)
                        {
                            if (sends[f].getComments().Contains("xor@en"))
                            {
                                ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], supportDoState);

                                SendTransitions[i].Add(new List<ISendTransition>());

                                SendTransitions[i].Last().Add(sendTransition);
                            }
                            else
                            {
                                ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends[f], supportDoState);

                                SendTransitions[i].Add(new List<ISendTransition>());

                                SendTransitions[i].Last().Add(sendTransition);
                                break;
                            }
                        }
                    }

                    //Erstelle eine Nachfolgerliste von möglichen DoStateGruppenNachfolger
                    List<string> EigenEventNachfolger = new List<string>();
                    foreach (List<string> vs in OrderList)
                    {
                        if (vs.Contains(e.Name))
                        {
                            if (vs.IndexOf(e.Name) < vs.Count() - 1)
                            {
                                if (!EigenEventNachfolger.Contains(vs[vs.IndexOf(e.Name) + 1]))
                                {
                                    EigenEventNachfolger.Add(vs[vs.IndexOf(e.Name) + 1]);
                                }

                            }
                        }
                    }

                    //Falls es SendStates gibt und der letzte normal ist, aber mehrere Nachfolger (evtl. Next?) exisitieren, dann füge eine Hilfsvariable ein
                    if (EigenEventNachfolger.Count() > 1)
                    {
                        if (!sends.Last().getComments().Contains("xor@en"))
                        {
                            //Füge HilfsState ein
                            IDoState supportDoState = SupportFunctionsToAddStates.AddDoState("supportDoState", subjectBehavior);
                            hilfs.Add(supportDoState);

                            HilfsDoStates[i].Add(supportDoState);

                            ISendTransition sendTransition = AddTransition.AddSendTransitionTo(sends.Last(), supportDoState);

                            SendTransitions[i].Add(new List<ISendTransition>());

                            SendTransitions[i].Last().Add(sendTransition);
                        }
                    }
                }



                List<IReceiveState> receives = new List<IReceiveState>();
                receiveStates.Add(receives);
                //falls das Event Vorgänger hat, durchlaufe diese
                if (Previous[i].Count() > 0)
                {
                    for (int j = 0; j < Previous[i].Count(); j++)
                    {
                        string überprüfeName = Previous[i][j].Replace("+complete", "");
                        //Erstelle nur einen ReceiveState, wenn Vorgänger nicht vom gleichen Subjekt ist
                        if (!this.EventNames.Contains(überprüfeName))
                        {
                            //Wenn ohne xor und falls nächstes existiert und nächstes auch ohne xor
                            if (!Previous[i][j].StartsWith("xor+"))
                            {
                                if (j == Previous[i].Count() - 1)
                                {
                                    if (receives.Count() > 0)
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        int vorletztes = receives.Count() - 1;
                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);
                                    }
                                    else
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, doState);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);
                                    }
                                }
                                else if (!Previous[i][j + 1].Contains("xor+"))
                                {
                                    if (receives.Count() > 0)
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        int vorletztes = receives.Count() - 1;
                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);
                                    }
                                    else
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, doState);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);
                                    }
                                }
                            }

                            //wenn nächstest exisitert und dieses ohne xor, aber nächstes mit
                            if (j + 1 < Previous[i].Count())
                            {
                                if (!Previous[i][j].StartsWith("xor+") && Previous[i][j + 1].StartsWith("xor+"))
                                {
                                    if (receives.Count() > 0)
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        int vorletztes = receives.Count() - 1;

                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);

                                        int k;
                                        //Für jedes folgende XorElement, erstelle einen SendState + Transiition von HilfsState zu SendState
                                        for (k = j + 1; k < Previous[i].Count(); k++)
                                        {
                                            if (Previous[i][k].StartsWith("xor+"))
                                            {
                                                IReceiveTransition receiveTransition1 = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                                ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                                ReceiveTransitions[i].Last().Add(receiveTransition1);
                                            }
                                        }

                                        //Setze den Counter von j ans Ende der XorListe
                                        j = k;
                                    }
                                    else
                                    {
                                        IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j], subjectBehavior);
                                        receives.Add(receiveState);

                                        IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, doState);

                                        ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                        ReceiveTransitions[i].Last().Add(receiveTransition);

                                        int k;
                                        //Für jedes folgende XorElement, erstelle einen SendState + Transiition von HilfsState zu SendState
                                        for (k = j + 1; k < Previous[i].Count(); k++)
                                        {
                                            if (Previous[i][k].StartsWith("xor+"))
                                            {
                                                IReceiveTransition receiveTransition1 = AddTransition.AddReceiveTransitionTo(receiveState, doState);

                                                ReceiveTransitions[i].Add(new List<IReceiveTransition>());

                                                ReceiveTransitions[i].Last().Add(receiveTransition1);
                                            }
                                            else
                                            {
                                                break;
                                            }


                                        }

                                        //Setze den Counter von j ans Ende der XorListe
                                        j = k;
                                    }
                                }
                            }
                        }

                    }
                }
                i++;
            }

            DoStates = doStates;
            ReceiveStates = receiveStates;
            SendStates = sendStates;
        }

        public void DoStateGruppenVerbinden() //7
        {
            List<bool> wiederholungsEnds = new List<bool>();
            int i = 0;
            foreach (Event ev in this.Events)
            {
                //Erstelle die Nachfolgerliste der Events, um jede mögliche Reihenfolge der DoStateGruppen auslesen zu können
                List<string> nachfolger = new List<string>();
                bool endBool = false;
                bool wiederholungsEndBool = false;
                bool startBool = false;
                foreach (List<string> vs in OrderList)
                {
                    if (vs.Contains(ev.Name))
                    {
                        if (!(vs.IndexOf(ev.Name) == vs.Count() - 1))
                        {
                            int nachfolgerIndex = (vs.IndexOf(ev.Name)) + 1;
                            if (!nachfolger.Contains(vs[nachfolgerIndex]))
                            {
                                nachfolger.Add(vs[nachfolgerIndex]);
                            }
                        }
                        //Starteigenschaft
                        if (vs.IndexOf(ev.Name) == 0)
                        {
                            startBool = true;
                        }
                        //Ist Event letztes Event?
                        if (ev.Name == vs.Last())
                        {
                            endBool = true;
                            //Ist Event davor das gleiche Event, falls ja --> Wiederholung möglich
                            if (vs.Count() > 1)
                            {
                                if (ev.Name == vs[vs.Count() - 2])
                                {
                                    wiederholungsEndBool = true;
                                    endBool = false;
                                    break;
                                }
                            }

                        }

                        //falls Wiederholung möglich, kontrolliere ob ReceiveState vorhanden, falls ja lege Kopie an und führe zurück zum DoState, ReceiveCopy wird EndState. Transitionen von Send oder DoState zu eigenem Receive nicht zulässig!


                    }
                }

                if (startBool)
                {
                    Start.Add(true);
                }
                else
                {
                    Start.Add(false);
                }

                if (endBool || wiederholungsEndBool)
                {
                    End.Add(true);
                }
                else
                {
                    End.Add(false);
                }


                //Falls kein ReceiveState vorhanden, dann nach Send/Do EndState einfügen, Transition zum eigenen DOState möglich
                //Erstelle einen EndState, wenn: Event als letztes auftritt und keine Wiederholung des Events am Ende möglich ist verbinde mit letztem SendState oder DoState
                if (endBool)
                {
                    if (SendStates[i].Count() > 0)
                    {
                        if (HilfsDoStates[i].Count() > 0)
                        {
                            IDoState EndState = SupportFunctionsToAddStates.AddDoState("End", this.subjectBehavior);

                            DoEndStates[i].Add(EndState);

                            IDoTransition doTransition = AddTransition.AddDoTransitionTo(HilfsDoStates[i].Last(), EndState);

                            HilfsDoTransitions[i].Add(new List<IDoTransition>());

                            HilfsDoTransitions[i].Last().Add(doTransition);
                        }
                        else
                        {
                            IDoState EndState = SupportFunctionsToAddStates.AddDoState("End", this.subjectBehavior);

                            DoEndStates[i].Add(EndState);

                            ISendTransition sendTransition = AddTransition.AddSendTransitionTo(SendStates[i].Last(), EndState);

                            SendTransitions[i].Add(new List<ISendTransition>());

                            SendTransitions[i].Last().Add(sendTransition);
                        }
                    }
                    else
                    {
                        IDoState EndState = SupportFunctionsToAddStates.AddDoState("End", this.subjectBehavior);

                        DoEndStates[i].Add(EndState);

                        IDoTransition doTransition = AddTransition.AddDoTransitionTo(this.DoStates[i], EndState);

                        DoTransitions[i].Add(new List<IDoTransition>());

                        DoTransitions[i].Last().Add(doTransition);
                    }

                }

                if (wiederholungsEndBool)
                {
                    wiederholungsEnds.Add(true);
                    End[i] = true;
                }
                else
                {
                    wiederholungsEnds.Add(false);
                }

                foreach (string s in nachfolger)
                {
                    foreach (Event @event in this.Events)
                    {
                        if (@event.Name == s)
                        {
                            int indexEvent = this.Events.IndexOf(@event);

                            if (this.Events[i].Name == s)
                            {
                                //Es ist das gleiche Event
                                if (wiederholungsEnds[i])
                                {
                                    //Das Event ist ein WiederholungsEnde
                                    if (ReceiveStates[i].Count() > 0)
                                    {
                                        //Das Event hat einen ReceiveState
                                        //Erstelle für alle ReceiveStates und Transitionen eine Kopie
                                        List<IReceiveState> receives = new List<IReceiveState>();
                                        //falls das Event Vorgänger hat, durchlaufe diese
                                        if (Previous[i].Count() > 0)
                                        {
                                            for (int j = 0; j < Previous[i].Count(); j++)
                                            {
                                                string überprüfeName = Previous[i][j].Replace("+complete", "");
                                                //Erstelle nur einen ReceiveState, wenn Vorgänger nicht vom gleichen Subjekt ist
                                                if (!this.EventNames.Contains(überprüfeName))
                                                {
                                                    //Wenn ohne xor und falls vorheriges existiert und nächstes auch ohne xor
                                                    if (!Previous[i][j].StartsWith("xor+"))
                                                    {
                                                        if (j == Previous[i].Count() - 1)
                                                        {
                                                            if (receives.Count() > 0)
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                int vorletztes = receives.Count() - 1;
                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);
                                                            }
                                                            else
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, DoStates[i]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);
                                                            }
                                                        }
                                                        else if (!Previous[i][j + 1].Contains("xor+"))
                                                        {
                                                            if (receives.Count() > 0)
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                int vorletztes = receives.Count() - 1;
                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);
                                                            }
                                                            else
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, DoStates[i]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);
                                                            }
                                                        }
                                                    }

                                                    //wenn vorheriges exisitert und dieses ohne xor, aber nächstes mit
                                                    if (j + 1 < Previous[i].Count())
                                                    {
                                                        if (!Previous[i][j].StartsWith("xor+") && Previous[i][j + 1].StartsWith("xor+"))
                                                        {
                                                            if (receives.Count() > 0)
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                int vorletztes = receives.Count() - 1;
                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);

                                                                int k;
                                                                //Für jedes folgende XorElement, erstelle einen SendState + Transiition von HilfsState zu SendState
                                                                for (k = j + 1; k < Previous[i].Count(); k++)
                                                                {
                                                                    if (Previous[i][k].StartsWith("xor+"))
                                                                    {
                                                                        IReceiveTransition receiveTransition1 = AddTransition.AddReceiveTransitionTo(receiveState, receives[vorletztes]);

                                                                        ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                        ReceiveEndTransitions[i].Last().Add(receiveTransition1);
                                                                    }
                                                                }

                                                                //Setze den Counter von j ans Ende der XorListe
                                                                j = k;
                                                            }
                                                            else
                                                            {
                                                                IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(Events[i].Name + " Copy", Previous[i][j], subjectBehavior);
                                                                receives.Add(receiveState);

                                                                ReceiveEndStates[i].Add(receiveState);

                                                                IReceiveTransition receiveTransition = AddTransition.AddReceiveTransitionTo(receiveState, DoStates[i]);

                                                                ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                ReceiveEndTransitions[i].Last().Add(receiveTransition);

                                                                int k;
                                                                //Für jedes folgende XorElement, erstelle einen SendState + Transiition von HilfsState zu SendState
                                                                for (k = j + 1; k < Previous[i].Count(); k++)
                                                                {
                                                                    if (Previous[i][k].StartsWith("xor+"))
                                                                    {
                                                                        IReceiveTransition receiveTransition1 = AddTransition.AddReceiveTransitionTo(receiveState, DoStates[i]);

                                                                        ReceiveEndTransitions[i].Add(new List<IReceiveTransition>());

                                                                        ReceiveEndTransitions[i].Last().Add(receiveTransition1);
                                                                    }
                                                                    else
                                                                    {
                                                                        break;
                                                                    }


                                                                }

                                                                //Setze den Counter von j ans Ende der XorListe
                                                                j = k;
                                                            }
                                                        }
                                                    }
                                                }

                                            }
                                        }

                                        if (SendStates[i].Count() > 0)
                                        {
                                            //Das Event hat SendState(s)
                                            if (HilfsDoStates[i].Count() > 0)
                                            {
                                                //Das Event hat als Abschluss einen HilfsDoState
                                                IDoTransition doTransition = AddTransition.AddDoTransitionTo(HilfsDoStates[i].Last(), receives.Last());

                                                HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                                HilfsDoTransitions[i].Last().Add(doTransition);

                                                //receives.Last().EndState;
                                            }
                                            else
                                            {
                                                //Das Event hat als Abschluss einen SendState
                                                ISendTransition sendTransition = AddTransition.AddSendTransitionTo(SendStates[i].Last(), receives.Last());

                                                SendTransitions[i].Add(new List<ISendTransition>());

                                                SendTransitions[i].Last().Add(sendTransition);

                                                //receives.Last().EndState;
                                            }
                                        }
                                        else
                                        {
                                            //Das Event hat keine SendStates
                                            IDoTransition doTransition = AddTransition.AddDoTransitionTo(DoStates[i], receives.Last());

                                            DoTransitions[i].Add(new List<IDoTransition>());

                                            DoTransitions[i].Last().Add(doTransition);

                                            IDoState EndState = SupportFunctionsToAddStates.AddDoState("EndState", subjectBehavior);

                                            DoEndStates[i].Add(EndState);
                                            //EndState.EndState;

                                            IDoTransition doTransition1 = AddTransition.AddDoTransitionTo(DoStates[i], EndState);

                                            DoTransitions[i].Add(new List<IDoTransition>());

                                            DoTransitions[i].Last().Add(doTransition1);
                                        }
                                    }
                                    else
                                    {
                                        //Das Event hat keinen ReceiveState
                                        if (SendStates[i].Count() > 0)
                                        {
                                            //Das Event hat SendState(s)
                                            if (HilfsDoStates[i].Count() > 0)
                                            {
                                                //Das Event hat als Abschluss einen HilfsDoState
                                                IDoState EndState = SupportFunctionsToAddStates.AddDoState("EndState", subjectBehavior);

                                                DoEndStates[i].Add(EndState);

                                                IDoTransition doTransition = AddTransition.AddDoTransitionTo(HilfsDoStates[i].Last(), EndState);
                                                IDoTransition doTransition1 = AddTransition.AddDoTransitionTo(HilfsDoStates[i].Last(), DoStates[i]);

                                                HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                                HilfsDoTransitions[i].Last().Add(doTransition);

                                                HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                                HilfsDoTransitions[i].Last().Add(doTransition1);

                                                //receives.Last().EndState;
                                            }
                                            else
                                            {
                                                //Das Event hat als Abschluss einen SendState

                                                IDoState EndState = SupportFunctionsToAddStates.AddDoState("EndState", subjectBehavior);

                                                DoEndStates[i].Add(EndState);

                                                IDoState HilfsState = SupportFunctionsToAddStates.AddDoState("HilfsState", subjectBehavior);

                                                HilfsDoStates[i].Add(HilfsState);

                                                ISendTransition sendTransition = AddTransition.AddSendTransitionTo(SendStates[i].Last(), HilfsState);

                                                SendTransitions[i].Add(new List<ISendTransition>());

                                                SendTransitions[i].Last().Add(sendTransition);

                                                IDoTransition doTransition = AddTransition.AddDoTransitionTo(HilfsState, EndState);
                                                IDoTransition doTransition1 = AddTransition.AddDoTransitionTo(HilfsState, DoStates[i]);

                                                HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                                HilfsDoTransitions[i].Last().Add(doTransition);

                                                HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                                HilfsDoTransitions[i].Last().Add(doTransition1);
                                                //receives.Last().EndState;
                                            }
                                        }
                                        else
                                        {
                                            //Das Event hat keine SendStates
                                            IDoTransition doTransition = AddTransition.AddDoTransitionTo(DoStates[i], DoStates[i]);

                                            DoTransitions[i].Add(new List<IDoTransition>());

                                            DoTransitions[i].Last().Add(doTransition);

                                            IDoState EndState = SupportFunctionsToAddStates.AddDoState("EndState", subjectBehavior);

                                            DoEndStates[i].Add(EndState);
                                            //EndState.EndState;

                                            IDoTransition doTransition1 = AddTransition.AddDoTransitionTo(DoStates[i], EndState);

                                            DoTransitions[i].Add(new List<IDoTransition>());

                                            DoTransitions[i].Last().Add(doTransition1);
                                        }
                                    }
                                }
                            }

                            if (!wiederholungsEnds[i])
                            {
                                //Empfänger hat einen ReceiveState
                                if (this.ReceiveStates[indexEvent].Count() > 0)
                                {
                                    if (this.SendStates[i].Count() > 0)
                                    {
                                        //Das Event hat SendState(s)
                                        if (HilfsDoStates[i].Count() > 0)
                                        {
                                            //Das Event hat als Abschluss einen HilfsDoState
                                            IDoTransition doTransition = AddTransition.AddDoTransitionTo(HilfsDoStates[i].Last(), this.ReceiveStates[indexEvent].Last());

                                            HilfsDoTransitions[i].Add(new List<IDoTransition>());

                                            HilfsDoTransitions[i].Last().Add(doTransition);

                                        }
                                        else
                                        {
                                            //Das Event hat als Abschluss einen SendState
                                            ISendTransition sendTransition = AddTransition.AddSendTransitionTo(SendStates[i].Last(), this.ReceiveStates[indexEvent].Last());

                                            SendTransitions[i].Add(new List<ISendTransition>());

                                            SendTransitions[i].Last().Add(sendTransition);

                                        }
                                    }
                                    else
                                    {
                                        //Das Event hat keine SendStates
                                        IDoTransition doTransition = AddTransition.AddDoTransitionTo(DoStates[i], this.ReceiveStates[indexEvent].Last());

                                        DoTransitions[i].Add(new List<IDoTransition>());

                                        DoTransitions[i].Last().Add(doTransition);

                                    }
                                }
                            }

                        }
                    }
                }
                i++;
            }
            WiederholungsEnds = wiederholungsEnds;
        }

        public void CreateMessages(PASSProcessModel model, List<Subject> subjects, List<List<string>> directlyNextToThisEvent, List<string> allEvents) //8
        {
            MessageExchanges = new List<List<IMessageExchange>>();

            int i = 0;
            foreach (Event e in this.Events)
            {
                MessageExchanges.Add(new List<IMessageExchange>());

                int sourceIndex = allEvents.IndexOf(e.Name + "+complete");

                if (!(sourceIndex > directlyNextToThisEvent.Count() - 1))
                {
                    if (directlyNextToThisEvent[sourceIndex].Count() > 0)
                    {
                        //Event hat Nachfolger
                        for (int j = 0; j < directlyNextToThisEvent[sourceIndex].Count(); j++)
                        {
                            //Durchlaufe alle Nachfolger
                            if (!this.EventNames.Contains(directlyNextToThisEvent[sourceIndex][j].Replace("xor+","").Replace("+complete","")))
                            {
                                //nächstes Event gehört zu einem anderen Subject
                                

                                //ZielSubject auswählen
                                int zielIndex = allEvents.IndexOf(directlyNextToThisEvent[sourceIndex][j].Replace("xor+", ""));
                                foreach (Subject s in subjects)
                                {
                                    if (s.EventNames.Contains(allEvents[zielIndex].Replace("+complete", "")) && !this.EventNames.Contains(allEvents[zielIndex].Replace("+complete", "")) && s.EventNames.Contains(directlyNextToThisEvent[sourceIndex][j].Replace("+complete","").Replace("xor+","")))
                                    {
                                        if (SendTransitions[i].Count() == 1)
                                        {
                                            //Falls nur eine Message gesendet wird, ist die SendTransition eindeutig bestimmbar
                                            IMessageExchange messageName = new MessageExchange(model.getBaseLayer());
                                            messageName.setSender(this.fullySpecified);
                                            messageName.setReceiver(s.fullySpecified);


                                            messageName.setMessageType(new MessageSpecification(model.getBaseLayer(), e.Name + " completed", null, null, e.Name + " completed"));
                                            ISendTransitionCondition messageNameCondition = new SendTransitionCondition(SendTransitions[i][0][0], e.Name + " completed", null, messageName, 0, 0, null, this.fullySpecified, messageName.getMessageType());
                                            
                                            MessageExchanges[i].Add(messageName);
                                        }
                                        else
                                        {
                                            //Falls es mehrere SendTransitionen gibt, muss klar definiert werden, welche Transition zu welchem Subjekt geht
                                            //Event [i] ist klar und Transition auch [0], fehlt noch der SendState
                                            //Der sendState muss erstellt worden sein, weil das Next[i]Event ein Event des Subjekts subjects[j] ist
                                            //Index dieses SendStates herausfinden und bei ? eintragen
                                            //for (int k = 0; k < SendTransitions[i].Count(); k++)
                                            //{
                                                if (s.EventNames.Contains(Next[i][j].Replace("xor+", "").Replace("+complete", "")))
                                                {
                                                    IMessageExchange messageName = new MessageExchange(model.getBaseLayer());
                                                    messageName.setSender(this.fullySpecified);
                                                    messageName.setReceiver(s.fullySpecified);


                                                    messageName.setMessageType(new MessageSpecification(model.getBaseLayer(), e.Name + " completed", null, null, e.Name + " completed"));
                                                    ISendTransitionCondition messageNameCondition = new SendTransitionCondition(SendTransitions[i][j][0], e.Name + " completed", null, messageName, 0, 0, null, this.fullySpecified, messageName.getMessageType());

                                                    MessageExchanges[i].Add(messageName);
                                                }
                                            //}
                                        }
                                        
                                    }
                                }

                            }
                        }
                    }
                }
                i++;
            }
        }

        public void InitializeEndStates() //9
        {
            EndStates = new List<List<IState>>();
            StartStates = new List<List<IState>>();
            
            int i = 0;
            foreach (string s in this.EventNames)
            {
                EndStates.Add(new List<IState>());
                StartStates.Add(new List<IState>());
                if (End[i])
                {
                    if (DoEndStates[i].Count() > 0)
                    {
                        DoEndStates[i].Last().setIsStateType(IState.StateType.EndState);
                        IState state = DoEndStates[i].Last();
                        EndStates[i].Add(state);
                    }

                    if (ReceiveEndStates[i].Count() > 0)
                    {
                        ReceiveEndStates[i].Last().setIsStateType(IState.StateType.EndState);
                        IState state = ReceiveEndStates[i].Last();
                        EndStates[i].Add(state);
                    }
                }

                if (Start[i])
                {
                    //Erster State des Events kann StartState sein 
                    if (ReceiveStates[i].Count() > 0)
                    {
                        this.subjectBehavior.setInitialState(ReceiveStates[i].Last());
                        IState state = ReceiveStates[i].Last();
                        StartStates[i].Add(state);
                    }
                    else
                    {
                        this.subjectBehavior.setInitialState(this.DoStates[i]);
                        IState state = DoStates[i];
                        StartStates[i].Add(state);
                    }
                }
                i++;
            }
        } 
    }
}
