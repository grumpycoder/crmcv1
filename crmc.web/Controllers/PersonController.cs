using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.OData;
using crmc.web.Data;
using crmc.web.Models;

namespace crmc.web.Controllers
{
    public class PersonController : ApiController
    {
        // GET: api/Person
        [EnableQuery]
        [ResponseType(typeof(Person))]
        public IEnumerable<Person> Get()
        {
            var repo = new CRMCRepository();
            return repo.People.Take(10).AsQueryable();
        }

        // GET: api/Person/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Person
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Person/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Person/5
        public void Delete(int id)
        {
        }
    }
}
