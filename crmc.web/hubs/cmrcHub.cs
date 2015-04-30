using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using crmc.web.Data;
using crmc.web.Models;
//using ApplicationDbContext = crmc.web.Data.ApplicationDbContext;

namespace crmc.web.Hubs
{
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

        public Task AddNameToWall(string kiosk, string name)
        {
            return Clients.All.nameAddedToWall(kiosk, name);
        }

        public Task SaveConfigSettings(AppConfig config)
        {
            return Clients.All.configSaved(config);
        }



    }
}