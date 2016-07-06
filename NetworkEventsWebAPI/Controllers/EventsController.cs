using Common;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace NetworkEventsWebAPI.Controllers
{
    public class EventsController : ApiController
    {
        // GET api/events 
        public async Task<IEnumerable<NetworkEvent>> GetAsync()
        {
            var eventSource = ServiceFactory.CreateNetworkEventSourceProxy();
            return await eventSource.GetNetworkLogs();
        }

        // GET api/events/5 
        public string Get(int id)
        {
            return "value";
        }

        // POST api/events 
        public void Post([FromBody]string value)
        {
        }

        // PUT api/events/5 
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/events/5 
        public void Delete(int id)
        {
        }
    }
}
