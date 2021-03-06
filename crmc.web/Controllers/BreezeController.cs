﻿using System.Linq;
using Breeze.WebApi2;
using System.Web.Http;
using Breeze.ContextProvider;
using crmc.domain;
using crmc.web.Data;
using crmc.web.Models;
using Newtonsoft.Json.Linq;

namespace crmc.web.Controllers
{
    [BreezeController]
    //[Authorize]
    public class BreezeController : ApiController
    {

        // Todo: inject via an interface rather than "new" the concrete class
        readonly CRMCRepository repository = new CRMCRepository();

        [HttpGet]
        [AllowAnonymous]
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

        [HttpGet]
        public Configuration Configurations()
        {
            return repository.Configurations.FirstOrDefault();
        }
    }
}