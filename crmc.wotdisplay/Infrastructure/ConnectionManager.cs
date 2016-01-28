using crmc.wotdisplay.Properties;
using Microsoft.AspNet.SignalR.Client;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;

namespace crmc.wotdisplay.Infrastructure
{
    public class ConnectionManager
    {
        private static readonly string ServerUri = SettingsManager.Configuration.Webserver + "/signalr";
        private static readonly HubConnection connection = new HubConnection(ServerUri);
        public static IHubProxy HubProxy;

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static HubConnection Connection { get { return connection; } }

        public static async void ConnectAsync()
        {
            Connection.StateChanged += ConnectionOnStateChanged;
            var hubName = SettingsManager.Configuration.HubName; 
            HubProxy = Connection.CreateHubProxy(hubName);
            try
            {
                Connection.Start().ContinueWith(task =>
                {
                    if (task.IsFaulted) return;
                    if (Connection.State != ConnectionState.Connected) return;

                    Logger.Info("WoT Connected to Hub {0}", ServerUri);
                }).Wait();
                await Task.Delay(1);

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
