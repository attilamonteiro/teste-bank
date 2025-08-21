using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EstudoApi.Models;
using EstudoApi.Request;
using EstudoApi.Interfaces.Repositories;
using EstudoApi.Infrastructure.Contexts;

namespace EstudoApi.Infrastructure.Repositories
{
    public class ProductRepository: IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ProductRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            return await _context.Products.ToListAsync();
        }

        public async Task<Product> GetProduct(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task AddProduct(BaseRequest product)
        {
            var produto = _mapper.Map<Product>(product);
            produto.Id = 0;

            _context.Products.Add(produto);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(int id, BaseRequest productRequest)
        {
            var existingProduct = await GetProduct(id);
            if (existingProduct == null)
            {
                throw new KeyNotFoundException("Product not found");
            }
            _mapper.Map(productRequest, existingProduct);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
