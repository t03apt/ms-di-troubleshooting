using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        public static void Main()
        {
            void TryExecute(Action action)
            {
                try
                {
                    Console.WriteLine($"Executing: {action.Method.Name}");
                    action();
                    Console.WriteLine($"Done: {action.Method.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed: {action.Method.Name} Error:{ex.Message}");
                }
            }

            var unitTest = new UnitTest1();
            TryExecute(unitTest.CanResolveINotificationHandlerOfINotification);
            TryExecute(unitTest.CanResolveINotificationHandlerOfPinged);
            TryExecute(unitTest.CanResolveINotificationHandlerOfPonged);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfINotification()
        {
            ServiceProvider provider = BuildServiceProvider();
            GetRequiredServices(provider, typeof(INotificationHandler<INotification>));
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPinged()
        {
            ServiceProvider provider = BuildServiceProvider();
            GetRequiredServices(provider, typeof(INotificationHandler<Pinged>));
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPonged()
        {
            ServiceProvider provider = BuildServiceProvider();
            GetRequiredServices(provider, typeof(INotificationHandler<Ponged>));
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();

            services.AddScoped(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>));
            var provider = services.BuildServiceProvider();
            return provider;
        }

        private static IEnumerable<object> GetRequiredServices(IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>)provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }

        interface INotification { }

        interface INotificationHandler<in TNotification>
            where TNotification : INotification
        {
            void Handle(TNotification notification);
        }

        class Pinged : INotification { }

        class Ponged : INotification { }

        class PingedHandler : INotificationHandler<Pinged>
        {
            public void Handle(Pinged notification) { }
        }

        class PongedHandler : INotificationHandler<Ponged>
        {
            public void Handle(Ponged notification) { }
        }

        class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
            where TNotification : Pinged
        {
            public void Handle(TNotification notification) { }
        }
    }
}
