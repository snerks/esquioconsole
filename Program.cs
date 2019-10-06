using Esquio.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var services = new ServiceCollection()
                .AddLogging(configure => configure.AddConsole())
                .AddEsquio()
                .AddConfigurationStore(configuration, "Esquio")
                .Services;

            // services.AddSingleton<IRuntimeFeatureStore, Esquio.Configuration.Store.ConfigurationFeatureStore>();

            var serviceProvider = services.BuildServiceProvider();

            var featureService = serviceProvider.GetService<IFeatureService>();

            var currentBackgroundColor = Console.BackgroundColor;

            if (await featureService.IsEnabledAsync("Colored", "Console"))
            {
                Console.BackgroundColor = ConsoleColor.Blue;
            }
            // else
            // {
            //     Console.BackgroundColor = ConsoleColor.Black;
            // }

            Console.WriteLine("Welcome to Esquio!");
            Console.Read();

            Console.BackgroundColor = currentBackgroundColor;
        }
    }
}


namespace Microsoft.Extensions.DependencyInjection
{
    using Esquio.Abstractions;
    using Esquio.Configuration.Store;
    using Esquio.Configuration.Store.Configuration;
    using Esquio.DependencyInjection;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Provides Esquio extensions methods for <see cref="IEsquioBuilder"/>
    /// </summary>
    public static class ConfigurationFeatureStoreExtensions
    {
        private const string DefaultSectionName = "Esquio";

        /// <summary>
        /// Add Esquio configuration using <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IEsquioBuilder"/> used.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> when the products, features and toggles are configured.</param>
        /// <param name="key">The configuration section key to use.[Optional] default value is Esquio.</param>
        /// <returns>A new <see cref="IEsquioBuilder"/> that can be chained for register services.</returns>
        public static IEsquioBuilder AddConfigurationStore(this IEsquioBuilder builder, IConfiguration configuration, string key = DefaultSectionName)
        {
            builder.Services
                .AddOptions()
                .Configure<EsquioConfiguration>(configuration.GetSection(key))
                .AddScoped<IRuntimeFeatureStore, ConfigurationFeatureStore>();

            return builder;
        }
    }
}

