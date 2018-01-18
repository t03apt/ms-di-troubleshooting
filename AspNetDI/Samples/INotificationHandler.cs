using System.Threading;
using System.Threading.Tasks;

namespace AspNetDI.Samples
{
    public interface INotificationHandler<in TNotification>
        where TNotification : INotification
    {
        /// <summary>
        /// Handles a notification
        /// </summary>
        /// <param name="notification">The notification message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task Handle(TNotification notification, CancellationToken cancellationToken);
    }
}
