using MinimalAPIs.Dominio.Entidades;

namespace Tests.Dominio.Entidades;

[TestClass]
public class VeiculoTest
{
    [TestMethod]
    public void TestarGetSetPropriedades()
    {
        // Arrange
        var veiculo = new Veiculo();

        // Act
        veiculo.Id = 1;
        veiculo.Nome = "nome";
        veiculo.Marca = "marca";
        veiculo.Ano = 1980;

        // Assert
        Assert.AreEqual(1, veiculo.Id);
        Assert.AreEqual("nome", veiculo.Nome);
        Assert.AreEqual("marca", veiculo.Marca);
        Assert.AreEqual(1980, veiculo.Ano);
    }
}