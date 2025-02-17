namespace OData2Linq.Helpers
{
    using Microsoft.AspNetCore.OData.Edm;
    using Microsoft.AspNetCore.OData.Query;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.OData.Edm;
    using Microsoft.OData.UriParser;
    using OData2Linq.Settings;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    internal class SelectExpandHelper<T>
    {
        private readonly ODataQuery<T> query;

        private readonly string entitySetName;

        private ODataRawQueryOptions RawValues { get; }
        private ODataQueryContext Context { get; }
        private SelectExpandQueryOption SelectExpand { get; set; }

        public SelectExpandHelper(ODataRawQueryOptions rawQueryOptions, ODataQuery<T> query, string entitySetName)
        {
            Context = new ODataQueryContext(query.EdmModel, query.ElementType, null);
            Context.RequestContainer = query.ServiceProvider;
            this.query = query;
            this.entitySetName = entitySetName;
            RawValues = rawQueryOptions ?? throw new ArgumentNullException(nameof(rawQueryOptions));
            if (RawValues.Select != null || RawValues.Expand != null)
            {
                Dictionary<string, string> raws = new Dictionary<string, string>();
                if (RawValues.Select != null)
                {
                    raws["$select"] = RawValues.Select;
                }

                if (RawValues.Expand != null)
                {
                    raws["$expand"] = RawValues.Expand;
                }

                ODataQueryOptionParser parser = ODataLinqExtensions.GetParser(this.query, this.entitySetName, raws);

                SelectExpand = new SelectExpandQueryOption(
                    RawValues.Select,
                    RawValues.Expand,
                    Context,
                    parser);
            }
        }
        public void AddAutoSelectExpandProperties()
        {
            bool containsAutoSelectExpandProperties = false;
            var autoExpandRawValue = GetAutoExpandRawValue();
            var autoSelectRawValue = GetAutoSelectRawValue();

            IDictionary<string, string> queryParameters = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(autoExpandRawValue) && !autoExpandRawValue.Equals(RawValues.Expand))
            {
                queryParameters["$expand"] = autoExpandRawValue;
                containsAutoSelectExpandProperties = true;
            }
            else
            {
                autoExpandRawValue = RawValues.Expand;
            }

            if (!string.IsNullOrEmpty(autoSelectRawValue) && !autoSelectRawValue.Equals(RawValues.Select))
            {
                queryParameters["$select"] = autoSelectRawValue;
                containsAutoSelectExpandProperties = true;
            }
            else
            {
                autoSelectRawValue = RawValues.Select;

                if (autoSelectRawValue != null)
                {
                    queryParameters["$select"] = autoSelectRawValue;
                }
            }

            if (containsAutoSelectExpandProperties)
            {
                ODataQueryOptionParser parser = ODataLinqExtensions.GetParser(
                    query,
                    entitySetName,
                    queryParameters);
                var originalSelectExpand = SelectExpand;
                SelectExpand = new SelectExpandQueryOption(
                    autoSelectRawValue,
                    autoExpandRawValue,
                    Context,
                    parser);
                if (originalSelectExpand != null && originalSelectExpand.LevelsMaxLiteralExpansionDepth > 0)
                {
                    SelectExpand.LevelsMaxLiteralExpansionDepth =
                        originalSelectExpand.LevelsMaxLiteralExpansionDepth;
                }
            }
        }

        public IQueryable Apply(ODataQuery<T> query)
        {
            IQueryable result = query;
            if (SelectExpand != null)
            {
                var tempResult = ApplySelectExpand(
                    result,
                    (ODataQuerySettings)query.ServiceProvider.GetService(typeof(ODataQuerySettings)));
                if (tempResult != default(IQueryable))
                {
                    result = tempResult;
                }
            }

            return result;
        }

        private TSelect ApplySelectExpand<TSelect>(TSelect entity, ODataQuerySettings settings)
        {
            var result = default(TSelect);

            SelectExpandClause processedClause = SelectExpand.ProcessLevels();
            SelectExpandQueryOption newSelectExpand = new SelectExpandQueryOption(
                SelectExpand.RawSelect,
                SelectExpand.RawExpand,
                SelectExpand.Context,
                processedClause);

            ODataSettings qsettings = Context.RequestContainer.GetRequiredService<ODataSettings>();

            newSelectExpand.Validate(qsettings.ValidationSettings);

            var type = typeof(TSelect);
            if (type == typeof(IQueryable))
            {
                result = (TSelect)newSelectExpand.ApplyTo((IQueryable)entity, settings);
            }
            else if (type == typeof(object))
            {
                result = (TSelect)newSelectExpand.ApplyTo(entity, settings);
            }


            return result;
        }

        private string GetAutoSelectRawValue()
        {
            var selectRawValue = RawValues.Select;
            IEdmEntityType baseEntityType = Context.TargetStructuredType as IEdmEntityType;
            if (string.IsNullOrEmpty(selectRawValue))
            {
                IList<SelectModelPath> autoSelectProperties = null;

                if (Context.TargetStructuredType != null && Context.TargetProperty != null)
                {
                    autoSelectProperties = Context.Model.GetAutoSelectPaths(Context.TargetStructuredType, Context.TargetProperty);
                }
                else if (Context.ElementType is IEdmStructuredType structuredType)
                {
                    autoSelectProperties = Context.Model.GetAutoSelectPaths(structuredType, null);
                }

                string autoSelectRawValue = autoSelectProperties == null ? null : string.Join(",", autoSelectProperties.Select(a => a.SelectPath));

                if (!string.IsNullOrEmpty(autoSelectRawValue))
                {
                    if (!string.IsNullOrEmpty(selectRawValue))
                    {
                        selectRawValue = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                            autoSelectRawValue, selectRawValue);
                    }
                    else
                    {
                        selectRawValue = autoSelectRawValue;
                    }
                }
            }

            return selectRawValue;
        }

        private string GetAutoExpandRawValue()
        {
            var expandRawValue = RawValues.Expand;

            IList<ExpandModelPath> autoExpandNavigationProperties = null;
            if (Context.TargetStructuredType != null && Context.TargetProperty != null)
            {
                autoExpandNavigationProperties = Context.Model.GetAutoExpandPaths(Context.TargetStructuredType, Context.TargetProperty, !string.IsNullOrEmpty(RawValues.Select));
            }
            else if (Context.ElementType is IEdmStructuredType elementType)
            {
                autoExpandNavigationProperties = Context.Model.GetAutoExpandPaths(elementType, null, !string.IsNullOrEmpty(RawValues.Select));
            }

            string autoExpandRawValue = autoExpandNavigationProperties == null ? null : string.Join(",", autoExpandNavigationProperties.Select(c => c.ExpandPath));


            if (!string.IsNullOrEmpty(autoExpandRawValue))
            {
                if (!string.IsNullOrEmpty(expandRawValue))
                {
                    expandRawValue = string.Format(CultureInfo.InvariantCulture, "{0},{1}",
                        autoExpandRawValue, expandRawValue);
                }
                else
                {
                    expandRawValue = autoExpandRawValue;
                }
            }
            return expandRawValue;
        }
    }
}