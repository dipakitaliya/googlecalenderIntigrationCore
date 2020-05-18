namespace Google_Calender.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GoogleRefreshTokens",
                c => new
                    {
                        RefreshTokenId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        RefreshToken = c.String(),
                    })
                .PrimaryKey(t => t.RefreshTokenId);
            
            CreateTable(
                "dbo.UserProfile",
                c => new
                    {
                        UserId = c.Int(nullable: false, identity: true),
                        UserName = c.String(),
                        Email = c.String(),
                    })
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserProfile");
            DropTable("dbo.GoogleRefreshTokens");
        }
    }
}
