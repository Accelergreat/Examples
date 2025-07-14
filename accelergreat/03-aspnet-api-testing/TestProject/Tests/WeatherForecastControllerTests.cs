using System;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Accelergreat.Environments.Pooling;
using Accelergreat.Xunit;
using AspNetApiExample.Components;
using FluentAssertions;
using Newtonsoft.Json;
using TestApi.Models;
using Xunit;

namespace AspNetApiExample.Tests;

public class WeatherForecastControllerTests : AccelergreatXunitTest
{
    public WeatherForecastControllerTests(IAccelergreatEnvironmentPool environmentPool) : base(environmentPool)
    {
    }

    [Fact]
    public async Task GetWeatherForecast_ShouldReturnDefaultFiveDayForecast()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/weatherforecast");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var forecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(content);

        forecasts.Should().NotBeNull();
        forecasts!.Should().HaveCount(5);
        forecasts.Should().OnlyContain(f => f.Date > DateOnly.FromDateTime(DateTime.Now));
        forecasts.Should().OnlyContain(f => f.TemperatureC >= -20 && f.TemperatureC <= 55);
        forecasts.Should().OnlyContain(f => !string.IsNullOrEmpty(f.Summary));
    }

    [Fact]
    public async Task GetWeatherForecastForDays_ShouldReturnSpecifiedNumberOfDays()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();
        const int requestedDays = 7;

        // Act
        var response = await client.GetAsync($"/api/weatherforecast/{requestedDays}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var forecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(content);

        forecasts.Should().NotBeNull();
        forecasts!.Should().HaveCount(requestedDays);
    }

    [Fact]
    public async Task GetWeatherForecastForDays_WithInvalidDays_ShouldReturnBadRequest()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act & Assert - Test zero days
        var response = await client.GetAsync("/api/weatherforecast/0");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act & Assert - Test negative days
        response = await client.GetAsync("/api/weatherforecast/-5");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        // Act & Assert - Test too many days
        response = await client.GetAsync("/api/weatherforecast/50");
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCurrentWeatherForecast_ShouldReturnSingleForecast()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/weatherforecast/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var forecast = JsonConvert.DeserializeObject<WeatherForecast>(content);

        forecast.Should().NotBeNull();
        forecast!.Date.Should().Be(DateOnly.FromDateTime(DateTime.Now));
        forecast.TemperatureC.Should().BeInRange(-20, 55);
        forecast.Summary.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TemperatureFahrenheitCalculation_ShouldBeCorrect()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act
        var response = await client.GetAsync("/api/weatherforecast/current");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var forecast = JsonConvert.DeserializeObject<WeatherForecast>(content);

        forecast.Should().NotBeNull();
        
        // Verify Fahrenheit calculation: F = C * 9/5 + 32
        var expectedF = 32 + (int)(forecast!.TemperatureC / 0.5556);
        forecast.TemperatureF.Should().Be(expectedF);
    }

    [Fact]
    public async Task MultipleParallelRequests_ShouldAllSucceed()
    {
        // Arrange
        var webAppComponent = GetComponent<TestApiWebAppComponent>();
        var client = webAppComponent.CreateClient();

        // Act - Make multiple parallel requests
        var tasks = Enumerable.Range(1, 10)
            .Select(_ => client.GetAsync("/api/weatherforecast/current"))
            .ToArray();

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
        
        // Verify all responses contain valid data
        foreach (var response in responses)
        {
            var content = await response.Content.ReadAsStringAsync();
            var forecast = JsonConvert.DeserializeObject<WeatherForecast>(content);
            
            forecast.Should().NotBeNull();
            forecast!.Summary.Should().NotBeNullOrEmpty();
        }
    }
} 