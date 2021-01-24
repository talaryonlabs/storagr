namespace Storagr
{
    public interface IStoragrListArgs
    {
        int Limit { get; set; }
        string Cursor { get; set; }
        int Skip { get; set; }
    }
}