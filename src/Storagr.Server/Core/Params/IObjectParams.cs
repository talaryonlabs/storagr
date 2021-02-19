namespace Storagr.Server
{
    public interface IObjectParams
    {
        IObjectParams Id(string objectId);
        IObjectParams RepositoryId(string repositoryId);
        IObjectParams Size(long sizeLimit);
    }
}