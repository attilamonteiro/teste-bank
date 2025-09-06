using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstudoApi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreateWithBanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contacorrente",
                columns: table => new
                {
                    IdContaCorrente = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    Numero = table.Column<int>(type: "INTEGER", nullable: false),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Ativo = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 1),
                    Senha = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Salt = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contacorrente", x => x.IdContaCorrente);
                    table.CheckConstraint("CK_contacorrente_ativo", "ativo IN (0,1)");
                });

            migrationBuilder.CreateTable(
                name: "idempotencia",
                columns: table => new
                {
                    ChaveIdempotencia = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    Requisicao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    Resultado = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_idempotencia", x => x.ChaveIdempotencia);
                });

            migrationBuilder.CreateTable(
                name: "movimento",
                columns: table => new
                {
                    IdMovimento = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    IdContaCorrente = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    DataMovimento = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    TipoMovimento = table.Column<string>(type: "TEXT", maxLength: 1, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimento", x => x.IdMovimento);
                    table.CheckConstraint("CK_movimento_tipo", "tipomovimento IN ('C','D')");
                    table.ForeignKey(
                        name: "FK_movimento_contacorrente_IdContaCorrente",
                        column: x => x.IdContaCorrente,
                        principalTable: "contacorrente",
                        principalColumn: "IdContaCorrente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tarifa",
                columns: table => new
                {
                    IdTarifa = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    IdContaCorrente = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    DataMovimento = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tarifa", x => x.IdTarifa);
                    table.ForeignKey(
                        name: "FK_tarifa_contacorrente_IdContaCorrente",
                        column: x => x.IdContaCorrente,
                        principalTable: "contacorrente",
                        principalColumn: "IdContaCorrente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transferencia",
                columns: table => new
                {
                    IdTransferencia = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    IdContaCorrenteOrigem = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    IdContaCorrenteDestino = table.Column<string>(type: "TEXT", maxLength: 37, nullable: false),
                    DataMovimento = table.Column<string>(type: "TEXT", maxLength: 25, nullable: false),
                    Valor = table.Column<decimal>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transferencia", x => x.IdTransferencia);
                    table.ForeignKey(
                        name: "FK_transferencia_contacorrente_IdContaCorrenteDestino",
                        column: x => x.IdContaCorrenteDestino,
                        principalTable: "contacorrente",
                        principalColumn: "IdContaCorrente",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transferencia_contacorrente_IdContaCorrenteOrigem",
                        column: x => x.IdContaCorrenteOrigem,
                        principalTable: "contacorrente",
                        principalColumn: "IdContaCorrente",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_contacorrente_Numero",
                table: "contacorrente",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_movimento_IdContaCorrente",
                table: "movimento",
                column: "IdContaCorrente");

            migrationBuilder.CreateIndex(
                name: "IX_tarifa_IdContaCorrente",
                table: "tarifa",
                column: "IdContaCorrente");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_IdContaCorrenteDestino",
                table: "transferencia",
                column: "IdContaCorrenteDestino");

            migrationBuilder.CreateIndex(
                name: "IX_transferencia_IdContaCorrenteOrigem",
                table: "transferencia",
                column: "IdContaCorrenteOrigem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "idempotencia");

            migrationBuilder.DropTable(
                name: "movimento");

            migrationBuilder.DropTable(
                name: "tarifa");

            migrationBuilder.DropTable(
                name: "transferencia");

            migrationBuilder.DropTable(
                name: "contacorrente");
        }
    }
}
