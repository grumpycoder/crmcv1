using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Net.Http;
using NLog;

namespace crmc.wotdisplay.Infrastructure
{
    public class ConnectionManager
    {
        private static readonly string ServerUri = "http://localhost/crmc" + "/signalr";
        private static readonly string HubName = Settings.Default.HubName;
        private static readonly HubConnection connection = new HubConnection(ServerUri);
        public static IHubProxy HubProxy;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static string KioskName = Settings.Default.KioskName;

        public static HubConnection Connection { get { return connection; } }

        public static async void ConnectAsync()
        {
            Connection.StateChanged += ConnectionOnStateChanged;

            HubProxy = Connection.CreateHubProxy(HubName);
            try
            {
                Connection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted) return;
                    if (Connection.State != ConnectionState.Connected) return;

                    Logger.Info(string.Format("{0} Connected", KioskName));
                    Debug.WriteLine("{0} Connected", KioskName);

                }).Wait();

            }
            catch (HttpRequestException e)
            {
                Logger.Error(e.InnerException);
                Debug.WriteLine(e.InnerException);
            }
            catch (HttpClientException clientException)
            {
                Logger.Error(clientException.InnerException);
                Debug.WriteLine(clientException.InnerException);
            }
        }

        private static void ConnectionOnStateChanged(StateChange stateChange)
        {
            Debug.WriteLine("Connection State: " + Connection.State);
        }

    }
}
