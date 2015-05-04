using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace crmc.wotdisplay.helpers
{
    public static class Extension
    {
        public static T Next<T>(this IEnumerable<T> list, T current, int? fromStartingPosition = 0)
        {
            try
            {
                if (current == null) return list.ElementAtOrDefault(fromStartingPosition ?? 0);

                return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
            }
            catch
            {
                return list.FirstOrDefault();
            }
        }

        public static T Previous<T>(this IEnumerable<T> list, T current, int? fromStartingPosition = 0)
        {
            try
            {
                if (current == null) return list.ElementAtOrDefault(fromStartingPosition ?? 0);
                return list.TakeWhile(x => !x.Equals(current)).Last();
            }
            catch
            {
                return list.LastOrDefault();
            }
        }

        public static int ToInt(this double x)
        {
            return Convert.ToInt32(x);
        }

        public static int RandomInt(this int x, int min, int max)
        {
            var random = new Random();
            if (max <= min) min = max - 1;
            return random.Next(min, max);
        }

     
    }
}