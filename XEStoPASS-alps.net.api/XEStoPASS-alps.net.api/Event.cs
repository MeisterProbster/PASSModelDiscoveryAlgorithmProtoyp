using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Xml.Linq;

namespace XEStoPASS_alps.net.api
{
    public class Event
    {
        public string Activity { get; }

        public string Name { get; }

        public string Transition { get; }

        public DateTime Timestamp { get; }

        public string Resource { get; set; }

        public string Tracename { get; }

        public Event(string name)
        {
            Name = name;
        }

        
        public Event(string tracename, string name, string transition, DateTime timestamp, string resource)
        {
            Name = name;
            Transition = transition;
            Timestamp = timestamp;
            Resource = resource;
            Tracename = tracename;

            Activity = Name + '+' + Transition;


        }

        public Event(string name, string transition, DateTime timestamp, string resource)
        {
            Name = name;
            Transition = transition;
            Timestamp = timestamp;
            Resource = resource;

            Activity = Name + '+' + Transition;


        }

        public Event(string name, DateTime timestamp)
        {
            Name = name;
           
            Timestamp = timestamp;
            
        }

        /*public bool Equals(Event other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;
            return string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;

            return Equals(obj as Event);
        }

        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }

        public static bool operator ==(Event left, Event right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Event left, Event right)
        {
            return !Equals(left, right);
        }*/
    }
}