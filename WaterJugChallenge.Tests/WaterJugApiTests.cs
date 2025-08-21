using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using FluentAssertions;
using WaterJugChallenge.Models;

namespace WaterJugChallenge.Tests;

public class WaterJugApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public WaterJugApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    [Fact]
    public async Task SolveEndpoint_WithValidRequirementExample_ShouldReturnOptimalSolution()
    {
        var request = new WaterJugRequest
        {
            XCapacity = 2,
            YCapacity = 10,
            ZAmountWanted = 4
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/waterjug/solve", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WaterJugResponse>(responseJson, _jsonOptions);

        result.Should().NotBeNull();
        result!.IsSolvable.Should().BeTrue();
        result.Solution.Should().NotBeNull();
        result.Solution!.Should().HaveCount(4); // Exact steps from requirements
        result.Solution!.Last().Status.Should().Be("Solved");
        result.TotalSteps.Should().Be(4);
    }

    [Fact]
    public async Task SolveEndpoint_WithImpossibleCase_ShouldReturnNoSolution()
    {
        var request = new WaterJugRequest
        {
            XCapacity = 2,
            YCapacity = 6,
            ZAmountWanted = 5
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/waterjug/solve", content);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<WaterJugResponse>(responseJson, _jsonOptions);

        result.Should().NotBeNull();
        result!.IsSolvable.Should().BeFalse();
        result.Message.Should().Be("No solution possible");
        result.TotalSteps.Should().Be(0);
    }

    [Theory]
    [InlineData(-1, 5, 3, "X capacity must be a positive integer")]
    [InlineData(5, -1, 3, "Y capacity must be a positive integer")]
    [InlineData(0, 5, 3, "X capacity must be a positive integer")]
    [InlineData(5, 0, 3, "Y capacity must be a positive integer")]
    [InlineData(5, 3, -1, "Target amount must be a non-negative integer")]
    [InlineData(2, 3, 10, "Target amount cannot exceed the capacity of the larger jug")]
    public async Task SolveEndpoint_WithInvalidInput_ShouldReturnValidationError(int x, int y, int z, string expectedError)
    {
        var request = new WaterJugRequest
        {
            XCapacity = x,
            YCapacity = y,
            ZAmountWanted = z
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/waterjug/solve", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ErrorResponse>(responseJson, _jsonOptions);

        result.Should().NotBeNull();
        result!.Error.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors!.Should().Contain(expectedError);
    }

    [Fact]
    public async Task InfoEndpoint_ShouldReturnApiInformation()
    {
        var response = await _client.GetAsync("/api/waterjug/info");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson, _jsonOptions);

        result.GetProperty("name").GetString().Should().Be("Water Jug Challenge API");
        result.GetProperty("version").GetString().Should().Be("1.0.0");
        result.GetProperty("endpoints").Should().NotBeNull();
    }

    [Fact]
    public async Task HealthEndpoint_ShouldReturnHealthyStatus()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson, _jsonOptions);

        result.GetProperty("status").GetString().Should().Be("Healthy");
        result.GetProperty("version").GetString().Should().Be("1.0.0");
    }

    [Fact]
    public async Task SolveEndpoint_CachingBehavior_ShouldReturnCachedResults()
    {
        var request = new WaterJugRequest
        {
            XCapacity = 3,
            YCapacity = 5,
            ZAmountWanted = 4
        };

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content1 = new StringContent(json, Encoding.UTF8, "application/json");
        var content2 = new StringContent(json, Encoding.UTF8, "application/json");

        var response1 = await _client.PostAsync("/api/waterjug/solve", content1);
        var response2 = await _client.PostAsync("/api/waterjug/solve", content2);

        response1.StatusCode.Should().Be(HttpStatusCode.OK);
        response2.StatusCode.Should().Be(HttpStatusCode.OK);

        var result1Json = await response1.Content.ReadAsStringAsync();
        var result2Json = await response2.Content.ReadAsStringAsync();

        var result1 = JsonSerializer.Deserialize<WaterJugResponse>(result1Json, _jsonOptions);
        var result2 = JsonSerializer.Deserialize<WaterJugResponse>(result2Json, _jsonOptions);

        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result2!.FromCache.Should().BeTrue();
    }

    [Fact]
    public async Task SolveEndpoint_WithMalformedJson_ShouldReturnBadRequest()
    {
        var content = new StringContent("{ invalid json }", Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/waterjug/solve", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RootEndpoint_ShouldReturnApiInformation()
    {
        var response = await _client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<JsonElement>(responseJson, _jsonOptions);

        result.GetProperty("message").GetString().Should().Be("Water Jug Challenge API");
    }
}