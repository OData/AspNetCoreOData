using OData2Linq.Settings;
using Xunit;

namespace OData2Linq.Tests
{
    public class HashTests
    {
        [Fact]
        public void Hashes()
        {
            ODataSettings s1 = new ODataSettings();
            ODataSettings s2 = new ODataSettings();

            Assert.Equal(s1.QuerySettings.GetHashCode(), s2.QuerySettings.GetHashCode());
            Assert.Equal(s1.DefaultQueryConfigurations.GetHashCode(), s2.DefaultQueryConfigurations.GetHashCode());
            Assert.Equal(HashCode.Combine(s1.QuerySettings, s1.DefaultQueryConfigurations), HashCode.Combine(s2.QuerySettings, s2.DefaultQueryConfigurations));

            s1.QuerySettings.EnsureStableOrdering = false;
            Assert.NotEqual(s1.QuerySettings.GetHashCode(), s2.QuerySettings.GetHashCode());
            Assert.Equal(s1.DefaultQueryConfigurations.GetHashCode(), s2.DefaultQueryConfigurations.GetHashCode());
            Assert.NotEqual(HashCode.Combine(s1.QuerySettings, s1.DefaultQueryConfigurations), HashCode.Combine(s2.QuerySettings, s2.DefaultQueryConfigurations));

            s1.QuerySettings.EnsureStableOrdering = true;
            Assert.Equal(s1.QuerySettings.GetHashCode(), s2.QuerySettings.GetHashCode());
            Assert.Equal(s1.DefaultQueryConfigurations.GetHashCode(), s2.DefaultQueryConfigurations.GetHashCode());
            Assert.Equal(HashCode.Combine(s1.QuerySettings, s1.DefaultQueryConfigurations), HashCode.Combine(s2.QuerySettings, s2.DefaultQueryConfigurations));

            s1.DefaultQueryConfigurations.EnableExpand = false;
            Assert.Equal(s1.QuerySettings.GetHashCode(), s2.QuerySettings.GetHashCode());
            Assert.NotEqual(s1.DefaultQueryConfigurations.GetHashCode(), s2.DefaultQueryConfigurations.GetHashCode());
            Assert.NotEqual(HashCode.Combine(s1.QuerySettings, s1.DefaultQueryConfigurations), HashCode.Combine(s2.QuerySettings, s2.DefaultQueryConfigurations));

            s1.DefaultQueryConfigurations.EnableExpand = true;
            Assert.Equal(s1.QuerySettings.GetHashCode(), s2.QuerySettings.GetHashCode());
            Assert.Equal(s1.DefaultQueryConfigurations.GetHashCode(), s2.DefaultQueryConfigurations.GetHashCode());
            Assert.Equal(HashCode.Combine(s1.QuerySettings, s1.DefaultQueryConfigurations), HashCode.Combine(s2.QuerySettings, s2.DefaultQueryConfigurations));

            // Microsoft classes public properties            
            Assert.Equal(HashCode.Combine(s1.ParserSettings.MaximumExpansionCount, s1.ParserSettings.MaximumExpansionDepth), HashCode.Combine(s2.ParserSettings.MaximumExpansionCount, s2.ParserSettings.MaximumExpansionDepth));
            s1.ParserSettings.MaximumExpansionCount = 1;
            Assert.NotEqual(HashCode.Combine(s1.ParserSettings.MaximumExpansionCount, s1.ParserSettings.MaximumExpansionDepth), HashCode.Combine(s2.ParserSettings.MaximumExpansionCount, s2.ParserSettings.MaximumExpansionDepth));
        }
    }
}
