namespace OData2Linq.Settings
{
    using Microsoft.AspNetCore.OData.Query.Validator;
    using Microsoft.OData.UriParser;
    using System;

    public class ODataSettings
    {
        private static readonly object SyncObj = new();

        private static Action<ODataSettings>? Initializer = null;

        /// <summary>
        /// Sets the action which will be used to initialize every instance of <type ref="ODataSettings"></type>.
        /// </summary>
        /// <param name="initializer">The action which will be used to initialize every instance of <type ref="ODataSettings"></type>.</param>
        /// <exception cref="ArgumentNullException">initializer</exception>
        /// <exception cref="InvalidOperationException">SetInitializer</exception>
        public static void SetInitializer(Action<ODataSettings> initializer)
        {
            ArgumentNullException.ThrowIfNull(initializer);

            if (Initializer == null)
            {
                lock (SyncObj)
                {
                    if (Initializer == null)
                    {
                        Initializer = initializer;
                        return;
                    }
                }
            }

            throw new InvalidOperationException($"{nameof(SetInitializer)} method can be invoked only once");
        }

        public ODataQuerySettingsHashable QuerySettings { get; } = new ODataQuerySettingsHashable { PageSize = 20 };

        public ODataValidationSettings ValidationSettings { get; } = new ODataValidationSettings();

        public ODataUriParserSettings ParserSettings { get; } = new ODataUriParserSettings();

        public ODataUriResolver Resolver { get; set; } = new StringAsEnumResolver { EnableCaseInsensitive = true };

        public bool EnableCaseInsensitive
        {
            get => Resolver.EnableCaseInsensitive;
            set => Resolver.EnableCaseInsensitive = value;
        }

        public DefaultQueryConfigurationsHashable DefaultQueryConfigurations { get; } = new DefaultQueryConfigurationsHashable
        {
            EnableFilter = true,
            EnableOrderBy = true,
            EnableExpand = true,
            EnableSelect = true,
            MaxTop = 100
        };

        public ODataSettings()
        {
            Initializer?.Invoke(this);
        }
    }
}