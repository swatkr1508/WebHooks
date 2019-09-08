using Microsoft.EntityFrameworkCore.Migrations;

namespace Microsoft.AspNetCore.WebHooks.Migrations
{
    /// <summary>
    /// Initial db migration
    /// </summary>
    public partial class initialdb : Migration
    {
        /// <summary>
        /// Initial DB Up
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WebHooks",
                columns: table => new
                {
                    Id = table.Column<string>(maxLength: 64, nullable: false),
                    User = table.Column<string>(maxLength: 256, nullable: false),
                    ProtectedData = table.Column<string>(nullable: false),
                    RowVer = table.Column<byte[]>(rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebHooks", x => new { x.Id, x.User });
                });
        }

        /// <summary>
        /// Initial DB Down
        /// </summary>
        /// <param name="migrationBuilder"></param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WebHooks");
        }
    }
}
