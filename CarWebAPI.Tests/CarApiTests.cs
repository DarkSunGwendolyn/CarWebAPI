using CarWebAPI.DTO;
using CarWebAPI.Enums;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System.Net.Http.Json;
using Xunit;

namespace CarWebAPI.Tests;

public class CarApiTests
{
    private readonly HttpClient _client;

    public CarApiTests()
    {
        _client = new HttpClient();
        _client.BaseAddress = new Uri("http://localhost:5219");
    }

    [Fact]
    public async Task CreateCar_1000CarsForOneUser()
    {
        
        string existingUserId = "69e3329f7b2dd7438d4c99af";

        int successCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            var createCar = new CreateCarDTO
            {
                ConfirmedBy = existingUserId,
                Brand = $"Toyota_{i}",
                Model = $"Camry_{i}",
                Year = 2000 + (i % 24),
                Price = 10000 + i,
                Color = (CarColor)(i % 4),
                BodyType = (BodyType)(i % 4)
            };

            var response = await _client.PostAsJsonAsync("/api/Car", createCar);

            if (response.IsSuccessStatusCode)
                successCount++;
        }

        Assert.Equal(1000, successCount);
        Console.WriteLine($"Создано 1000 машин для пользователя {existingUserId}");
    }
}