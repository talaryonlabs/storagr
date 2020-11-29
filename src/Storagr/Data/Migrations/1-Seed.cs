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
            // Admin
            var authId = StoragrHelper.UUID();
            var userId = StoragrHelper.UUID();
            
            // Initial Admin User
            Insert.IntoTable("BackendAuth")
                .Row(new
                {
                    Id = authId,
                    Username = "admin",
                    Password = "AQAAAAEAACcQAAAAEGTYFmFw+/mzx8Ef4yq2znUwkl5Y6Bs6ZV7NgINEG8GsomDerF2ZV0GfDIbmtBNhDw==", // _storagr
                });

            Insert.IntoTable("User")
                .Row(new
                {
                    Id = userId,
                    AuthId = authId,
                    AuthAdapter = "storagr-backend",
                    IsEnabled = true,
                    IsAdmin = true,
                    Username = "admin"
                });

            Insert.IntoTable("Repository")
                .Row(new
                {
                    Id = "test",
                    OwnerId = userId,
                    SizeLimit = -1
                });
        }

        public override void Down()
        {
            Delete.FromTable("BackendAuth")
                .Row(new
                {
                    Username = "admin"
                });
            Delete.FromTable("User")
                .Row(new
                {
                    Username = "admin"
                });
            Delete.FromTable("Repository")
                .Row(new
                {
                    Id = "test"
                });
        }
    }
}