using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetDI.Samples
{
    public class ConstrainedPingedHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : Pinged
    {
        private readonly TextWriter _writer;

        public ConstrainedPingedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(TNotification notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got pinged constrained async.");
        }
    }
}
