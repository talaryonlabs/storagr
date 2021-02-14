namespace Storagr.Server
{
    public interface IRepositoryParams
    {
        IRepositoryParams Id(string repositoryId);
        IRepositoryParams Name(string name);
        IRepositoryParams Owner(string owner);
        IRepositoryParams SizeLimit(ulong sizeLimit);
    }
}