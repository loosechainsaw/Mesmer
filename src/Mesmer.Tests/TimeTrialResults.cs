using System;
using System.Collections.Generic;

namespace Mesmer.Tests
{
    public class TimeTrialResults
    {
        public TimeSpan Duration { get; set; }
        public List<CalcResult> Results { get; set; }
    }
    public class CalcResult
    {
        public int Input { get; set; }
        public long Result { get; set; }

        protected bool Equals(CalcResult other)
        {
            return Input == other.Input && Result == other.Result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CalcResult)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Input * 397) ^ Result.GetHashCode();
            }
        }
    }
}