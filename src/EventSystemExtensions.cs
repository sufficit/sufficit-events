using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Sufficit.Events
{
    public static class EventSystemExtensions
    {
        /// <summary>
        /// Registers the EventBus and scans assemblies for implementations of IEventHandler{T}.
        /// If no assemblies are provided, scans AppDomain.CurrentDomain.GetAssemblies().
        /// </summary>
        public static IServiceCollection AddEventSystem(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddSingleton<IEventBus, EventBus>();

            var scanned = (assemblies != null && assemblies.Length > 0) ? assemblies : AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in scanned)
            {
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch { continue; }

                var handlers = types
                    .Where(t => !t.IsAbstract && !t.IsInterface)
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEventHandler<>))
                        .Select(i => new { Service = i, Implementation = t }));

                foreach (var h in handlers)
                {
                    services.AddTransient(h.Service, h.Implementation);
                }
            }

            return services;
        }
    }
}
