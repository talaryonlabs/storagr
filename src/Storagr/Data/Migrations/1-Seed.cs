using System.Reflection.Metadata;
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
            Insert.IntoTable("BackendAuth")
                .Row(new
                {
                    Id = StoragrHelper.UUID(),
                    Username = "admin",
                    Password = "AQAAAAEAACcQAAAAEGTYFmFw+/mzx8Ef4yq2znUwkl5Y6Bs6ZV7NgINEG8GsomDerF2ZV0GfDIbmtBNhDw==", // _storagr
                });

            Insert.IntoTable("Repository")
                .Row(new
                {
                    Id = StoragrHelper.UUID(),
                    Name = "test",
                    OwnerId = "_",
                    SizeLimit = 0
                });
        }

        public override void Down()
        {
            Delete.FromTable("BackendAuth")
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