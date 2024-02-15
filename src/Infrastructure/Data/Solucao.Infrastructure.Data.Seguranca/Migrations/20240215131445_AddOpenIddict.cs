namespace Solucao.Infrastructure.Data.Seguranca.Migrations;

using System;
using Microsoft.EntityFrameworkCore.Migrations;

/// <inheritdoc />
public partial class AddOpenIddict : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SecurityApplications",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApplicationType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                ClientSecret = table.Column<string>(type: "nvarchar(max)", nullable: true),
                ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                ConsentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                JsonWebKeySet = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Permissions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                PostLogoutRedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RedirectUris = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Requirements = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Settings = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecurityApplications", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SecurityScopes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Descriptions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                DisplayNames = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Resources = table.Column<string>(type: "nvarchar(max)", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecurityScopes", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "SecurityAuthorizations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecurityAuthorizations", x => x.Id);
                table.ForeignKey(
                    name: "FK_SecurityAuthorizations_SecurityApplications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "SecurityApplications",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateTable(
            name: "SecurityTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ApplicationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                AuthorizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                RedemptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SecurityTokens", x => x.Id);
                table.ForeignKey(
                    name: "FK_SecurityTokens_SecurityApplications_ApplicationId",
                    column: x => x.ApplicationId,
                    principalTable: "SecurityApplications",
                    principalColumn: "Id");
                table.ForeignKey(
                    name: "FK_SecurityTokens_SecurityAuthorizations_AuthorizationId",
                    column: x => x.AuthorizationId,
                    principalTable: "SecurityAuthorizations",
                    principalColumn: "Id");
            });

        migrationBuilder.CreateIndex(
            name: "IX_SecurityApplications_ClientId",
            table: "SecurityApplications",
            column: "ClientId",
            unique: true,
            filter: "[ClientId] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityAuthorizations_ApplicationId_Status_Subject_Type",
            table: "SecurityAuthorizations",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_SecurityScopes_Name",
            table: "SecurityScopes",
            column: "Name",
            unique: true,
            filter: "[Name] IS NOT NULL");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityTokens_ApplicationId_Status_Subject_Type",
            table: "SecurityTokens",
            columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

        migrationBuilder.CreateIndex(
            name: "IX_SecurityTokens_AuthorizationId",
            table: "SecurityTokens",
            column: "AuthorizationId");

        migrationBuilder.CreateIndex(
            name: "IX_SecurityTokens_ReferenceId",
            table: "SecurityTokens",
            column: "ReferenceId",
            unique: true,
            filter: "[ReferenceId] IS NOT NULL");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SecurityScopes");

        migrationBuilder.DropTable(
            name: "SecurityTokens");

        migrationBuilder.DropTable(
            name: "SecurityAuthorizations");

        migrationBuilder.DropTable(
            name: "SecurityApplications");
    }
}
