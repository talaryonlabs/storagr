using System.Threading;
using System.Threading.Tasks;

namespace Storagr.Client
{
    public interface IStoragrUpdater<T>
    {
        Task<T> Update(CancellationToken cancellationToken = default);
    }
}