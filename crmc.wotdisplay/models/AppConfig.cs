using System.Runtime.Serialization;

namespace crmc.wotdisplay.models
{
    [DataContract]
    public class AppConfig
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string HubName { get; set; }
        [DataMember]
        public string WebServerURL { get; set; }
        [DataMember]
        public double Volume { get; set; }
        [DataMember]
        public int ScrollSpeed { get; set; }
        [DataMember]
        public double AddNewItemSpeed { get; set; }
        [DataMember]
        public int MinFontSize { get; set; }
        [DataMember]
        public int MaxFontSize { get; set; }
        [DataMember]
        public int MinFontSizeVIP { get; set; }
        [DataMember]
        public int MaxFontSizeVIP { get; set; }
        [DataMember]
        public string FontFamily { get; set; }
        [DataMember]
        public string AudioFilePath { get; set; }
        [DataMember]
        public bool UseLocalDataSource { get; set; }

    }
}