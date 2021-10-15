using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Microsoft.OData.UriParser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ODataRoutingSample.Extensions
{
    public class FilterSegmentTemplate : ODataSegmentTemplate
    {
        public override IEnumerable<string> GetTemplates(ODataRouteOptions options)
        {
            yield return "/$filter({filterClause})";
        }

        public override bool TryTranslate(ODataTemplateTranslateContext context)
        {
            if (!context.RouteValues.TryGetValue("filterClause", out object filterClause))
            {
                return false;
            }

            EntitySetSegment lastSegment = context.Segments.Last() as EntitySetSegment;
            IEdmType elementType = lastSegment.EntitySet.EntityType();

            Type clrType = GetClrType(context.Model, elementType);
            ODataQueryContext queryContext = new ODataQueryContext(context.Model, clrType, null);

            ODataQueryOptionParser queryOptionParser = new ODataQueryOptionParser(
                    context.Model,
                    elementType,
                    lastSegment.EntitySet,
                    new Dictionary<string, string> { { "$filter", $"{filterClause}" } });

            FilterQueryOption filter = new FilterQueryOption((string)filterClause, queryContext, queryOptionParser);

         //   context.Segments.Add(new FilterSegment(filter.FilterClause.Expression, filter.FilterClause.RangeVariable, lastSegment.EntitySet));

            context.UpdatedValues.Add("filter", filter);

            return true;
        }

        internal static Type GetClrType(IEdmModel edmModel, IEdmType edmType)
        {
            ClrTypeAnnotation annotation = edmModel.GetAnnotationValue<ClrTypeAnnotation>(edmType);
            if (annotation != null)
            {
                return annotation.ClrType;
            }

            throw new ODataException("Cannot find the CLR type!");
        }
    }
}
