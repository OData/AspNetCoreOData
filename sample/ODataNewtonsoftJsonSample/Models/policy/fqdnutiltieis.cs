namespace Microsoft.Naas.Infra.Utilities.FqdnUtilities;

using System.Text.RegularExpressions;

public static class FqdnUtilities
{
    private static readonly string _domainNameLabel = "(?!-)[a-zA-Z0-9\\-]{1,63}(?<!-)";
    private static readonly string _domainNameLabelWithDot = $"(?:{_domainNameLabel}\\.)";
    private static readonly string _wildcardDomainNameLabelWithDot = $"((\\*\\.)|{_domainNameLabelWithDot})";

    private static readonly string _topLevelDomain = "(?:[a-zA-Z]{2,63})";

    // regular expression matching an fqdn according to https://www.rfc-editor.org/rfc/rfc1035 and https://www.rfc-editor.org/rfc/rfc3696#section-2
    // ex. contoso.com, www.contoso.com, www.contoso.prime.com, www.con-toso.com etc.
    // OR a wildcard fqdn when the first domain *
    // ex. *.contoso.com
    private static readonly Regex _fqdnRegex = new Regex($"(?=^.{{1,254}}$)^{_wildcardDomainNameLabelWithDot}({_domainNameLabelWithDot}*{_topLevelDomain}$)");

    // regular expression matching a single word domain name. ex hrweb, localhost etc.
    private static readonly Regex _singleDomainNameLabelRegex = new Regex($"(?:^{_domainNameLabel}$)");

    public static bool IsValidFqdn(string fqdn, bool allowSingleWordDomain = false) =>
        _fqdnRegex.Match(fqdn).Success ||
        (allowSingleWordDomain && _singleDomainNameLabelRegex.Match(fqdn).Success);
}
