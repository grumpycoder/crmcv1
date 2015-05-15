using System.Linq;
using Breeze.WebApi2;
using System.Web.Http;
using Breeze.ContextProvider;
using crmc.web.Data;
using crmc.web.Models;
using Newtonsoft.Json.Linq;

namespace crmc.web.Controllers
{
    [BreezeController]
    public class PublicController : ApiController
    {

        // Todo: inject via an interface rather than "new" the concrete class
        readonly CRMCRepository repository = new CRMCRepository();

        [HttpGet]
        public string Metadata()
        {
            return repository.Metadata;
        }

        [HttpPost]
        public SaveResult SaveChanges(JObject saveBundle)
        {
            return repository.SaveChanges(saveBundle);
        }

        [HttpGet]
        public IQueryable<Person> People()
        {
            return repository.People;
        }

        [HttpGet]
        public IQueryable<Censor> Censors()
        {
            return repository.Censors;
        }

        [HttpGet]
        public AppConfig AppConfigs()
        {
            return repository.AppConfigs.FirstOrDefault();
        }

    }
}