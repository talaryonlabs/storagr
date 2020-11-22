using FluentMigrator;
using Storagr.Shared;

namespace Storagr.Data.Migrations
{
    [Migration(1, "Seed")]
    public class Seed : Migration
    {
        public override void Up()
        {
            // Initial Admin User
            Insert.IntoTable("_backendAuth")
                .Row(new
                {
                    AuthId = StoragrHelper.UUID(),
                    Username = "admin",
                    Password = "AQAAAAEAACcQAAAAEGTYFmFw+/mzx8Ef4yq2znUwkl5Y6Bs6ZV7NgINEG8GsomDerF2ZV0GfDIbmtBNhDw==", // _storagr
                    Mail = "no-mail",
                    Role = "Admin"
                });

            Insert.IntoTable("repositories")
                .Row(new
                {
                    RepositoryId = "test",
                    OwnerId = "_",
                    SizeLimit = -1
                });
        }

        public override void Down()
        {
            Delete.FromTable("_backendAuth")
                .Row(new
                {
                    Username = "admin"
                });
            Delete.FromTable("repositories")
                .Row(new
                {
                    RepositoryId = "test"
                });
        }
    }
}