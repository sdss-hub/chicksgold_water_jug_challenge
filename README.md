# Water Jug Challenge API

A C# .NET 8 Web API solution for the classic Water Jug Riddle that finds the optimal sequence of steps to measure exactly Z gallons using two jugs of capacities X and Y gallons.


##  Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022, VS Code, or any .NET-compatible IDE

### Setup & Run

1. **Clone the repository:**
   ```bash
   git clone https://github.com/sdss-hub/chicksgold_water_jug_challenge.git
   cd chicksgold_water_jug_challenge
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run --project WaterJugChallenge
   ```

4. **Access the API:**
   - **Swagger UI:** `https://localhost:7000` (interactive documentation)
   - **Health Check:** `https://localhost:7000/health`
   - **API Info:** `https://localhost:7000/api/waterjug/info`

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

##  API Documentation


**POST** `/api/waterjug/solve`

Solves the water jug riddle and returns the optimal sequence of steps.

**Request Body:**
```json
{
  "xCapacity": 2,
  "yCapacity": 10,
  "zAmountWanted": 4
}
```

**Parameters:**
- `xCapacity` (int): Capacity of the first jug (must be positive)
- `yCapacity` (int): Capacity of the second jug (must be positive)
- `zAmountWanted` (int): Target amount to measure (must be non-negative)

**Success Response (200 OK):**
```json
{
  "solution": [
    {"step": 1, "bucketX": 2, "bucketY": 0, "action": "Fill bucket X"},
    {"step": 2, "bucketX": 0, "bucketY": 2, "action": "Transfer from bucket X to Y"},
    {"step": 3, "bucketX": 2, "bucketY": 2, "action": "Fill bucket X"},
    {"step": 4, "bucketX": 0, "bucketY": 4, "action": "Transfer from bucket X to Y", "status": "Solved"}
  ],
  "isSolvable": true,
  "totalSteps": 4,
  "fromCache": false
}
```

**No Solution Response (200 OK):**
```json
{
  "solution": null,
  "message": "No solution possible",
  "isSolvable": false,
  "totalSteps": 0,
  "fromCache": false
}
```

**Validation Error Response (400 Bad Request):**
```json
{
  "error": "Validation failed",
  "message": "Invalid input parameters",
  "validationErrors": [
    "X capacity must be a positive integer",
    "Target amount cannot exceed the capacity of the larger jug"
  ]
}
```

#### API Information
**GET** `/api/waterjug/info`

Returns API metadata and usage information.

#### Health Check
**GET** `/health`

Returns API health status and uptime information.

##  Algorithm Explanation

### Approach: Breadth-First Search (BFS)

This solution uses **Breadth-First Search** to guarantee finding the **optimal** (minimum steps) solution.

#### I used BFS because of: 
- **Optimality Guarantee**: BFS explores states level by level, ensuring the first solution found uses the minimum number of steps
- **Completeness**: If a solution exists, BFS will always find it
- **Efficiency**: Avoids exploring unnecessarily deep paths

#### Mathematical Foundation

A target amount `Z` is mathematically achievable if and only if:
```
Z % GCD(X, Y) == 0
```

Where `GCD(X, Y)` is the Greatest Common Divisor of the two jug capacities.

**Example:**
- Jugs: X=2, Y=6, Target=5
- GCD(2, 6) = 2
- 5 % 2 = 1 ≠ 0 → **No solution possible**

#### State Space

Each state represents: `(current_water_in_X, current_water_in_Y)`

#### Possible Operations

From any state, up to 6 transitions are possible:

1. **Fill bucket X** - Fill jug X to full capacity
2. **Fill bucket Y** - Fill jug Y to full capacity
3. **Empty bucket X** - Empty jug X completely
4. **Empty bucket Y** - Empty jug Y completely
5. **Transfer from X to Y** - Pour from X to Y until X is empty or Y is full
6. **Transfer from Y to X** - Pour from Y to X until Y is empty or X is full

#### Algorithm Steps

1. **Pre-validation**: Check mathematical feasibility using GCD
2. **Initialize**: Start from state (0, 0) with empty queue and visited set
3. **BFS Loop**: 
   - Dequeue current state
   - Check if target is reached
   - Generate all valid next states
   - Add unvisited states to queue
4. **Path Reconstruction**: Trace back through parent pointers to build solution

#### Complexity Analysis

- **Time Complexity**: O(X × Y) - worst case explores all possible states
- **Space Complexity**: O(X × Y) - for visited states set and BFS queue
- **Practical Performance**: Much faster due to early termination and mathematical pre-checks

##  Example Solutions

### Example 1: Standard Case (X=2, Y=10, Z=4)
```
Initial: (0, 0)
Step 1:  (2, 0)  "Fill bucket X"
Step 2:  (0, 2)  "Transfer from bucket X to Y"
Step 3:  (2, 2)  "Fill bucket X"
Step 4:  (0, 4)  "Transfer from bucket X to Y"  SOLVED
```

### Example 2: Large Jug Case (X=2, Y=100, Z=96)
```
Initial: (0, 0)
Step 1:  (0, 100)  "Fill bucket Y"
Step 2:  (2, 98)   "Transfer from bucket Y to X"
Step 3:  (0, 98)   "Empty bucket X"
Step 4:  (2, 96)   "Transfer from bucket Y to X"  SOLVED
```

### Example 3: Impossible Case (X=2, Y=6, Z=5)
```
GCD(2, 6) = 2
5 % 2 = 1 ≠ 0
Result: "No solution possible"
```

## Technical Implementation Details

### Caching Strategy
- **Memory Cache**: Results cached for 1 hour with 30-minute sliding expiration
- **Cache Key Format**: `waterjug_{X}_{Y}_{Z}`
- **Performance Benefit**: Instant responses for repeated identical requests

### Error Handling
- **Input Validation**: FluentValidation with detailed error messages
- **Exception Handling**: Global exception handling with structured responses
- **HTTP Status Codes**: Proper REST API status code usage

### Security & Performance
- **CORS**: Configured for cross-origin requests
- **Memory Limits**: Reasonable request size limits
- **Early Termination**: Mathematical impossibility detected before expensive search

## Testing Strategy

### Unit Tests
- **Algorithm Correctness**: All requirement examples tested
- **Edge Cases**: Zero target, direct capacity matches
- **Mathematical Cases**: Solvable vs impossible scenarios
- **Performance**: Large capacity stress testing

### Integration Tests  
- **End-to-End API**: Full request/response cycle testing
- **Validation**: All error scenarios with expected messages
- **Caching**: Verification of cache hit behavior
- **HTTP Protocol**: Status codes, headers, JSON formatting

## Development & Testing

### Local Development

```bash
# Watch mode for development
dotnet watch run --project WaterJugChallenge

# Test in watch mode
dotnet test --watch
```

### Manual Testing with cURL

**Valid Request:**
```bash
curl -X POST "https://localhost:7000/api/waterjug/solve" \
  -H "Content-Type: application/json" \
  -d '{
    "xCapacity": 2,
    "yCapacity": 10,
    "zAmountWanted": 4
  }'
```

**Invalid Request:**
```bash
curl -X POST "https://localhost:7000/api/waterjug/solve" \
  -H "Content-Type: application/json" \
  -d '{
    "xCapacity": 2,
    "yCapacity": 6,
    "zAmountWanted": 5
  }'
```

##  Performance Characteristics

### Optimization Features
- **Mathematical Pre-check**: GCD validation before expensive search
- **BFS Efficiency**: Optimal pathfinding without unnecessary exploration  
- **Memory Caching**: Eliminates redundant computations
- **Early Returns**: Immediate responses for edge cases

### Benchmarks
- **Small Problems** (X,Y < 20): < 1ms response time
- **Medium Problems** (X,Y < 100): < 10ms response time  
- **Large Problems** (X,Y < 1000): < 100ms response time

##  Deployment

### Local Deployment
```bash
dotnet publish -c Release -o ./publish
dotnet ./publish/WaterJugChallenge.dll
```

### Docker Support
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "WaterJugChallenge.dll"]
```