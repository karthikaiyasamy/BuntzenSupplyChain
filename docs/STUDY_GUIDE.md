# Buntzen C# and .NET 10 Comprehensive Architecture Study Guide

Notice: Synthetic data environment for practicing C#, .NET 10, ASP.NET Core Web API, ASP.NET Core MVC, EF Core, and T-SQL database architecture.

---

## Table of Contents
1. [Platform Overview and Clean Architecture](#1-platform-overview-and-clean-architecture)
2. [C# Language Core Fundamentals and Memory Management](#2-c-language-core-fundamentals-and-memory-management)
3. [Java vs. C# Complete Rosetta Stone and Syntax Guide](#3-java-vs-c-complete-rosetta-stone-and-syntax-guide)
4. [ASP.NET Core Architecture and Request Pipeline](#4-aspnet-core-architecture-and-request-pipeline)
5. [Entity Framework Core (EF Core) and Dapper Deep Dive](#5-entity-framework-core-ef-core-and-dapper-deep-dive)
6. [T-SQL and SQL Server Performance Tuning Mastery](#6-t-sql-and-sql-server-performance-tuning-mastery)
7. [XML, XSLT, and EDI Integration Engine](#7-xml-xslt-and-edi-integration-engine)
8. [Kestrel Server, Docker, and Azure DevOps Infrastructure](#8-kestrel-server-docker-and-azure-devops-infrastructure)
9. [Line-by-Line Code Walkthrough of Key Project Files](#9-line-by-line-code-walkthrough-of-key-project-files)

---

## 1. Platform Overview and Clean Architecture

### Clean Architecture Layers
The Buntzen Supply Chain application follows Microsoft's recommended **Clean Architecture** (Domain-Driven Design) layout:

```
BuntzenSupplyChain/
├── src/
│   ├── BuntzenSupplyChain.Domain/          # Core Domain Entities, Enums, Value Objects (No Dependencies)
│   ├── BuntzenSupplyChain.Application/     # Application Logic, Service Interfaces, DTOs, CQRS Handlers
│   ├── BuntzenSupplyChain.Infrastructure/  # EF Core DbContext, Dapper, SQL Services, XSLT Transformer, Seeder
│   └── BuntzenSupplyChain.Api/             # ASP.NET Core MVC Controllers, Web API Controllers, Razor Views
├── db/
│   └── sql_tuning_lab/                     # Native T-SQL DDL and Performance Tuning Scripts
└── docs/
    └── STUDY_GUIDE.md                      # Comprehensive Architecture Study Guide
```

#### Layer Dependency Rule
Dependencies flow **inward**. Lower-level layers (Domain, Application) have **zero knowledge** of database technologies, web frameworks, or infrastructure details:

* `BuntzenSupplyChain.Domain` depends on: **Nothing** (Pure C#).
* `BuntzenSupplyChain.Application` depends on: `Domain`.
* `BuntzenSupplyChain.Infrastructure` depends on: `Application` and `Domain`.
* `BuntzenSupplyChain.Api` depends on: `Infrastructure`, `Application`, and `Domain`.

---

## 2. C# Language Core Fundamentals and Memory Management

### Value Types vs. Reference Types

| Category | Value Types (`struct`, `enum`, `int`, `decimal`, `bool`, `Guid`) | Reference Types (`class`, `interface`, `delegate`, `string`, arrays, `object`) |
| :--- | :--- | :--- |
| **Memory Allocation** | Allocated on the **Stack** (or inline within containing object) | Allocated on the **Managed Heap** |
| **Assignment Behavior** | Value is **copied** by value | Reference (memory pointer) is copied |
| **Default Value** | `0`, `false`, `Guid.Empty` | `null` |
| **Garbage Collector** | Not managed by GC (popped off stack instantly) | Managed and collected by .NET Garbage Collector |

```csharp
// Value Type Example: Passing by value creates a copy
int a = 10;
int b = a; // b is a copy of 10. Modifying b does not change a.

// Reference Type Example: Passing by reference shares object pointer
SupplyItem item1 = new SupplyItem { Name = "N95 Mask" };
SupplyItem item2 = item1; // item2 points to exact same memory object on heap.
item2.Name = "Surgical Scalpel"; // Both item1.Name and item2.Name are now "Surgical Scalpel".
```

---

### Memory Management and Garbage Collection (GC)
The .NET CLR Garbage Collector manages heap memory across three generations:

1. **Generation 0 (Gen 0)**: Short-lived objects (e.g. temporary loop variables, local HTTP request DTOs). Collection happens frequently and rapidly (sub-millisecond).
2. **Generation 1 (Gen 1)**: Buffer generation acting as a memory bridge between short-lived and long-lived objects.
3. **Generation 2 (Gen 2)**: Long-lived objects (e.g. `DbContext` pools, singleton caches, application state). Collections are expensive.
4. **Large Object Heap (LOH)**: Objects larger than 85,000 bytes (e.g. large byte arrays, massive XML strings).

> **Performance Rule**: Use `AsNoTracking()` in EF Core queries when reading data for reports to prevent EF Core from keeping entities inside the Change Tracker heap memory.

---

### Nullable Reference Types (`string?`)
C# includes compile-time null safety:

```csharp
#nullable enable

public class SupplyItem
{
    public string ItemNumber { get; set; } = string.Empty; // Non-nullable: Cannot be null
    public string? Description { get; set; }               // Nullable: Can be null

    public void ProcessItem(string requiredSku, string? optionalNotes)
    {
        // Compiler warning if requiredSku is not checked before dereferencing
        int length = requiredSku.Length; 

        // Safe dereferencing with null-conditional operator
        int notesLength = optionalNotes?.Length ?? 0;
    }
}
```

---

### Extension Methods
Extension methods allow adding new methods to existing types without modifying the source code. They are defined as `static` methods in a `static` class using the `this` keyword:

```csharp
public static class SupplyItemExtensions
{
    // Extends SupplyItem class
    public static bool NeedsUrgentReorder(this SupplyItem item, int currentOnHand)
    {
        return currentOnHand < item.DefaultReorderPoint;
    }
}

// Usage in code:
SupplyItem mask = new SupplyItem { DefaultReorderPoint = 50 };
bool reorder = mask.NeedsUrgentReorder(12); // Returns true
```

---

## 3. Java vs. C# Complete Rosetta Stone and Syntax Guide

### Language Syntax Rosetta Stone

| Concept | Java (Spring Boot) | C# (.NET 10 / ASP.NET Core) | Technical Notes |
| :--- | :--- | :--- | :--- |
| **REST Controller** | `@RestController` | `[ApiController]` + `[Route("api/[controller]")]` | C# uses Attributes `[...]` |
| **Dependency Injection** | `@Autowired` / Constructor | Constructor Injection (`builder.Services.AddScoped<...>`) | Native DI in `Program.cs` |
| **Data Properties** | `private String name;` + getters/setters | `public string Name { get; set; }` | Auto-Properties eliminate boilerplate |
| **Data Streams / LINQ** | `stream().filter(...).collect(...)` | `.Where(...).ToList();` | LINQ extension methods (`System.Linq`) |
| **Async Programming** | `CompletableFuture<T>` / RxJava | `Task<T>` with `async` / `await` | Language keywords `async` & `await` |
| **DTO / Data Classes** | Java Records (`public record Item(...)`) | C# Records (`public record Item(...)`) | Identical immutability semantics |
| **ORM / Data Access** | Spring Data JPA / Hibernate | Entity Framework Core (`DbContext`) | EF Core compiles LINQ to T-SQL |
| **Exception Filter** | `@ExceptionHandler` | `try { ... } catch (Exception ex) when (ex.HResult == ...)` | C# supports `when` exception filters |
| **JSON Library** | Jackson (`ObjectMapper`) | `System.Text.Json` | Standard PascalCase / camelCase conversion |
| **Package Manager** | Maven (`pom.xml`) / Gradle | NuGet (`.csproj` / `BuntzenSupplyChain.slnx`) | Command: `dotnet add package ...` |

---

### LINQ vs. Java Streams (15 Comprehensive Conversions)

#### 1. Filtering (`filter` vs. `Where`)
```java
List<Item> result = list.stream().filter(i -> i.getQty() < 10).collect(Collectors.toList());
```
```csharp
List<Item> result = list.Where(i => i.Qty < 10).ToList();
```

#### 2. Projection / Mapping (`map` vs. `Select`)
```java
List<String> names = list.stream().map(Item::getName).collect(Collectors.toList());
```
```csharp
List<string> names = list.Select(i => i.Name).ToList();
```

#### 3. Sorting (`sorted` vs. `OrderBy` / `OrderByDescending`)
```java
List<Item> sorted = list.stream().sorted(Comparator.comparing(Item::getPrice)).collect(Collectors.toList());
```
```csharp
List<Item> sorted = list.OrderBy(i => i.Price).ToList();
List<Item> desc = list.OrderByDescending(i => i.Price).ToList();
```

#### 4. Finding First (`findFirst` vs. `FirstOrDefault`)
```java
Item item = list.stream().filter(i -> i.getId().equals(id)).findFirst().orElse(null);
```
```csharp
Item item = list.FirstOrDefault(i => i.Id == id);
```

#### 5. Any Match (`anyMatch` vs. `Any`)
```java
boolean exists = list.stream().anyMatch(i -> i.getQty() == 0);
```
```csharp
bool exists = list.Any(i => i.Qty == 0);
```

#### 6. All Match (`allMatch` vs. `All`)
```java
boolean allValid = list.stream().allMatch(i -> i.getPrice() > 0);
```
```csharp
bool allValid = list.All(i => i.Price > 0);
```

#### 7. Counting (`count` vs. `Count`)
```java
long total = list.stream().filter(i -> i.isParDeficit()).count();
```
```csharp
int total = list.Count(i => i.IsParDeficit);
```

#### 8. Summation (`mapToDouble().sum()` vs. `Sum()`)
```java
double totalCost = list.stream().mapToDouble(Item::getPrice).sum();
```
```csharp
decimal totalCost = list.Sum(i => i.Price);
```

#### 9. Minimum / Maximum (`min/max` vs. `MinBy` / `MaxBy`)
```java
Item cheapest = list.stream().min(Comparator.comparing(Item::getPrice)).orElse(null);
```
```csharp
Item cheapest = list.MinBy(i => i.Price);
Item priciest = list.MaxBy(i => i.Price);
```

#### 10. Grouping (`Collectors.groupingBy` vs. `GroupBy`)
```java
Map<Category, List<Item>> grouped = list.stream().collect(Collectors.groupingBy(Item::getCategory));
```
```csharp
var grouped = list.GroupBy(i => i.Category).ToDictionary(g => g.Key, g => g.ToList());
```

#### 11. Pagination (`skip/limit` vs. `Skip/Take`)
```java
List<Item> page = list.stream().skip(20).limit(10).collect(Collectors.toList());
```
```csharp
List<Item> page = list.Skip(20).Take(10).ToList();
```

#### 12. Flattening Nested Collections (`flatMap` vs. `SelectMany`)
```java
List<LineItem> allLines = orders.stream().flatMap(o -> o.getLines().stream()).collect(Collectors.toList());
```
```csharp
List<LineItem> allLines = orders.SelectMany(o => o.Lines).ToList();
```

#### 13. Distinct Elements (`distinct` vs. `DistinctBy`)
```java
List<String> categories = list.stream().map(Item::getCategory).distinct().collect(Collectors.toList());
```
```csharp
List<string> categories = list.Select(i => i.Category).Distinct().ToList();
List<Item> uniqueSkus = list.DistinctBy(i => i.ItemNumber).ToList();
```

#### 14. Lookup Dictionary Creation (`Collectors.toMap` vs. `ToDictionary`)
```java
Map<String, Item> map = list.stream().collect(Collectors.toMap(Item::getSku, i -> i));
```
```csharp
Dictionary<string, Item> map = list.ToDictionary(i => i.ItemNumber, i => i);
```

#### 15. Reducing / Aggregating (`reduce` vs. `Aggregate`)
```java
String csv = list.stream().map(Item::getSku).collect(Collectors.joining(","));
```
```csharp
string csv = list.Select(i => i.ItemNumber).Aggregate((current, next) => current + ", " + next);
```

---

## 4. ASP.NET Core Architecture and Request Pipeline

### End-to-End Request Pipeline Flow

```
HTTP Request
     │
     ▼
┌────────────────────────────────────────────────────────┐
│ Kestrel Web Server (Asynchronous Event Loop)          │
└────────────────────────┬───────────────────────────────┘
                         │
                         ▼
┌────────────────────────────────────────────────────────┐
│ ASP.NET Core Middleware Pipeline                       │
│  ├── 1. UseDeveloperExceptionPage() / Error Handling   │
│  ├── 2. UseStaticFiles()                               │
│  ├── 3. UseRouting()                                   │
│  ├── 4. UseAuthentication()                            │
│  └── 5. UseAuthorization()                             │
└────────────────────────┬───────────────────────────────┘
                         │
                         ▼
┌────────────────────────────────────────────────────────┐
│ Routing & Model Binding                                │
│  ├── Matches URL route to Controller Action            │
│  └── Binds HTTP Body/Query/Route to Action Parameters  │
└────────────────────────┬───────────────────────────────┘
                         │
                         ▼
┌────────────────────────────────────────────────────────┐
│ Controller Action Execution                            │
│  ├── ItemsController.Index() / InventoryController     │
│  └── Calls Application / Infrastructure Services       │
└────────────────────────┬───────────────────────────────┘
                         │
                         ▼
┌────────────────────────────────────────────────────────┐
│ EF Core DbContext Execution                            │
│  └── Compiles LINQ to T-SQL -> Executes on Database    │
└────────────────────────┬───────────────────────────────┘
                         │
                         ▼
┌────────────────────────────────────────────────────────┐
│ Result Execution & Response Output                     │
│  ├── MVC: Renders Razor HTML View (_Layout.cshtml)     │
│  └── API: Serializes JSON via System.Text.Json         │
└────────────────────────────────────────────────────────┘
```

---

### Model Binding Attributes
ASP.NET Core automatically maps incoming HTTP request data to C# action parameters using attributes:

* `[FromBody]`: Binds data from HTTP Request Body (JSON payload).
* `[FromQuery]`: Binds data from URL query string parameters (`?siteCode=BCCH&onlyDeficits=true`).
* `[FromRoute]`: Binds data from URL route segments (`/api/items/details/{id}`).
* `[FromHeader]`: Binds data from HTTP Request Headers.

```csharp
[HttpPost("requisitions")]
public async Task<IActionResult> CreateRequisition(
    [FromHeader(Name = "X-Staff-Id")] string staffId,
    [FromBody] CreateRequisitionRequest request)
{
    if (!ModelState.IsValid) return BadRequest(ModelState);

    // Business Logic
    return CreatedAtAction(nameof(GetRequisition), new { id = request.SiteId }, request);
}
```

---

## 5. Entity Framework Core (EF Core) and Dapper Deep Dive

### EF Core Entity Change Tracker States

1. **`Unchanged`**: Entity is tracked and has not changed since queried.
2. **`Added`**: Entity is new and will be inserted into the database upon calling `SaveChangesAsync()`.
3. **`Modified`**: Entity properties were changed and will generate an `UPDATE` T-SQL statement.
4. **`Deleted`**: Entity is marked for removal and will generate a `DELETE` T-SQL statement.
5. **`Detached`**: Entity is not tracked by the `DbContext`.

### `AsNoTracking()` Performance Best Practice
When querying data for display or read-only reports, always add `.AsNoTracking()`. This disables Entity Tracking memory overhead, making queries **3x to 5x faster**:

```csharp
// Read-Only Query (Fast, zero memory tracking overhead)
var readOnlyItems = await _db.Items
    .AsNoTracking()
    .Where(x => x.UnitPrice > 20.0m)
    .ToListAsync();

// Write Query (Tracked, for updates)
var itemToUpdate = await _db.Items.FindAsync(id);
if (itemToUpdate != null)
{
    itemToUpdate.UnitPrice = 25.50m;
    await _db.SaveChangesAsync(); // Generates UPDATE T-SQL statement
}
```

---

### Eager Loading (`Include` / `ThenInclude`)
In EF Core, related entities are not loaded by default. Use `.Include()` and `.ThenInclude()` for Eager Loading:

```csharp
var requisitions = await _db.Requisitions
    .Include(r => r.SourceSite)                          // Loads HealthAuthoritySite
    .Include(r => r.LineItems)                           // Loads Line Items list
        .ThenInclude(line => line.Item)                  // Loads SupplyItem for each Line Item
    .AsNoTracking()
    .ToListAsync();
```

---

## 6. T-SQL and SQL Server Performance Tuning Mastery

### Execution Plan Operators

1. **Clustered Index Seek**: The optimal operator. Traverses the index B-tree directly to locate exact target rows.
2. **Clustered Index Scan**: Scans every row in the index table. Occurs when no usable predicate index exists.
3. **Table Scan**: Occurs on heap tables (tables without a clustered index). Scans entire table data pages.
4. **Key Lookup (Bookmark Lookup)**: Happens when a non-clustered index does not include all requested columns, forcing SQL Server to fetch remaining columns from the clustered index. Fixed by adding `INCLUDE (...)`.

```sql
-- Non-Clustered Covering Index eliminating Key Lookups
CREATE NONCLUSTERED INDEX IX_SupplyChainAuditLogs_Action_Entity
ON SupplyChainAuditLogs(Action, EntityName)
INCLUDE (Timestamp, PerformedBy);
```

---

### SARGability Rules (Search Argumentable)
A query predicate is **SARGable** if the SQL Server Query Optimizer can use an Index Seek to execute it.

#### Rule 1: Never apply functions to indexed columns in `WHERE` clauses
* ❌ **Non-SARGable**: `WHERE UPPER(ItemNumber) = 'PHSA-PPE-204'` (Forces Index Scan)
* ✓ **SARGable**: `WHERE ItemNumber = 'PHSA-PPE-204'` (Enables Index Seek)

#### Rule 2: Never use `CAST` or `CONVERT` on date columns
* ❌ **Non-SARGable**: `WHERE CAST(CreatedAt AS DATE) = '2026-08-10'`
* ✓ **SARGable**: `WHERE CreatedAt >= '2026-08-10T00:00:00' AND CreatedAt < '2026-08-11T00:00:00'`

#### Rule 3: Avoid leading wildcards in `LIKE` queries
* ❌ **Non-SARGable**: `WHERE ItemNumber LIKE '%PPE%'` (Forces Index Scan)
* ✓ **SARGable**: `WHERE ItemNumber LIKE 'PHSA-PPE%'` (Enables Index Seek)

---

### CTE Window Functions (`ROW_NUMBER() OVER`)
Replacing correlated scalar subqueries with Common Table Expressions (CTEs) and Window Functions eliminates TempDB spooling:

```sql
WITH RankedInventory AS (
    SELECT 
        SiteId, 
        ItemId, 
        QuantityOnHand, 
        ParLevel,
        ROW_NUMBER() OVER (PARTITION BY SiteId ORDER BY (ParLevel - QuantityOnHand) DESC) as DeficitRank
    FROM SiteInventories
    WHERE QuantityOnHand < ReorderThreshold
)
SELECT * 
FROM RankedInventory 
WHERE DeficitRank <= 5;
```

---

## 7. XML, XSLT, and EDI Integration Engine

### C# XSLT Transformation (`System.Xml.Xsl`)
The integration engine uses `XslCompiledTransform` to execute XSLT stylesheets that transform incoming vendor XML payloads into canonical JSON:

```csharp
using System.Xml;
using System.Xml.Xsl;
using System.Text.Json;

public async Task<EdiTransaction> TransformVendorXmlAsync(string rawXml, string xsltString)
{
    using var xmlReader = XmlReader.Create(new StringReader(rawXml));
    using var xsltReader = XmlReader.Create(new StringReader(xsltString));
    
    var transform = new XslCompiledTransform();
    transform.Load(xsltReader);

    using var sw = new StringWriter();
    using var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true });
    
    transform.Transform(xmlReader, xmlWriter);
    string transformedXml = sw.ToString();

    var doc = new XmlDocument();
    doc.LoadXml(transformedXml);
    string jsonPayload = JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });

    return new EdiTransaction
    {
        RawXmlPayload = rawXml,
        TransformedJsonPayload = jsonPayload,
        Status = EdiProcessingStatus.DispatchedToEsb
    };
}
```

---

## 8. Kestrel Server, Docker, and Azure DevOps Infrastructure

### Kestrel Web Server Configuration
ASP.NET Core runs cross-platform using the built-in C# **Kestrel** server. It handles HTTP/1.1, HTTP/2, and HTTP/3 requests without requiring IIS:

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

---

### SQL Server Docker Container (`docker-compose.yml`)

```yaml
version: '3.8'

services:
  sqlserver:
    image: mcr.microsoft.com/azure-sql-edge:latest
    container_name: buntzen_sqlserver
    restart: always
    environment:
      - ACCEPT_EULA=Y
      - MSSQL_SA_PASSWORD=PHSA_SC_Perf_2026!
    ports:
      - "1433:1433"
    volumes:
      - mssql_data:/var/opt/mssql

volumes:
  mssql_data:
```

---

## 9. Line-by-Line Code Walkthrough of Key Project Files

### 1. `ItemsController.cs` (C# ASP.NET Core MVC CRUD Controller)

```csharp
namespace BuntzenSupplyChain.Api.Controllers;

public class ItemsController : Controller
{
    private readonly BuntzenDbContext _db; // Dependency Injected EF Core DbContext

    public ItemsController(BuntzenDbContext db)
    {
        _db = db;
    }

    // GET: /Items
    public async Task<IActionResult> Index()
    {
        // Queries all Supply Items asynchronously without tracking overhead
        var items = await _db.Items.AsNoTracking().ToListAsync();
        return View(items); // Passes items list to Views/Items/Index.cshtml
    }

    // GET: /Items/Create
    public IActionResult Create()
    {
        return View(new SupplyItem()); // Renders creation form
    }

    // POST: /Items/Create
    [HttpPost]
    [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery
    public async Task<IActionResult> Create(SupplyItem item)
    {
        if (ModelState.IsValid)
        {
            item.Id = Guid.NewGuid();
            item.CreatedAt = DateTime.UtcNow;
            _db.Items.Add(item); // Marks state as Added
            await _db.SaveChangesAsync(); // Executes INSERT INTO T-SQL
            return RedirectToAction(nameof(Index)); // Redirects to Index list
        }
        return View(item);
    }
}
```

---

### Summary Checklist
* Clean Architecture layer boundaries enforced.
* C# Auto-Properties, Nullable Types, and LINQ methods mastered.
* ASP.NET Core Kestrel server, Middleware chain, and Model Binding understood.
* EF Core Change Tracker, `AsNoTracking()`, and Eager Loading mastered.
* T-SQL SARGability, Covering Indexes, and Window Functions mastered.
* All project files committed and pushed to GitHub repository.
