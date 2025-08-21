using EstudoApi.Models;
using EstudoApi.Request;

namespace EstudoApi.Interfaces.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProduct(int id);
        Task AddProduct(BaseRequest product);
        Task UpdateProduct(int id, BaseRequest product);
        Task DeleteProduct(Product product);
    }
}
