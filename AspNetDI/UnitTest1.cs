using System;
using System.Collections.Generic;
using System.IO;
using AspNetDI.Samples;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AspNetDI
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var services = new ServiceCollection();

            services.AddSingleton<TextWriter>(Console.Out);

            ////Pipeline
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPreProcessorBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestPostProcessorBehavior<,>));
            //services.AddScoped(typeof(IPipelineBehavior<,>), typeof(GenericPipelineBehavior<,>));
            //services.AddScoped(typeof(IRequestPreProcessor<>), typeof(GenericRequestPreProcessor<>));
            //services.AddScoped(typeof(IRequestPostProcessor<,>), typeof(GenericRequestPostProcessor<,>));

            services.AddScoped(typeof(INotificationHandler<>), typeof(ConstrainedPingedHandler<>));

            var provider = services.BuildServiceProvider();

            //MediatR.INotificationHandler`1[MediatR.Examples.Ponged]

            var types = GetRequiredServices(provider, typeof(INotificationHandler<Pinged>));
            //types = GetRequiredServices(provider, typeof(INotificationHandler<INotification>));
            Console.WriteLine();
        }

        private static IEnumerable<object> GetRequiredServices(IServiceProvider provider, Type serviceType)
        {
            return (IEnumerable<object>)provider.GetRequiredService(typeof(IEnumerable<>).MakeGenericType(serviceType));
        }
    }
}
