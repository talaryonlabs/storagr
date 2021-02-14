namespace Storagr
{
    public interface IStoragrRepositoryParams
    {
        IStoragrRepositoryParams Id(string repositoryId);
        IStoragrRepositoryParams Name(string name);
        IStoragrRepositoryParams Owner(string owner);
        IStoragrRepositoryParams SizeLimit(ulong sizeLimit);
    }
}