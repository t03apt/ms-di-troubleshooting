using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetDI
{
    [TestClass]
    public partial class MicrosoftDITest
    {
        static TextWriter _writer;
        static ServiceProvider _serviceProvider;

        [TestInitialize()]
        public void TestInitialize()
        {
            _serviceProvider = BuildServiceProvider();
            _writer = new StringWriter();
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }

        private void AssertHandlersCalled<TMessage>(TMessage message) where TMessage : INotification
        {
            var handlers = _expectations.Where(o => o.Value.Contains(typeof(TMessage))).Select(o => o.Key);
            var result = _writer.ToString();
            foreach (var handler in handlers)
            {
                if (!result.Contains(handler.ToString()))
                {
                    throw new InvalidOperationException($"Handler not called: {handler}");
                }
            }
        }

        private static Dictionary<Type, List<Type>> _expectations = new Dictionary<Type, List<Type>>
        {
            {
                typeof(GenericHandler), new List<Type>{
                    typeof(INotification),
                    typeof(Pinged),
                    typeof(Ponged),
                    typeof(SpecialPinged)
                }
            },
            {
                typeof(PingedHandler), new List<Type>{
                    typeof(Pinged),
                    typeof(SpecialPinged),
                }
            },
            {
                typeof(PongedHandler), new List<Type>{
                    typeof(Ponged),
                }
            },
            {
                typeof(SpecialPingedHandler), new List<Type>{
                    typeof(SpecialPinged)
                }
            },
            //{
            //    typeof(ConstrainedPingedHandler<>), new List<Type>{
            //        typeof(Pinged),
            //        typeof(SpecialPinged),
            //    }
            //},
        };

        [TestMethod]
        public void CanResolveINotificationHandlerOfINotification()
        {
            var handlers = GetRequiredServices<INotificationHandler<INotification>>(_serviceProvider);
            var message = new Pinged();
            Publish(handlers, message);
            AssertHandlersCalled<INotification>(message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPinged()
        {
            var message = new Pinged();
            PublishMessage(message);
            AssertHandlersCalled(message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfPonged()
        {
            var message = new Ponged();
            PublishMessage(message);
            AssertHandlersCalled(message);
        }

        [TestMethod]
        public void CanResolveINotificationHandlerOfSpecialPinged()
        {
            var message = new SpecialPinged();
            PublishMessage(message);
            AssertHandlersCalled(message);
        }

        private static void PublishMessage<TMessage>(TMessage message) where TMessage : INotification
        {
            var handlers = GetRequiredServices<INotificationHandler<TMessage>>(_serviceProvider);
            Publish(handlers, message);
        }

        private static void Publish<T>(IEnumerable<INotificationHandler<T>> handlers, T message) where T : INotification
        {
            handlers.ToList().ForEach(o => o.Handle(message));
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.RegisterNotificationHandlers(Assembly.GetExecutingAssembly());
            var provider = services.BuildServiceProvider();
            return provider;
        }

        private static void HandleNotification(object handler, INotification notification)
        {
            _writer.WriteLine(handler.GetType().ToString());
            Console.WriteLine(handler.GetType());
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
            public void Handle(INotification notification) => HandleNotification(this, notification);
        }

        public class PingedHandler : INotificationHandler<Pinged>
        {
            public void Handle(Pinged notification) => HandleNotification(this, notification);
        }

        public class SpecialPingedHandler : INotificationHandler<SpecialPinged>
        {
            public void Handle(SpecialPinged notification) => HandleNotification(this, notification);
        }

        public class PongedHandler : INotificationHandler<Ponged>
        {
            public void Handle(Ponged notification) => HandleNotification(this, notification);
        }

        public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
            where TNotification : Pinged
        {
            public void Handle(TNotification notification) => HandleNotification(this, notification);
        }
    }
}
