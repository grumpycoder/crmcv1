using System;

namespace crmc.wotdisplay.models
{
    public class Person
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public bool? IsPriority { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", Firstname, Lastname); }
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Firstname, Lastname);
        }

    }
}