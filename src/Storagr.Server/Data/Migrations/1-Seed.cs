using FluentMigrator;

namespace Storagr.Server.Data.Migrations
{
    [Migration(1, "Seed")]
    public class Seed : Migration
    {
        public override void Up()
        {
            var uid = StoragrHelper.UUID();
            
            // Initial Admin User
            Insert.IntoTable("User")
                .Row(new
                {
                    Id = uid,
                    AuthId = uid,
                    AuthAdapter = "default",
                    Username = "admin",
                    Password = "AQAAAAEAACcQAAAAEGTYFmFw+/mzx8Ef4yq2znUwkl5Y6Bs6ZV7NgINEG8GsomDerF2ZV0GfDIbmtBNhDw==", // _storagr
                });

            Insert.IntoTable("Repository")
                .Row(new
                {
                    Id = StoragrHelper.UUID(),
                    Name = "test",
                    OwnerId = uid,
                    SizeLimit = 0
                });
        }

        public override void Down()
        {
            Delete.FromTable("User")
                .Row(new
                {
                    Username = "admin"
                });
            Delete.FromTable("Repository")
                .Row(new
                {
                    Name = "test"
                });
        }
    }
}