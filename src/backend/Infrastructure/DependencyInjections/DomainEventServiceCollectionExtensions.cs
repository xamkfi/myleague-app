using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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
            
            // Register SignalR services
            services.AddSignalR();
            services.AddScoped<DomainEventNotifier>();
            
            // Register notification services
            services.AddScoped<INotificationSender, SignalRNotificationSender>();
            

            return services;
        }
    }
} 
