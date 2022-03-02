using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace XEStoPASS_alps.net.api
{
    public static class XesEventLog
    {
        public static EventLog Parsen(XDocument document)
        {
            var traces = document.Root
                .Elements("trace")
                .Select(traceElement => traceElement
                    .Elements("event")
                    .Select(eventElement => new Event(
                        name: eventElement.GetXesAttribute("concept:name"),
                        transition: eventElement.GetXesAttribute("lifecycle:transition"),
                        timestamp: Convert.ToDateTime(eventElement.GetXesAttribute("time:timestamp")),
                        resource: eventElement.GetXesAttribute("org:resource"))));

            return new EventLog(traces.Select(trace => new Trace(trace.ToArray())).ToArray());
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

            foreach (Trace trace in traces2)
            {
                IReadOnlyList<Event> ev = trace.Events;
                List<string> order = new List<string>();
                foreach (Event e in ev)
                {
                    if (essentielleEvents.Contains(e.Activity))
                    {
                        if (!order.Contains(e.Activity))
                        {
                            order.Add(e.Activity);
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
            }
            return orderList;
        }

        public static List<List<string>> Previous(List<string> essentielleEvents, List<List<string>> orderList)
        {
            List<List<string>> previousToThisEvent = new List<List<string>>();

            for (int f = 0; f <= essentielleEvents.Count(); f++)
            {
                previousToThisEvent.Add(new List<string> { });
            }
            int k = 0;
            foreach (string s in essentielleEvents)
            {
                
                bool test = true;

                for (int j = 0; j < orderList.Count(); j++)
                {
                    for (int f = 0; f < orderList.Count(); f++)
                    {
                        if (!(essentielleEvents[k] == orderList[j][k]))
                        {
                            test = false;
                            break;
                        }
                    }
                        
                }

                if (test)
                {
                    for (int m = 0; m < k; m++)
                    {
                        previousToThisEvent[k].Add(essentielleEvents[m]);
                    }
                }
                k++;
            }

            return previousToThisEvent;
        }
        public static List<List<string>> Next(List<string> essentielleEvents, List<List<string>> orderList)
        {
            List<List<string>> nextToThisEvent = new List<List<string>>();
            for (int f = 0; f <= essentielleEvents.Count(); f++)
            {
                nextToThisEvent.Add(new List<string> { });
            }
            int k = 0;
            foreach (string s in essentielleEvents)
            {
                
                bool test = true;

                for (int j = 0; j < orderList.Count(); j++)
                {
                    if (!(essentielleEvents[k] == orderList[j][k]))
                    {
                        test = false;
                        break;
                    }
                }

                if (test)
                {
                    
                    nextToThisEvent.Add(new List<string> { });
                    
                    for (int m = k + 1; m < essentielleEvents.Count(); m++)
                    {
                        nextToThisEvent[k].Add(essentielleEvents[m]);
                    }
                }
                k++;
            }

            return nextToThisEvent;
        }

        public static List<string> essEvents(XDocument document, List<string> eventNames)
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

            List<string> events = new List<string>();

            
            foreach(string s in eventNames)
            {
                bool ess = true;
                foreach (Trace trace in traces2)
                {
                    IReadOnlyList<Event> ev = trace.Events;
                    foreach (Event e in ev)
                    {
                        if (!ev.Any(p => p.Activity == s))
                        {
                            ess = false;
                        }
                    }
                    if (!ess)
                    {
                        break;
                    }
                }
                
                if (ess)
                {
                    events.Add(s);
                }
                
            }


            return events;
        }

        

        public static string GetXesAttribute(this XElement element, string key)
        {
            return element.Elements().First(e => e.Attribute("key").Value == key).Attribute("value").Value;
        }

        /*public static EventLog ParseTrace(XDocument document)
        {
            var traces = document.Root
                .Elements("trace")
                .Select(traceElement => new Trace(
                        name: traceElement.GetXesTracename("concept:name")));

            return new EventLog(traces.Select(trace => new Trace(trace.ToArray())).ToArray());
        }*/

        public static string GetXesTracename(this XElement element, string key)
        {
            return element.Elements().First(e => e.Attribute("key").Value == key).Attribute("value").Value;
        }
    }
}
