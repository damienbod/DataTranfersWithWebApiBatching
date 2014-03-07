using System.Collections.Generic;
using System.Web.Http;
using Damienbod.Common;

namespace Damienbod.WebAPI.Server.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values/5
        public ProtobufModelDto Get(int id)
        {
            return new ProtobufModelDto() { Id = 1, Name = "HelloWorld", StringValue = "My first Protobuf web api service" };
        }

        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }



        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
