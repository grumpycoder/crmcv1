using System;

namespace crmc.web.Models
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
        public decimal? FuzzyMatchValue { get; set; }

        public string FullName
        {
            get { return string.Format("{0} {1}", Firstname, Lastname); }
        }

        public DateTime DateCreated { get; set; }


        //For EF...
        public Person()
        {
        }

        public Person(string accountId, string firstname, string lastname, string emailaddress, string zipcode, bool isdonor = false, bool ispriority = false)
        {
            //AccountId = accountId;
            Firstname = firstname;
            Lastname = lastname;
            EmailAddress = emailaddress;
            Zipcode = zipcode;
            IsDonor = isdonor;
            IsPriority = ispriority;
            DateCreated = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0} {1}", Firstname, Lastname);
        }

    }
}