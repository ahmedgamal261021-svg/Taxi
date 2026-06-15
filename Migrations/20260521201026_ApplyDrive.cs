using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxiiii.Migrations
{
    /// <inheritdoc />
    public partial class ApplyDrive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drive_RigesterUsers_UserId",
                table: "Drive");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Drive_DriverId",
                table: "Trips");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drive",
                table: "Drive");

            migrationBuilder.DropIndex(
                name: "IX_Drive_UserId",
                table: "Drive");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Drive");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Drive");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "Drive");

            migrationBuilder.RenameTable(
                name: "Drive",
                newName: "Drives");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Trips",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "DistanceKm",
                table: "Trips",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "RigesterUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RigesterUsers",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalTrips",
                table: "Drives",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Drives",
                type: "int",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Drives",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Drives",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DriverAvailabilityStatu",
                table: "Drives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "DriverStatu",
                table: "Drives",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drives",
                table: "Drives",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RigesterUsers_Email",
                table: "RigesterUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RigesterUsers_PhoneNumber",
                table: "RigesterUsers",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drives_LicenseNumber",
                table: "Drives",
                column: "LicenseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Drives_UserId",
                table: "Drives",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Drives_RigesterUsers_UserId",
                table: "Drives",
                column: "UserId",
                principalTable: "RigesterUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Drives_DriverId",
                table: "Trips",
                column: "DriverId",
                principalTable: "Drives",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drives_RigesterUsers_UserId",
                table: "Drives");

            migrationBuilder.DropForeignKey(
                name: "FK_Trips_Drives_DriverId",
                table: "Trips");

            migrationBuilder.DropIndex(
                name: "IX_RigesterUsers_Email",
                table: "RigesterUsers");

            migrationBuilder.DropIndex(
                name: "IX_RigesterUsers_PhoneNumber",
                table: "RigesterUsers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Drives",
                table: "Drives");

            migrationBuilder.DropIndex(
                name: "IX_Drives_LicenseNumber",
                table: "Drives");

            migrationBuilder.DropIndex(
                name: "IX_Drives_UserId",
                table: "Drives");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Drives");

            migrationBuilder.DropColumn(
                name: "DriverAvailabilityStatu",
                table: "Drives");

            migrationBuilder.DropColumn(
                name: "DriverStatu",
                table: "Drives");

            migrationBuilder.RenameTable(
                name: "Drives",
                newName: "Drive");

            migrationBuilder.AlterColumn<int>(
                name: "DurationMinutes",
                table: "Trips",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<double>(
                name: "DistanceKm",
                table: "Trips",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "RigesterUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "RigesterUsers",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "TotalTrips",
                table: "Drive",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Rating",
                table: "Drive",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LicenseNumber",
                table: "Drive",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Drive",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Drive",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "Drive",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Drive",
                table: "Drive",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Drive_UserId",
                table: "Drive",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Drive_RigesterUsers_UserId",
                table: "Drive",
                column: "UserId",
                principalTable: "RigesterUsers",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trips_Drive_DriverId",
                table: "Trips",
                column: "DriverId",
                principalTable: "Drive",
                principalColumn: "Id");
        }
    }
}
