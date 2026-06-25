# Weather API

A RESTful ASP.NET Core Web API that retrieves weather data from the Open-Meteo service, stores it in SQL Server, and provides resilient access to the latest available weather information.

## Overview

This project was developed based on the following requirements:

* Retrieve weather data from an external weather service.
* Store received data in SQL Server.
* Return weather data through a RESTful endpoint.
* Handle unreliable network conditions.
* Continue operating autonomously for long periods without manual intervention.
* Return cached data when the external service is unavailable.
* Return `null` only when no cached data exists and the weather service is unreachable.

## Technologies

* ASP.NET Core 9
* Entity Framework Core
* SQL Server
* HttpClientFactory
* Polly Retry Policy
* Swagger / OpenAPI
* Health Checks

## Architecture

The application consists of the following layers:

* **Controllers** – Expose REST endpoints.
* **Services** – Business logic and fallback strategy.
* **Http Clients** – Communication with the external weather service.
* **Data Layer** – Persistence using Entity Framework Core and SQL Server.

## Features

### Weather Retrieval

Weather data is fetched from the Open-Meteo API:

https://open-meteo.com/

### Persistence

Successfully received weather data is stored in SQL Server.

### Duplicate Prevention

A SHA256 hash is generated from the relevant weather payload to prevent storing duplicate records.

### Retry Mechanism

Network failures are handled using Polly retry policies.

* Retry count: 3
* Delay between retries: 100ms

### Fallback Strategy

The application follows this behavior:

1. If the weather service responds successfully:

   * Return fresh data.
   * Save new data if it is not a duplicate.

2. If the weather service fails:

   * Return the latest cached record from SQL Server.

3. If both the weather service and cache are unavailable:

   * Return `null`.

### Health Monitoring

Health check endpoint:

```text
GET /health
```

### API Documentation

Swagger UI is available at:

```text
/swagger
```

## Main Endpoint

### Get Weather

```http
GET /api/weather
```

Response:

```json
{
  "latitude": 52.52,
  "longitude": 13.41,
  "hourly": {
    "temperature_2m": [...]
  }
}
```

## Running the Application

### 1. Configure Database

Update the connection string in:

```text
appsettings.json
```

### 2. Apply Migrations

```bash
dotnet ef database update
```

### 3. Run Application

```bash
dotnet run
```

### 4. Open Swagger

```text
https://localhost:{port}/swagger
```

## Reliability Scenarios Tested

* Successful weather retrieval and persistence.
* Duplicate data prevention.
* External API failure with cached data available.
* External API failure with empty cache.
* Multiple consecutive requests under load.

## Design Principles

* KISS (Keep It Simple, Stupid)
* Separation of Concerns
* Dependency Injection
* Resilient Communication
* Autonomous Operation

## Author

Developed as a Weather API challenge using ASP.NET Core and SQL Server.



### Run API
```bash
git clone https://github.com/HNaeemaei8/weather-api.git