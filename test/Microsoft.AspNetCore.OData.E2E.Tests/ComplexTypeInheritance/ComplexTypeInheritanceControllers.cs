//-----------------------------------------------------------------------------
// <copyright file="ComplexTypeInheritanceControllers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Deltas;
using Microsoft.AspNetCore.OData.Formatter;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Xunit;

namespace Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance;

[Route("convention")]
[Route("explicit")]
public class WindowsController : ODataController
{
    private IList<Window> _windows = new List<Window>();

    public WindowsController()
    {
        Polygon triagle = new Polygon() { HasBorder = true, Vertexes = new List<Point>() };
        triagle.Vertexes.Add(new Point() { X = 1, Y = 2 });
        triagle.Vertexes.Add(new Point() { X = 2, Y = 3 });
        triagle.Vertexes.Add(new Point() { X = 4, Y = 8 });

        Rectangle rectangle = new Rectangle(topLeft: new Point(), width: 2, height: 2);
        Circle circle = new Circle() { HasBorder = true, Center = new Point(), Radius = 2 };

        Window dashboardWindow = new Window
        {
            Id = 1,
            Name = "CircleWindow",
            CurrentShape = circle,
            OptionalShapes = new List<Shape>(),
        };
        dashboardWindow.OptionalShapes.Add(rectangle);
        _windows.Add(dashboardWindow);

        Window popupWindow = new Window
        {
            Id = 2,
            Name = "Popup",
            CurrentShape = rectangle,
            OptionalShapes = new List<Shape>(),
            Parent = dashboardWindow,
        };

        popupWindow.OptionalShapes.Add(triagle);
        popupWindow.OptionalShapes.Add(circle);
        _windows.Add(popupWindow);

        Window anotherPopupWindow = new Window
        {
            Id = 3,
            Name = "AnotherPopup",
            CurrentShape = rectangle,
            OptionalShapes = new List<Shape>(),
            Parent = popupWindow,
        };

        anotherPopupWindow.OptionalShapes.Add(triagle);
        anotherPopupWindow.OptionalShapes.Add(circle);
        _windows.Add(anotherPopupWindow);
    }

    [EnableQuery]
    public IActionResult Get()
    {
        return Ok(_windows);
    }

    [EnableQuery]
    public SingleResult<Window> GetWindow([FromODataUri] int key)
    {
        return SingleResult.Create<Window>(_windows.Where(w => w.Id == key).AsQueryable());
    }

    public IActionResult Post([FromBody]Window window)
    {
        _windows.Add(window);
        window.Id = _windows.Count + 1;
        Rectangle rectangle = window.CurrentShape as Rectangle;
        if(rectangle!=null)
        {
            rectangle.Fill();
        }
        window.OptionalShapes.OfType<Rectangle>().ToList().ForEach(r => r.Fill());
        return Created(window);
    }

    // [HttpPatch("Windows({key})")]
    [HttpPatch]
    public IActionResult Patch(int key, [FromBody]Delta<Window> delta)
    {
        delta.TrySetPropertyValue("Id", key); // It is the key property, and should not be updated.

        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            window = new Window();
            delta.Patch(window);
            return Created(window);
        }

        try
        {
            delta.Patch(window);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }

        return Ok(window);
    }

    [HttpPatch("Windows({key})/CurrentShape")]
    public IActionResult PatchShape(int key, [FromBody] Delta<Shape> delta)
    {
        Window window = _windows.First(e => e.Id == key);
        var currShape = window.CurrentShape;
        Shape newcurrShape = null;

        try
        {
            newcurrShape = delta.Patch(currShape);
        }
        catch (ArgumentException ae)
        {
            return BadRequest(ae.Message);
        }

        return Ok(newcurrShape);
    }

    public IActionResult Put(int key, [FromBody]Window window)
    {
        if (key != window.Id)
        {
            return BadRequest();
        }
        Rectangle rectangle = window.CurrentShape as Rectangle;
        if (rectangle != null)
        {
            rectangle.Fill();
        }
        window.OptionalShapes.OfType<Rectangle>().ToList().ForEach(r => r.Fill());

        Window originalWindow = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            _windows.Add(window);
            return Created(window);
        }

        _windows.Remove(originalWindow);
        _windows.Add(window);

        return Ok(window);
    }

    [EnableQuery]
    public IActionResult Delete([FromODataUri] int key)
    {
        Window window = _windows.Single(w => w.Id == key);
        _windows.Remove(window);
        return StatusCode(204);
    }

    [HttpGet("Windows({key})/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle")]
    public IActionResult GetCurrentShape(int key)
    {
        Window window = _windows.FirstOrDefault(w => w.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        Circle circle = window.CurrentShape as Circle;
        if (circle == null)
        {
            return NotFound();
        }
        return Ok(circle);
    }

    [HttpGet("Windows({key})/CurrentShape/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle/Radius")]
    public IActionResult GetRadius(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        return Ok(((Circle)window.CurrentShape).Radius);
    }

    [HttpGet("Windows({key})/CurrentShape/HasBorder")]
    public IActionResult GetHasBorder(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        return Ok(window.CurrentShape.HasBorder);
    }

    public IActionResult GetOptionalShapes(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        return Ok(window.OptionalShapes);
    }

    public IActionResult GetPolygonalShapes(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        return Ok(window.PolygonalShapes);
    }

    // [HttpGet("Windows({key})/OptionalShapes/Microsoft.AspNetCore.OData.E2E.Tests.ComplexTypeInheritance.Circle")]
    // use convention
    public IActionResult GetOptionalShapesOfCircle(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        return Ok(window.OptionalShapes.OfType<Circle>());
    }

    [HttpPut]
    public IActionResult PutToCurrentShapeOfCircle(int key, [FromBody]Delta<Circle> shape)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        Circle origin = window.CurrentShape as Circle;
        if (origin == null)
        {
            return NotFound();
        }

        shape.Put(origin);
        return Ok(origin);
    }

    [HttpPut("Windows({key})/OptionalShapes")]
    public IActionResult ReplaceOptionalShapes(int key, IEnumerable<Shape> shapes)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        Assert.NotNull(shapes);
        window.OptionalShapes = shapes.ToList();
        return Ok(shapes);
    }

    [HttpPost]
    public IActionResult PostToOptionalShapes(int key, [FromBody]Shape newShape)
    {
        Window window = _windows.FirstOrDefault(w => w.Id == key);
        if (window == null)
        {
            return NotFound();
        }
        Assert.NotNull(newShape);
        window.OptionalShapes.Add(newShape);
        return Updated(window.OptionalShapes);
    }

    [HttpPost]
    public IActionResult PostToPolygonalShapes(int key, [FromBody]Polygon newPolygon)
    {
        Window window = _windows.FirstOrDefault(w => w.Id == key);
        if (window == null)
        {
            return NotFound();
        }
        Assert.NotNull(newPolygon);
        window.PolygonalShapes.Add(newPolygon);
        return Updated(window.PolygonalShapes);
    }

    [HttpPatch]
    public IActionResult PatchToOptionalShapes(int key, [FromBody]Delta<Shape> shapes)
    {
        return Ok("Not Supported");
    }

    [HttpPatch]
    public IActionResult PatchToCurrentShapeOfCircle(int key, [FromBody]Delta<Circle> shape)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        Circle origin = window.CurrentShape as Circle;
        if (origin == null)
        {
            return NotFound();
        }

        shape.Patch(origin);
        return Ok(origin);
    }

    public IActionResult DeleteToCurrentShape(int key)
    {
        Window window = _windows.FirstOrDefault(e => e.Id == key);
        if (window == null)
        {
            return NotFound();
        }

        window.CurrentShape = null;
        return Updated(window);
    }
}
