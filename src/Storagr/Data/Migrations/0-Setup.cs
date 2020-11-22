using FluentMigrator;
using Storagr.Shared;

namespace Storagr.Data.Migrations
{
    [Migration(0, "Setup")]
    public class Setup : Migration
    {
        public override void Up()
        {
            Create.Table("_backendAuth")
                .WithColumn("AuthId").AsString().PrimaryKey().NotNullable().Unique()
                .WithColumn("Username").AsString()
                .WithColumn("Password").AsString()
                .WithColumn("Mail").AsString()
                .WithColumn("Role").AsString();
            
            Create.Table("users")
                .WithColumn("UserId").AsString().PrimaryKey().NotNullable().Unique()
                .WithColumn("AuthId").AsString()
                .WithColumn("AuthAdapter").AsString()
                .WithColumn("Username").AsString()
                .WithColumn("Mail").AsString()
                .WithColumn("Role").AsString();

            Create.Table("repositories")
                .WithColumn("RepositoryId").AsString().PrimaryKey().NotNullable().Unique()
                .WithColumn("OwnerId").AsString()
                .WithColumn("SizeLimit").AsInt64();

            Create.Table("objects")
                .WithColumn("ObjectId").AsString().PrimaryKey().NotNullable().Unique()
                .WithColumn("RepositoryId").AsString()
                .WithColumn("Size").AsInt64();

            Create.Table("locks")
                .WithColumn("LockId").AsString().PrimaryKey().NotNullable().Unique()
                .WithColumn("OwnerId").AsString()
                .WithColumn("RepositoryId").AsString()
                .WithColumn("Path").AsString()
                .WithColumn("LockedAt").AsDateTime2();

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
        }

        public override void Down()
        {
            Delete.Table("_backendAuth");
            Delete.Table("users");
            Delete.Table("repositories");
            Delete.Table("objects");
            Delete.Table("locks");
        }
    }
}