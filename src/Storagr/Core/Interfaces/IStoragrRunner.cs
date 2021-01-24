using System.Threading;
using System.Threading.Tasks;

namespace Storagr
{
    public interface IStoragrRunner
    {
        void Run();
        Task RunAsync(CancellationToken cancellationToken = default);
    }
    
    public interface IStoragrRunner<TResult>
    {
        TResult Run();
        Task<TResult> RunAsync(CancellationToken cancellationToken = default);
    }
}