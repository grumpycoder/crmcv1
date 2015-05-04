using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Net.Http;

namespace crmc.wotdisplay.Infrastructure
{
    public class ConnectionManager
    {
        private static readonly string ServerURI = Settings.Default.WebServerUrl + "/signalr";
        private static readonly string HubName = Settings.Default.HubName;
        private static readonly HubConnection connection = new HubConnection(ServerURI);
        public static IHubProxy HubProxy;

        //private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string KioskName = Settings.Default.KioskName;

        public static HubConnection Connection { get { return connection; } }

        //public static async void ConnectAsync()
        public static void ConnectAsync()
        {
            Connection.StateChanged += ConnectionOnStateChanged;

            HubProxy = Connection.CreateHubProxy(HubName);
            try
            {
                //await Connection.Start();
                Connection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted) return;
                    if (Connection.State != ConnectionState.Connected) return;

                    //Logger.Trace("{0} Connected", KioskName);
                    Debug.WriteLine("{0} Connected", KioskName);
                    HubProxy.Invoke("Join", KioskName).ContinueWith(taskJoin =>
                    {
                        if (taskJoin.IsFaulted)
                        {
                            //Logger.Error("{0} Error during joining the server", KioskName);
                            Debug.WriteLine("{0} Error during joining the server", KioskName);
                        }
                        else
                        {
                            var sub = HubProxy.Subscribe("addMessage");
                        }
                    });
                }).Wait();

            }
            catch (HttpRequestException e)
            {
                //Logger.Error(e.InnerException);
                Debug.WriteLine(e.InnerException);
            }
            catch (HttpClientException clientException)
            {
                //Logger.Error(clientException.InnerException);
                Debug.WriteLine(clientException.InnerException);
            }
        }

        private static void ConnectionOnStateChanged(StateChange stateChange)
        {
            Debug.WriteLine("Connection State: " + Connection.State);
        }

        public static void SendMessage(string message)
        {
            HubProxy.Invoke("SendMessage", message).ContinueWith(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.WriteLine("{0} Error during sending message", KioskName);
                    //Logger.Trace("{0} Error during sending message", KioskName);
                }
                else
                {
                    //Logger.Trace("{0} Message send successfully", KioskName);
                    Debug.WriteLine("{0} Message send successfully", KioskName);
                }
            });
        }

    }
}
