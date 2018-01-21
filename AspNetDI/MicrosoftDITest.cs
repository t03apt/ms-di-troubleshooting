using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
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

                services.AddScoped<SingleInstanceFactory>(p => t => p.GetService(t));
                services.AddScoped<MultiInstanceFactory>(p => p.GetRequiredServices);
                services.AddScoped<IMediator, Mediator>();

                services.AddTransient<INotification, EmptyNotification>();

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
            PublishAndAssert(new EmptyNotification(), new List<Type> {
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
            var mediator = _serviceProvider.GetRequiredService<IMediator>();
            mediator.Publish(message);
            CollectionAssert.AreEqual(expectedHandlers, _handlersCalled);
        }

        private static Task HandleNotification(object handler, INotification notification)
        {
            _handlersCalled.Add(handler.GetType());
            Console.WriteLine(handler.GetType());
            return Task.CompletedTask;
        }

        public class EmptyNotification : INotification { }

        public class Pinged : INotification { }

        public class Ponged : INotification { }

        public class SpecialPinged : Pinged { }

        public class GenericHandler : INotificationHandler<INotification>
        {
            public Task Handle(INotification notification, CancellationToken cancellationToken) => HandleNotification(this, notification);
        }

        public class PingedHandler : INotificationHandler<Pinged>
        {
            public Task Handle(Pinged notification, CancellationToken cancellationToken) => HandleNotification(this, notification);
        }

        public class SpecialPingedHandler : INotificationHandler<SpecialPinged>
        {
            public Task Handle(SpecialPinged notification, CancellationToken cancellationToken) => HandleNotification(this, notification);
        }

        public class PongedHandler : INotificationHandler<Ponged>
        {
            public Task Handle(Ponged notification, CancellationToken cancellationToken) => HandleNotification(this, notification);
        }

        public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
            where TNotification : Pinged
        {
            public Task Handle(TNotification notification, CancellationToken cancellationToken) => HandleNotification(this, notification);
        }
    }
}
