using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetDI.Samples
{
    public class PongedHandler : INotificationHandler<Ponged>
    {
        private readonly TextWriter _writer;

        public PongedHandler(TextWriter writer)
        {
            _writer = writer;
        }

        public Task Handle(Ponged notification, CancellationToken cancellationToken)
        {
            return _writer.WriteLineAsync("Got ponged async.");
        }
    }
}
