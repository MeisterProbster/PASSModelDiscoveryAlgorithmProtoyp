using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace XEStoPASS_alps.net.api
{
    public class Trace
    {
        

        public IReadOnlyList<Event> Events { get; }

        public Trace(IReadOnlyList<Event> events)
        {
            Events = events;
        }

       
    }
}