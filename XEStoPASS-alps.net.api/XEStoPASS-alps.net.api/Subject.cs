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
        public List<List<ISendState>> SendStates { get; set; }
        public List<List<IReceiveState>> ReceiveStates { get; set; }
        public List<List<string>> OrderList { get; set; }
        public List<List<string>> Next { get; set; }
        public List<List<string>> Previous { get; set; }

        public Subject(string name)
        {
            Name = name;
            Ressources = new List<Ressource>();
            Events = new List<Event>();
            Vorgänger = new List<List<Event>>();
            Nachfolger = new List<List<Event>>();
        }

        public Subject(List<Ressource> ressources)
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

        /// <summary>
        /// Create new Subject
        /// </summary>
        /// <param name="ressource1"></param>
        /// <param name="ressource2"></param>
        public Subject(Ressource ressource1, Ressource ressource2)
        {
            this.Name = ressource1.Name + ressource2.Name;
            //this.Ressources.Add(ressource1);
            //this.Ressources.Add(ressource2);

            for (int i = 0; i < ressource1.Events.Count; i++)
            {
                this.Events.Add(ressource1.Events[i]);
            }
            for (int i = 0; i < ressource2.Events.Count; i++)
            {
                if (!this.Events.Contains(ressource2.Events[i]))
                {
                    this.Events.Add(ressource2.Events[i]);
                }
            }

            for (int i = 0; i < ressource1.EventNameList.Count; i++)
            {
                this.EventNames.Add(ressource1.EventNameList[i]);
            }
            for (int i = 0; i < ressource2.EventNameList.Count; i++)
            {
                if (!this.EventNames.Contains(ressource2.EventNameList[i]))
                {
                    this.EventNames.Add(ressource2.EventNameList[i]);
                }
            }

            //Nachfolger- / Vorgängerlisten zu den jeweiligen Events adden
            //auf Duplikate überprüfen
            for (int i = 0; i < ressource1.Vorgänger.Count; i++)
            {
                this.Vorgänger.Add(ressource1.Vorgänger[i]);
            }
            for (int i = 0; i < ressource2.Vorgänger.Count; i++)
            {
                if (!this.Vorgänger.Contains(ressource2.Vorgänger[i]))
                {
                    this.Vorgänger.Add(ressource2.Vorgänger[i]);
                }
            }

            for (int i = 0; i < ressource1.Nachfolger.Count; i++)
            {
                this.Nachfolger.Add(ressource1.Nachfolger[i]);
            }
            for (int i = 0; i < ressource2.Nachfolger.Count; i++)
            {
                if (!this.Nachfolger.Contains(ressource2.Nachfolger[i]))
                {
                    this.Nachfolger.Add(ressource2.Nachfolger[i]);
                }
            }
        }

        /// <summary>
        /// Add new Resource 
        /// </summary>
        /// <param name="ressource"></param>
        public void AddResource(Ressource ressource)
        {
            this.Name = this.Name + ressource.Name;
            this.Ressources.Add(ressource);

            for (int i = 0; i < ressource.Events.Count; i++)
            {
                if (!this.Events.Contains(ressource.Events[i]))
                {
                    this.Events.Add(ressource.Events[i]);
                }
            }
            for (int i = 0; i < ressource.EventNameList.Count; i++)
            {
                if (!this.EventNames.Contains(ressource.EventNameList[i]))
                {
                    this.EventNames.Add(ressource.EventNameList[i]);
                }
            }

            //Nachfolger- / Vorgängerlisten zu den jeweiligen Events adden
            //auf Duplikate überprüfen
            for (int i = 0; i < ressource.Vorgänger.Count; i++)
            {
                if (!this.Vorgänger.Contains(ressource.Vorgänger[i]))
                {
                    this.Vorgänger.Add(ressource.Vorgänger[i]);
                }
            }

            for (int i = 0; i < ressource.Nachfolger.Count; i++)
            {
                if (!this.Nachfolger.Contains(ressource.Nachfolger[i]))
                {
                    this.Nachfolger.Add(ressource.Nachfolger[i]);
                }
            }
        }

        //Auch EventNameLsite durchlaufen
        //Laufe die Eventlisten ab, falls Namen gleich, führe zusammen und zwar so, dass Kombination von Subjekt + Event bei Nach/Vor nur einmal vorkommt
        public void MergeSameEvents()
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

        public void EventReihenfolge(XDocument document1)
        {

            List<List<string>> orderList = ConfirmOrder(document1, this.EventNames);
            OrderList = orderList;
        }

        public static List<List<string>> ConfirmOrder(XDocument document, List<string> essentielleEvents)
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
            orderList.Add(essentielleEvents);

            int q = 0;
            foreach (Trace trace in traces2)
            {
                IReadOnlyList<Event> ev = trace.Events;
                List<string> order = new List<string>();
                foreach (Event e in ev)
                {
                    if (essentielleEvents.Contains(e.Name))
                    {
                        if (!(e.Activity.Contains("+start")))
                        {
                            order.Add(e.Name);
                        }
                    }
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

        /*Durchlaufe alle Events mit Index
         *      erstelle für jedes einen DoState
         *          falls Nachfolger vorhanden und nicht vom gleichen Subjekt erstelle für jeden einen SendState
         *          falls Vorgänger vorhanden und nicht vom gleichen Subjekt erstelle für einen ReceiveState, falls ein ReceiveState vorhanden erstelle keine weiteren mehr
         *          
         *          allevents und directlynextevents
         *              alleventsdruchlaifen, falls mit meinem eventname übereinstimmr index bestimmen
         *                  directlynextevtns an diesem INdex durhclaufen
         *                      falls nur eins --> Send + Transition
         *                      falls zwei und zweites enthält xor --> zwei Send mit je einer Tranition
         *                      falls mehrere und kein xor --> für jedes ein Send und verbinden von do state zum ersten, dann vom ersten zum zweiten usw
         *                      generell: eins oder mehrere?
         *                      falls mehrere:  1.Nomral erstelle Send verbinde Send und vorgänger
         *                                      2.Xor erstelle Send und verbinde Send und vorgänger von dem ersten normalen vor allen xors                                                           
         *                                      3.Xor erstelle Send und verbinde Send und vorgänger von dem ersten normalen vor allen xors
         *                                      4.Nomral erstelle Send und verbinde mit allen xor bis zu dem nächsten normalen vor dir
         *                                      5.normal erstelle Send und verbinde mit dem vor dir
         *                                      6.Xor erstelle Send und verbinde Send und vorgänger von dem ersten normalen vor allen xors 
         */
        public void CreateStates()
        {
            List<IDoState> doStates = new List<IDoState>();
            List<List<IReceiveState>> receiveStates = new List<List<IReceiveState>>();
            List<List<ISendState>> sendStates = new List<List<ISendState>>();

            int i = 0;
            foreach (Event e in Events)
            {
                IDoState doState = SupportFunctionsToAddStates.AddDoState(e.Name, subjectBehavior);
                doStates.Add(doState);

                if (true)//Next[i].Count() > 0)
                {
                    List<ISendState> sends = new List<ISendState>();
                    sendStates.Add(sends);
                    foreach (string name in Next[i])
                    {
                        ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, subjectBehavior);
                        sends.Add(sendState);


                        AddTransition.AddDoTransitionTo(doState, sendState);

                        
                    }

                }

                if (true)//Previous[i].Count() > 0)
                {
                    List<IReceiveState> receives = new List<IReceiveState>();
                    receiveStates.Add(receives);

                    for (int j = 0; j < Previous[i].Count(); j++)
                    {
                        if (j == 0)
                        {
                            IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, Previous[i][j] , subjectBehavior);
                            receives.Add(receiveState);
                            AddTransition.AddReceiveTransitionTo(receiveState, doState);
                        }
                        else
                        {
                            AddTransition.AddReceiveTransitionTo(receiveStates[i][0], doState);
                        }
                    }
                }
                i++;
            }

            DoStates = doStates;
            ReceiveStates = receiveStates;
            SendStates = sendStates;
        }

        /* Durhclaufe allEvents
         *      falls event vorhanden
         *             bekomme Index
         *             suche Nachfolger und Vorgänger in den Listen
         *             trage sie in die NachfolgerVorgänger listen ein
         */
        public void PreviousNext(List<string> allEvents, List<List<string>> directlyPreviousToThisEvent, List<List<string>> directlyNextToThisEvent)
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

        /*richtige Reihenfolge der Do-State-Pakete --> OrderList
         *      Durchlaufe für jedes Event die OrderList
         *          lege eine Nachfolgerliste an
         *              durhclaufe nochmals die Orderlisten
         *                  Wenn ein Event auf mich folgt, dass noch nicht in Nachfolgerliste steht --> adde
         *                  ansonsten mache nichts
         *              wenn alle durlcaufen, haben wir alle möglich Nachfogler
         *                  falls ich einen SendState habe
         *                      verbinde eine Sendtransition von meinem Sendstate zu jedem Receive(falls vorhanden) oder DoState meiner NAchfolger
         *                  falls ich kein SendState habe
         *                      verbinde eine Dotransition von meinem Dostate zu jedem Receive(falls vorhanden) oder DoState meiner NAchfolger
         */

        public void DoStateGruppenVerbinden()
        {
            int i = 0;
            foreach(Event ev in this.Events)
            {
                List<string> nachfolger = new List<string>();
                foreach (List<string> vs in OrderList)
                {
                    if (vs.Contains(ev.Name))
                    {
                        if (!(vs.IndexOf(ev.Name) == vs.Count()-1))
                        {
                            int nachfolgerIndex = (vs.IndexOf(ev.Name)) + 1;
                            if (!nachfolger.Contains(vs[nachfolgerIndex]))
                            {
                                nachfolger.Add(vs[nachfolgerIndex]);
                            }
                        }
                    }
                    else
                    {
                        //erstelle DoState mit Endfunktion
                    }
                }


                foreach (string s in nachfolger)
                {
                    foreach (Event @event in this.Events)
                    {
                        if (@event.Name == s)
                        {
                            int indexEvent = this.Events.IndexOf(@event);

                            if (this.ReceiveStates[indexEvent].Count() > 0)
                            {
                                if (this.SendStates[i].Count() > 0)
                                {
                                    foreach (SendState sendState in this.SendStates[i])
                                    {
                                        AddTransition.AddSendTransitionTo(sendState, this.ReceiveStates[indexEvent][0]);
                                    }
                                }
                                else
                                {
                                    AddTransition.AddDoTransitionTo(this.DoStates[i], this.ReceiveStates[indexEvent][0]);
                                }
                            }
                            else
                            {
                                if (this.SendStates[i].Count() > 0)
                                {
                                    foreach (SendState sendState in this.SendStates[i])
                                    {
                                        AddTransition.AddSendTransitionTo(sendState, this.DoStates[indexEvent]);
                                    }
                                }
                                else
                                {
                                    AddTransition.AddDoTransitionTo(this.DoStates[i], this.DoStates[indexEvent]);
                                }
                            }
                        }

                        
                    }
                }
                i++;
            }
        }
    }
}
