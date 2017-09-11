namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Themes", "Subject", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Themes", "Subject");
        }
    }
}
