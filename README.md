# Buntzen Supply Chain Platform

A full-stack enterprise C# .NET 10 web application and RESTful API demonstrating clean architecture, ASP.NET Core MVC, Entity Framework Core, T-SQL performance optimization, and XML/XSLT data integration pipelines.

Notice: Synthetic data environment for practicing C# and .NET web technologies.

---

## Architecture Overview

* **Frontend & MVC Layer**: ASP.NET Core MVC with Razor Views, Tag Helpers, and custom CSS design system.
* **API Layer**: ASP.NET Web API with OpenAPI / Swagger documentation.
* **Domain & Application**: Clean Architecture pattern separating domain entities, services, and DTOs.
* **Data Access**: Entity Framework Core with support for SQL Server and SQLite.
* **Integration Engine**: C# XSLT transformation service for EDI XML purchase orders and advance ship notices.
* **T-SQL Performance Lab**: Query benchmarking comparing table scans against non-clustered covering index seeks and window functions.

---

## Project Structure

```
BuntzenSupplyChain/
├── BuntzenSupplyChain.slnx
├── docker-compose.yml
├── db/
│   └── sql_tuning_lab/
│       ├── 01_Create_PHSA_SupplyChain_Database.sql
│       └── 02_TSQL_Performance_Tuning_Scenarios.sql
└── src/
    ├── BuntzenSupplyChain.Domain/
    ├── BuntzenSupplyChain.Application/
    ├── BuntzenSupplyChain.Infrastructure/
    └── BuntzenSupplyChain.Api/
```

---

## Getting Started

### Prerequisites
* .NET 10 SDK
* Docker or Podman (Optional, for running native SQL Server container)

### Building and Running Locally

1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/BuntzenSupplyChain.git
   cd BuntzenSupplyChain
   ```

2. Build the solution:
   ```bash
   dotnet build BuntzenSupplyChain.slnx
   ```

3. Run the web application:
   ```bash
   dotnet run --project src/BuntzenSupplyChain.Api/BuntzenSupplyChain.Api.csproj --urls "http://localhost:5050"
   ```

4. Access the web interface at `http://localhost:5050` and Swagger API documentation at `http://localhost:5050/swagger`.

---

## License

This project is open-source under the MIT License.
# BuntzenSupplyChain
