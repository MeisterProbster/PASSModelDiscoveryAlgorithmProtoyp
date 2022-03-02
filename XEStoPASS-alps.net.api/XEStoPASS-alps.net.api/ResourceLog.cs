using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace XEStoPASS_alps.net.api
{
    class ResourceLog
    {
        public static List<Ressource> Parse(XDocument document)
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

            List<Ressource> ressources = new List<Ressource>();

            foreach (Trace trace in traces2)
            {
                IReadOnlyList<Event> events = trace.Events;
                int i = 0;
                foreach (Event e in events)
                {
                    
                    if (!ressources.Any(p => p.Name == e.Resource))
                    {
                        Ressource ressource = new Ressource(e.Resource);

                        //erstes Event
                        if ((i == 0) && (events.Count() > 1))
                        {
                            Event next = events[i + 1];
                            ressource.AddEvent(e, null, next);
                        }


                        //mittleres Event
                        else if ((i != 0) && (i < events.Count() - 1))
                        {
                            Event previous;
                            bool isPreviousEvent = false;
                            int k = 1;
                            while (!isPreviousEvent)
                            {
                                if (events[i - k].Transition == "complete")
                                {
                                    isPreviousEvent = true;
                                }
                                else
                                {
                                    k++;
                                }
                            }
                            previous = events[i - k];

                            //drurchlaufe die nächsten Events, bis lifecycle:transition = "start" / event.Transition = "start"
                            Event next;
                            bool isNextEvent = false;
                            int j = 1;
                            while (!isNextEvent)
                            {
                                //erstes Event nach Current mit complete und anderem Namen
                                if (j + i == events.Count())
                                {
                                    j = 1;
                                    while (!isNextEvent)
                                    {
                                        if (events[i + j].Name != events[i].Name && events[i + j].Transition == "complete")
                                        {
                                            isNextEvent = true;
                                        }
                                        else
                                        {
                                            j++;
                                        }
                                    }
                                }
                                else if (events[i + j].Transition == "start")
                                {
                                    isNextEvent = true;
                                }
                                else
                                {
                                    j++;
                                }
                            }
                            next = events[i + j];

                            ressource.AddEvent(e, previous, next);
                        }

                        //letztes Event
                        else if (i == events.Count())
                        {
                            Event previous = events[i - 1];
                            ressource.AddEvent(e, previous, null);
                        }
                        ressources.Add(ressource);
                    }
                    else
                    {
                        int index = ressources.FindIndex(a => a.Name == e.Resource);

                        //erstes Event
                        if ((i == 0) && (events.Count() > 1))
                        {
                            Event next = events[i + 1];
                            ressources[index].AddEvent(e, null, next);
                        }


                        //mittleres Event
                        if ((i != 0) && (i < events.Count() - 1))
                        {
                            //drurchlaufe die vorherigen Events, bis lifecycle:transition = "complete" / event.Transition = "complete"
                            Event previous;
                            bool isPreviousEvent = false;
                            int k = 1;
                            while (!isPreviousEvent)
                            {
                                if (events[i - k].Transition == "complete")
                                {
                                    isPreviousEvent = true;
                                }
                                else
                                {
                                    k++;
                                }
                            }
                            previous = events[i - k];

                            //drurchlaufe die nächsten Events, bis lifecycle:transition = "start" / event.Transition = "start"
                            Event next;
                            bool isNextEvent = false;
                            int j = 1;
                            while (!isNextEvent)
                            {
                                //erstes Event nach Current mit complete und anderem Namen
                                if (j + i == events.Count())
                                {
                                    j = 1;
                                    while (!isNextEvent)
                                    {
                                        if (events[i + j].Name != events[i].Name && events[i + j].Transition == "complete")
                                        {
                                            isNextEvent = true;
                                        }
                                        else
                                        {
                                            j++;
                                        }
                                    }
                                }
                                else if (events[i + j].Transition == "start")
                                {
                                    isNextEvent = true;
                                }
                                else
                                {
                                    j++;
                                }
                            }
                            next = events[i + j];

                            ressources[index].AddEvent(e, previous, next);
                        }

                        //letztes Event
                        if (i == events.Count() - 1)
                        {
                            Event previous = events[i - 1];
                            ressources[index].AddEvent(e, previous, null);
                        }
                    }
                    i++;
                }
            }
            return ressources;
        }
    }
}
