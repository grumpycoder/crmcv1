namespace crmc.web.Models
{
    public class WallConfiguration
    {
        public int Id { get; set; }
        public string HubName { get; set; }
        public string Webserver { get; set; }
        public double? Volume { get; set; }
        public string FontFamily { get; set; }
        public int? DefaultMinFontSize { get; set; }
        public int? DefaultMaxFontSize { get; set; }
        public int? DefaultPriorityMinFontSize { get; set; }
        public int? DefaultPriorityMaxFontSize { get; set; }
        public double? DefaultNewItemDelay { get; set; }
        public string DefaultAudioFilePath { get; set; }

    }
}