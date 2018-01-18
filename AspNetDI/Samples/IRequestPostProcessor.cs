using System.Threading.Tasks;

namespace AspNetDI.Samples
{
    public interface IRequestPostProcessor<in TRequest, in TResponse>
    {
        Task Process(TRequest request, TResponse response);
    }
}
