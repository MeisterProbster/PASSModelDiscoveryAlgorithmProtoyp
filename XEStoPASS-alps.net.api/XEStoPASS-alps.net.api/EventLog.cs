using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace XEStoPASS_alps.net.api
{
    public class EventLog
    {
        public IReadOnlyList<Trace> Traces { get; }

        public EventLog(IReadOnlyList<Trace> traces)
        {
            Traces = traces;
        }
    }
}