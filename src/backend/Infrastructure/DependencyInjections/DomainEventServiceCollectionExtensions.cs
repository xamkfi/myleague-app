using Domain.DomainEvents;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MyLeague.Infrastructure.DomainEvents;
using MyLeague.Infrastructure.DomainEvents.Handlers;
using MyLeague.Infrastructure.SignalR;
using System.Reflection;
using Microsoft.AspNetCore.Routing;

namespace MyLeague.Infrastructure.DependencyInjections
{
    /// <summary>
    /// Extension methods for setting up domain event services in an IServiceCollection
    /// </summary>
    public static class DomainEventServiceCollectionExtensions
    {
        /// <summary>
        /// Adds domain event services to the specified IServiceCollection
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to</param>
        /// <returns>The IServiceCollection so that additional calls can be chained</returns>
        public static IServiceCollection AddDomainEvents(this IServiceCollection services)
        {
            // Register domain event dispatcher
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            
            // Register SignalR services
            services.AddSignalR();
            services.AddScoped<DomainEventNotifier>();
            
            // Register domain event handlers
            // Find and register all classes that implement IDomainEventHandler<T>
            Assembly assembly = typeof(DomainEventServiceCollectionExtensions).Assembly;

            IEnumerable<Type> handlerTypes = assembly.GetTypes()
                 .Where(type => !type.IsAbstract && !type.IsInterface)
                 .Where(type => type.GetInterfaces()
                 .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)));

            foreach (Type handlerType in handlerTypes)
            {
                foreach (Type implementedInterface in handlerType.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDomainEventHandler<>)))
                {
                    services.AddScoped(implementedInterface, handlerType);
                }
            }
            
            return services;
        }
    }
} 
