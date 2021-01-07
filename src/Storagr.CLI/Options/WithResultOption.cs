namespace Storagr.CLI
{
    public class WithResultOption : StoragrOption<bool>
    {
        public WithResultOption()
            : base(new[] {"--with-result"}, StoragrConstants.WithResultOptionDescription)
        {
        }
    }
}