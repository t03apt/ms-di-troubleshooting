using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using static AspNetDI.MicrosoftDITest;

namespace AspNetDI
{
    public static class ServiceCollectionExtensions
    {
        public static void RegisterNotificationHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            RegisterNotificationHandlersInternal(services, assemblies);
        }

        public static void RegisterRequestHandlers(this IServiceCollection services, params Assembly[] assemblies)
        {
            RegisterRequestHandlersInternal(services, assemblies);
        }

        public static void RegisterRequestProcessors(this IServiceCollection services, params Assembly[] assemblies)
        {
            RegisterRequestProcessorsInternal(services, assemblies);
        }

        private static void RegisterNotificationHandlersInternal(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            var openNotificationHandlerInterfaces = new[]
            {
                typeof(INotificationHandler<>),
            };
            AddInterfacesAsTransient(openNotificationHandlerInterfaces, services, assembliesToScan, true);
        }

        private static void RegisterRequestHandlersInternal(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            var openRequestInterfaces = new Type[]
            {
                //typeof(IRequestHandler<,>),
                //typeof(IRequestHandler<>)
            };
            AddInterfacesAsTransient(openRequestInterfaces, services, assembliesToScan, false);
        }

        private static void RegisterRequestProcessorsInternal(IServiceCollection services, IEnumerable<Assembly> assembliesToScan)
        {
            var multiOpenInterfaces = new Type[]
            {
                //typeof(IRequestPreProcessor<>),
                //typeof(IRequestPostProcessor<,>)
            };

            foreach (var multiOpenInterface in multiOpenInterfaces)
            {
                var concretions = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.ExportedTypes))
                {
                    IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(multiOpenInterface).ToArray();
                    if (!interfaceTypes.Any()) continue;

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }
                }

                concretions
                    .Where(HasNoConstraints)
                    .ToList()
                    .ForEach(c => services.AddTransient(multiOpenInterface, c));
            }
        }

        private static void AddInterfacesAsTransient(Type[] openRequestInterfaces,
            IServiceCollection services,
            IEnumerable<Assembly> assembliesToScan,
            bool addIfAlreadyExists)
        {
            foreach (var openInterface in openRequestInterfaces)
            {
                var concretions = new List<Type>();
                var interfaces = new List<Type>();

                foreach (var type in assembliesToScan.SelectMany(a => a.ExportedTypes))
                {
                    IEnumerable<Type> interfaceTypes = type.FindInterfacesThatClose(openInterface).ToArray();
                    if (!interfaceTypes.Any()) continue;

                    if (type.IsConcrete())
                    {
                        concretions.Add(type);
                    }

                    foreach (Type interfaceType in interfaceTypes)
                    {
                        interfaces.Fill(interfaceType);
                    }
                }

                foreach (var @interface in interfaces)
                {
                    var matches = concretions
                        .Where(HasNoConstraints)
                        .Where(t => t.CanBeCastTo(@interface))
                        .ToList();
                    if (addIfAlreadyExists)
                    {
                        matches
                            .ForEach(match => services.AddTransient(@interface, match));
                    }
                    else
                    {
                        matches
                            .ForEach(match => services.TryAddTransient(@interface, match));
                    }

                    if (!@interface.IsOpenGeneric())
                    {
                        AddConcretionsThatCouldBeClosed(@interface, concretions, services);
                    }
                }
            }
        }

        private static void AddConcretionsThatCouldBeClosed(Type @interface, List<Type> concretions, IServiceCollection services)
        {
            foreach (var type in concretions
                .Where(x => x.IsOpenGeneric() && x.CouldCloseTo(@interface)))
            {
                try
                {
                    services.TryAddTransient(@interface, type.MakeGenericType(@interface.GenericTypeArguments));
                }
                catch (Exception)
                {
                }
            }
        }

        private static bool CouldCloseTo(this Type openConcretion, Type closedInterface)
        {
            var openInterface = closedInterface.GetGenericTypeDefinition();
            var arguments = closedInterface.GenericTypeArguments;

            var concreteArguments = openConcretion.GenericTypeArguments;
            return arguments.Length == concreteArguments.Length && openConcretion.CanBeCastTo(openInterface);
        }

        private static bool CanBeCastTo(this Type pluggedType, Type pluginType)
        {
            if (pluggedType == null) return false;

            if (pluggedType == pluginType) return true;

            return pluginType.GetTypeInfo().IsAssignableFrom(pluggedType.GetTypeInfo());
        }

        private static bool IsOpenGeneric(this Type type)
        {
            return type.GetTypeInfo().IsGenericTypeDefinition || type.GetTypeInfo().ContainsGenericParameters;
        }

        private static IEnumerable<Type> FindInterfacesThatClose(this Type pluggedType, Type templateType)
        {
            if (!pluggedType.IsConcrete()) yield break;

            if (templateType.GetTypeInfo().IsInterface)
            {
                foreach (
                    var interfaceType in
                        pluggedType.GetTypeInfo().ImplementedInterfaces
                            .Where(type => type.GetTypeInfo().IsGenericType && (type.GetGenericTypeDefinition() == templateType)))
                {
                    yield return interfaceType;
                }
            }
            else if (pluggedType.GetTypeInfo().BaseType.GetTypeInfo().IsGenericType &&
                     (pluggedType.GetTypeInfo().BaseType.GetGenericTypeDefinition() == templateType))
            {
                yield return pluggedType.GetTypeInfo().BaseType;
            }

            if (pluggedType == typeof(object)) yield break;
            if (pluggedType.GetTypeInfo().BaseType == typeof(object)) yield break;

            foreach (var interfaceType in FindInterfacesThatClose(pluggedType.GetTypeInfo().BaseType, templateType))
            {
                yield return interfaceType;
            }
        }

        private static bool IsConcrete(this Type type)
        {
            return !type.GetTypeInfo().IsAbstract && !type.GetTypeInfo().IsInterface;
        }

        private static void Fill<T>(this IList<T> list, T value)
        {
            if (list.Contains(value)) return;
            list.Add(value);
        }

        private static bool HasNoConstraints(Type type)
        {
            // MS IServiceCollection does not support constrained open generic types.
            // See: https://github.com/aspnet/DependencyInjection/issues/471 Support constrained open generic types
            var ret = !(type.ContainsGenericParameters && type.GetGenericArguments().Any(p => p.GetGenericParameterConstraints().Any()));
            return ret;
        }
    }
}