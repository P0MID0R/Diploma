namespace Diplom.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class _8 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Messages",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FromUser = c.String(nullable: false),
                        ToUsers = c.String(nullable: false),
                        Topic = c.String(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Text = c.String(nullable: false),
                        Files = c.String(),
                        Read = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Messages");
        }
    }
}
