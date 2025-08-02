using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Api.Migrations
{
    /// <inheritdoc />
    public partial class v1_SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Animals",
                columns: new[] { "Id", "BirthDate", "Name", "OwnerEmail", "OwnerId", "OwnerName" },
                values: new object[,]
                {
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d476"), new DateTime(2024, 8, 2, 16, 20, 12, 715, DateTimeKind.Local).AddTicks(5922), "Rabbit", "rabbitsowner@example.com", new Guid("bc013cd8-d77b-4000-8a12-13c3a1dda6eb"), "Rabbit Owner" },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d477"), new DateTime(2023, 8, 2, 16, 20, 12, 715, DateTimeKind.Local).AddTicks(5894), "Cat", "catowner@example.com", new Guid("2fceeec4-e524-4905-80eb-f4870d396be0"), "Cat Owner" },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), new DateTime(2022, 8, 2, 16, 20, 12, 713, DateTimeKind.Local).AddTicks(5201), "Dog", "dogowner@example.com", new Guid("62fadb2e-b711-4850-9903-8b9b1c81a2e0"), "Dog Owner" }
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AnimalId", "CustomerId", "EndTime", "Notes", "StartTime", "Status", "VeterinarianId" },
                values: new object[,]
                {
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), new Guid("62fadb2e-b711-4850-9903-8b9b1c81a2e0"), new DateTime(2025, 8, 3, 17, 20, 12, 716, DateTimeKind.Local).AddTicks(8727), "Vet appointment", new DateTime(2025, 8, 3, 16, 20, 12, 716, DateTimeKind.Local).AddTicks(8551), 0, new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d481") },
                    { new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d480"), new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"), new Guid("62fadb2e-b711-4850-9903-8b9b1c81a2e0"), new DateTime(2025, 8, 7, 17, 20, 12, 716, DateTimeKind.Local).AddTicks(9281), "Follow-up check", new DateTime(2025, 8, 7, 16, 20, 12, 716, DateTimeKind.Local).AddTicks(9276), 0, new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d481") }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d476"));

            migrationBuilder.DeleteData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d477"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"));

            migrationBuilder.DeleteData(
                table: "Appointments",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d480"));

            migrationBuilder.DeleteData(
                table: "Animals",
                keyColumn: "Id",
                keyValue: new Guid("f47ac10b-58cc-4372-a567-0e02b2c3d479"));
        }
    }
}
