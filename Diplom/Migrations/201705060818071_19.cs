namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _19 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Messages", "Read", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Messages", "Read", c => c.Boolean(nullable: false));
        }
    }
}
