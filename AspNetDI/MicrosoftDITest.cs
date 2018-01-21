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
        static List<Type> _handlersCalled;
        static ServiceProvider _serviceProvider;

        [TestInitialize()]
        public void TestInitialize()
        {
            ServiceProvider BuildServiceProvider()
            {
                var services = new ServiceCollection();
                services.RegisterNotificationHandlers(Assembly.GetExecutingAssembly());
                var provider = services.BuildServiceProvider();
                return provider;
            }

            _serviceProvider = BuildServiceProvider();
            _handlersCalled = new List<Type>();
        }

        [TestCleanup()]
        public void TestCleanup()
        {
        }

        [TestMethod]
        public void CanHandleINotification()
        {
            PublishAndAssert<INotification>(new Pinged(), new List<Type> {
                typeof(GenericHandler)
            });
        }

        [TestMethod]
        public void CanHandlePinged()
        {
            PublishAndAssert(new Pinged(), new List<Type> {
                typeof(GenericHandler),
                typeof(PingedHandler),
                typeof(ConstrainedPingedHandler<>)
            });
        }

        [TestMethod]
        public void CanHandlePonged()
        {
            PublishAndAssert(new Ponged(), new List<Type> {
                typeof(GenericHandler),
                typeof(PongedHandler)
            });
        }

        [TestMethod]
        public void CanHandleSpecialPinged()
        {
            PublishAndAssert(new SpecialPinged(), new List<Type> {
                typeof(GenericHandler),
                typeof(PingedHandler),
                typeof(SpecialPingedHandler),
                typeof(ConstrainedPingedHandler<>)
            });
        }

        [TestMethod]
        public void CanHandlePingedWithoutConstrainedHandlers()
        {
            PublishAndAssert(new Pinged(), new List<Type> {
                typeof(GenericHandler),
                typeof(PingedHandler)
            });
        }

        [TestMethod]
        public void CanHandleSpecialPingedWithoutConstrainedHandlers()
        {
            PublishAndAssert(new SpecialPinged(), new List<Type> {
                typeof(GenericHandler),
                typeof(PingedHandler),
                typeof(SpecialPingedHandler)
            });
        }

        private static void PublishAndAssert<TMessage>(TMessage message, List<Type> expectedHandlers) where TMessage : INotification
        {
            var handlers = (IEnumerable<INotificationHandler<TMessage>>)_serviceProvider.GetRequiredService(typeof(IEnumerable<INotificationHandler<TMessage>>));
            handlers.ToList().ForEach(o => o.Handle(message));
            CollectionAssert.AreEqual(expectedHandlers, _handlersCalled);
        }

        private static void HandleNotification(object handler, INotification notification)
        {
            _handlersCalled.Add(handler.GetType());
            Console.WriteLine(handler.GetType());
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
