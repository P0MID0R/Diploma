namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _3 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teachers", "Subject", c => c.String(nullable: false));
            AlterColumn("dbo.Students", "Group", c => c.String(nullable: false));
            DropColumn("dbo.Teachers", "Group");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Teachers", "Group", c => c.Int(nullable: false));
            AlterColumn("dbo.Students", "Group", c => c.Int(nullable: false));
            DropColumn("dbo.Teachers", "Subject");
        }
    }
}
