namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _20 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Themes", "Mark");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Themes", "Mark", c => c.Int(nullable: false));
        }
    }
}
