using System.Collections.Generic;
using System.Runtime.Serialization;

namespace crmc.wotdisplay.models
{
    [DataContract]
    public class DownloadResult
    {
        [DataMember(Name = "Results")]
        public List<Person> People { get; set; }
        [DataMember]
        public int InlineCount { get; set; }
    }
}