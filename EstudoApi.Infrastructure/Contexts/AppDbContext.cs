
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EstudoApi.Domain.Models;
using EstudoApi.Infrastructure.Identity;


namespace EstudoApi.Infrastructure.Contexts
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Nova entidade unificada
        public DbSet<ContaBancaria> ContasBancarias { get; set; }
        
        // DbSets seguindo o esquema SQLite da Ana (manter para compatibilidade)
        public DbSet<ContaCorrente> ContasCorrentes { get; set; }
        public DbSet<Movimento> Movimentos { get; set; }
        public DbSet<Transferencia> Transferencias { get; set; }
        public DbSet<Idempotencia> Idempotencias { get; set; }
        public DbSet<Tarifa> Tarifas { get; set; }

        // Manter para compatibilidade com Identity
        public DbSet<Account> Accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configuração do Identity
            builder.Entity<AppUser>(b =>
            {
                b.ToTable("Users");
                b.HasIndex(u => u.Email).IsUnique();
                b.HasIndex(u => u.Cpf).IsUnique();
                b.Property(u => u.Nome).IsRequired().HasMaxLength(120);
                b.Property(u => u.Cpf).IsRequired().HasMaxLength(11);
            });

            // Configuração da tabela contacorrente
            builder.Entity<ContaCorrente>(b =>
            {
                b.ToTable("contacorrente");
                b.HasKey(cc => cc.IdContaCorrente);
                b.Property(cc => cc.IdContaCorrente).HasMaxLength(37).IsRequired();
                b.Property(cc => cc.Numero).IsRequired();
                b.Property(cc => cc.Nome).HasMaxLength(100).IsRequired();
                b.Property(cc => cc.Ativo).IsRequired().HasDefaultValue(1);
                b.Property(cc => cc.Senha).HasMaxLength(100).IsRequired();
                b.Property(cc => cc.Salt).HasMaxLength(100).IsRequired();

                // Índices
                b.HasIndex(cc => cc.Numero).IsUnique();

                // Check constraint para ativo
                b.ToTable(tb => tb.HasCheckConstraint("CK_contacorrente_ativo", "ativo IN (0,1)"));
            });

            // Configuração da tabela movimento
            builder.Entity<Movimento>(b =>
            {
                b.ToTable("movimento");
                b.HasKey(m => m.IdMovimento);
                b.Property(m => m.IdMovimento).HasMaxLength(37).IsRequired();
                b.Property(m => m.IdContaCorrente).HasMaxLength(37).IsRequired();
                b.Property(m => m.DataMovimento).HasMaxLength(25).IsRequired();
                b.Property(m => m.TipoMovimento).HasMaxLength(1).IsRequired();
                b.Property(m => m.Valor).HasColumnType("REAL").IsRequired();

                // Relacionamento
                b.HasOne(m => m.ContaCorrente)
                 .WithMany(cc => cc.Movimentos)
                 .HasForeignKey(m => m.IdContaCorrente)
                 .HasPrincipalKey(cc => cc.IdContaCorrente);

                // Check constraint para tipo de movimento
                b.ToTable(tb => tb.HasCheckConstraint("CK_movimento_tipo", "tipomovimento IN ('C','D')"));
            });

            // Configuração da tabela transferencia
            builder.Entity<Transferencia>(b =>
            {
                b.ToTable("transferencia");
                b.HasKey(t => t.IdTransferencia);
                b.Property(t => t.IdTransferencia).HasColumnName("idtransferencia").HasMaxLength(37).IsRequired();
                b.Property(t => t.IdContaCorrenteOrigem).HasColumnName("idcontacorrente_origem").HasMaxLength(37).IsRequired();
                b.Property(t => t.IdContaCorrenteDestino).HasColumnName("idcontacorrente_destino").HasMaxLength(37).IsRequired();
                b.Property(t => t.DataMovimento).HasColumnName("datamovimento").HasMaxLength(25).IsRequired();
                b.Property(t => t.Valor).HasColumnName("valor").HasColumnType("REAL").IsRequired();

                // Relacionamentos
                b.HasOne(t => t.ContaOrigem)
                 .WithMany()
                 .HasForeignKey(t => t.IdContaCorrenteOrigem)
                 .HasPrincipalKey(cc => cc.IdContaCorrente)
                 .OnDelete(DeleteBehavior.Restrict);

                b.HasOne(t => t.ContaDestino)
                 .WithMany()
                 .HasForeignKey(t => t.IdContaCorrenteDestino)
                 .HasPrincipalKey(cc => cc.IdContaCorrente)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuração da tabela idempotencia
            builder.Entity<Idempotencia>(b =>
            {
                b.ToTable("idempotencia");
                b.HasKey(i => i.ChaveIdempotencia);
                b.Property(i => i.ChaveIdempotencia).HasColumnName("chave_idempotencia").HasMaxLength(37).IsRequired();
                b.Property(i => i.Requisicao).HasColumnName("requisicao").HasMaxLength(1000);
                b.Property(i => i.Resultado).HasColumnName("resultado").HasMaxLength(1000);
            });

            // Configuração da tabela tarifa
            builder.Entity<Tarifa>(b =>
            {
                b.ToTable("tarifa");
                b.HasKey(t => t.IdTarifa);
                b.Property(t => t.IdTarifa).HasMaxLength(37).IsRequired();
                b.Property(t => t.IdContaCorrente).HasMaxLength(37).IsRequired();
                b.Property(t => t.DataMovimento).HasMaxLength(25).IsRequired();
                b.Property(t => t.Valor).HasColumnType("REAL").IsRequired();

                // Relacionamento
                b.HasOne(t => t.ContaCorrente)
                 .WithMany()
                 .HasForeignKey(t => t.IdContaCorrente)
                 .HasPrincipalKey(cc => cc.IdContaCorrente);
            });

            // Configuração da nova entidade unificada ContaBancaria
            builder.Entity<ContaBancaria>(b =>
            {
                b.ToTable("conta_bancaria");
                b.HasKey(cb => cb.Id);
                b.Property(cb => cb.Id).HasMaxLength(37).IsRequired();
                b.Property(cb => cb.Numero).IsRequired();
                b.Property(cb => cb.Cpf).HasMaxLength(11).IsRequired();
                b.Property(cb => cb.SenhaHash).HasMaxLength(100).IsRequired();
                b.Property(cb => cb.Salt).HasMaxLength(100).IsRequired();
                b.Property(cb => cb.Ativo).IsRequired().HasDefaultValue(true);
                b.Property(cb => cb.DataCriacao).IsRequired();
                b.Property(cb => cb.DataInativacao);

                // Índices únicos
                b.HasIndex(cb => cb.Numero).IsUnique();
                b.HasIndex(cb => cb.Cpf).IsUnique();

                // Relacionamento com movimentos
                b.HasMany(cb => cb.Movimentos)
                 .WithOne()
                 .HasForeignKey(m => m.IdContaCorrente)
                 .HasPrincipalKey(cb => cb.Id);
            });

            // Tabelas do Identity
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRole>().ToTable("Roles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().ToTable("UserRoles");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserClaim<string>>().ToTable("UserClaims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserLogin<string>>().ToTable("UserLogins");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>>().ToTable("RoleClaims");
            builder.Entity<Microsoft.AspNetCore.Identity.IdentityUserToken<string>>().ToTable("UserTokens");
        }
    }
}
