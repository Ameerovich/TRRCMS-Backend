using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TRRCMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdministrativeHierarchyTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Governorate code (2 digits)"),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Arabic name"),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "English name"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Whether this governorate is active"),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                    table.UniqueConstraint("AK_Governorates_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "District code (2 digits)"),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent governorate code"),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Arabic name"),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "English name"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Whether this district is active"),
                    GovernorateId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.UniqueConstraint("AK_Districts_GovernorateCode_Code", x => new { x.GovernorateCode, x.Code });
                    table.ForeignKey(
                        name: "FK_Districts_Governorates_GovernorateCode",
                        column: x => x.GovernorateCode,
                        principalTable: "Governorates",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Districts_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SubDistricts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Sub-district code (2 digits)"),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent governorate code"),
                    DistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent district code"),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Arabic name"),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "English name"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Whether this sub-district is active"),
                    DistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDistricts", x => x.Id);
                    table.UniqueConstraint("AK_SubDistricts_GovernorateCode_DistrictCode_Code", x => new { x.GovernorateCode, x.DistrictCode, x.Code });
                    table.ForeignKey(
                        name: "FK_SubDistricts_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SubDistricts_Districts_GovernorateCode_DistrictCode",
                        columns: x => new { x.GovernorateCode, x.DistrictCode },
                        principalTable: "Districts",
                        principalColumns: new[] { "GovernorateCode", "Code" },
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, comment: "Community code (3 digits)"),
                    GovernorateCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent governorate code"),
                    DistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent district code"),
                    SubDistrictCode = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: false, comment: "Parent sub-district code"),
                    NameArabic = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "Arabic name"),
                    NameEnglish = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, comment: "English name"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true, comment: "Whether this community is active"),
                    SubDistrictId = table.Column<Guid>(type: "uuid", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "bytea", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DeletedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Communities_SubDistricts_GovernorateCode_DistrictCode_SubDi~",
                        columns: x => new { x.GovernorateCode, x.DistrictCode, x.SubDistrictCode },
                        principalTable: "SubDistricts",
                        principalColumns: new[] { "GovernorateCode", "DistrictCode", "Code" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Communities_SubDistricts_SubDistrictId",
                        column: x => x.SubDistrictId,
                        principalTable: "SubDistricts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Communities_FullHierarchy_Code",
                table: "Communities",
                columns: new[] { "GovernorateCode", "DistrictCode", "SubDistrictCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_GovernorateCode_DistrictCode_SubDistrictCode",
                table: "Communities",
                columns: new[] { "GovernorateCode", "DistrictCode", "SubDistrictCode" });

            migrationBuilder.CreateIndex(
                name: "IX_Communities_SubDistrictId",
                table: "Communities",
                column: "SubDistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GovernorateCode",
                table: "Districts",
                column: "GovernorateCode");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GovernorateCode_Code",
                table: "Districts",
                columns: new[] { "GovernorateCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GovernorateId",
                table: "Districts",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_Code",
                table: "Governorates",
                column: "Code",
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SubDistricts_DistrictId",
                table: "SubDistricts",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDistricts_GovernorateCode_DistrictCode",
                table: "SubDistricts",
                columns: new[] { "GovernorateCode", "DistrictCode" });

            migrationBuilder.CreateIndex(
                name: "IX_SubDistricts_GovernorateCode_DistrictCode_Code",
                table: "SubDistricts",
                columns: new[] { "GovernorateCode", "DistrictCode", "Code" },
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Communities");

            migrationBuilder.DropTable(
                name: "SubDistricts");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Governorates");
        }
    }
}
