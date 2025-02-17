[![NuGet Version](https://img.shields.io/nuget/v/OData2Linq?label=NuGet)](https://www.nuget.org/packages/OData2Linq/)

# OData2Linq
Apply an OData text query (filter, order by, select, ..) to an IQueryable expression.

# Features
- Manipulate an IQueryable expression with OData text query

## Supported OData parameters
| Params        | In Memory Collections | Entity Framework | CosmosDB SQL API |
| ------------- |:---------------------:|:----------------:| :---------------:|
| $filter       |+                      | +                | +                |
| $orderby      |+                      | +                | +                |
| $select       |+                      | +                | -                |
| $expand       |+                      | +                | -                |
| $top          |+                      | +                | +                |
| $skip         |+                      | +                | +                |

# Samples
Please check samples below to get started

## .NET Fiddle
https://dotnetfiddle.net/6dLB2g

## Console app
```csharp
using System;
using System.Linq;
using OData2Linq;

public class Entity
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public static class GetStartedDemo
{
    public static void Demo()
    {
        Entity[] items =
        {
            new Entity { Id = 1, Name = "n1" },
            new Entity { Id = 2, Name = "n2" },
            new Entity { Id = 3, Name = "n3" }
        };
        IQueryable<Entity> query = items.AsQueryable();

        var result = query.OData().Filter("Id eq 1 or Name eq 'n3'").OrderBy("Name desc").TopSkip("10", "0").ToArray();

        // Id: 3 Name: n3
        // Id: 1 Name: n1
        foreach (Entity entity in result)
        {
            Console.WriteLine("Id: {0} Name: {1}", entity.Id, entity.Name);
        }
    }
}
```

## Support ToArrayAsync(), ToListAsync(), and all other provider specific methods.
Use `.ToOriginalQuery()` after finishing working with OData to be able to support provider specific methods of original query.

### Entity Framework async data fetch.
```
Student[] array = await dbContext.Students.OData()
                .Filter("LastName eq 'Alexander' or FirstMidName eq 'Laura'")
                .OrderBy("EnrollmentDate desc")
                .TopSkip("1","1")
                .ToOriginalQuery() // required to be able to use .ToArrayAsync() next.
                .ToArrayAsync();

ISelectExpandWrapper[] select2 = await dbContext.Students.OData()
                .Filter("LastName eq 'Alexander' or FirstMidName eq 'Laura'")
                .OrderBy("EnrollmentDate desc")
                .SelectExpandAsQueryable("LastName", "Enrollments($select=CourseId)") //.SelectExpandAsQueryable() use .ToOriginalQuery() implicitly, so not need to call it.
                .ToArrayAsync()
```
### CosmosDb SQL API async data fetch.
```
var item = await Container.GetItemLinqQueryable<TestEntity>().OData()
                .Filter($"Id eq '{id1}'")
                .TopSkip("1")
                .ToOriginalQuery() // required to be able to use .ToFeedIterator() next.
                .ToFeedIterator()
                .ReadNextAsync()
```

## Advanced code samples at wiki
See the [Wiki pages](https://github.com/ArnaudB88/OData2Linq/wiki)

# Contribution
Please feel free to create issues and pull requests to the main branch.

# Nuget
| Package                         | NuGet                                                                                                                                              | Info             |
|---------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------|------------------|
| OData2Linq                      | [![NuGet Version](https://img.shields.io/nuget/v/OData2Linq?label=NuGet)](https://www.nuget.org/packages/OData2Linq/)                              | OData v8 support |
| Community.OData.Linq.Json       | [![NuGet Version](https://img.shields.io/nuget/v/Community.OData.Linq.Json)](https://www.nuget.org/packages/Community.OData.Linq.Json)             | Still works on odata v7 libraries. Open an issue to create a new package for odata v8. |
| Community.OData.Linq.AspNetCore | [![NuGet Version](https://img.shields.io/nuget/v/Community.OData.Linq.AspNetCore)](https://www.nuget.org/packages/Community.OData.Linq.AspNetCore) | Still works on odata v7 libraries. Open an issue to create a new package for odata v8. |

# References
This project is based on the following project:
https://github.com/IharYakimush/comminity-data-odata-linq

The repository is a fork from:
https://github.com/OData/AspNetCoreOData
