using alps.net.api.StandardPASS;
using System;
using alps.net.api.StandardPASS.BehaviorDescribingComponents;
using alps.net.api.StandardPASS.InteractionDescribingComponents;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;


namespace XEStoPASS_alps.net.api
{
    class Program
    {
        static void Main(string[] args)
        {
            var file = File.OpenText("repairExampleCleansed.xes");
            var document = XDocument.Load(file);
            var log = XesEventLog.Parse(document);
            var net = AlphaMiner.MinePetriNet(log);

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

            //Durchlaufen der ResourceList und zusammenfassen von Resourcen mit denselben EventNames 
            List<List<Ressource>> subjectList = Compare(resourceList);

            //Erstellen der Subjects (Name, Eventnamensliste, Eventlisten zusammenfassen, Vorgängerlisten zusammenfassen, Nachfolgerlisten zusammenfassen, Ressourcenliste)
            List<Subject> subjects = new List<Subject>();
            for (int i = 0; i < subjectList.Count(); i++)
            {
                Subject subject = new Subject(subjectList[i]);
                subjects.Add(subject);
            }

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

            foreach (Subject s in subjects)
            {
                s.MergeSameEvents();
            }

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

            //richtige Reihenfolge der Do-State-Pakete --> OrderList
            foreach (Subject s in subjects)
            {
                s.DoStateGruppenVerbinden();
            }

            foreach(Subject s in subjects)
            {
                s.CreateMessages(model, subjects, directlyNextToThisEvent, allEvents);
            }

            foreach (Subject s in subjects)
            {
                if (s.StartSubject)
                {
                    s.fullySpecified.assignRole(ISubject.Role.StartSubject);
                }

                s.InitializeEndStates();
               
            }


            string exportedTo = model.export("C:/Users/Flo/Documents/KIT/Bachelorarbeit/Elstermann/12_Algorithmus/OWL-Modell");


            Console.ReadKey();
        }

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