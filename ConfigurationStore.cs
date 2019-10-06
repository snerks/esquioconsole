using Esquio.Abstractions;
using Esquio.Configuration.Store.Configuration;
// using Esquio.Configuration.Store.Diagnostics;
using Esquio.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Esquio.Configuration.Store.Configuration
{
    internal class EsquioConfiguration
    {
        public EsquioConfiguration()
        {

        }

        public ProductConfiguration[] Products { get; set; }
    }

    internal class ProductConfiguration
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public FeatureConfiguration[] Features { get; set; } = new FeatureConfiguration[0];

        internal Product To()
        {
            return new Product(Name, Description);
        }
    }
}

namespace Esquio.Configuration.Store.Configuration
{
    internal class FeatureConfiguration
    {
        public string Name { get; set; }
        public bool Enabled { get; set; }
        public ToggleConfiguration[] Toggles { get; set; } = new ToggleConfiguration[] { };
        public Feature To()
        {
            var feature = new Feature(Name);

            if (Enabled)
            {
                feature.Enabled();
            }
            else
            {
                feature.Disabled();
            }

            foreach (var toggleConfiguration in Toggles)
            {
                var toggle = new Toggle(toggleConfiguration.Type);
                var configuredParameters = toggleConfiguration.Parameters;

                toggle.AddParameters(configuredParameters.Select(p => new Parameter(p.Key, p.Value)));
                feature.AddToggle(toggle);
            }

            return feature;
        }
    }
}

namespace Esquio.Configuration.Store.Configuration
{
    internal class ToggleConfiguration
    {
        public string Type { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
    }
}

namespace Esquio.Configuration.Store
{
    internal class ConfigurationFeatureStore
        : IRuntimeFeatureStore
    {
        private readonly ILogger<ConfigurationFeatureStore> _logger;
        private readonly IOptionsSnapshot<EsquioConfiguration> _options;

        public ConfigurationFeatureStore(IOptionsSnapshot<EsquioConfiguration> options, ILogger<ConfigurationFeatureStore> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<Feature> FindFeatureAsync(string featureName, string productName, CancellationToken cancellationToken = default)
        {
            var feature = GetFeatureFromConfiguration(featureName, productName);

            if (feature != null)
            {
                return Task.FromResult(feature.To());
            }

            // Log.FeatureNotExist(_logger, featureName, productName);
            return Task.FromResult<Feature>(null);
        }

        private FeatureConfiguration GetFeatureFromConfiguration(string featureName, string productName)
        {
            // Log.FindFeature(_logger, featureName, productName ?? EsquioConstants.DEFAULT_PRODUCT_NAME);

            var product =
                _options?
                .Value?
                .Products?
                .FirstOrDefault(a =>
                    a.Name.Equals(
                        productName ??
                        EsquioConstants.DEFAULT_PRODUCT_NAME,
                        StringComparison.InvariantCultureIgnoreCase));

            if (product != null)
            {
                return product
                    .Features
                    .SingleOrDefault(f =>
                        f.Name.Equals(
                            featureName,
                            StringComparison.InvariantCultureIgnoreCase));
            }

            return null;
        }
    }
}