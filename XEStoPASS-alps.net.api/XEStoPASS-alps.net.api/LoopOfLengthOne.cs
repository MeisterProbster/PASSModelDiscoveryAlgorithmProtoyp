using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace XEStoPASS_alps.net.api

{
    [DebuggerDisplay("{PrecedingActivity} -> {Activity} -> {FollowingActivity}")]
    public class LoopOfLengthOne : IEquatable<LoopOfLengthOne>
    {
        public string Activity { get; }

        public string PrecedingActivity { get; }

        public string FollowingActivity { get; }

        public LoopOfLengthOne(string activity, string precedingActivity, string followingActivity)
        {
            Activity = activity;
            PrecedingActivity = precedingActivity;
            FollowingActivity = followingActivity;
        }

        public bool Equals(LoopOfLengthOne other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Activity, other.Activity) && string.Equals(PrecedingActivity, other.PrecedingActivity) && string.Equals(FollowingActivity, other.FollowingActivity);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LoopOfLengthOne)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Activity != null ? Activity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (PrecedingActivity != null ? PrecedingActivity.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FollowingActivity != null ? FollowingActivity.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
