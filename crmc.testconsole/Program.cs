
using Microsoft.AspNet.SignalR.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {

//            const string getUrl = "http://localhost/crmc/breeze/Breeze/appconfigs";
//
//            var syncClient = new WebClient();
//            var content = syncClient.DownloadString(getUrl);
//            AppConfig config = new AppConfig(); 
//
//            // Create the Json serializer and parse the response
//            var serializer1 = new DataContractJsonSerializer(typeof(AppConfig));
//            using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(content)))
//            {
//                // deserialize the JSON object using the WeatherData type.
//                config = (AppConfig)serializer1.ReadObject(ms);
//            }
//
//            Debug.WriteLine(config);
//
//            
//var objConfig = JObject.FromObject(config);
//            const string saveUrl = "http://localhost/crmc/breeze/Breeze/savechanges";
//            syncClient.Headers[HttpRequestHeader.ContentType] = "application/json"; 
//
//            var response = syncClient.UploadData(saveUrl, Encoding.UTF8.GetBytes(objConfig.ToString()));
//
//            Debug.WriteLine(objConfig);


            var connection = new HubConnection("http://localhost/crmc/signalr");

            //Make proxy to hub based on hub name on server
            var myHub = connection.CreateHubProxy("CRMCHub");

            connection.Start().ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Console.WriteLine("There was an error opening the connection:{0}",
                                      task.Exception.GetBaseException());
                }
                else
                {
                    Console.WriteLine("Connected");
                }

            }).Wait();


            //Console.Read();
            //connection.Stop();

            myHub.On("configSettingsSaved", () =>
            {
                Console.WriteLine("config saved");
            }); 


            myHub.On<string>("addMessage", (message) =>
            {
                Console.WriteLine("From Hub: {0}", message);
            });

            myHub.On<string, string>("addNameToWall", (kiosk, message) =>
            {
                Console.WriteLine("From Hub: {0} : {1}", kiosk, message);
            });

//            myHub.On<AppConfig>("configSaved", (config) =>
//            {
//                Console.WriteLine("From Hub: {0}", config);
//            });


            while (true) // Loop indefinitely
            {
                Console.WriteLine("Enter input:"); // Prompt
                var line = Console.ReadLine(); // Get string from user

                if (line == "exit") // Check string
                {
                    break;
                }

                //var configs = new List<AppConfig>()
                //{
                // new AppConfig() { AppName = "Display", Name = "MaxFontSize", Value = line }, 
                //};
                //var configs = new List<AppConfig>()
                //{
                // new AppConfig() { AppName = "Display", Name = "Volume", Value = line  }, 
                // new AppConfig() { AppName = "Display", Name = "FontSize", Value = line + 1}
                //};

                //var config = new AppConfig()
                //{
                //    AppName = "Display",
                //    Name = "Volume",
                //    Value = line
                //};

                //myHub.Invoke<AppConfig>("SaveConfigSettings", configs).ContinueWith(task =>
                //{
                //    if (task.IsFaulted)
                //    {
                //        Console.WriteLine("There was an error calling send: {0}",
                //                          task.Exception.GetBaseException());
                //    }
                //});

//                myHub.Invoke<string>("SendMessage", line).ContinueWith(task =>
//                {
//                    if (task.IsFaulted)
//                    {
//                        Console.WriteLine("There was an error calling send: {0}",
//                                          task.Exception.GetBaseException());
//                    }
//                });

                //var kiosk = line.Split(':')[0];
                //var name = line.Split(':')[1]; 
                myHub.Invoke("AddNameToWall", "1", line).ContinueWith(task =>
                {
                    if (task.IsFaulted)
                    {
                        Console.WriteLine("There was an error calling send: {0}",
                            task.Exception.GetBaseException());
                    }
                    else
                    {
                        Console.WriteLine("Added name: {0}", line);
                    }
                });

            }


        }
    }
}
