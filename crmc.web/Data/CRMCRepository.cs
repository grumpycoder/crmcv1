using System.Linq;
using Breeze.ContextProvider;
using Breeze.ContextProvider.EF6;
using crmc.web.Models;
using Newtonsoft.Json.Linq;

namespace crmc.web.Data
{
    public class CRMCRepository
    {
        private readonly EFContextProvider<ApplicationDbContext>
        _contextProvider = new EFContextProvider<ApplicationDbContext>();

        private ApplicationDbContext Context { get { return _contextProvider.Context; } }

        public string Metadata
        {
            get { return _contextProvider.Metadata(); }
        }
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return _contextProvider.SaveChanges(saveBundle);
        }

        public IQueryable<Person> People
        {
            get { return Context.People; }
        }

        public IQueryable<Censor> Censors
        {
            get { return Context.Censors; }
        }

        public IQueryable<AppConfig> AppConfigs
        {
            get { return Context.AppSettings; }
        }

        public IQueryable<WallConfiguration> Configurations
        {
            get { return Context.WallConfigurations; }
        }
    }
}