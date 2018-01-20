using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetDI
{
    [TestClass]
    public partial class MicrosoftDITest
    {
        [TestMethod]
        public void CanResolveINotificationHandlerOfINotification()
        {
            ServiceProvider provider = BuildServiceProvider();
            var handlers = GetRequiredServices<INotificationHandler<INotification>>(provider);
            var message = new Pinged();
            Publish(handlers, message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPinged()
        {
            ServiceProvider provider = BuildServiceProvider();
            var handlers = GetRequiredServices<INotificationHandler<Pinged>>(provider);
            var message = new Pinged();
            Publish(handlers, message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPonged()
        {
            ServiceProvider provider = BuildServiceProvider();
            var handlers = GetRequiredServices<INotificationHandler<Ponged>>(provider);
            var message = new Ponged();
            Publish(handlers, message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfSpecialPinged()
        {
            ServiceProvider provider = BuildServiceProvider();
            var handlers = GetRequiredServices<INotificationHandler<SpecialPinged>>(provider);
            var message = new SpecialPinged();
            Publish(handlers, message);
        }

        private static void Publish<T>(IEnumerable<INotificationHandler<T>> handlers, T message) where T: INotification
        {
            handlers.ToList().ForEach(o => o.Handle(message));
        }

        private void TraceHandlers(IEnumerable<object> handlers)
        {
            Console.WriteLine(string.Join(" ", handlers));
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.RegisterNotificationHandlers(Assembly.GetExecutingAssembly());
            var provider = services.BuildServiceProvider();
            return provider;
        }

        private static IEnumerable<T> GetRequiredServices<T>(IServiceProvider provider)
        {
            return (IEnumerable<T>)provider.GetRequiredService(typeof(IEnumerable<T>));
        }

        public interface INotification { }

        public interface INotificationHandler<in TNotification>
            where TNotification : INotification
        {
            void Handle(TNotification notification);
        }

        public class Pinged : INotification { }

        public class Ponged : INotification { }

        public class SpecialPinged : Pinged { }

        public class GenericHandler : INotificationHandler<INotification>
        {
            public void Handle(INotification notification) { Console.WriteLine(nameof(GenericHandler)); }
        }

        public class PingedHandler : INotificationHandler<Pinged>
        {
            public void Handle(Pinged notification) { Console.WriteLine(nameof(PingedHandler)); }
        }

        public class SpecialPingedHandler : INotificationHandler<SpecialPinged>
        {
            public void Handle(SpecialPinged notification) { Console.WriteLine(nameof(SpecialPingedHandler)); }
        }

        public class PongedHandler : INotificationHandler<Ponged>
        {
            public void Handle(Ponged notification) { Console.WriteLine(nameof(PongedHandler)); }
        }

        public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
            where TNotification : Pinged
        {
            public void Handle(TNotification notification) { Console.WriteLine(nameof(ConstrainedPingedHandler<TNotification>)); }
        }
    }
}
