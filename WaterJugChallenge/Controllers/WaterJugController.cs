using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using FluentValidation;
using WaterJugChallenge.Models;
using WaterJugChallenge.Services;

namespace WaterJugChallenge.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WaterJugController : ControllerBase
{
    private readonly IWaterJugSolver _solver;
    private readonly IMemoryCache _cache;
    private readonly IValidator<WaterJugRequest> _validator;
    private readonly ILogger<WaterJugController> _logger;

    public WaterJugController(
        IWaterJugSolver solver, 
        IMemoryCache cache, 
        IValidator<WaterJugRequest> validator,
        ILogger<WaterJugController> logger)
    {
        _solver = solver;
        _cache = cache;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("solve")]
    [ProducesResponseType(typeof(WaterJugResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WaterJugResponse>> SolveWaterJugProblem([FromBody] WaterJugRequest request)
    {
        try
        {
            var validationResult = await _validator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errorResponse = new ErrorResponse
                {
                    Error = "Validation failed",
                    Message = "Invalid input parameters",
                    ValidationErrors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
                };
                
                _logger.LogWarning("Validation failed for water jug problem: X={X}, Y={Y}, Z={Z}. Errors: {Errors}", 
                    request.XCapacity, request.YCapacity, request.ZAmountWanted, 
                    string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage)));
                
                return BadRequest(errorResponse);
            }

            var cacheKey = $"waterjug_{request.XCapacity}_{request.YCapacity}_{request.ZAmountWanted}";

            if (_cache.TryGetValue(cacheKey, out WaterJugResponse? cachedResult) && cachedResult != null)
            {
                _logger.LogInformation("Serving cached result for X={X}, Y={Y}, Z={Z}. Solvable: {IsSolvable}", 
                    request.XCapacity, request.YCapacity, request.ZAmountWanted, cachedResult.IsSolvable);
                cachedResult.FromCache = true;
                return Ok(cachedResult);
            }

            _logger.LogInformation("Attempting to solve water jug problem: X={X}, Y={Y}, Z={Z}", 
                request.XCapacity, request.YCapacity, request.ZAmountWanted);

            var result = _solver.Solve(request.XCapacity, request.YCapacity, request.ZAmountWanted);

            if (result.IsSolvable)
            {
                _logger.LogInformation("Water jug problem solved successfully: X={X}, Y={Y}, Z={Z}. Steps: {TotalSteps}", 
                    request.XCapacity, request.YCapacity, request.ZAmountWanted, result.TotalSteps);
            }
            else
            {
                _logger.LogInformation("Water jug problem has no solution: X={X}, Y={Y}, Z={Z}. Reason: {Message}", 
                    request.XCapacity, request.YCapacity, request.ZAmountWanted, result.Message);
            }

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),
                SlidingExpiration = TimeSpan.FromMinutes(30)
            };
            _cache.Set(cacheKey, result, cacheOptions);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while solving water jug problem: X={X}, Y={Y}, Z={Z}", 
                request.XCapacity, request.YCapacity, request.ZAmountWanted);
                
            var errorResponse = new ErrorResponse
            {
                Error = "Internal server error",
                Message = "An error occurred while processing your request"
            };
            return StatusCode(500, errorResponse);
        }
    }
    [HttpGet("info")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult GetApiInfo()
    {
        _logger.LogInformation("API info requested");
        
        return Ok(new
        {
            Name = "Water Jug Challenge API",
            Version = "1.0.0",
            Description = "Solves the classic water jug riddle using optimal algorithms",
            Endpoints = new
            {
                Solve = "POST /api/waterjug/solve",
                Info = "GET /api/waterjug/info",
                Health = "GET /health"
            },
            SampleRequest = new
            {
                XCapacity = 2,
                YCapacity = 10,
                ZAmountWanted = 4
            }
        });
    }
}