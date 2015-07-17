using System;

namespace crmc.wotdisplay.models
{
    public class Person
    {
        public int Id { get; set; }
        public string AccountId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string Zipcode { get; set; }
        public bool? IsDonor { get; set; }
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