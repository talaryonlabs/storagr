using System.Threading;
using System.Threading.Tasks;

namespace Storagr.Client
{
    public interface IStoragrClientRunner
    {
        void Run();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IStoragrClientRunner<T>
    {
        T Run();
        Task<T> RunAsync(CancellationToken cancellationToken = default);
    }
}