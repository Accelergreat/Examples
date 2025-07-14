using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using TestApi.Models;

namespace TestApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet]
    public ActionResult<IEnumerable<WeatherForecast>> Get()
    {
        var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(forecasts);
    }

    [HttpGet("{days:int}")]
    public ActionResult<IEnumerable<WeatherForecast>> GetForDays(int days)
    {
        if (days <= 0 || days > 30)
            return BadRequest("Days must be between 1 and 30");

        var forecasts = Enumerable.Range(1, days).Select(index => new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();

        return Ok(forecasts);
    }

    [HttpGet("current")]
    public ActionResult<WeatherForecast> GetCurrent()
    {
        var current = new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        };

        return Ok(current);
    }
} 