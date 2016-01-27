using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using crmc.wotdisplay.models;
using Newtonsoft.Json;
using NLog;

namespace crmc.wotdisplay
{
    public class PersonRepository
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public string WebServer { get; set; }
        private int recordCount;

        public PersonRepository(string webServer)
        {
            WebServer = webServer;
        }

        private readonly Random rand = new Random();

        public async Task<List<Person>> Get(int count, bool isPriorityList = false)
        {
            var upperBound = recordCount == 0 ? 10000 : recordCount; 
            var skip = isPriorityList ? 0 : rand.Next(1, upperBound - count);
            //TODO: Refactor hardcoded url
            var baseUrl = WebServer +
                           "/breeze/public/People?$filter=IsPriority%20eq%20{0}&$orderby=SortOrder&$skip={1}&$top={2}&$inlinecount=allpages";

            var url = string.Format(baseUrl, isPriorityList.ToString().ToLower(), skip, count);
            Log.Info("Skipping: {0}", skip);
            Log.Info("Repository Url: {0}", url);    

            var people = new List<Person>();
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var data  = JsonConvert.DeserializeObject<DownloadResult>(result);
                    recordCount = data.InlineCount;
                    people = data.People;
                }
                else
                {
                    //TODO: Log error of download
                    Log.Debug("Error downloading from person repository");
                    Log.Debug("Error: {0}", response.StatusCode);
                }
            }

            return people; 
        }
    }
}