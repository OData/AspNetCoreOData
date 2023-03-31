using Issue879.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Issue879.Controllers
{
    // 2) I use conventional routing in this controller, so the controller name is same as the entity set name
    public class MyTestEntitiesController : ODataController
    {
        public MyTestEntitiesController()
        {
        }

        // 3) Don't put any attribute template in HttpGet since we are using conventional routing
        // 4) Make the action name as "Get"
        [HttpGet/*(Name = "ListMyTestEntitiesAsync")*/]
        [EnableQuery]
        public async Task<ActionResult<IEnumerable<MyTestEntity>>>
            Get(ODataQueryOptions<MyTestEntity> options)
        {
            try
            {
                //StreamReader str = new StreamReader("C:\\Temp.txt");
                string data = "{ \"Prop1\":[\"10.10.10.1\"]}";
                var prop = JsonConvert.DeserializeObject<Dictionary<string, object>>(data,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                // 5) OData doesn't know how to serialize 'JToken, JArray', etc. 
                // So, you'd better to change them to "known" types.
                foreach (var kv in prop)
                {
                    if (kv.Value is JArray jArray)
                    {
                        IList<string> list = new List<string>();
                        jArray.ToList().ForEach(t => { list.Add(t.ToString()); });
                        prop[kv.Key] = list;
                    }
                }

                return new List<MyTestEntity>
                {
                    new MyTestEntity
                    {
                        Id = 1,
                        MyProperties = prop
                    }
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult(HttpStatusCode.InternalServerError);
            }
        }
    }
}
