using System;
using FluentMigrator;

namespace Storagr.Data.Migrations
{
    [Migration(0, "Setup")]
    public class Setup : Migration
    {
        public override void Up()
        {
            Create.Table("BackendAuth")
                .WithColumn("Id").AsString().NotNullable().Unique().PrimaryKey()
                .WithColumn("Username").AsString().NotNullable().Unique()
                .WithColumn("Password").AsString().NotNullable();

            Create.Table("User")
                .WithColumn("Id").AsString().NotNullable().Unique().PrimaryKey()
                .WithColumn("AuthId").AsString().NotNullable()
                .WithColumn("AuthAdapter").AsString().NotNullable()
                .WithColumn("IsEnabled").AsBoolean().WithDefaultValue(true)
                .WithColumn("IsAdmin").AsBoolean().WithDefaultValue(false)
                .WithColumn("Username").AsString().NotNullable().Unique();

            Create.Table("Repository")
                .WithColumn("Id").AsString().NotNullable().Unique().PrimaryKey()
                .WithColumn("Name").AsString().NotNullable().Unique()
                .WithColumn("OwnerId").AsString().NotNullable()
                .WithColumn("SizeLimit").AsInt64().NotNullable();

            Create.Table("Object")
                .WithColumn("Id").AsString().NotNullable().Unique().PrimaryKey()
                .WithColumn("RepositoryId").AsString().NotNullable()
                .WithColumn("Size").AsInt64();

            Create.Table("Lock")
                .WithColumn("Id").AsString().NotNullable().Unique().PrimaryKey()
                .WithColumn("OwnerId").AsString().NotNullable()
                .WithColumn("RepositoryId").AsString().NotNullable()
                .WithColumn("Path").AsString().NotNullable()
                .WithColumn("LockedAt").AsDateTime2().NotNullable();

            Create.Table("Log")
                .WithColumn("Id").AsInt32().Unique().PrimaryKey()
                .WithColumn("Date").AsDateTime2().NotNullable()
                .WithColumn("Level").AsString().NotNullable()
                .WithColumn("Category").AsString().NotNullable()
                .WithColumn("Message").AsString().NotNullable()
                .WithColumn("Exception").AsString();

            Create.ForeignKey("FK_Repository_User")
                .FromTable("Repository").ForeignColumn("OwnerId")
                .ToTable("User").PrimaryColumn("Id");
            
            Create.ForeignKey("FK_Object_Repository")
                .FromTable("Object").ForeignColumn("RepositoryId")
                .ToTable("Repository").PrimaryColumn("Id");
            
            Create.ForeignKey("FK_Lock_User")
                .FromTable("Lock").ForeignColumn("OwnerId")
                .ToTable("User").PrimaryColumn("Id");
            
            Create.ForeignKey("FK_Lock_Repository")
                .FromTable("Lock").ForeignColumn("RepositoryId")
                .ToTable("Repository").PrimaryColumn("Id");
        }

        public override void Down()
        {
            Delete.ForeignKey("FK_Repository_User");
            Delete.ForeignKey("FK_Object_Repository");
            Delete.ForeignKey("FK_Lock_User");
            Delete.ForeignKey("FK_Lock_Repository");
            
            Delete.Table("BackendAuth");
            Delete.Table("User");
            Delete.Table("Repository");
            Delete.Table("Object");
            Delete.Table("Lock");
            Delete.Table("Log");
        }
    }
}