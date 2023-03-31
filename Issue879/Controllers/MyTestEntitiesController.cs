using Issue879.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace Issue879.Controllers
{
    public class MyTestEntitiesController : ODataController
    {
        public MyTestEntitiesController()
        {
        }


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
