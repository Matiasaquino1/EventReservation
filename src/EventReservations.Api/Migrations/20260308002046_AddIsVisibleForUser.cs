using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EventReservations.Migrations
{
    /// <inheritdoc />
    public partial class AddIsVisibleForUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Solo dejamos la columna nueva. 
            // EF ya no intentará crear las tablas que ya existen.
            migrationBuilder.AddColumn<bool>(
                name: "IsVisibleForUser",
                table: "Reservations",
                type: "boolean",
                nullable: false,
                defaultValue: true); // Le ponemos true por defecto para las existentes
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Si queremos volver atrás, solo borramos la columna
            migrationBuilder.DropColumn(
                name: "IsVisibleForUser",
                table: "Reservations");
        }
    }
}
