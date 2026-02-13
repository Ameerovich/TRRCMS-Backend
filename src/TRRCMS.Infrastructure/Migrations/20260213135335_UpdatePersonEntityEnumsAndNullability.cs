using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonEntityEnumsAndNullability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "YearOfBirth",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToHead",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Relationship to head of household (enum converted to string)",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Relationship to head of household");

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Nationality (enum converted to string)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Nationality (controlled vocabulary)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Gender (enum converted to string)",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Gender (controlled vocabulary: M/F)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "الاسم الأول - First name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "الاسم الأول - First name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FatherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "اسم الأب - Father's name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "اسم الأب - Father's name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "الكنية - Family/Last name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "الكنية - Family/Last name in Arabic");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "Persons",
                type: "timestamp with time zone",
                nullable: true,
                comment: "تاريخ الميلاد - Date of birth (full date or year-only)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "Persons");

            migrationBuilder.AlterColumn<string>(
                name: "RelationshipToHead",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "Relationship to head of household",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "Relationship to head of household (enum converted to string)");

            migrationBuilder.AlterColumn<string>(
                name: "Nationality",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Nationality (controlled vocabulary)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Nationality (enum converted to string)");

            migrationBuilder.AlterColumn<string>(
                name: "Gender",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Gender (controlled vocabulary: M/F)",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true,
                oldComment: "Gender (enum converted to string)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "الاسم الأول - First name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "الاسم الأول - First name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FatherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "اسم الأب - Father's name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "اسم الأب - Father's name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                comment: "الكنية - Family/Last name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "الكنية - Family/Last name in Arabic");

            migrationBuilder.AddColumn<int>(
                name: "YearOfBirth",
                table: "Persons",
                type: "integer",
                nullable: true,
                comment: "تاريخ الميلاد - Year of birth (integer)");
        }
    }
}
