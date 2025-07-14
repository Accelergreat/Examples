using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using AspNetApiExample.Components;
using FluentAssertions;
using Newtonsoft.Json;
using TestApi.Models;
using Xunit;

namespace AspNetApiExample.Tests;

public class ProductsControllerTests : AccelergreatXunitTest
{
    public ProductsControllerTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task GetAllProducts_ShouldReturnSeededProducts()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonConvert.DeserializeObject<Product[]>(content);

        products.Should().NotBeNull();
        products!.Should().HaveCount(3);
        products.Should().Contain(p => p.Name == "Laptop");
        products.Should().Contain(p => p.Name == "Smartphone");
        products.Should().Contain(p => p.Name == "Headphones");
        products.Should().OnlyContain(p => p.IsAvailable);
        products.Should().OnlyContain(p => p.Price > 0);
    }

    [Fact]
    public async Task GetProductById_WithValidId_ShouldReturnProduct()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int productId = 1;

        // Act
        var response = await client.GetAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var product = JsonConvert.DeserializeObject<Product>(content);

        product.Should().NotBeNull();
        product!.Id.Should().Be(productId);
        product.Name.Should().Be("Laptop");
        product.Price.Should().Be(999.99m);
        product.Stock.Should().Be(10);
    }

    [Fact]
    public async Task GetProductById_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidProductId = 999;

        // Act
        var response = await client.GetAsync($"/api/products/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task SearchProducts_WithValidTerm_ShouldReturnMatchingProducts()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/search?q=phone");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonConvert.DeserializeObject<Product[]>(content);

        products.Should().NotBeNull();
        products!.Should().HaveCount(2); // Smartphone and Headphones
        products.Should().Contain(p => p.Name.Contains("phone", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchProducts_WithEmptyTerm_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/search?q=");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SearchProducts_WithNoMatches_ShouldReturnEmptyArray()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/products/search?q=nonexistent");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var products = JsonConvert.DeserializeObject<Product[]>(content);

        products.Should().NotBeNull();
        products!.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProduct_WithValidData_ShouldCreateProduct()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var newProduct = new Product
        {
            Name = "Tablet",
            Description = "High-resolution tablet",
            Price = 499.99m,
            Stock = 20
        };

        var json = JsonConvert.SerializeObject(newProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var responseContent = await response.Content.ReadAsStringAsync();
        var createdProduct = JsonConvert.DeserializeObject<Product>(responseContent);

        createdProduct.Should().NotBeNull();
        createdProduct!.Name.Should().Be(newProduct.Name);
        createdProduct.Description.Should().Be(newProduct.Description);
        createdProduct.Price.Should().Be(newProduct.Price);
        createdProduct.Stock.Should().Be(newProduct.Stock);
        createdProduct.Id.Should().BeGreaterThan(0);
        createdProduct.IsAvailable.Should().BeTrue();
        createdProduct.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task CreateProduct_WithMissingName_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var invalidProduct = new Product
        {
            Description = "Product without name",
            Price = 100m,
            Stock = 5
            // Name is missing
        };

        var json = JsonConvert.SerializeObject(invalidProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProduct_WithInvalidPrice_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var invalidProduct = new Product
        {
            Name = "Invalid Product",
            Description = "Product with invalid price",
            Price = -10m, // Invalid price
            Stock = 5
        };

        var json = JsonConvert.SerializeObject(invalidProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PostAsync("/api/products", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateProduct_WithValidData_ShouldUpdateProduct()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int productId = 2;

        var updatedProduct = new Product
        {
            Name = "Updated Smartphone",
            Description = "Latest updated smartphone",
            Price = 799.99m,
            Stock = 30
        };

        var json = JsonConvert.SerializeObject(updatedProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/products/{productId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseContent = await response.Content.ReadAsStringAsync();
        var returnedProduct = JsonConvert.DeserializeObject<Product>(responseContent);

        returnedProduct.Should().NotBeNull();
        returnedProduct!.Id.Should().Be(productId);
        returnedProduct.Name.Should().Be(updatedProduct.Name);
        returnedProduct.Description.Should().Be(updatedProduct.Description);
        returnedProduct.Price.Should().Be(updatedProduct.Price);
        returnedProduct.Stock.Should().Be(updatedProduct.Stock);
    }

    [Fact]
    public async Task UpdateProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidProductId = 999;

        var updatedProduct = new Product
        {
            Name = "Updated Product",
            Description = "Updated description",
            Price = 199.99m,
            Stock = 15
        };

        var json = JsonConvert.SerializeObject(updatedProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Act
        var response = await client.PutAsync($"/api/products/{invalidProductId}", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithValidId_ShouldDeleteProduct()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int productId = 3;

        // Act
        var response = await client.DeleteAsync($"/api/products/{productId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify product is no longer accessible
        var getResponse = await client.GetAsync($"/api/products/{productId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteProduct_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int invalidProductId = 999;

        // Act
        var response = await client.DeleteAsync($"/api/products/{invalidProductId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CompleteProductLifecycle_ShouldWork()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        var newProduct = new Product
        {
            Name = "Lifecycle Test Product",
            Description = "Product for testing complete lifecycle",
            Price = 299.99m,
            Stock = 10
        };

        // Act 1: Create product
        var json = JsonConvert.SerializeObject(newProduct);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var createResponse = await client.PostAsync("/api/products", content);

        // Assert 1: Product created
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdProduct = JsonConvert.DeserializeObject<Product>(await createResponse.Content.ReadAsStringAsync());

        // Act 2: Update product
        createdProduct!.Name = "Updated Lifecycle Product";
        createdProduct.Price = 349.99m;
        var updateJson = JsonConvert.SerializeObject(createdProduct);
        var updateContent = new StringContent(updateJson, Encoding.UTF8, "application/json");
        var updateResponse = await client.PutAsync($"/api/products/{createdProduct.Id}", updateContent);

        // Assert 2: Product updated
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act 3: Search for product
        var searchResponse = await client.GetAsync("/api/products/search?q=Lifecycle");

        // Assert 3: Product found in search
        searchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var searchResults = JsonConvert.DeserializeObject<Product[]>(await searchResponse.Content.ReadAsStringAsync());
        searchResults!.Should().Contain(p => p.Id == createdProduct.Id);

        // Act 4: Delete product
        var deleteResponse = await client.DeleteAsync($"/api/products/{createdProduct.Id}");

        // Assert 4: Product deleted
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Act 5: Verify product no longer exists
        var getResponse = await client.GetAsync($"/api/products/{createdProduct.Id}");

        // Assert 5: Product not found
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
} 