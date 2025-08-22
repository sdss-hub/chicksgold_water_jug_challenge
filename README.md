# Water Jug Challenge API

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
   - **Swagger UI:** `http://localhost:7000/swagger` (interactive documentation)
   - **Health Check:** `http://localhost:7000/health`
   - **API Info:** `http://localhost:7000/api/waterjug/info`

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
