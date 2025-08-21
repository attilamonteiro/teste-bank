using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EstudoApi.Models
{
    public class Product: BaseModel
    {
        public string Name { get; set; }
        public decimal Price { get; private set; }

        public Product() { }
        public Product(int id, string nome, decimal price)
        {
            Id = id;
            Name = nome;
            Price = price;
        }
    }
}
