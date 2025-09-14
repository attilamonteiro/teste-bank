using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstudoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContaBancariaUnificada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "conta_bancaria",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", maxLength: 11, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    SenhaHash = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataInativacao = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conta_bancaria", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_conta_bancaria_Cpf",
                table: "conta_bancaria",
                column: "Cpf",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conta_bancaria_Email",
                table: "conta_bancaria",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conta_bancaria_Numero",
                table: "conta_bancaria",
                column: "Numero",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "conta_bancaria");
        }
    }
}
