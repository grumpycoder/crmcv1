using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using crmc.wotdisplay.models;
using Newtonsoft.Json;

namespace crmc.wotdisplay.Infrastructure
{
    public static class SettingsManager
    {
        public static WallConfiguration WallConfiguration { get; set; }

        public static async Task<WallConfiguration> LoadAsync(string apiUrl)
        {
            WallConfiguration = new WallConfiguration();
            //Load configuration from a repository
            using (var client = new HttpClient())
            {

                //TODO: Use apiUrl here
                //client.BaseAddress = new Uri("http://localhost/crmc/");
                //client.BaseAddress = new Uri(apiUrl);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                //var response = await client.GetAsync("http://localhost/crmc/breeze/public/configurations");
                var response = await client.GetAsync(apiUrl);
                if (response.IsSuccessStatusCode)
                {
                    var s = await response.Content.ReadAsStringAsync();
                    WallConfiguration = JsonConvert.DeserializeObject<WallConfiguration>(s);
                }
                else
                {
                    //Log response status code error
                    //Console.WriteLine(response.StatusCode);
                }
            }
            return WallConfiguration;
        }

        public static void SaveSettingsAsync()
        {
            //TODO: Save settings back to database
            Console.WriteLine("Settings saved");
        }
    }


}