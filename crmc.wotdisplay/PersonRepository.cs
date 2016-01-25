using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using crmc.wotdisplay.models;
using Newtonsoft.Json;

namespace crmc.wotdisplay
{
    public class PersonRepository
    {
        public string WebServer { get; set; }

        public PersonRepository(string webServer)
        {
            WebServer = webServer;
        }

        private readonly Random rand = new Random();

        public async Task<List<Person>> Get(int count, bool isPriorityList = false)
        {
            var skip = isPriorityList ? 0 : rand.Next(1, 100000);
            //TODO: Refactor hardcoded url
            var baseUrl = WebServer +
                           "/breeze/public/People?$filter=IsPriority%20eq%20{0}&$orderby=SortOrder&$skip={1}&$top={2}&$inlinecount=allpages";

            var url = string.Format(baseUrl, isPriorityList.ToString().ToLower(), skip, count);

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
                    people = data.People;
                }
                else
                {
                    //TODO: Log error of download
                    //Log response status code error
                    //Console.WriteLine(response.StatusCode);
                }
            }

            return people; 
        }
    }
}