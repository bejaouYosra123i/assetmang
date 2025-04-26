using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITAssetManagement1.Migrations
{
    /// <inheritdoc />
    public partial class Youyou : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "IsPortable",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Justification",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "RequestDate",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Profiles");

            migrationBuilder.CreateTable(
                name: "ITRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequesterName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Department = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fonction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    RequesterId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    PcType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NeedDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ITRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ITRequests_AspNetUsers_RequesterId",
                        column: x => x.RequesterId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Validations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Role = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Remark = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Signature = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ITRequestId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Validations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Validations_ITRequests_ITRequestId",
                        column: x => x.ITRequestId,
                        principalTable: "ITRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ITRequests_RequesterId",
                table: "ITRequests",
                column: "RequesterId");

            migrationBuilder.CreateIndex(
                name: "IX_Validations_ITRequestId",
                table: "Validations",
                column: "ITRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Validations");

            migrationBuilder.DropTable(
                name: "ITRequests");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Profiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedBy",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Profiles",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsPortable",
                table: "Profiles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Justification",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestDate",
                table: "Profiles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
