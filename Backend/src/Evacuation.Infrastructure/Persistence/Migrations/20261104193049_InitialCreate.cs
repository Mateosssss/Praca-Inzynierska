using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Evacuation.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ImagePng = table.Column<byte[]>(type: "bytea", nullable: false),
                    WidthPx = table.Column<int>(type: "integer", nullable: false),
                    HeightPx = table.Column<int>(type: "integer", nullable: false),
                    PixelsPerMeter = table.Column<double>(type: "double precision", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    UploadedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorPlans_Buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "Buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloorPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    RegionId = table.Column<int>(type: "integer", nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CentroidX = table.Column<int>(type: "integer", nullable: false),
                    CentroidY = table.Column<int>(type: "integer", nullable: false),
                    PixelCount = table.Column<int>(type: "integer", nullable: false),
                    IsExit = table.Column<bool>(type: "boolean", nullable: false),
                    IsStaircase = table.Column<bool>(type: "boolean", nullable: false),
                    IsStart = table.Column<bool>(type: "boolean", nullable: false),
                    HasPaintedDanger = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_FloorPlans_FloorPlanId",
                        column: x => x.FloorPlanId,
                        principalTable: "FloorPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DangerZones",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Kind = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ClearedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DangerZones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DangerZones_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoomEdges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FloorPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    FromRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeightPixels = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoomEdges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoomEdges_FloorPlans_FloorPlanId",
                        column: x => x.FloorPlanId,
                        principalTable: "FloorPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoomEdges_Rooms_FromRoomId",
                        column: x => x.FromRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RoomEdges_Rooms_ToRoomId",
                        column: x => x.ToRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_Latitude_Longitude",
                table: "Buildings",
                columns: new[] { "Latitude", "Longitude" });

            migrationBuilder.CreateIndex(
                name: "IX_DangerZones_RoomId_Status",
                table: "DangerZones",
                columns: new[] { "RoomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorPlans_BuildingId_Level",
                table: "FloorPlans",
                columns: new[] { "BuildingId", "Level" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomEdges_FloorPlanId_FromRoomId_ToRoomId",
                table: "RoomEdges",
                columns: new[] { "FloorPlanId", "FromRoomId", "ToRoomId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoomEdges_FromRoomId",
                table: "RoomEdges",
                column: "FromRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEdges_ToRoomId",
                table: "RoomEdges",
                column: "ToRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FloorPlanId_IsExit",
                table: "Rooms",
                columns: new[] { "FloorPlanId", "IsExit" });

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_FloorPlanId_RegionId",
                table: "Rooms",
                columns: new[] { "FloorPlanId", "RegionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DangerZones");

            migrationBuilder.DropTable(
                name: "RoomEdges");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "FloorPlans");

            migrationBuilder.DropTable(
                name: "Buildings");
        }
    }
}
