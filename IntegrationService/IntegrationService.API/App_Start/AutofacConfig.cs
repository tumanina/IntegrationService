using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using ApacKernel;
using ApacKernel.AspNet.WebApi.Clients;
using Autofac;
using Autofac.Integration.WebApi;
using IntegrationService.EventHubProcessing;
using IntegrationService.EventHubProcessing.Consumers.Product;
using IntegrationService.EventHubProcessing.Consumers.Sku;
using IntegrationService.EventHubProcessing.Senders;
using Admin.Client;
using IntegrationService.API.ConfigurationSections;
using ApiVersionV1 = IntegrationService.Business;
using IntegrationService.Business.Clients.Admin;
using Taxonomy.Client;
using Owin;
using EventHubSender = IntegrationService.EventHubProcessing.EventHubSender;

namespace IntegrationService.API
{
    public static class AutofacConfig
    {
        public static void Register(IAppBuilder app, HttpConfiguration configuration)
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.RegisterWebApiFilterProvider(configuration);
            ConfigureDependencies(builder);
            var container = builder.Build();
            configuration.DependencyResolver = new AutofacWebApiDependencyResolver(container);
            app.UseAutofacMiddleware(container);

            var listeners = container.Resolve<IEnumerable<IEventHubListener>>();
            foreach (var listener in listeners)
            {
                listener.Register().Wait();
            }
        }

        private static void ConfigureDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<AdminApiClient>().As<IAdminApiClient>().SingleInstance();
            builder.RegisterType<TaxonomyApiClient>().As<ITaxonomyApiClient>().SingleInstance();

            var taxonomyTrees = ConfigurationManager.GetSection("taxonomy") as TaxonomySection;

            var trees = new Dictionary<string, Guid>();
            for (var i = 0; i < taxonomyTrees.TaxonomyTrees.Count; i++)
            {
                var tree = taxonomyTrees.TaxonomyTrees[i];
                trees.Add(tree.CountryId, new Guid(tree.TreeId));
            }

            builder.RegisterType<ApiVersionV1.ProductService>().As<ApiVersionV1.IProductService>()
                .WithParameter("taxonomyTrees", trees)
                .SingleInstance();

            builder.RegisterType<ApiVersionV1.ProductService>().As<ApiVersionV1.IProductService>()
                .WithParameter("taxonomyTrees", trees)
                .SingleInstance();
            
            builder.RegisterType<ProductCreateConsumer>().As<IProductEventConsumer>().SingleInstance();
            builder.RegisterType<ProductChangeConsumer>().As<IProductEventConsumer>().SingleInstance();
            builder.RegisterType<ProductDeleteConsumer>().As<IProductEventConsumer>().SingleInstance();

            builder.RegisterType<SkuChangeConsumer>().As<ISkuEventConsumer>().SingleInstance();
            builder.RegisterType<SkuCreateConsumer>().As<ISkuEventConsumer>().SingleInstance();
            builder.RegisterType<SkuDeleteConsumer>().As<ISkuEventConsumer>().SingleInstance();

            builder.RegisterType<AdminRestClient>().As<IAdminRestClient>()
                .WithParameter("settings", new JsonClientSettings("AdminApiConfig"))
                .SingleInstance();
            
            builder.RegisterType<TaxonomyClient>().As<ITaxonomyClient>()
                .WithParameter("settings", new JsonClientSettings("TaxonomyApiConfig"))
                .SingleInstance();

            ConfigureEventHub(builder);
        }

        private static void ConfigureEventHub(ContainerBuilder builder)
        {
            var eventHubProductParameters = new NamedParameter[]
            {
                new NamedParameter("eventHubPath", ApacConfig.AppSettings.GetStringSetting("EventHubPath")),
                new NamedParameter(
                    "eventHubConsumerGroup",
                    ApacConfig.AppSettings.GetStringSetting("EventHubConsumerGroup")),
                new NamedParameter(
                    "eventHubConnectionString",
                    ApacConfig.ConnectionStrings.GetString("EventHubConnectionString")),
                new NamedParameter(
                    "storageConnectionString",
                    ApacConfig.ConnectionStrings.GetString("BlobStorageConnectionString"))
            };

            builder.RegisterType<EventHubListener<IProductEventConsumer, IProductEventSender>>()
                .As<IEventHubListener>()
                .WithParameters(eventHubProductParameters).SingleInstance();

            var senders = ConfigurationManager.GetSection("events") as EventsSection;

            if (senders != null)
            {
                for (var i = 0; i < senders.Senders.Count; i++)
                {
                    var sender = senders.Senders[i];

                    var countryIds = (string.IsNullOrEmpty(sender.CountryIds)) ? new List<string>() : sender.CountryIds.Split(',').Select(t => t.Trim());

                    var invalid = countryIds.Any(t => t.Length != 2);

                    if (invalid)
                    {
                        throw new ConfigurationErrorsException($"CountryIds sender configuration '{sender.CountryIds}' is wrong. Example of correct configuration: 'AS, TA, NZ'.");
                    }

                    if (!string.IsNullOrEmpty(sender.ProductConnectionString))
                    {
                        builder.RegisterType<ProductEventSender>()
                            .As<IProductEventSender>()
                            .WithParameter("eventHubSender", new EventHubSender(sender.ProductConnectionString))
                            .WithParameter("countryIds", countryIds)
                            .SingleInstance();
                    }
                }
            }
        }
    }
}