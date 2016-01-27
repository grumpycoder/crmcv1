using System.Collections.Generic;

namespace crmc.wotdisplay.models
{
    public class Configuration
    {
        public Configuration()
        {
            ConfigurationColors = new HashSet<ConfigurationColor>();
        }
        public int Id { get; set; }
        public string HubName { get; set; }
        public string Webserver { get; set; }
        public double Volume { get; set; }
        public string FontFamily { get; set; }
        public int? DefaultMinFontSize { get; set; }
        public int? DefaultMaxFontSize { get; set; }
        public int? DefaultPriorityMinFontSize { get; set; }
        public int? DefaultPriorityMaxFontSize { get; set; }
        public double DefaultItemDelay { get; set; }
        public double DefaultSpeedModifier { get; set; }
        public double DefaultPriorityItemDelay { get; set; }
        public double DefaultLocalItemDelay { get; set; }
        public string DefaultAudioFilePath { get; set; }
        public double NewItemOnScreenDelay { get; set; }
        public double NewItemOnScreenGrowTime { get; set; }
        public double NewItemOnScreenShrinkTime { get; set; }
        public double NewItemFallAnimationDelay { get; set; }
        public double NewItemFallAnimationDelayOffset { get; set; }
        public double NewItemTopMargin { get; set; }
        public double TopMarginOffset { get; set; }

        public virtual ICollection<ConfigurationColor> ConfigurationColors { get; set; }
    }
}