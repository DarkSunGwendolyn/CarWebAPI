using CarWebAPI.DTO;
using CarWebAPI.Enums;
using Confluent.Kafka;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace CarWebAPI.AuthTests;

public class AuthTests
{
    private readonly HttpClient _gateway;
    //private readonly HttpClient _userApi;

    //данные пользователя

    private const string TestEmail = "test_c2a3c3be-6fa5-474b-9ceb-d0de69b9ce5e@test.com";
    private const string TestPassword = "123456";
    private const string TestId = "69eb4787fc081755b845462e";

    public AuthTests()
    {
        _gateway = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:5218")
        };

        //_userApi = new HttpClient
        //{
        //    BaseAddress = new Uri("http://localhost:5220")
        //};
    }

    [Fact]
    public async Task Access_Without_Token()
    {
        var response = await _gateway.PostAsJsonAsync("/gateway/car", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Access_With_Invalid_Token()
    {
        _gateway.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", "invalid.token");

        var response = await _gateway.PostAsJsonAsync("/gateway/car", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_Return_Token()
    {
        var token = await GetToken();
        Assert.False(string.IsNullOrEmpty(token));
    }

    [Fact]
    public async Task Access_With_Valid_Token()
    {
        var token = await GetToken();
        _gateway.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var createCar = new CreateCarDTO
        {
            ConfirmedBy = TestId,
            Brand = $"ToyotaTest",
            Model = $"CamryTest",
            Year = 2000,
            Price = 10000,
            Color = 0,
            BodyType = 0,
        };

        var response = await _gateway.PostAsJsonAsync("/gateway/car", createCar);
        Assert.NotEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }



    [Fact]
    public async Task Access_With_Expired_Token()
    {
        var expiredToken = CreateExpiredToken();
        _gateway.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", expiredToken);

        var response = await _gateway.PostAsJsonAsync("/gateway/car", new { });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> GetToken()
    {
        var login = new { Email = TestEmail, Password = TestPassword };
        var loginResponse = await _gateway.PostAsJsonAsync("/gateway/Auth/login", login);

        if (loginResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception("Логин не сработал, проверь Email и Password в тесте.");

        var json = await loginResponse.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()
               ?? throw new Exception("Токен пустой");
    }

    private string CreateExpiredToken()
    {
        var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes("SuperSecretKeyForJwtTokenGeneration"));

        var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(
            key,
            Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

        var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
            issuer: "CarSystem",
            audience: "CarSystemUsers",
            expires: DateTime.UtcNow.AddMinutes(-10),
            signingCredentials: creds
        );

        return new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler().WriteToken(token);
    }
}