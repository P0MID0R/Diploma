namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _5 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Themes", "Description", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Themes", "Description", c => c.String(nullable: false));
        }
    }
}
