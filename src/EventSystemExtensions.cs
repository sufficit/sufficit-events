using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
        // Core implementation used by public overloads - only processes the provided assemblies
        // If assemblies is null or empty, it does not scan the AppDomain.
        public static IServiceCollection AddSufficitEvents(this IServiceCollection services, params Assembly[] assemblies)
        {
            // Register EventBus only if not already registered to keep this method idempotent
            services.TryAddSingleton<IEventBus, EventBus>();

            if (assemblies == null || assemblies.Length == 0)
                return services; // no assemblies to scan

            foreach (var assembly in assemblies)
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
                    // Use TryAddEnumerable to avoid duplicate handler registrations when this
                    // method is called multiple times (idempotent registration).
                    var descriptor = ServiceDescriptor.Transient(h.Service, h.Implementation);
                    services.TryAddEnumerable(new[] { descriptor });
                }
            }

            return services;
        }

        /// <summary>
        /// Convenience overload: supply an assembly name filter (e.g. 'Sufficit') which will
        /// be used to select AppDomain assemblies and register handlers from them.
        /// </summary>
        public static IServiceCollection AddSufficitEvents(this IServiceCollection services, string assemblyNameFilter = nameof(Sufficit))
        {
            var scanned = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a =>
                {
                    var name = a.GetName()?.Name;
                    return !string.IsNullOrEmpty(name) && name.IndexOf(assemblyNameFilter ?? string.Empty, StringComparison.OrdinalIgnoreCase) >= 0;
                })
                .ToArray();

            return services.AddSufficitEvents(scanned);
        }
    }
}
