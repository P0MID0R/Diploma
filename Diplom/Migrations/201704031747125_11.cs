namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _11 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Messages", "Files");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Messages", "Files", c => c.String());
        }
    }
}
