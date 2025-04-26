using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement1.Migrations
{
    /// <inheritdoc />
    public partial class Yo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RequesterId",
                table: "ITRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PcType",
                table: "ITRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "RequesterId1",
                table: "ITRequests",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequesterId1",
                table: "ITRequests",
                column: "RequesterId1");

            migrationBuilder.AddForeignKey(
                name: "FK_ITRequests_AspNetUsers_RequesterId1",
                table: "ITRequests",
                column: "RequesterId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ITRequests_AspNetUsers_RequesterId1",
                table: "ITRequests");

            migrationBuilder.DropIndex(
                name: "IX_ITRequests_RequesterId1",
                table: "ITRequests");

            migrationBuilder.DropColumn(
                name: "RequesterId1",
                table: "ITRequests");

            migrationBuilder.AlterColumn<string>(
                name: "RequesterId",
                table: "ITRequests",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "PcType",
                table: "ITRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
