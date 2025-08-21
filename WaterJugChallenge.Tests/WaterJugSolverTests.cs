using Xunit;
using FluentAssertions;
using WaterJugChallenge.Services;

namespace WaterJugChallenge.Tests;

public class WaterJugSolverTests
{
    private readonly IWaterJugSolver _solver;

    public WaterJugSolverTests()
    {
        _solver = new WaterJugSolver();
    }

    [Fact]
    public void Solve_WithRequiredExampleCase_ShouldReturnOptimalSolution()
    {
        int x = 2, y = 10, z = 4;

        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeTrue();
        result.Solution.Should().NotBeNull();
        result.Solution!.Should().HaveCount(4); 
        result.Solution!.Last().Status.Should().Be("Solved");
        
        var finalStep = result.Solution!.Last();
        (finalStep.BucketX == z || finalStep.BucketY == z).Should().BeTrue();
        
        result.Solution![0].Action.Should().Be("Fill bucket X");
        result.Solution![1].Action.Should().Be("Transfer from bucket X to Y");
        result.Solution![2].Action.Should().Be("Fill bucket X");
        result.Solution![3].Action.Should().Be("Transfer from bucket X to Y");
    }

    [Fact]
    public void Solve_WithLargeJugExample_ShouldReturnOptimalSolution()
    {
        int x = 2, y = 100, z = 96;

        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeTrue();
        result.Solution.Should().NotBeNull();
        result.Solution!.Should().HaveCount(4); 
        result.Solution!.Last().Status.Should().Be("Solved");
        
        var finalStep = result.Solution!.Last();
        finalStep.BucketY.Should().Be(96); 
    }

    [Fact]
    public void Solve_WithImpossibleCase_ShouldReturnNoSolution()
    {
        int x = 2, y = 6, z = 5;

        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeFalse();
        result.Solution.Should().BeNull();
        result.Message.Should().Be("No solution possible");
        result.TotalSteps.Should().Be(0);
    }

    [Fact]
    public void Solve_WithTargetZero_ShouldReturnImmediateSolution()
    {
        int x = 5, y = 3, z = 0;

        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeTrue();
        result.Solution.Should().NotBeNull();
        result.Solution!.Should().HaveCount(1);
        result.Solution![0].BucketX.Should().Be(0);
        result.Solution![0].BucketY.Should().Be(0);
        result.Solution![0].Status.Should().Be("Solved");
    }

    [Theory]
    [InlineData(3, 5, 1)]
    [InlineData(3, 5, 2)]
    [InlineData(3, 5, 3)]
    [InlineData(3, 5, 4)]
    [InlineData(3, 5, 5)]
    public void Solve_WithSolvableCases_ShouldFindOptimalSolution(int x, int y, int z)
    {
        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeTrue();
        result.Solution.Should().NotBeNull();
        result.Solution!.Should().NotBeEmpty();
        
        var finalStep = result.Solution!.Last();
        (finalStep.BucketX == z || finalStep.BucketY == z).Should().BeTrue();
    }

    [Theory]
    [InlineData(2, 4, 3)] 
    [InlineData(6, 9, 2)] 
    [InlineData(4, 6, 5)] 
    public void Solve_WithMathematicallyImpossibleCases_ShouldReturnNoSolution(int x, int y, int z)
    {
        var result = _solver.Solve(x, y, z);

        result.IsSolvable.Should().BeFalse();
        result.Message.Should().Be("No solution possible");
    }

    [Fact]
    public void Solve_WithPerformanceTest_ShouldCompleteQuickly()
    {
        int x = 137, y = 251, z = 17;

        var startTime = DateTime.UtcNow;
        var result = _solver.Solve(x, y, z);
        var duration = DateTime.UtcNow - startTime;

        result.Should().NotBeNull();
        duration.Should().BeLessThan(TimeSpan.FromSeconds(5)); 
        
        if (result.IsSolvable)
        {
            var finalStep = result.Solution!.Last();
            (finalStep.BucketX == z || finalStep.BucketY == z).Should().BeTrue();
        }
    }
}