using WaterJugChallenge.Models;

namespace WaterJugChallenge.Services;

public interface IWaterJugSolver
{
    WaterJugResponse Solve(int xCapacity, int yCapacity, int zTarget);
}

public class WaterJugSolver : IWaterJugSolver
{
    private class State
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Action { get; set; } = string.Empty;
        public State? Parent { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is State other)
                return X == other.X && Y == other.Y;
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }

    public WaterJugResponse Solve(int xCapacity, int yCapacity, int zTarget)
    {
        if (!IsSolvable(xCapacity, yCapacity, zTarget))
        {
            return new WaterJugResponse
            {
                IsSolvable = false,
                Message = "No solution possible",
                TotalSteps = 0
            };
        }

        if (zTarget == 0)
        {
            return new WaterJugResponse
            {
                Solution = new List<SolutionStep>
                {
                    new SolutionStep
                    {
                        Step = 1,
                        BucketX = 0,
                        BucketY = 0,
                        Action = "Both buckets are already empty",
                        Status = "Solved"
                    }
                },
                IsSolvable = true,
                TotalSteps = 1
            };
        }

        var queue = new Queue<State>();
        var visited = new HashSet<State>();
        var startState = new State { X = 0, Y = 0, Action = "Initial state" };
        
        queue.Enqueue(startState);
        visited.Add(startState);

        while (queue.Count > 0)
        {
            var currentState = queue.Dequeue();

            if (currentState.X == zTarget || currentState.Y == zTarget)
            {
                var solution = ReconstructPath(currentState);
                return new WaterJugResponse
                {
                    Solution = solution,
                    IsSolvable = true,
                    TotalSteps = solution.Count
                };
            }

            var nextStates = GetNextStates(currentState, xCapacity, yCapacity);

            foreach (var nextState in nextStates)
            {
                if (!visited.Contains(nextState))
                {
                    visited.Add(nextState);
                    queue.Enqueue(nextState);
                }
            }
        }

        return new WaterJugResponse
        {
            IsSolvable = false,
            Message = "No solution found",
            TotalSteps = 0
        };
    }

    private bool IsSolvable(int xCapacity, int yCapacity, int zTarget)
    {
        if (zTarget == 0) return true;

        if (zTarget > Math.Max(xCapacity, yCapacity)) return false;

        return zTarget % CalculateGCD(xCapacity, yCapacity) == 0;
    }

    private int CalculateGCD(int a, int b)
    {
        while (b != 0)
        {
            int temp = b;
            b = a % b;
            a = temp;
        }
        return a;
    }

    private List<State> GetNextStates(State current, int xCapacity, int yCapacity)
    {
        var states = new List<State>();

        if (current.X < xCapacity)
        {
            states.Add(new State
            {
                X = xCapacity,
                Y = current.Y,
                Action = "Fill bucket X",
                Parent = current
            });
        }

        if (current.Y < yCapacity)
        {
            states.Add(new State
            {
                X = current.X,
                Y = yCapacity,
                Action = "Fill bucket Y",
                Parent = current
            });
        }

        if (current.X > 0)
        {
            states.Add(new State
            {
                X = 0,
                Y = current.Y,
                Action = "Empty bucket X",
                Parent = current
            });
        }
        if (current.Y > 0)
        {
            states.Add(new State
            {
                X = current.X,
                Y = 0,
                Action = "Empty bucket Y",
                Parent = current
            });
        }

        if (current.X > 0 && current.Y < yCapacity)
        {
            int transferAmount = Math.Min(current.X, yCapacity - current.Y);
            states.Add(new State
            {
                X = current.X - transferAmount,
                Y = current.Y + transferAmount,
                Action = "Transfer from bucket X to Y",
                Parent = current
            });
        }

        if (current.Y > 0 && current.X < xCapacity)
        {
            int transferAmount = Math.Min(current.Y, xCapacity - current.X);
            states.Add(new State
            {
                X = current.X + transferAmount,
                Y = current.Y - transferAmount,
                Action = "Transfer from bucket Y to X",
                Parent = current
            });
        }

        return states;
    }

    private List<SolutionStep> ReconstructPath(State finalState)
    {
        var path = new List<State>();
        var current = finalState;

        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Reverse();

        var solution = new List<SolutionStep>();
        for (int i = 1; i < path.Count; i++)
        {
            var step = new SolutionStep
            {
                Step = i,
                BucketX = path[i].X,
                BucketY = path[i].Y,
                Action = path[i].Action
            };

            if (i == path.Count - 1)
            {
                step.Status = "Solved";
            }

            solution.Add(step);
        }

        return solution;
    }
}