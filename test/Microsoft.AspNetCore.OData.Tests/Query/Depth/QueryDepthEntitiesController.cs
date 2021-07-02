// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData.Tests.Query
{

    public class LevelsEntitiesController : ODataController
    {

        public IList<LevelsEntity> Entities;

        public LevelsEntitiesController()
        {
            Entities = new List<LevelsEntity>();
            for (int i = 1; i <= 10; i++)
            {
                if (i % 2 == 1)
                {
                    var newEntity = new LevelsEntity
                    {
                        ID = i,
                        Name = "Name " + i,
                        Parent = Entities.LastOrDefault(),
                        BaseEntities = Entities.Concat(new[]
                            {
                                new LevelsBaseEntity
                                {
                                    ID = i + 10,
                                    Name = "Name " + (i + 10)
                                }
                            }).ToArray(),
                        DerivedAncestors = Entities.OfType<LevelsDerivedEntity>().ToArray()
                    };
                    Entities.Add(newEntity);
                }
                else
                {
                    var newEntity = new LevelsDerivedEntity
                    {
                        ID = i,
                        Name = "Name " + i,
                        DerivedName = "DerivedName " + i,
                        Parent = Entities.LastOrDefault(),
                        BaseEntities = Entities.Concat(new[]
                            {
                                new LevelsBaseEntity
                                {
                                    ID = i + 10,
                                    Name = "Name " + (i + 10)
                                }
                            }).ToArray(),
                        DerivedAncestors = Entities.OfType<LevelsDerivedEntity>().ToArray(),
                        AncestorsInDerivedEntity = Entities.ToArray()
                    };
                    Entities.Add(newEntity);
                }
            }
            Entities[8].Parent = Entities[9];
            Entities[1].DerivedAncestors = new LevelsDerivedEntity[] { (LevelsDerivedEntity)Entities[3] };
        }

        public IActionResult Get(ODataQueryOptions<LevelsEntity> queryOptions)
        {
            var validationSettings = new ODataValidationSettings { MaxExpansionDepth = 5 };

            try
            {
                queryOptions.Validate(validationSettings);
            }
            catch (ODataException e)
            {
                //var responseMessage = new HttpResponseMessage(HttpStatusCode.BadRequest);
                //responseMessage.Content = new StringContent(
                //    Error.Format("The query specified in the URI is not valid. {0}", e.Message));
                Request.ODataFeature().Path = null;
                return BadRequest(Error.Format("The query specified in the URI is not valid. {0}", e.Message));
            }

            var querySettings = new ODataQuerySettings();
            var result = queryOptions.ApplyTo(Entities.AsQueryable(), querySettings).AsQueryable();

            return Ok(result);
            //return Ok(result, result.GetType());
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        public IActionResult Get(int key)
        {
            return Ok(Entities.Single(e => e.ID == key));
        }

        //private IActionResult Ok(object content, Type type)
        //{
        //    var resultType = typeof(OkNegotiatedContentResult<>).MakeGenericType(type);
        //    return Activator.CreateInstance(resultType, content, this) as IActionResult;
        //}
    }

}
