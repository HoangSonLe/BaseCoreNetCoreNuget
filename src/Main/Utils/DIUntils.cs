using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;

namespace BaseNetCore.Core.src.Main.Utils
{
    public static class DIUntils
    {
        public static IServiceCollection AddAutoRegisterDI<T>(this IServiceCollection services)
        {
            if (services is null) throw new ArgumentNullException(nameof(services));

            var interfaceType = typeof(T);

            // Helper to avoid ReflectionTypeLoadException
            static Type[] GetLoadableTypes(Assembly assembly)
            {
                try { return assembly.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t != null)!.ToArray(); }
            }

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            var implType = assemblies
                .SelectMany(GetLoadableTypes)
                .Where(t => t is not null && t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t))
                .FirstOrDefault();

            if (implType != null)
            {
                // Register only if no existing registration for the interface
                services.TryAddScoped(interfaceType, implType);
            }

            return services;
        }
    }
}
