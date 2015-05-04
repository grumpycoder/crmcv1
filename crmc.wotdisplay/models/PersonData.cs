using System.Collections.Generic;
using System.Runtime.Serialization;

namespace crmc.wotdisplay.models
{
    [DataContract]
    public class PersonData
    {
        [DataMember]
        public List<Result> Results { get; set; }
        [DataMember]
        public int InlineCount { get; set; }
    }

    public class Result
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public object EmailAddress { get; set; }
        public string Zipcode { get; set; }
        public bool IsDonor { get; set; }
        public bool IsPriority { get; set; }
        public double FuzzyMatchValue { get; set; }
        public string FullName { get; set; }
        public string DateCreated { get; set; }
    }

}