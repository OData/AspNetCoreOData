using Microsoft.Spatial;
using MsGeography = Microsoft.Spatial.Geography;
using MsGeometry = Microsoft.Spatial.Geometry;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.Tests.Query.LegacyFilter
{
    public class LegacySpatialOperations : SpatialOperations
    {
        public override double Distance(MsGeometry geomA, MsGeometry geomB)
        {
            // Simple Euclidean distance in the coordinate units (SRID 0 => unitless plane).
            if (geomA is GeometryPoint p1 && geomB is GeometryPoint p2)
            {
                double dx = p1.X - p2.X;
                double dy = p1.Y - p2.Y;
                double result = Math.Sqrt(dx * dx + dy * dy);
                return result;
            }

            return base.Distance(geomA, geomB);
        }

        public override double Distance(MsGeography operand1, MsGeography operand2)
        {
            return base.Distance(operand1, operand2);
        }

        public override double Length(MsGeometry operand)
        {
            return base.Length(operand);
        }

        public override double Length(MsGeography operand)
        {
            return base.Length(operand);
        }
    }
}
