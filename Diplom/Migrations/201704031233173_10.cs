namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _10 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.News", "Files");
        }
        
        public override void Down()
        {
            AddColumn("dbo.News", "Files", c => c.String());
        }
    }
}
