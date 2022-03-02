using alps.net.api.StandardPASS;
using System;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using System.Linq;
using alps.net.api;
using System.Collections.Generic;
using alps.net.api.parsing;
using alps.net.api.ALPS.ALPSModelElements;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Threading.Tasks;


namespace XEStoPASS_alps.net.api
{
    class Program
    {
        static void Main(string[] args)
        {
            //HelloGItHUb
            var file = File.OpenText("repairExampleCleansed.xes");
            var document = XDocument.Load(file);



            var log1 = XesEventLog.Parsen(document);
            var net = AlphaMiner.MinePetriNet(log1);

            //Für Erstellung der Do-States.Gruppen
            //Erstelle Liste mit Vorgänger/nachfolger der Events
            List<string> allEvents = new List<string>();
            List<List<string>> directlyPreviousToThisEvent = new List<List<string>>();
            List<List<string>> directlyNextToThisEvent = new List<List<string>>();

            for (int i = 0; i < net.Count(); i++)
            {
                for (int j = 0; j < net.ElementAt(i).Item1.Count(); j++)
                {
                    if (!allEvents.Contains(net.ElementAt(i).Item1[j]))
                    {
                        allEvents.Add(net.ElementAt(i).Item1[j]);
                        int index = allEvents.IndexOf(net.ElementAt(i).Item1[j]);
                        directlyNextToThisEvent.Add(new List<string> { });

                        for (int l = 0; l < net.ElementAt(i).Item2.Count(); l++)
                        {
                            if (net.ElementAt(i).Item2[l] != allEvents[index])
                            {
                                if (l == 0)
                                {
                                    directlyNextToThisEvent[index].Add(net.ElementAt(i).Item2[l]);
                                }
                                else
                                {
                                    directlyNextToThisEvent[index].Add("xor+" + net.ElementAt(i).Item2[l]);
                                }
                            }
                        }
                    }
                    else if (allEvents.Contains(net.ElementAt(i).Item1[j]))
                    {
                        int index = allEvents.IndexOf(net.ElementAt(i).Item1[j]);
                        for (int l = 0; l < net.ElementAt(i).Item2.Count(); l++)
                        {
                            if (net.ElementAt(i).Item2[l] != allEvents[index])
                            {
                                if (!directlyNextToThisEvent[index].Contains(net.ElementAt(i).Item2[l]))
                                {
                                    if (l == 0)
                                    {
                                        directlyNextToThisEvent[index].Add(net.ElementAt(i).Item2[l]);
                                    }
                                    else
                                    {
                                        directlyNextToThisEvent[index].Add("xor+" + net.ElementAt(i).Item2[l]);
                                    }
                                }
                            }
                        }
                    }
                }
            }    

            for (int f = 0; f <= allEvents.Count(); f++)
            {
                directlyPreviousToThisEvent.Add(new List<string> { });
            }
            
            for (int i = 0; i < net.Count(); i++)
            {
                for (int j = 0; j < net.ElementAt(i).Item2.Count(); j++)
                {
                    if (!allEvents.Contains(net.ElementAt(i).Item2[j]))
                    {
                        allEvents.Add(net.ElementAt(i).Item2[j]);
                        int index = allEvents.IndexOf(net.ElementAt(i).Item2[j]);

                        for (int l = 0; l < net.ElementAt(i).Item1.Count(); l++)
                        {
                            if (net.ElementAt(i).Item1[l] != allEvents[index])
                            {
                                if (l == 0)
                                {
                                    directlyPreviousToThisEvent[index].Add(net.ElementAt(i).Item1[l]);
                                }
                                else
                                {
                                    directlyPreviousToThisEvent[index].Add("xor+" + net.ElementAt(i).Item1[l]);
                                }
                            }
                        }
                    }
                    else if (allEvents.Contains(net.ElementAt(i).Item2[j]))
                    {
                        int index = allEvents.IndexOf(net.ElementAt(i).Item2[j]);
                        for (int l = 0; l < net.ElementAt(i).Item1.Count(); l++)
                        {
                            if (net.ElementAt(i).Item1[l] != allEvents[index])
                            {
                                if (!directlyPreviousToThisEvent[index].Contains(net.ElementAt(i).Item1[l]))
                                {
                                    if (l == 0)
                                    {
                                        directlyPreviousToThisEvent[index].Add(net.ElementAt(i).Item1[l]);
                                    }
                                    else
                                    {
                                        directlyPreviousToThisEvent[index].Add("xor+" + net.ElementAt(i).Item1[l]);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            //Ausgabe von dem was im Dokument steht
            List<Ressource> resourceList = ResourceLog.Parse(document);

            //Ausgabe der ResourceList
            RessourcenAusgabe(resourceList);

            //Durchlaufen der ResourceList und zusammenfassen von Resourcen mit denselben EventNames 
            List<List<Ressource>> subjectList = Compare(resourceList);

            //Ausgabe der subjectList
            SubjektAusgabe(subjectList);

            //Erstellen der Subjects (Name, Eventnamensliste, Eventlisten zusammenfassen, Vorgängerlisten zusammenfassen, Nachfolgerlisten zusammenfassen, Ressourcenliste)
            List<Subject> subjects = new List<Subject>();
            for (int i = 0; i < subjectList.Count(); i++)
            {
                Subject subject = new Subject(subjectList[i]);
                subjects.Add(subject);
            }

            //Ausgabe der neu angelegten Subjects und Kontrolle der Eigenschaften
            AusgabeSubjekte(subjects);

            //SubjektNamen in Ressourcen ändern
            /*
             * Alle Subjekte durchlaufen
             *      auf die Ressourcen darin zugreifen
             *          subjektname in die Ressource übergeben
             */

            foreach (Subject s in subjects)
            {
                foreach (Ressource r in s.Ressources)
                {
                    r.SubjectName = s.Name;
                }
            }

            foreach (Subject s in subjects)
            {
                s.EventReihenfolge(document);
            }







            /* Alle Ressourcen durchlaufen
            *      vorgänger und Nachfolgerlisten bei e.ressource mit allen ressourcen abgleichen und falls Name übereinstimmt durch Subjektname ersetzen
            

            foreach (Ressource r in resourceList)
            {
                foreach (List<Event> events in r.Vorgänger)
                {
                    foreach (Event @event in events)
                    {
                        for (int i = 0; i < resourceList.Count(); i++)
                        {
                            if (@event.Resource == resourceList[i].Name)
                            {
                                @event.Resource = resourceList[i].SubjectName;
                            }
                        }
                        
                    }
                }

                foreach (List<Event> events in r.Nachfolger)
                {
                    foreach (Event @event in events)
                    {
                        for (int i = 0; i < resourceList.Count(); i++)
                        {
                            if (@event.Resource == resourceList[i].Name)
                            {
                                @event.Resource = resourceList[i].SubjectName;
                            }
                        }

                    }
                }
            }

            RessourcenAusgabe(resourceList);
            */
            //Vorgänger und Nachfolgerressourcen in Subjekte ändern
            /* Subjekte nochmal neu initialisieren
            *      vorgänger und Nachfolgerlisten durchlaufen
            *          falls kombination von name und event mehrmals vorkommt --> zusammenfassen
            * 
            * 
            */
            /*
            subjects.ForEach(delegate (Subject subject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Subjekt Name:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(subject.Name);
                Console.WriteLine();
                int i = 0;

                Console.WriteLine("Zugehörige Ressourcen: ");
                foreach (var d in subject.Ressources)
                {
                    Console.WriteLine(d.Name);
                }

                foreach (var e in subject.Events)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Event Name:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(e.Name);
                    Console.WriteLine();
                    if (subject.Vorgänger.Count() > 0)
                    {
                        Console.Write("         Vorgänger:");

                        foreach (var f in subject.Vorgänger[i])
                        {

                            Console.Write(f.Resource + " mit " + f.Name + "; ");
                        }
                        Console.WriteLine();
                    }
                    
                    if (subject.Nachfolger.Count > 0)
                    {
                        Console.Write("         Nachfolger:");
                        foreach (var g in subject.Nachfolger[i])
                        {
                            Console.Write(g.Resource + " mit " + g.Name + "; ");
                        }
                        Console.WriteLine();
                    }
                    

                }
                i++;
                Console.WriteLine();
            });

            

            subjects.ForEach(delegate (Subject subject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Subjekt Name:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(subject.Name);
                Console.WriteLine();
                int i = 0;

                Console.WriteLine("Zugehörige Ressourcen: ");
                foreach (var d in subject.Ressources)
                {
                    Console.WriteLine(d.Name);
                }

                foreach (var e in subject.Events)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Event Name:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(e.Name);
                    Console.WriteLine();
                    if (subject.Vorgänger.Count() > 0)
                    {
                        Console.Write("         Vorgänger:");

                        foreach (var f in subject.Vorgänger[i])
                        {

                            Console.Write(f.Resource + " mit " + f.Name + "; ");
                        }
                        Console.WriteLine();
                    }

                    if (subject.Nachfolger.Count > 0)
                    {
                        Console.Write("         Nachfolger:");
                        foreach (var g in subject.Nachfolger[i])
                        {
                            Console.Write(g.Resource + " mit " + g.Name + "; ");
                        }
                        Console.WriteLine();
                    }


                }
                i++;
                Console.WriteLine();
            });
            */
            foreach (Subject s in subjects)
            {
                s.MergeSameEvents();
            }
            /*geg: grobe Reihenfolge innerhalb der Subjekte --> erstellen der SBDs + Do-States mit OrderList in Subject (Immer Kontrolle ob schon erstellt)
            *                                                                       Abgleich der directlyPrevious/Next Listen und Erstellung der Send / ReceiveStates
            *                                                                       Verbinden der ReceiveDoSendStates                           
            */

            PASSProcessModel model = new PASSProcessModel("http://www.exampleTestUri.com");

            //Subjects + SBD's erstellen
            foreach (Subject s in subjects)
            {
                s.fullySpecified = AddSubject.NewSubject(s.Name, model);
                s.subjectBehavior = AddSBD.NewSBD(s.fullySpecified);
            }

            //Nachfolger/Vorgänger aller Events der Subjekte
            foreach (Subject s in subjects)
            {
                s.PreviousNext(allEvents, directlyPreviousToThisEvent, directlyNextToThisEvent);
            }

            //CreateStates
            foreach (Subject s in subjects)
            {
                s.CreateStates();
            }

            //Wie kann ich kontrollieren ob erzeugte States die gewünschten sind?
            foreach (Subject s in subjects)
            {
                int i = 0;
                foreach (DoState doState in s.DoStates)
                {
                    Console.WriteLine("Hier steht der State");
                    Console.WriteLine(doState.getModelComponentLabelsAsStrings().First());

                    if (s.SendStates[i].Count() > 0)
                    {
                        Console.WriteLine("SendStates");
                        for (int j = 0; j < s.SendStates[i].Count(); j++)
                        {
                            Console.WriteLine(s.SendStates[i][j].getModelComponentLabelsAsStrings().First());
                        }
                    }
                    

                    if (s.ReceiveStates[i].Count() > 0)
                    {
                        Console.WriteLine("ReceiveStates");
                        for (int j = 0; j < s.ReceiveStates[i].Count(); j++)
                        {
                            Console.WriteLine(s.ReceiveStates[i][j].getModelComponentLabelsAsStrings().First());
                        }
                    }
                    
                    i++;
                }
            }

            //richtige Reihenfolge der Do-State-Pakete --> OrderList
            foreach (Subject s in subjects)
            {
                s.DoStateGruppenVerbinden();
            }


            //verbinden von 
            foreach (Subject s in subjects)
            {
                List<IDoState> doStates = new List<IDoState>();
                List<List<IReceiveState>> receiveStates = new List<List<IReceiveState>>();
                List<List<ISendState>> sendStates = new List<List<ISendState>>();
                int i = 0;
                foreach (Event e in s.Events)
                {
                    IDoState doState = SupportFunctionsToAddStates.AddDoState(e.Name, s.subjectBehavior);
                    doStates.Add(doState);

                    if (s.Nachfolger[i].Count() > 0)
                    {
                        List<ISendState> sends = new List<ISendState>();
                        sendStates.Add(sends);
                        foreach (Event @event in s.Nachfolger[i])
                        {
                            ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, s.subjectBehavior);
                            AddTransition.AddDoTransitionTo(doState, sendState);

                            sends.Add(sendState);
                            Console.WriteLine(sendState.getModelComponentLabelsAsStrings().First());
                        }
                    }
                    i++;
                }

                s.SendStates = sendStates;
                s.ReceiveStates = receiveStates;
                s.DoStates = doStates;

            }

            

            //States erstellen
            /*
             *  DoState --> gleiches Subjekt (Do zu Do)
             *  Do state --> anderes Subjekt (do zu send zu receive zu do)
             *  anderes SUbjekt --> dostate (receive zu do)
             *  gleichen SUbjekt --> dostate (do zu do) und somit wie 1.
             * 
             * 
             * durchlaufe alle subjekte
             *      subjekt[i] 
             *      erstelle DoState-Liste --> Eigenschaft???
             *              rufe addstate.dostate mit(eventName,behavior[i]) auf
             *              durchlaufe die zum Event zugehörige Vorgängerliste
             *                  falls vorgänger vorhanden
             *                      rufe addstate.receivestate(eventName, eventname vom vorgänger,behavior[i])
             *                      rufe addtransition.Addreceivetransitionto(receivestate, dostate)
             *              durchlaufe die zum Event zugehörige Nachfolgerliste
             *                  falls nachfolger vorhanden
             *                      rufe addstate.sendstate(eventName,behavior[i])
             *                      rufe addtransitoin.addDotransitionto(dostate, sendstate)
            */




            /* Nur bei anderen Subjekten Send und ReceiveStates
            foreach (Subject s in subjects)
            {
                List<IDoState> doStates = new List<IDoState>();
                List<List<IReceiveState>> receiveStates = new List<List<IReceiveState>>();
                List<List<ISendState>> sendStates = new List<List<ISendState>>();
                int i = 0;
                foreach (Event e in s.Events)
                {
                    IDoState doState = SupportFunctionsToAddStates.AddDoState(e.Name, s.subjectBehavior);
                    doStates.Add(doState);

                    /*vorgänger nur bei anderem Subjekt
                    if (s.Vorgänger[i].Count() > 0)
                    {
                        foreach (Event @event in s.Vorgänger[i])
                        {
                            if (!(@event.Resource == All ressourcennames durchlaufen))
                            {

                            }
                            IReceiveState receiveState = SupportFunctionsToAddStates.AddReceiveState(e.Name, @event.Name, s.subjectBehavior);
                            AddTransition.AddReceiveTransitionTo(receiveState, doState);

                            receiveStates[i].Add(receiveState);
                        }

                    }

                    
                    if (s.Nachfolger[i].Count() > 0)
                    {
                        List<ISendState> sends = new List<ISendState>();
                        sendStates.Add(sends);
                        foreach (Event @event in s.Nachfolger[i])
                        {
                            ISendState sendState = SupportFunctionsToAddStates.SendState(e.Name, s.subjectBehavior);
                            AddTransition.AddDoTransitionTo(doState, sendState);

                            sends.Add(sendState);
                            Console.WriteLine(sendState.getModelComponentLabelsAsStrings().First());
                        }
                    }
                    i++;
                }

                s.SendStates = sendStates;
                s.ReceiveStates = receiveStates;
                s.DoStates = doStates;

            }*/

            //Wie kann ich kontrollieren ob erzeugte States die gewünschten sind?

            foreach (Subject s in subjects)
            {
                foreach (DoState doState in s.DoStates)
                {
                    Console.WriteLine("Hier steht der State");
                    Console.WriteLine(doState.getModelComponentLabelsAsStrings().First());

                }
                
            }


            

            //Transitions erstellen / die reihenfolge der dostates hier beachten!
            /*durchlaufe alle dostates
             *      falls nachfolger = gleiches Subjekt 
             *          addtransitoin.addDotransitionto(dieserdostate, nächsterdostate);
             *          
             *      falls nachfolger = anderes Subjekt
             *          addtransition.addSendtransitionto(SendstateDiesesSoState, ReceiveStateNächsterDoStateDiesesSubjekts)
             */

            foreach (Subject s in subjects)
            {
                foreach (IDoState doState in s.DoStates)
                {

                }
            }

            //Messages erstellen







            /*subjelist durchlaufen und für jede subjectList[i] Liste ein Subjekt erstelle | Done
             * Subjekt(List<Resource>) | Done
             * 
             * Das Zusammenfassen von Ressourcen zu Subjekten muss in allen Vorgänger/Nachfolgerlisten aktualisiert werden von Ressourcen zu Subjekten
             * 
             * Ressource bekommt neue Subjektname
             * 
             * welche ressource ist mein nachfolger und welches subjekt ist diese resourdce?
             * 
             * 
             * Subjekte:
             * Eventnames bleiben gleich
             * aber Vorgänger name und eventname von vorgänger
             * und Nachfolger name und eventname von vorgänger
             * zusammenfassen
             * 
             * 
             * 
             * Ressource:
             * Ressource mit neuer Liste über Events, die gestartet aber noch nicht completed sind. und erst wenn completed in die richtige eventliste übertragen.
             * 
             * 
             * 
             *
             */

            /*
            subjectList.ForEach(delegate (Subject subject)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Resource Name:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(subject.Name);
                Console.WriteLine();
                int i = 0;
                /*Console.WriteLine("Event Names:");
                foreach (var d in ressource.EventNameList)
                {
                    
                    Console.WriteLine(d);
                }
                Console.WriteLine("Event Name:");
                foreach (var e in subject.Events)
                {

                    Console.Write(e.Name);
                    Console.Write("         Vorgänger:");

                    foreach (var f in subject.Vorgänger[i])
                    {

                        Console.Write(f.Resource + " mit " + f.Name);
                    }
                    Console.Write("         Nachfolger:");
                    foreach (var g in subject.Nachfolger[i])
                    {
                        Console.WriteLine(g.Resource + " mit " + g.Name);
                    }
                    Console.WriteLine();

                }
            });*/




        }

        public static void SubjektAusgabe(List<List<Ressource>> subjectList)
        {
            subjectList.ForEach(delegate (List<Ressource> resourceList)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Resource Nummer:" + subjectList.IndexOf(resourceList));
                Console.ForegroundColor = ConsoleColor.White;
                resourceList.ForEach(delegate (Ressource r)
                {
                    Console.WriteLine(r.Name);
                });
            });
            Console.WriteLine();
        }

        public static void RessourcenAusgabe(List<Ressource> resourceList)
        {
            resourceList.ForEach(delegate (Ressource ressource)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Resource Name:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(ressource.Name);
                Console.WriteLine();
                Console.WriteLine("Subjektname: " + ressource.SubjectName);
                Console.WriteLine();
                int i = 0;
                /*Console.WriteLine("Event Names:");
                foreach (var d in ressource.EventNameList)
                {
                    
                    Console.WriteLine(d);
                }*/
                
                foreach (var e in ressource.Events)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Event Name:");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write(e.Name);
                    Console.WriteLine();
                    Console.Write("         Vorgänger:");

                    foreach (var f in ressource.Vorgänger[i])
                    {

                        Console.Write(f.Resource + " mit " + f.Name + "; ");
                    }
                    Console.WriteLine();
                    Console.Write("         Nachfolger:");
                    foreach (var g in ressource.Nachfolger[i])
                    {
                        Console.Write(g.Resource + " mit " + g.Name + "; ");
                    }
                    Console.WriteLine();

                }
                Console.WriteLine();
            });
        }

        //previous fehler
        //nur eines der beiden start/completed einlesen (Achtung bei Events, bei denen zwischen start und completed andere Events starten) previous und nachfolger (das vor started completed wurde) (das nach completed gestarted wird)
        //Sind bei Subjects, dann listlistlist vorgänger?

        /// <summary>
        /// Laufe die Resourcenlist durch und vergleiche alle Einträge miteinander. Erstelle eine List von Resspurcenlisten, in denen Ressourcen mit gleichen Eventnamen zusammengefasst werden.
        /// </summary>
        /// <param name="resourceList"></param>
        /// <returns></returns>
        public static List<List<Ressource>> Compare(List<Ressource> resourceList)
        {
            List<List<Ressource>> ressourceListSorted = new List<List<Ressource>>();
            //erstellt, damit an [0][0] erste Ressource und bei         [0][1] Ressource, die gleiche Events wie erste Ressource hat
            //                   [1][0] Ressource != erste Ressource    [1][1] Ressource, die gleiche Events wie [1][0] hat  

            //laufe alle Ressource, der übergebenen Liste ab
            for (int i = 0; i < resourceList.Count(); i++)
            {
                Ressource ressourceI = resourceList[i];

                //erste Ressource wir immer [0][0] zugeordnet
                if (i == 0)
                {
                    ressourceListSorted.Add(new List<Ressource> { ressourceI });
                }
                else
                {
                    int countPrevious = ressourceListSorted.Count();
                    bool addNew = false;

                    for (int m = 0; m < countPrevious; m++)
                    {
                        bool issimilar = true;
                        foreach (Ressource r in ressourceListSorted[m])
                        {
                            if (ressourceI.compareEventNameList(r, true) == 1)
                            {
                                //Console.WriteLine(ressourceI.Name + " und " + r.Name + " sind gleich");

                                //ordne der Liste[0] von Ressourcenlisten ressource j zu
                                
                            }
                            //falls ressourcen unterschiedliche eventnamen haben
                            else
                            {
                                //Console.WriteLine(ressourceI.Name + " und " + r.Name + " sind nicht gleich");
                                issimilar = false;
                            }
                        }

                        if (issimilar)
                        {
                            ressourceListSorted[m].Add(ressourceI);
                            addNew = false;
                            break;
                        }
                        else
                        {
                            
                            addNew = true;
                        }
                        
                    }

                    if (addNew)
                    {
                        ressourceListSorted.Add(new List<Ressource> { ressourceI });
                    }
                }

                /*
                    //laufe alle anderen Ressourcen ab
                    for (int j = i + 1; j < resourceList.Count(); j++)
                    {
                        Ressource ressourceI = resourceList[i];
                        Ressource ressourceJ = resourceList[j];

                        //falls ressourcen gleiche eventnamen haben
                        if (ressourceI.compareEventNameList(ressourceJ, true) == 1)
                        {
                            Console.WriteLine(resourceList[i].Name + " und " + resourceList[j].Name + " sind gleich");

                            //ordne der Liste[0] von Ressourcenlisten ressource j zu
                            ressourceListSorted[0].Add(resourceList[j]);
                        }
                        //falls ressourcen unterschiedliche eventnamen haben
                        else
                        {
                            Console.WriteLine(resourceList[i].Name + " und " + resourceList[j].Name + " sind nicht gleich");
                        }
                    }
                }
                //alle anderen Resourcen neben der ersten
                else 
                {
                    //laufe alle Resourcen, die danach kommen, ab
                    for (int j = i + 1; j < resourceList.Count(); j++)
                    {
                        Ressource ressourceI = resourceList[i];

                        //laufe durch die Anzahl an Listen in ressourcenlistsorted durch (min. 1)
                        for (int m = 0; 0 < ressourceListSorted.Count(); m++)
                        {
                            //falls die liste[m] die ressource noch nicht enthält
                            if (!ressourceListSorted[m].Contains(ressourceI))
                            {
                                //vergleiche mit den ressourcen dort, ob eine gleich ist
                                foreach (Ressource r in ressourceListSorted[m])
                                {
                                    if (ressourceI.compareEventNameList(r, true) == 1)
                                    {
                                        Console.WriteLine(resourceList[i].Name + " und " + r.Name + " sind gleich");

                                                ressourceListSorted[m].Add(resourceList[i]);
                                    }
                                }
                            }
                            //eine Liste enthält die ressource schon
                            else if (ressourceListSorted[m].Contains(ressourceI))
                            {
                                break;
                            }

                            //keine der listen in resourcelistsorted enthält die neue resource oder eine resource mit gleichen eventnames
                            if (false)
                            {
                                ressourceListSorted[ressourceListSorted.Count() + 1].Add(resourceList[i]);
                            }
                        }



                    }
                }
                
            }


            /*
            List<Subject> subjects = new List<Subject>();

            for (int i = 0; i < resourceList.Count(); i++)
            {
                for (int j = i+1; j < resourceList.Count(); j++)
                {
                    Ressource ressourceI = resourceList[i];
                    Ressource ressourceJ = resourceList[j];

                    if (ressourceI.compareEventNameList(ressourceJ, true) == 1)
                    {
                        Console.WriteLine(resourceList[i].Name + " und " + resourceList[j].Name + " sind gleich");
                        //Ziel: Erstelle ein Subjekt mit den beiden Resourcen
                        //geht nie in die foreach schleife, da subjects zu Beginn leer
                        if (subjects.Count == 0)
                        {
                            Subject subject = new Subject(ressourceI, ressourceJ);
                            subjects.Add(subject);
                        }
                        else {
                            foreach (Subject s in subjects)
                            {
                                foreach (Ressource r in s.Ressources)
                                {
                                    if (!s.Ressources.Contains(ressourceI) && !s.Ressources.Contains(ressourceJ))
                                    {
                                        Subject subject = new Subject(ressourceI, ressourceJ);
                                        subjects.Add(subject);
                                    }
                                    else if (!s.Ressources.Contains(ressourceI))
                                    {
                                        s.AddResource(ressourceI);
                                    }
                                    else if (!s.Ressources.Contains(ressourceJ))
                                    {
                                        s.AddResource(ressourceJ);
                                    }
                                }

                            } }
                    }
                    else
                    {
                        Console.WriteLine(resourceList[i].Name + " und " + resourceList[j].Name + " sind nicht gleich");
                    }
                }
            }
            return subjects;*/

            }
            return ressourceListSorted;
        }

        public static void AusgabeSubjekte(List<Subject> subjects)
        {
            foreach (Subject s in subjects)
            {
                Console.WriteLine(s.Name);
                foreach (string eventName in s.EventNames)
                {
                    Console.WriteLine(eventName);
                }
                foreach (Ressource ressource in s.Ressources)
                {
                    Console.WriteLine(ressource.Name);
                }
            }
        }
    }
}


//Anhang

//Falls der EventName mit einer der EventnameListeinträge von Subject zusammenpasst, aktualisiere die Vorgänger/Nachfolger davon
/*Aufteilung in essentielle/optionale Events
List<string> essentielleEvents = XesEventLog.essEvents(document1, allEvents);
List<List<string>> orderList = XesEventLog.ConfirmOrder(document1, essentielleEvents);
List<string> optionaleEvents = allEvents.Except(essentielleEvents).ToList();
List<List<string>> previousToThisEvent = XesEventLog.Previous(essentielleEvents, orderList);
List<List<string>> nextToThisEvent = XesEventLog.Next(essentielleEvents, orderList);
*/
//Reihenfolge
/*Überprüfe Reihenfolge der essEvents: gibt es Reihenfolgen, die immer so bleiben oder sogar feste Plätze?
 *      falls ja: Event ist immer vor denen die nach ihm kommen und immer nach denen, die vor ihm kommen
 *      
 *  Die Events, die keine festen Plätze/reihenfolge haben und zwischen blöcken von festzugeordenten sind, in welchem Bezug stehen diese zueinander?
 *          
 *          alle festgeordneten vor ihnen sind immer vor ihnen, alle nach ihnen immer nach ihnen
 *  
 *          die events selber können vertauscht werden, da sie essentiell sind, aber nicht im gleichen ast verlaufen 
 *          
 *          
 *Überprüfe optionale Events: 
 *  kommen diese, falls sie auftreten immer direkt nach und vor denselben Events? (Im gleichen Subjekt)
 *      falls ja
 *          von erstem ess zu diesem opt und von diesem opt zu nächstem ess eine verbindung einbauen
 *          
 *      falls nein
 *          gibt es events, die immer vor / nach diesem Event kommen
 *              falls ja 
 *                  alle anderen ess events dieses Subjekts, die auch von diesen vor und nach events eingeschlossen werde, sind parallel zu diesem optEvent
 *                  --> von ersten ess zu diesem opt zu dem parallelen und von dem parallelen zu diesem opt zu dem nächsten ess
*/