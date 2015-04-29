namespace crmc.web.Models
{
    public class AppConfig
    {
        public int Id { get; set; }
        public string HubName { get; set; }
        public string WebServerURL { get; set; }
        public double Volume { get; set; }
        public int ScrollSpeed { get; set; }
        public double AddNewItemSpeed { get; set; }
        public int MinFontSize { get; set; }
        public int MaxFontSize { get; set; }
        public int MinFontSizeVIP { get; set; }
        public int MaxFontSizeVIP { get; set; }
        public string FontFamily { get; set; }
        public string AudioFilePath { get; set; }
        public bool UseLocalDataSource { get; set; }

    }
}