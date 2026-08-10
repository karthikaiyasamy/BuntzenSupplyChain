# Buntzen C# and .NET 10 Platform Study Guide and Reference

> [!NOTE]
> **Synthetic Data Environment**: This guide is a self-contained reference for practicing C#, .NET 10, ASP.NET Core Web API, ASP.NET Core MVC, EF Core, and T-SQL database architecture.

---

## Table of Contents
1. [Platform Context and Architecture](#1-platform-context-and-architecture)
2. [Java-to-C# (.NET 10) Complete Rosetta Stone and Syntax Guide](#2-java-to-c-net-10-complete-rosetta-stone-and-syntax-guide)
3. [ASP.NET Core Web API and MVC Architecture](#3-aspnet-core-web-api-and-mvc-architecture)
4. [Entity Framework Core (EF Core) and Dapper vs. Spring Data JPA](#4-entity-framework-core-ef-core-and-dapper-vs-spring-data-jpa)
5. [T-SQL and SQL Server Performance Tuning](#5-t-sql-and-sql-server-performance-tuning)
6. [Supply Chain Systems and XML/XSLT Integration Engine](#6-supply-chain-systems-and-xmlxslt-integration-engine)
7. [Azure DevOps CI/CD and Server Infrastructure](#7-azure-devops-cicd-and-server-infrastructure)

---

## 1. Platform Context and Architecture

### What is the Buntzen Supply Chain Platform?
The Buntzen Supply Chain Platform is an enterprise-grade C# .NET 10 web application designed for supply chain analytics, PAR level fulfillment tracking, item movement logging, and EDI integration.

### Core Architecture Components
* **C# ASP.NET Core MVC**: Handles user interface pages (Items CRUD, Inventory Dashboard, XML Integrations).
* **C# ASP.NET Web API**: Provides RESTful JSON API endpoints for external integrations and frontend clients.
* **Entity Framework Core**: ORM for object mapping, database context (`DbContext`), and T-SQL migration management.
* **T-SQL & SQL Server**: Database queries, indexes, stored procedures, and execution plan optimizations.

---

## 2. Java-to-C# (.NET 10) Complete Rosetta Stone and Syntax Guide

### Key Concept Comparisons

| Concept | Java (Spring Boot) | C# (.NET 10 / ASP.NET Core) | Key Difference |
| :--- | :--- | :--- | :--- |
| **Language Runtime** | JVM (Java Virtual Machine) | .NET CLR (Common Language Runtime) | High-performance JIT compilation |
| **Annotations / Attributes** | `@RestController`, `@Autowired` | `[ApiController]`, `[Inject]` | C# uses square brackets `[...]` |
| **Data Properties** | `private String name;` + getters/setters | `public string Name { get; set; }` | C# Auto-Properties remove boilerplate |
| **Data Streams / LINQ** | `stream().filter(...).collect(...)` | `.Where(...).ToList()` | C# LINQ extension methods (`System.Linq`) |
| **Async / Multi-threading** | `CompletableFuture<T>` / RxJava | `Task<T>` with `async` / `await` | Native language keywords `async` and `await` |
| **DTO / Data Classes** | Java Records (`public record Item(...)`) | C# Records (`public record Item(...)`) | Identical immutability semantics |
| **ORM / Data Access** | Spring Data JPA / Hibernate | Entity Framework Core (`DbContext`) | EF Core compiles LINQ directly to T-SQL |
| **Dependency Injection** | `@Autowired` or constructor | Constructor Injection (`IServiceCollection`) | DI container built directly into ASP.NET Core |
| **JSON Serialization** | Jackson (`ObjectMapper`) | `System.Text.Json` | Standard PascalCase to camelCase conversion |
| **Build Tool / Packages** | Maven (`pom.xml`) / Gradle | NuGet (`.csproj` / `BuntzenSupplyChain.slnx`) | Command line: `dotnet add package ...` |

---

### Side-by-Side Code Examples

#### Example A: Auto-Properties and Data Classes
```java
// Java: Requires Lombok or explicit getters/setters
public class SupplyItem {
    private String itemNumber;
    private double unitPrice;

    public String getItemNumber() { return itemNumber; }
    public void setItemNumber(String itemNumber) { this.itemNumber = itemNumber; }
    public double getUnitPrice() { return unitPrice; }
    public void setUnitPrice(double unitPrice) { this.unitPrice = unitPrice; }
}
```

```csharp
// C# (.NET 10): Auto-Properties with default initializers
public class SupplyItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ItemNumber { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
}
```

---

#### Example B: LINQ vs. Java Streams (10 Direct Conversions)

1. **Filtering (`filter` vs. `Where`)**
   ```java
   List<Item> def = items.stream().filter(i -> i.getQty() < 10).collect(Collectors.toList());
   ```
   ```csharp
   List<Item> def = items.Where(i => i.Qty < 10).ToList();
   ```

2. **Transforming / Mapping (`map` vs. `Select`)**
   ```java
   List<String> skus = items.stream().map(Item::getItemNumber).collect(Collectors.toList());
   ```
   ```csharp
   List<string> skus = items.Select(i => i.ItemNumber).ToList();
   ```

3. **Sorting (`sorted` vs. `OrderBy`)**
   ```java
   List<Item> sorted = items.stream().sorted(Comparator.comparing(Item::getPrice)).collect(Collectors.toList());
   ```
   ```csharp
   List<Item> sorted = items.OrderBy(i => i.Price).ToList();
   ```

4. **Finding First Matching Element (`findFirst` vs. `FirstOrDefault`)**
   ```java
   Item match = items.stream().filter(i -> i.getSku().equals("N95")).findFirst().orElse(null);
   ```
   ```csharp
   Item match = items.FirstOrDefault(i => i.Sku == "N95");
   ```

5. **Checking Condition (`anyMatch` vs. `Any`)**
   ```java
   boolean hasDeficit = items.stream().anyMatch(i -> i.getQty() < i.getPar());
   ```
   ```csharp
   bool hasDeficit = items.Any(i => i.Qty < i.Par);
   ```

---

## 3. ASP.NET Core Web API and MVC Architecture

### Dependency Injection Lifetimes
In `Program.cs`, service dependencies are registered into the `IServiceCollection`:

```csharp
// 1. Transient: Created every time requested (Stateless utility helpers)
builder.Services.AddTransient<IXmlValidator, XmlValidator>();

// 2. Scoped: Created ONCE per HTTP request lifetime (Default for DbContext)
builder.Services.AddScoped<ISqlPerformanceTuningService, SqlPerformanceTuningService>();

// 3. Singleton: Created ONCE for the entire application life (Caching / App Config)
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
```

---

## 4. Entity Framework Core (EF Core) and Dapper

### EF Core (`DbContext` & `DbSet<T>`)
EF Core is the official C# Object-Relational Mapper (ORM), similar to Hibernate/JPA in Java.

```csharp
public class BuntzenDbContext : DbContext
{
    public BuntzenDbContext(DbContextOptions<BuntzenDbContext> options) : base(options) { }

    public DbSet<HealthAuthoritySite> Sites => Set<HealthAuthoritySite>();
    public DbSet<SupplyItem> Items => Set<SupplyItem>();
    public DbSet<SiteInventory> Inventories => Set<SiteInventory>();
}
```

---

## 5. T-SQL and SQL Server Performance Tuning

### Clustered vs. Non-Clustered Indexes

* **Clustered Index**: Physical storage order of table data (1 per table, typically the Primary Key).
* **Non-Clustered Index**: Separate B-tree structure pointing to table data rows.
* **Covering Index (`INCLUDE`)**: A non-clustered index that includes all requested query columns in the index leaf nodes.

```sql
-- Non-Clustered Covering Index DDL
CREATE NONCLUSTERED INDEX IX_SupplyChainAuditLogs_Action_Entity
ON SupplyChainAuditLogs(Action, EntityName)
INCLUDE (Timestamp, PerformedBy);
```
