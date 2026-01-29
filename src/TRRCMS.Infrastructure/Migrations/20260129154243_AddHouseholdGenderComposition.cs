using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHouseholdGenderComposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total persons with disabilities - legacy total",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of persons with disabilities");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Households",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "ملاحظات - Household notes",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "Household notes");

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد البالغين الذكور - Number of adult males",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of male members");

            migrationBuilder.AlterColumn<int>(
                name: "HouseholdSize",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد الأفراد - Total household size",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Total household size");

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "رب الأسرة/العميل - Name of head of household",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "Name of head of household");

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد البالغين الإناث - Number of adult females",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of female members");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of elderly (65+ years) - legacy total",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of elderly (65+ years)");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of children (2-12 years) - legacy total",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of children (2-12 years)");

            migrationBuilder.AddColumn<int>(
                name: "FemaleChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد الأطفال الإناث (أقل من 18) - Number of female children under 18");

            migrationBuilder.AddColumn<int>(
                name: "FemaleDisabledCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد المعاقين الإناث - Number of female persons with disabilities");

            migrationBuilder.AddColumn<int>(
                name: "FemaleElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد كبار السن الإناث (أكثر من 65) - Number of female elderly over 65");

            migrationBuilder.AddColumn<int>(
                name: "MaleChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد الأطفال الذكور (أقل من 18) - Number of male children under 18");

            migrationBuilder.AddColumn<int>(
                name: "MaleDisabledCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد المعاقين الذكور - Number of male persons with disabilities");

            migrationBuilder.AddColumn<int>(
                name: "MaleElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "عدد كبار السن الذكور (أكثر من 65) - Number of male elderly over 65");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FemaleChildCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "FemaleDisabledCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "FemaleElderlyCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "MaleChildCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "MaleDisabledCount",
                table: "Households");

            migrationBuilder.DropColumn(
                name: "MaleElderlyCount",
                table: "Households");

            migrationBuilder.AlterColumn<int>(
                name: "PersonsWithDisabilitiesCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of persons with disabilities",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Total persons with disabilities - legacy total");

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Households",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true,
                comment: "Household notes",
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000,
                oldNullable: true,
                oldComment: "ملاحظات - Household notes");

            migrationBuilder.AlterColumn<int>(
                name: "MaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of male members",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "عدد البالغين الذكور - Number of adult males");

            migrationBuilder.AlterColumn<int>(
                name: "HouseholdSize",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Total household size",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "عدد الأفراد - Total household size");

            migrationBuilder.AlterColumn<string>(
                name: "HeadOfHouseholdName",
                table: "Households",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                comment: "Name of head of household",
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldComment: "رب الأسرة/العميل - Name of head of household");

            migrationBuilder.AlterColumn<int>(
                name: "FemaleCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of female members",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "عدد البالغين الإناث - Number of adult females");

            migrationBuilder.AlterColumn<int>(
                name: "ElderlyCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of elderly (65+ years)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of elderly (65+ years) - legacy total");

            migrationBuilder.AlterColumn<int>(
                name: "ChildCount",
                table: "Households",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Number of children (2-12 years)",
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 0,
                oldComment: "Number of children (2-12 years) - legacy total");
        }
    }
}
