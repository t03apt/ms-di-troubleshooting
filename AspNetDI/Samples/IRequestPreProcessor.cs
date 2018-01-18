using System.Threading;
using System.Threading.Tasks;

namespace AspNetDI.Samples
{
    public interface IRequestPreProcessor<in TRequest>
    {
        Task Process(TRequest request, CancellationToken cancellationToken);
    }
}
