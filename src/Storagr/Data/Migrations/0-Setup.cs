using FluentMigrator;

namespace Storagr.Data.Migrations
{
    [Migration(0, "Init")]
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
                .WithColumn("AuthAdapter").AsString()
                .WithColumn("AuthId").AsString()
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
                .WithColumn("Path").AsString()
                .WithColumn("LockedAt").AsDateTime2()
                .WithColumn("OwnerId").AsString()
                .WithColumn("RepositoryId").AsString();
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