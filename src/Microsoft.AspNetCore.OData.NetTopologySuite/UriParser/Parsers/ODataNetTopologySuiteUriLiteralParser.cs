//-----------------------------------------------------------------------------
// <copyright file="ODataNetTopologySuiteUriLiteralParser.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved.
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Diagnostics;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Microsoft.AspNetCore.OData.NetTopologySuite.UriParser.Parsers;

/// <summary>
/// Parses OData spatial literals (<c>geometry'…'</c> / <c>geography'…'</c>) into
/// NetTopologySuite <see cref="Geometry"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// Supports the OData V4 spatial literal form:
/// <c>geometry'SRID=&lt;int&gt;;&lt;WKT&gt;'</c> or <c>geography'SRID=&lt;int&gt;;&lt;WKT&gt;'</c>.
/// The <c>SRID=…;</c> prefix is optional; when omitted, defaults are applied based on the
/// literal prefix:
/// </para>
/// <list type="bullet">
///   <item><description><c>geography</c> ⇒ default SRID <c>4326</c></description></item>
///   <item><description><c>geometry</c>  ⇒ default SRID <c>0</c></description></item>
/// </list>
/// <para>
/// The inner text (after removing the OData literal wrapper and optional SRID) is expected
/// to be a valid WKT string (e.g., <c>POINT(-122.35 47.65)</c>, <c>LINESTRING(…)</c>, etc.),
/// which is parsed by <see cref="WKTReader"/>.
/// </para>
/// <para>
/// If the incoming literal is the keyword <c>null</c>, this parser returns <c>null</c> and
/// does not set parsing exception.
/// </para>
/// </remarks>
internal class ODataNetTopologySuiteUriLiteralParser : IUriLiteralParser
{
    private const string LiteralPrefixGeometry = "geometry";
    private const string LiteralPrefixGeography = "geography";

    /// <summary>
    /// Singleton instance of the parser.
    /// </summary>
    public readonly static ODataNetTopologySuiteUriLiteralParser Instance = new ODataNetTopologySuiteUriLiteralParser();

    private ODataNetTopologySuiteUriLiteralParser()
    {
    }

    /// <summary>
    /// Attempts to parse an OData URI spatial literal into a NetTopologySuite <see cref="Geometry"/> value
    /// when the requested Edm type is spatial (<c>Edm.Geometry*</c> or <c>Edm.Geography*</c>).
    /// </summary>
    /// <param name="text">The OData literal text, e.g., <c>geography'POINT(-122.35 47.65)'</c>.</param>
    /// <param name="targetType">
    /// The Edm type requested by the URI pipeline. Must be a primitive spatial type
    /// (e.g., <c>Edm.GeometryPoint</c>, <c>Edm.GeographyPoint</c>).
    /// </param>
    /// <param name="parsingException">
    /// Set to a <see cref="UriLiteralParsingException"/> describing the failure when this parser recognizes the literal
    /// format but cannot parse it (e.g., invalid WKT). Set to <c>null</c> on success, or when the literal/type is
    /// not recognized and the parser declines to handle it.
    /// </param>
    /// <returns>
    /// The parsed <see cref="Geometry"/> on success, or <c>null</c> if the literal is the keyword <c>null</c>,
    /// if the target type is not spatial, or if this parser chooses not to handle the input.
    /// </returns>
    /// <remarks>
    /// Returning <c>null</c> with <paramref name="parsingException"/> also <c>null</c> indicates that another parser
    /// may attempt to handle the literal. Returning <c>null</c> with a non-null <paramref name="parsingException"/>
    /// signals that the parser recognized the target type/literal but parsing failed.
    /// </remarks>
    public object ParseUriStringToType(string text, IEdmTypeReference targetType, out UriLiteralParsingException parsingException)
    {
        parsingException = null;

        if (text == "null")
        {
            return null;
        }

        IEdmPrimitiveTypeReference primitiveTargetType = targetType == null
                ? null
                : targetType.TypeKind() == EdmTypeKind.Primitive || targetType.TypeKind() == EdmTypeKind.TypeDefinition ? targetType.AsPrimitive() : null;

        if (primitiveTargetType == null)
        {
            return null;
        }

        EdmPrimitiveTypeKind targetTypeKind = primitiveTargetType.PrimitiveKind();

        if (targetTypeKind == EdmPrimitiveTypeKind.Geography || targetTypeKind == EdmPrimitiveTypeKind.Geometry)
        {
            Geometry geometry;
            if (TryUriStringToGeometry(text, out geometry, out parsingException))
            {
                return geometry;
            }
        }

        return null;
    }

    /// <summary>
    /// Tries to parse an OData spatial literal into a NetTopologySuite <see cref="Geometry"/>.
    /// </summary>
    /// <param name="text">The literal text, e.g., <c>geometry'POINT(1 2)'</c> or <c>geography'SRID=4326;POINT(…)'</c>.</param>
    /// <param name="targetValue">Receives the parsed <see cref="Geometry"/> on success.</param>
    /// <param name="parsingException">
    /// Receives a <see cref="UriLiteralParsingException"/> describing the failure if the input is recognized
    /// as a spatial literal but cannot be parsed (e.g., invalid WKT or SRID section). Otherwise <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if parsing succeeded; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// <para>
    /// The method removes the <c>geometry</c>/<c>geography</c> OData literal prefix and surrounding single quotes.
    /// If the remaining text does not start with <c>SRID=…;</c>, a default SRID is injected:
    /// <c>4326</c> for <c>geography</c> and <c>0</c> for <c>geometry</c>.
    /// </para>
    /// <para>
    /// The final string is parsed using <see cref="WKTReader"/> from <c>NetTopologySuite.IO</c>.
    /// </para>
    /// </remarks>
    private static bool TryUriStringToGeometry(string text, out Geometry targetValue, out UriLiteralParsingException parsingException)
    {
        parsingException = null;

        int defaultSrid = 0;

        if (TryRemoveLiteralPrefix(LiteralPrefixGeography, ref text))
        {
            defaultSrid = 4326; // WGS 84
        }
        else if (!TryRemoveLiteralPrefix(LiteralPrefixGeometry, ref text))
        {
            targetValue = default(Geometry);
            return false;
        }

        if (!TryRemoveQuotes(ref text))
        {
            targetValue = default(Geometry);
            return false;
        }

        // According to the OData V4 spec, a spatial literal may include a SRID prefix: 'SRID=4326;POINT(1 2)'.
        if (!text.StartsWith("SRID=", StringComparison.OrdinalIgnoreCase))
        {
            // If no SRID is specified, we assume the default of 4326 for Geography and 0 for Geometry.
            text = $"SRID={defaultSrid};{text}";
        }

        try
        {
            WKTReader wktReader = new WKTReader(NtsGeometryServices.Instance);
            targetValue = wktReader.Read(text);

            return true;
        }
        catch (ParseException ex)
        {
            targetValue = default(Geometry);
            parsingException = new UriLiteralParsingException(
                $"Failed to parse spatial literal '{text}': {ex.Message}");
            
            return false;
        }
    }

    /// <summary>
    /// Attempts to remove a literal <paramref name="prefix"/> (<c>geometry</c> or <c>geography</c>)
    /// from the start of <paramref name="text"/> (case-insensitive).
    /// </summary>
    /// <param name="prefix">The expected literal prefix.</param>
    /// <param name="text">The text to inspect; updated in-place if the prefix is removed.</param>
    /// <returns>
    /// <c>true</c> if the prefix was found and removed; otherwise <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method is tolerant to case (e.g., <c>GeOgRaPhY'…'</c>), matching OData literal rules.
    /// </remarks>
    private static bool TryRemoveLiteralPrefix(string prefix, ref string text)
    {
        Debug.Assert(prefix != null, "prefix != null");

        if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            text = text.Remove(0, prefix.Length);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Removes the leading and trailing single quotes around a literal payload and unescapes doubled quotes.
    /// </summary>
    /// <param name="text">The quoted literal payload; updated in-place on success.</param>
    /// <returns><c>true</c> if quotes were successfully removed and unescaped; otherwise <c>false</c>.</returns>
    /// <remarks>
    /// Validates that the string is at least two characters long, begins and ends with a single quote,
    /// and that any internal quote characters are doubled (OData single-quote escaping). Returns <c>false</c>
    /// if the quoted form is invalid.
    /// </remarks>
    private static bool TryRemoveQuotes(ref string text)
    {
        Debug.Assert(text != null, "text != null");

        if (text.Length < 2)
        {
            return false;
        }

        char quote = text[0];
        if (quote != '\'' || text[text.Length - 1] != quote)
        {
            return false;
        }

        string s = text.Substring(1, text.Length - 2);
        int start = 0;
        while (true)
        {
            int i = s.IndexOf(quote, start);
            if (i < 0)
            {
                break;
            }

            s = s.Remove(i, 1);
            if (s.Length < i + 1 || s[i] != quote)
            {
                return false;
            }

            start = i + 1;
        }

        text = s;
        return true;
    }
}
