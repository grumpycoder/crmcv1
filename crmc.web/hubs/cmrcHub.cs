using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using crmc.web.Data;
using crmc.web.Models;
using Microsoft.AspNet.SignalR.Hubs;

//using ApplicationDbContext = crmc.web.Data.ApplicationDbContext;

namespace crmc.web.Hubs
{
    [HubName("crmcHub")]
    public class CRMCHub : Hub
    {
//        private readonly ApplicationDbContext context;

//        public CRMCHub()
//        {
//            
//        }
//
//        public CRMCHub(ApplicationDbContext context)
//        {
//            this.context = context;
//        }

        //Test method
        public Task SendMessage(string message)
        {
            return Clients.All.addMessage(message);
        }

//        public Task AddNameToWall(string kiosk, string name)
//        {
//            return Clients.All.nameAddedToWall(kiosk, name);
//        }

        public Task AddNameToWall(string kiosk, Person person)
        {
            return Clients.All.nameAddedToWall(kiosk, person);
        }

/*        public Task SaveConfigSettings(AppConfig config)
        {
            return Clients.All.configSaved(config);
        }*/

        public Task ConfigSettingsSaved()
        {
            return Clients.All.configSettingsSaved();
        }

        public override Task OnConnected()
        {
            var name = Context.User.Identity.Name;

            return base.OnConnected();
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            
            return base.OnDisconnected(stopCalled);
        }
    }
}