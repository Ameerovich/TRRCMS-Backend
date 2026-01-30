using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePersonEmailAndPhoneFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Person_PrimaryPhoneNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PrimaryPhoneNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "SecondaryPhoneNumber",
                table: "Persons");

            migrationBuilder.AlterColumn<int>(
                name: "YearOfBirth",
                table: "Persons",
                type: "integer",
                nullable: true,
                comment: "تاريخ الميلاد - Year of birth (integer)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Year of birth (integer)");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "الرقم الوطني - National ID or identification number",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "National ID or identification number");

            migrationBuilder.AlterColumn<string>(
                name: "MotherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "الاسم الأم - Mother's name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "Mother's name in Arabic (اسم الأم)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "الاسم الأول - First name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "First name in Arabic (الاسم الأول)");

            migrationBuilder.AlterColumn<string>(
                name: "FatherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "اسم الأب - Father's name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Father's name in Arabic (اسم الأب)");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "الكنية - Family/Last name in Arabic",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "Family/Last name in Arabic (اسم العائلة)");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Persons",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "البريد الالكتروني - Email address");

            migrationBuilder.AddColumn<string>(
                name: "MobileNumber",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "رقم الموبايل - Mobile phone number");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "رقم الهاتف - Landline phone number");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Email",
                table: "Persons",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Person_MobileNumber",
                table: "Persons",
                column: "MobileNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Person_Email",
                table: "Persons");

            migrationBuilder.DropIndex(
                name: "IX_Person_MobileNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "MobileNumber",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Persons");

            migrationBuilder.AlterColumn<int>(
                name: "YearOfBirth",
                table: "Persons",
                type: "integer",
                nullable: true,
                comment: "Year of birth (integer)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "تاريخ الميلاد - Year of birth (integer)");

            migrationBuilder.AlterColumn<string>(
                name: "NationalId",
                table: "Persons",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                comment: "National ID or identification number",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true,
                oldComment: "الرقم الوطني - National ID or identification number");

            migrationBuilder.AlterColumn<string>(
                name: "MotherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                comment: "Mother's name in Arabic (اسم الأم)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true,
                oldComment: "الاسم الأم - Mother's name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FirstNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "First name in Arabic (الاسم الأول)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "الاسم الأول - First name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FatherNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Father's name in Arabic (اسم الأب)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "اسم الأب - Father's name in Arabic");

            migrationBuilder.AlterColumn<string>(
                name: "FamilyNameArabic",
                table: "Persons",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                comment: "Family/Last name in Arabic (اسم العائلة)",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldComment: "الكنية - Family/Last name in Arabic");

            migrationBuilder.AddColumn<string>(
                name: "PrimaryPhoneNumber",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Primary phone number");

            migrationBuilder.AddColumn<string>(
                name: "SecondaryPhoneNumber",
                table: "Persons",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                comment: "Secondary phone number");

            migrationBuilder.CreateIndex(
                name: "IX_Person_PrimaryPhoneNumber",
                table: "Persons",
                column: "PrimaryPhoneNumber");
        }
    }
}
