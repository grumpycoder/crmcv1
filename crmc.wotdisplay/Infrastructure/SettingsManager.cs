using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using crmc.wotdisplay.models;
using Newtonsoft.Json;
using NLog;

namespace crmc.wotdisplay.Infrastructure
{
    public static class SettingsManager
    {
        private static readonly List<Color> colors = new List<Color>();
        private static readonly Random Random = new Random();
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static Configuration Configuration { get; set; }

        static SettingsManager()
        {
            Configuration = new Configuration();
        }

        public static async Task<Configuration> LoadAsync(string apiUrl)
        {
            //Load configuration from a repository
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    Configuration = JsonConvert.DeserializeObject<Configuration>(s);
                    LoadColors();
                }
                else
                {
                    //Log response status code error
                    Log.Debug(response.StatusCode);
                }
            }
            return Configuration;
        }

        private static void LoadColors()
        {
            foreach (var color in Configuration.ConfigurationColors)
            {
                if (color.RGB != null)
                {
                    var items = color.RGB.Split(',').Select(byte.Parse).ToArray();
                    colors.Add(Color.FromRgb(items[0], items[1], items[2]));
                }
                if (color.Hex != null)
                {
                    colors.Add((Color)ColorConverter.ConvertFromString(color.Hex));
                }
                if (color.Name != null)
                {
                 colors.Add((Color)ColorConverter.ConvertFromString(color.Name));   
                }

            }
        }

        public static async Task<bool> SaveSettingsAsync(string apiUrl)
        {
            //TODO: Save settings back to database
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //BUG: [SettingsManager] Saving settings 
                string postBody = JsonConvert.SerializeObject(Configuration, Formatting.None, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore});
                //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                apiUrl = "http://localhost/crmc/breeze/public/savechanges"; 
                var response = await client.PostAsync(apiUrl, new StringContent(postBody, Encoding.UTF8, "application/json"));

                // New code:
                if (response.IsSuccessStatusCode)
                {
                    Log.Debug("Saved configuration settings");
                }
                else
                {
                    //Log response status code error
                    Log.Debug("Error saving configuration changes {0}", response.StatusCode);
                }
            }
            return true;
        }

        public static Color RandomColor()
        {
            var color = colors[RandomNumber(0, colors.Count)];
            return color; 
        }

        private static int RandomNumber(int min, int max)
        {
            if (max <= min) min = max - 1;
            return Random.Next(min, max);
        }
    }


}