namespace crmc.web.Models
{
    public class ConfigurationColor
    {
        public int Id { get; set; }
        public string RGB { get; set; }
        public string Hex { get; set; }
        public string Name { get; set; }

        public int? ConfigurationId { get; set; }

        public virtual Configuration Configuration { get; set; }
    }
}