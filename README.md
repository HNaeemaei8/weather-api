Weather API - Spaceship Challenge
A RESTful ASP.NET Core Web API that retrieves weather data from the Open-Meteo service, stores it in SQL Server, and provides resilient access to the latest available weather information. This project is specifically designed to operate autonomously in a noisy environment for up to 20 years without manual intervention.

Overview
This project was developed based on strict operational requirements:

Retrieve weather data from an external weather service (Open-Meteo).
Store received data in SQL Server.
Return weather data through a RESTful endpoint.
Handle unreliable network conditions (1% noise).
Continue operating autonomously for 20 years without disk overflow.
Return cached data when the external service is unavailable within 5 seconds.
Return null only on the first run when no cached data exists and the weather service is unreachable.
Technologies
ASP.NET Core Web API (Controllers)
Entity Framework Core
SQL Server
HttpClientFactory
Polly (Resilience & Timeout Policies)
Swagger / OpenAPI
Health Checks
Architecture & Design Principles
1. Architectural Choice: Pragmatic Layered Architecture
The application uses a strictly separated Layered Architecture (Presentation, Business Logic, Data Access) organized via folders. While enterprise patterns like Clean Architecture or Onion Architecture (with separate csproj projects) are popular, they were intentionally avoided here to strictly adhere to the KISS principle requested in the task. For a single-endpoint microservice deployed to a constrained environment like a spaceship, adding unnecessary abstraction layers (Domain, Application, Infrastructure projects) would cause:

Over-engineering: Adding complexity for a simple CRUD-like operation.
Resource Overhead: Increasing deployment size and runtime memory allocation.
Failure Points: More abstractions mean more places where things can go wrong over 20 years.
Instead, Separation of Concerns (SoC) is fully maintained logically: Controllers handle HTTP, Services handle business logic/fallback, and Infrastructure handles external calls. This provides a clean codebase without the bulk of enterprise architecture.

2. API Architecture
Based on the task requirements (*از minimal api استفاده نشود), the application uses traditional Controllers to expose REST endpoints.

3. 20-Year Autonomy (KISS Principle)
To ensure the application runs for 20 years without filling up the spaceship's disk storage, the system does not store historical weather data or use complex hashing/duplicate-checking mechanisms. Instead, the database holds exactly one record. Every successful fetch updates this single record. This guarantees a constant database size and avoids CPU-heavy JSON parsing.

4. 5-Second Client Timeout & Noisy Environment
The network to the weather service is noisy, and the client can wait a maximum of 5 seconds. Polly policies are configured to ensure the total request time stays under 5 seconds:

Per-request Timeout: 1.5 seconds.
Retry Policy: 2 retries with a 100ms delay (handles transient 5xx errors and network drops).
Total Policy Timeout: 4.8 seconds (aborts further attempts and falls back to cache).
5. Decoupled Error Handling
Network errors (falling back to cache) and database errors (logging and returning fresh data) are handled in separate try-catch blocks to prevent application crashes during partial system failures (Fault Tolerance).

Features
Weather Retrieval
Weather data is fetched from the Open-Meteo API:https://open-meteo.com/

Persistence
Successfully received weather data updates a single record in SQL Server to ensure constant disk usage.

Fallback Strategy
The application follows this behavior:

Success: Fetches from API, updates the single DB record, returns fresh data.
Network Failure/Timeout: Retries 2 times. If total time hits 4.8s, aborts and returns the last successfully saved record from SQL Server.
First Run Failure: If the database is empty and the API is unreachable, returns null (HTTP 200 OK).
Database Failure during save: Returns the fresh data to the client without crashing, logs the DB error.
Health Monitoring
Health check endpoint:

GET /health
Main Endpoint
Get Weather
http

GET /api/weather
Response (Success):
Returns the fresh or cached weather JSON.

Response (First Run Failure):
If it's the first run, the database is empty, and the API is unreachable, returns:

json

null
Deployment Notes (Important)
The appsettings.json file currently uses Trusted_Connection=True (Windows Authentication) for local development purposes only.

Since the target environment is a spaceship running on Linux, Windows Authentication will not work. Before deploying, the connection string must be updated to use SQL Authentication:

json

"DefaultConnection": "Server=<server_address>;Database=WeatherDb;User Id=<username>;Password=<password>;TrustServerCertificate=True"
Running the Application Locally
1. Configure Database
Ensure your local SQL Server is running. The default connection string in appsettings.json uses Server=. and Windows Auth.

2. Apply Migrations
Create the database and apply the schema:

bash

dotnet ef database update
3. Run Application
bash

dotnet run
4. Open Swagger (Available in Development)
text

https://localhost:{port}/swagger
Reliability Scenarios Handled
Successful weather retrieval and persistence.
External API failure with cached data available.
External API failure with empty cache (Returns null).
Network timeout causing fallback to cache (Under 5 seconds).
Database error handling without crashing the application.
Author
Developed as a resilient Weather API challenge using ASP.NET Core and SQL Server.

Run API
bash

git clone https://github.com/HNaeemaei8/weather-api.git