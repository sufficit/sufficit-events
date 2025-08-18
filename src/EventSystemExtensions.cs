using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Sufficit.Events
{
    public static class EventSystemExtensions
    {
    /// <summary>
    /// Registers the <see cref="EventBus"/> as a singleton implementation of <see cref="IEventBus"/>
    /// and scans the provided assemblies for implementations of <see cref="IEventHandler{TEvent}"/> to
    /// register them as transient services.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="assemblies">Optional assemblies to scan; if none are provided the current AppDomain assemblies are scanned.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance to allow chaining.</returns>
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
