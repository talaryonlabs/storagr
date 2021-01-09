using System.Threading;
using System.Threading.Tasks;

namespace Storagr.Client
{
    public interface IStoragrCreator<T>
    {
        Task<T> Create(CancellationToken cancellationToken = default);
    }
}