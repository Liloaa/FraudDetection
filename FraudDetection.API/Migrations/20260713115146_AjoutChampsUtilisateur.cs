using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FraudDetection.API.Migrations
{
    /// <inheritdoc />
    public partial class AjoutChampsUtilisateur : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Logs_Alertes_AlerteId",
                table: "Logs");

            migrationBuilder.DropIndex(
                name: "IX_Logs_AlerteId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "AlerteId",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "FaitLe",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "MotDePasse",
                table: "Utilisateurs",
                newName: "Prenom");

            migrationBuilder.RenameColumn(
                name: "Commentaire",
                table: "Logs",
                newName: "Details");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreeLe",
                table: "Utilisateurs",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Utilisateurs",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DerniereConnexion",
                table: "Utilisateurs",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotDePasseHash",
                table: "Utilisateurs",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Utilisateurs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTransaction",
                table: "Transactions",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AddColumn<string>(
                name: "AdresseIp",
                table: "Logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateLog",
                table: "Logs",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "OuvertLe",
                table: "Comptes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreeLe",
                table: "Clients",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DetecteLe",
                table: "Alertes",
                type: "timestamp without time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "DerniereConnexion",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "MotDePasseHash",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Utilisateurs");

            migrationBuilder.DropColumn(
                name: "AdresseIp",
                table: "Logs");

            migrationBuilder.DropColumn(
                name: "DateLog",
                table: "Logs");

            migrationBuilder.RenameColumn(
                name: "Prenom",
                table: "Utilisateurs",
                newName: "MotDePasse");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "Logs",
                newName: "Commentaire");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreeLe",
                table: "Utilisateurs",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateTransaction",
                table: "Transactions",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AddColumn<int>(
                name: "AlerteId",
                table: "Logs",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FaitLe",
                table: "Logs",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<DateTime>(
                name: "OuvertLe",
                table: "Comptes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreeLe",
                table: "Clients",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DetecteLe",
                table: "Alertes",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp without time zone");

            migrationBuilder.CreateIndex(
                name: "IX_Logs_AlerteId",
                table: "Logs",
                column: "AlerteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Logs_Alertes_AlerteId",
                table: "Logs",
                column: "AlerteId",
                principalTable: "Alertes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
