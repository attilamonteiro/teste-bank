using System.Collections.Generic;
using System.Threading.Tasks;
using EstudoApi.Models;
using EstudoApi.Request;

namespace EstudoApi.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetProducts();
        Task<Product> GetProduct(int id);
        Task AddProduct(BaseRequest product);
        Task UpdateProduct(int id, BaseRequest product);
        Task DeleteProduct(Product product);
    }
}
