using EstudoApi.Banking.Transfer.Commands;
using Xunit;

namespace EstudoApi.Banking.Tests;

public class UnitTest1
{
    [Fact]
    public void TransferCommand_Properties_Should_Work()
    {
        // Arrange & Act
        var command = new TransferCommand
        {
            ContaOrigem = 123456,
            ContaDestino = 789012,
            Valor = 100.50m,
            Descricao = "Teste unitário",
            RequisicaoId = "test-key-001"
        };

        // Assert
        Assert.Equal(123456, command.ContaOrigem);
        Assert.Equal(789012, command.ContaDestino);
        Assert.Equal(100.50m, command.Valor);
        Assert.Equal("Teste unitário", command.Descricao);
        Assert.Equal("test-key-001", command.RequisicaoId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("test-value")]
    public void TransferCommand_Should_Handle_String_Values(string value)
    {
        // Arrange & Act
        var command = new TransferCommand
        {
            ContaOrigem = 123456,
            ContaDestino = 789012,
            Valor = 100.00m,
            Descricao = value,
            RequisicaoId = "test-key"
        };

        // Assert
        Assert.Equal(value, command.Descricao);
    }

    [Fact]
    public void TransferCommand_Should_Accept_Decimal_Values()
    {
        // Arrange
        var command = new TransferCommand();

        // Act
        command.Valor = 999.99m;

        // Assert
        Assert.Equal(999.99m, command.Valor);
    }
}

















