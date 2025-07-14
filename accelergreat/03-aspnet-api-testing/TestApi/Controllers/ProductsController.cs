using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestApi.Models;
using TestApi.Services;

namespace TestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetAllProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound($"Product with ID {id} not found");

        return Ok(product);
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term 'q' is required");

        var products = await _productService.SearchProductsAsync(q);
        return Ok(products);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Name is required");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than 0");

        var createdProduct = await _productService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Product>> UpdateProduct(int id, [FromBody] Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
            return BadRequest("Name is required");

        if (product.Price <= 0)
            return BadRequest("Price must be greater than 0");

        var updatedProduct = await _productService.UpdateProductAsync(id, product);
        if (updatedProduct == null)
            return NotFound($"Product with ID {id} not found");

        return Ok(updatedProduct);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> DeleteProduct(int id)
    {
        var deleted = await _productService.DeleteProductAsync(id);
        if (!deleted)
            return NotFound($"Product with ID {id} not found");

        return NoContent();
    }
} 