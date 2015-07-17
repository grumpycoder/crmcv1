using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using crmc.wotdisplay.Infrastructure;
using crmc.wotdisplay.models;

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
            var baseUrl = WebServer +
                           "/breeze/public/People?$filter=IsPriority%20eq%20{0}&$orderby=DateCreated&$skip={1}&$top={2}&$inlinecount=allpages";

            var url = string.Format(baseUrl, isPriorityList.ToString().ToLower(), skip, count);

            var result = await Downloader.DownloadPersonDataAsync(url);
            var people = result.People.Distinct();

            return people.ToList();
        }
    }
}