using System.ComponentModel.DataAnnotations;

namespace WaterJugChallenge.Models;

public class WaterJugRequest
{
    public int XCapacity { get; set; }
    public int YCapacity { get; set; }

    [Required(ErrorMessage = "Target amount is required")]
    public int ZAmountWanted { get; set; }
}

public class WaterJugResponse
{
    public List<SolutionStep>? Solution { get; set; }
    public string? Message { get; set; }
    public bool IsSolvable { get; set; }
    public int TotalSteps { get; set; }
    public bool FromCache { get; set; } = false;
}

public class SolutionStep
{
    public int Step { get; set; }
    public int BucketX { get; set; }
    public int BucketY { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Status { get; set; }
}

public class ErrorResponse
{
    public string Error { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string>? ValidationErrors { get; set; }
}