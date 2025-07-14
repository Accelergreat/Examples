using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestApi.Models;

namespace TestApi.Services;

public class ProductService : IProductService
{
    private readonly List<Product> _products;
    private int _nextId = 1;

    public ProductService()
    {
        _products = new List<Product>
        {
            new Product 
            { 
                Id = _nextId++, 
                Name = "Laptop", 
                Description = "High-performance laptop", 
                Price = 999.99m, 
                Stock = 10,
                CreatedAt = DateTime.UtcNow 
            },
            new Product 
            { 
                Id = _nextId++, 
                Name = "Smartphone", 
                Description = "Latest model smartphone", 
                Price = 699.99m, 
                Stock = 25,
                CreatedAt = DateTime.UtcNow 
            },
            new Product 
            { 
                Id = _nextId++, 
                Name = "Headphones", 
                Description = "Wireless noise-canceling headphones", 
                Price = 199.99m, 
                Stock = 15,
                CreatedAt = DateTime.UtcNow 
            }
        };
        _nextId = 4;
    }

    public Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        return Task.FromResult(_products.Where(p => p.IsAvailable).AsEnumerable());
    }

    public Task<Product?> GetProductByIdAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsAvailable);
        return Task.FromResult(product);
    }

    public Task<Product> CreateProductAsync(Product product)
    {
        product.Id = _nextId++;
        product.CreatedAt = DateTime.UtcNow;
        product.IsAvailable = true;
        _products.Add(product);
        return Task.FromResult(product);
    }

    public Task<Product?> UpdateProductAsync(int id, Product product)
    {
        var existingProduct = _products.FirstOrDefault(p => p.Id == id && p.IsAvailable);
        if (existingProduct == null)
            return Task.FromResult<Product?>(null);

        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.Stock = product.Stock;
        return Task.FromResult<Product?>(existingProduct);
    }

    public Task<bool> DeleteProductAsync(int id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id && p.IsAvailable);
        if (product == null)
            return Task.FromResult(false);

        product.IsAvailable = false;
        return Task.FromResult(true);
    }

    public Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
    {
        var results = _products.Where(p => 
            p.IsAvailable && 
            (p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
             p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)));
        
        return Task.FromResult(results.AsEnumerable());
    }
} 