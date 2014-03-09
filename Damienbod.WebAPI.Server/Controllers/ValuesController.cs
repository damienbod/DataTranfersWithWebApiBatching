using System.Collections.Generic;
using System.Web.Http;
using Damienbod.Common;

namespace Damienbod.WebAPI.Server.Controllers
{
    [RoutePrefix("api/values")]
    public class ValuesController : ApiController
    {
        [HttpGet]
        [Route("{id}")]
        public ProtobufModelDto Get(int id)
        {
            return new ProtobufModelDto() { Id = 1, Name = "HelloWorld", StringValue = "My first Protobuf web api service" };
        }

        [HttpGet]
        [Route("")]
        public IEnumerable<ProtobufModelDto> Get()
        {
            return new ProtobufModelDto[] { new ProtobufModelDto() { Id = 1, Name = "HelloWorld", StringValue = "My first Protobuf web api service" } };
        }

        [HttpPost]
        [Route("")]
        public void Post([FromBody]ProtobufModelDto value)
        {
            var objectToDelete = value;
        }

        [HttpPut]
        [Route("{id}")]
        public void Put(int id, [FromBody]ProtobufModelDto value)
        {
            var objectToDelete = value;
            var idOfDtoToDelete = id;
        }

        [HttpDelete]
        [Route("{id}")]
        public void Delete(int id)
        {
            var idOfDtoToDelete = id;
        }
    }
}
