namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _15 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Messages", "FromUser", c => c.String());
            AlterColumn("dbo.Messages", "ToUsers", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Messages", "ToUsers", c => c.String(nullable: false));
            AlterColumn("dbo.Messages", "FromUser", c => c.String(nullable: false));
        }
    }
}
