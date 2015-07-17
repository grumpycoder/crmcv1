using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using crmc.wotdisplay.models;
using NLog;
using NLog.Fluent;

namespace crmc.wotdisplay.Infrastructure
{
    public class Downloader
    {
        public static Logger Log { get; set; }

        //public Downloader(Logger log)
        //{
        //    Log = log;
        //}

        public static async Task<DownloadResult> DownloadPersonDataAsync(string url)
        {
            var client = new WebClient();
            var downloadResult = new DownloadResult();

            try
            {
                var buffer = client.DownloadString(url);
                var serializer = new DataContractJsonSerializer(typeof(DownloadResult));

                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(buffer)))
                {
                    downloadResult = (DownloadResult)serializer.ReadObject(ms);
                }
                await Task.Delay(1);

            }
            catch (Exception exception)
            {
                Console.WriteLine("Exception occurred: " + exception.Message);
            }
            return downloadResult;
        }

        public static async Task<AppConfig> DownloadConfigDataAsync(string url)
        {
            var client = new WebClient();

            var buffer = await client.DownloadStringTaskAsync(url);
            var serializer = new DataContractJsonSerializer(typeof(AppConfig));
            AppConfig downloadResult;

            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(buffer)))
            {
                downloadResult = (AppConfig)serializer.ReadObject(ms);
            }

            return downloadResult;

        }
    }

    [DataContract]
    public class DownloadResult
    {
        [DataMember(Name = "Results")]
        public List<Person> People { get; set; }
        [DataMember]
        public int InlineCount { get; set; }
    }


}
