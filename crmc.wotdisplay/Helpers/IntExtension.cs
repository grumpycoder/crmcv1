using System;

namespace crmc.wotdisplay.helpers
{
    public static class IntExtension
    {
        public static int Half(this int x)
        {
            return x / 2;
        }

        public static int Quarter(this int x)
        {
            return x / 4;
        }

        public static int ToInt(this string x)
        {
            return Convert.ToInt32(x);
        }
    }
}