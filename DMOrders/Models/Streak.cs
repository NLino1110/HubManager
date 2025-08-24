using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMOrders.Models
{
    public class Streak : IComparable
    {
        public Result Result { get; set; }
        public int NumStreak { get; set; }

        public int CompareTo(object obj)
        {
            var score = Result == Result.Won ? NumStreak : -NumStreak;
            if (obj is Streak s)
            {
                var otherScore = s.Result == Result.Won ? s.NumStreak : -s.NumStreak;
                return score - otherScore;
            }

            return score;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{Enum.GetName(typeof(Result), Result)} {NumStreak}";
        }
    }

    public enum Result
    {
        Lost = 0,
        Won = 1
    }
}
