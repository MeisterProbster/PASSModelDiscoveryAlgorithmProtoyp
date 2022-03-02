using System;
using System.Collections.Generic;
using System.Linq;

namespace XEStoPASS_alps.net.api
{
    public enum RelationType
    {
        NotConnected = 0,
        Precedes,
        Follows,
        Parallel
    }

    public class FootprintTable
    {
        private readonly RelationType[,] _table;
        private readonly string[] _activities;

        public FootprintTable(EventLog log)
        {
            _activities = log.Traces.SelectMany(trace => trace.Events.Select(@event => @event.Activity)).Distinct().ToArray();
            _table = new RelationType[_activities.Length, _activities.Length];

            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 1; i++)
                {
                    var currentActivity = events[i].Activity;
                    var nextActivity = events[i + 1].Activity;

                    if (this[currentActivity, nextActivity] == RelationType.NotConnected)
                    {
                        this[currentActivity, nextActivity] = RelationType.Precedes;
                        this[nextActivity, currentActivity] = RelationType.Follows;
                    }
                    else if (this[currentActivity, nextActivity] == RelationType.Follows)
                    {
                        this[currentActivity, nextActivity] = this[nextActivity, currentActivity] = RelationType.Parallel;
                    }
                }
            }

            // Fix ordering relations for loops of length two
            foreach (var trace in log.Traces)
            {
                var events = trace.Events;
                for (int i = 0; i < events.Count - 2; i++)
                {
                    var currentActivity = events[i].Activity;
                    var nextActivity = events[i + 1].Activity;
                    var nextNextActivity = events[i + 2].Activity;

                    if (currentActivity == nextNextActivity)
                    {
                        this[currentActivity, nextActivity] = RelationType.Precedes;
                        this[nextActivity, currentActivity] = RelationType.Precedes;
                    }
                }
            }
        }

        public RelationType this[string activity1, string activity2]
        {
            get
            {
                var firstEventIndex = ActivityToIndex(activity1);
                var secondEventIndex = ActivityToIndex(activity2);

                return _table[firstEventIndex, secondEventIndex];
            }
            private set
            {
                var firstEventIndex = ActivityToIndex(activity1);
                var secondEventIndex = ActivityToIndex(activity2);

                _table[firstEventIndex, secondEventIndex] = value;
            }
        }

        public RelationType this[int activity1, int activity2] => _table[activity1, activity2];

        public int ActivityToIndex(string activity)
        {
            return Array.IndexOf(_activities, activity);
        }

        public string IndexToActivity(int index)
        {
            return _activities[index];
        }
    }
}
