# Buntzen C# and .NET 10 Comprehensive Architecture Study Guide

> **Notice:** Synthetic data environment for practicing C#, .NET 10, ASP.NET Core Web API, ASP.NET Core MVC, EF Core, and T-SQL database architecture.

> **Interview Target:** PHSA / Fraser Health — Intermediate/Senior Programmer (SC Performance role)
> **Strategy:** One round — mix of technical questions + behavioral (STAR). Be ready to speak fluently about *why* you made architectural choices, not just *what* the code does.

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
10. [Interview STAR Stories — Behavioral Coaching](#10-interview-star-stories--behavioral-coaching)
11. [What's New in .NET 8 / .NET 9 / .NET 10 (Must-Know for 2026)](#11-whats-new-in-net-8--net-9--net-10-must-know-for-2026)
12. [30 Likely PHSA Interview Questions with Model Answers](#12-30-likely-phsa-interview-questions-with-model-answers)

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

#### Why This Architecture Matters for PHSA

In a healthcare supply chain system like PHSA's, vendors change, databases get migrated, and clinical workflows evolve frequently. Clean Architecture insulates domain logic from those external changes:

- **Testability**: Domain and Application layers have zero infrastructure dependencies so they can be unit tested without a database or web server running.
- **Replaceability**: You can swap EF Core for Dapper or SQL Server for PostgreSQL by only changing the Infrastructure layer — the Application and Domain layers remain untouched.
- **Regulatory compliance**: Healthcare software (think Philips clinical devices) often requires evidence of separation between business logic and I/O concerns. Clean Architecture creates that evidence naturally.

> 💬 **STAR Interview Bridge**: *"In my Brigham and Women's / Philips cardiac monitor work, the domain logic for alarm thresholds was completely separated from the persistence layer. This meant we could unit test that a 120bpm threshold correctly triggered a critical alert without ever spinning up a database — a key requirement for our FDA submission documentation."*

#### What is the Dependency Inversion Principle (DIP)?

DIP says: *high-level modules (Application) should not depend on low-level modules (Infrastructure); both should depend on abstractions (interfaces).*

In this project:
- `ISqlPerformanceTuningService` lives in the **Application** layer (abstraction)
- `SqlPerformanceTuningService` lives in the **Infrastructure** layer (concrete implementation)
- `Program.cs` in the **Api** layer wires them together: `builder.Services.AddScoped<ISqlPerformanceTuningService, SqlPerformanceTuningService>()`

This means the `HomeController` only knows about the **interface**, not the SQL Server implementation — you could substitute a mock implementation in tests instantly.

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

#### Common Gotcha: `struct` vs `class` for DTOs

Java developers instinctively reach for objects (classes) for everything. In C#:

- `struct` is a **value type** — copied on every assignment. Good for small, immutable data carriers (coordinates, money amounts). Bad for large objects (high copy cost).
- `class` is a **reference type** — passed by reference. Default for most entities and service objects.
- **`record`** (C# 9+) creates either a value-type (`record struct`) or reference-type (`record class`) with built-in equality, immutability, and `with` expressions.

```csharp
// C# record (reference type by default) — perfect for DTOs
public record SupplyItemDto(Guid Id, string ItemNumber, string Name, decimal UnitPrice);

// Usage — positional syntax (like Java records)
var dto = new SupplyItemDto(Guid.NewGuid(), "PHSA-PPE-204", "N95 Mask", 28.50m);

// Non-destructive mutation using 'with' expression (immutable objects!)
var updatedDto = dto with { UnitPrice = 32.00m }; // Creates a NEW record with only UnitPrice changed
```

In Java, you'd write:
```java
public record SupplyItemDto(UUID id, String itemNumber, String name, BigDecimal unitPrice) {}
// Java records are implicitly final and immutable, same semantic concept
```

---

### Memory Management and Garbage Collection (GC)
The .NET CLR Garbage Collector manages heap memory across three generations:

1. **Generation 0 (Gen 0)**: Short-lived objects (e.g. temporary loop variables, local HTTP request DTOs). Collection happens frequently and rapidly (sub-millisecond).
2. **Generation 1 (Gen 1)**: Buffer generation acting as a memory bridge between short-lived and long-lived objects.
3. **Generation 2 (Gen 2)**: Long-lived objects (e.g. `DbContext` pools, singleton caches, application state). Collections are expensive.
4. **Large Object Heap (LOH)**: Objects larger than 85,000 bytes (e.g. large byte arrays, massive XML strings).

> **Performance Rule**: Use `AsNoTracking()` in EF Core queries when reading data for reports to prevent EF Core from keeping entities inside the Change Tracker heap memory.

#### How GC Differs from Java's GC (Interview-Ready Comparison)

| Aspect | Java (JVM G1GC / ZGC) | .NET CLR (GC) |
| :--- | :--- | :--- |
| **Generational model** | Same concept (Young/Old/Metaspace) | Gen 0 / Gen 1 / Gen 2 + LOH |
| **Triggering** | JVM decides heuristically | CLR decides; `GC.Collect()` is manual override (avoid!) |
| **Finalization** | `finalize()` method (deprecated Java 9+) | `~Destructor()` / `IDisposable.Dispose()` |
| **Deterministic cleanup** | `try-with-resources` | `using` statement / `using` declaration |
| **Large object handling** | Humongous regions (G1GC) | LOH (>85 KB), collected in Gen 2 |

#### `IDisposable` and `using` — The Java `try-with-resources` Equivalent

In Java:
```java
try (FileInputStream fis = new FileInputStream("data.xml")) {
    // file is auto-closed when block exits
}
```

In C# — two equivalent patterns:

```csharp
// Pattern 1: Classic using statement (identical scope to Java try-with-resources)
using (var xmlReader = XmlReader.Create(new StringReader(rawXml)))
{
    // XmlReader.Dispose() called automatically when block exits
}

// Pattern 2: C# 8+ using declaration (preferred in modern .NET)
// Disposes at the end of the enclosing scope, not a block
using var xmlReader = XmlReader.Create(new StringReader(rawXml));
// ... xmlReader auto-disposed at end of method
```

You can see Pattern 2 used throughout `XmlXsltTransformationService.cs` in this project:
```csharp
using var xmlReader = XmlReader.Create(new StringReader(rawXml));
using var xsltReader = XmlReader.Create(new StringReader(xsltString));
using var sw = new StringWriter();
using var xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true });
```

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

#### Null Safety Operators — Complete Reference

| Operator | Name | Example | What it does |
| :--- | :--- | :--- | :--- |
| `?.` | Null-conditional | `item?.Name` | Returns `null` if `item` is null; else evaluates `item.Name` |
| `??` | Null-coalescing | `name ?? "Unknown"` | Returns left side if not null; else returns right side |
| `??=` | Null-coalescing assignment | `name ??= "Unknown"` | Assigns right side to left only if left is currently null |
| `!` | Null-forgiving | `item!.Name` | Tells compiler "trust me, this is not null" (use sparingly) |

```csharp
// Chaining null-conditional operators safely:
string? city = hospital?.Address?.Split(',').FirstOrDefault()?.Trim();

// Null-coalescing assignment — lazy initialization
_cachedSites ??= await _db.Sites.AsNoTracking().ToListAsync();

// Null-forgiving in EF Core navigation properties (common pattern)
public HealthAuthoritySite SourceSite { get; set; } = null!;
// The `= null!` tells the compiler: "EF Core will populate this via eager loading — trust me"
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

### Deeper Mapping: Spring Boot vs ASP.NET Core Dependency Injection

In Spring Boot:
```java
@Service
public class InventoryService {
    @Autowired
    private SupplyItemRepository repository;
}
```

In ASP.NET Core (`Program.cs`):
```csharp
// Built-in DI container — no separate Spring framework needed
builder.Services.AddScoped<ISqlPerformanceTuningService, SqlPerformanceTuningService>();
builder.Services.AddTransient<IXmlXsltTransformationService, XmlXsltTransformationService>();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
```

#### Service Lifetimes — Critical for Interviews

| Lifetime | ASP.NET Core | Java Spring | Meaning |
| :--- | :--- | :--- | :--- |
| **Scoped** | `AddScoped<T>()` | Default `@Service` | One instance per HTTP request |
| **Transient** | `AddTransient<T>()` | `@Scope("prototype")` | New instance every injection |
| **Singleton** | `AddSingleton<T>()` | `@Scope("singleton")` | One instance for app lifetime |

> **Captive Dependency Trap**: A **Singleton** that injects a **Scoped** service captures it forever. `DbContext` is `Scoped` — **never inject it into a Singleton**.

---

### LINQ Deferred Execution — The Key Concept

LINQ queries are **not executed** until you materialize them with `ToList()`, `ToArray()`, `FirstOrDefault()`, `Count()`, or iterate with `foreach`.

```csharp
// Builds query expression tree — does NOT hit the database yet
IQueryable<SiteInventory> query = _db.Inventories
    .Include(x => x.Site)
    .AsNoTracking();

// Filtering still deferred:
if (!string.IsNullOrEmpty(siteCode))
    query = query.Where(x => x.Site.SiteCode == siteCode);  // Still NOT executed

// .ToListAsync() materializes — THIS is when SQL is sent to the database
var results = await query.ToListAsync(); // NOW the SQL executes
```

This is the pattern in `InventoryController.GetStock()`. Java Hibernate/JPA has the equivalent with `CriteriaBuilder`.

---

### String Interpolation and Formatting

In Java:
```java
String msg = String.format("Requisition %s created for site %s", req.getNumber(), site.getCode());
```

In C#:
```csharp
// String interpolation (most common)
string msg = $"Requisition {req.RequisitionNumber} created for site {site.SiteCode} at {DateTime.UtcNow:yyyy-MM-dd}";

// Raw string literals (C# 11+ — like Java text blocks)
string json = """
    {
        "site": "BCCH",
        "qty": 150
    }
    """;

// Verbatim strings (@ prefix disables escape sequences)
string path = @"C:\Program Files\PHSA\SupplyChain\logs";

// Combining verbatim + interpolation:
string logPath = $@"C:\PHSA\logs\{DateTime.UtcNow:yyyy-MM-dd}.log";
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

### Middleware in Depth — Java Filter vs C# Middleware

In Spring Boot:
```java
@Component
public class AuditLogFilter implements HandlerInterceptor {
    @Override
    public boolean preHandle(HttpServletRequest req, ...) {
        log.info("Request: {} {}", req.getMethod(), req.getRequestURI());
        return true;
    }
}
```

In ASP.NET Core, middleware is a chain of `RequestDelegate` functions:
```csharp
// Simple inline middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] --> {context.Request.Method} {context.Request.Path}");
    await next(context);
    Console.WriteLine($"[{DateTime.UtcNow:HH:mm:ss}] <-- {context.Response.StatusCode}");
});

// Reusable middleware class
public class HealthcareAuditMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
        // Log completed requests to audit trail
    }
}
// Register in Program.cs:
app.UseMiddleware<HealthcareAuditMiddleware>();
```

#### Middleware Order Matters!

```csharp
app.UseExceptionHandler("/Error");  // 1. Catch unhandled exceptions FIRST
app.UseStaticFiles();               // 2. Serve static files before auth check
app.UseRouting();                   // 3. Determine which controller handles request
app.UseAuthentication();            // 4. Read JWT/cookie, set User.Identity
app.UseAuthorization();             // 5. Check [Authorize] attributes
app.MapControllers();               // 6. Execute matched controller action
```

> If `UseAuthorization()` comes before `UseAuthentication()`, User.Identity is never set — all authorization fails. Classic production bug.

---

### Action Results — HTTP Response Types

| Result Method | HTTP Status | Java Equivalent |
| :--- | :--- | :--- |
| `Ok(data)` | 200 | `ResponseEntity.ok(data)` |
| `Created(url, data)` | 201 | `ResponseEntity.created(uri).body(data)` |
| `CreatedAtAction(...)` | 201 + Location header | `ResponseEntity.created(uri).body(data)` |
| `NoContent()` | 204 | `ResponseEntity.noContent().build()` |
| `BadRequest(model)` | 400 | `ResponseEntity.badRequest().body(errors)` |
| `NotFound()` | 404 | `ResponseEntity.notFound().build()` |
| `Unauthorized()` | 401 | `ResponseEntity.status(401).build()` |
| `StatusCode(500)` | 500 | `ResponseEntity.status(500).build()` |

---

### MVC vs Web API Controllers — The Two Types in This Project

#### 1. MVC Controller (renders HTML views)
```csharp
public class HomeController : Controller  // Inherits Controller (full MVC)
{
    public async Task<IActionResult> Index()
    {
        var stock = await _db.Inventories.Include(x => x.Site).Include(x => x.Item).AsNoTracking().ToListAsync();
        return View(stock); // Renders Views/Home/Index.cshtml
    }
}
```

#### 2. Web API Controller (returns JSON)
```csharp
[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase  // ControllerBase = no View support (lighter)
{
    [HttpGet("stock")]
    public async Task<IActionResult> GetStock()
    {
        var data = await _db.Inventories.AsNoTracking().ToListAsync();
        return Ok(data); // Serializes 'data' to JSON
    }
}
```

> `Controller` inherits from `ControllerBase` and adds Razor View support. Java Spring's `@RestController` = `[ApiController]` + `ControllerBase`.

---

### Minimal APIs (New in .NET 6+)

.NET 6+ introduced **Minimal APIs** — a lighter alternative to controller-based APIs:

```csharp
// Program.cs — entire API without any controller class
app.MapGet("/api/sites", async (BuntzenDbContext db) =>
{
    var sites = await db.Sites.AsNoTracking().ToListAsync();
    return Results.Ok(sites);
});

app.MapGet("/api/items/{id:guid}", async (Guid id, BuntzenDbContext db) =>
{
    var item = await db.Items.FindAsync(id);
    return item is not null ? Results.Ok(item) : Results.NotFound();
});
```

Minimal APIs are great for microservices. Controller-based APIs are better for large applications needing filters, versioning, and Swagger.

---

## 5. Entity Framework Core (EF Core) and Dapper Deep Dive

### What is EF Core? (The Hibernate of .NET)

| Concept | Java (Hibernate/JPA) | C# (EF Core) |
| :--- | :--- | :--- |
| Session / Unit of Work | `EntityManager` / `Session` | `DbContext` |
| Entity Registration | `@Entity` annotation | `DbSet<T>` in `DbContext` |
| JPQL / HQL | `@Query("FROM User u WHERE...")` | LINQ expressions |
| Schema Migration | Flyway / Liquibase | `dotnet ef migrations add` |
| Relationship Mapping | `@OneToMany`, `@ManyToOne` | Fluent API in `OnModelCreating()` |
| Lazy Loading | Default in Hibernate | Opt-in with proxies (avoided in modern EF) |
| Eager Loading | `JOIN FETCH` in JPQL | `.Include()` / `.ThenInclude()` |

---

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

### N+1 Query Problem — The Most Common ORM Bug

This is a guaranteed interview topic at senior level.

**Java Hibernate N+1:**
```java
// 1 query for orders + 1 per order for lineItems = N+1 problem
List<RequisitionOrder> orders = requisitionRepository.findAll();
for (RequisitionOrder order : orders) {
    System.out.println(order.getLineItems().size()); // Triggers individual SELECT per order!
}
// Fix in Java: @EntityGraph or JOIN FETCH
```

**EF Core N+1:**
```csharp
// PROBLEM: 1 for Requisitions + N for each LineItems collection = N+1 queries
var orders = await _db.Requisitions.AsNoTracking().ToListAsync();
foreach (var order in orders)
{
    var count = order.LineItems.Count; // Lazy load triggers separate SELECT per order!
}

// FIX: Use Include() — single LEFT JOIN query, no N+1
var orders = await _db.Requisitions
    .Include(r => r.LineItems)
    .AsNoTracking()
    .ToListAsync();
```

---

### EF Core Fluent API — Configuring Relationships

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Many-to-One: SiteInventory -> HealthAuthoritySite
    modelBuilder.Entity<SiteInventory>()
        .HasOne(x => x.Site)
        .WithMany()
        .HasForeignKey(x => x.SiteId);

    // One-to-Many: RequisitionOrder -> RequisitionLineItems
    modelBuilder.Entity<RequisitionOrder>()
        .HasMany(x => x.LineItems)
        .WithOne()
        .HasForeignKey(x => x.RequisitionOrderId);

    // Unique index on ItemNumber
    modelBuilder.Entity<SupplyItem>()
        .HasIndex(x => x.ItemNumber)
        .IsUnique();
}
```

#### Java JPA Equivalent
```java
@Entity
public class SiteInventory {
    @ManyToOne(fetch = FetchType.LAZY)
    @JoinColumn(name = "site_id")
    private HealthAuthoritySite site;
}

@Entity
public class RequisitionOrder {
    @OneToMany(mappedBy = "requisitionOrder", cascade = CascadeType.ALL)
    private List<RequisitionLineItem> lineItems;
}
```

---

### EF Core Migrations — Schema Management

Equivalent to Flyway/Liquibase. Track schema changes as versioned C# code files.

```bash
# Create a new migration
dotnet ef migrations add AddAuditLogIndex --project BuntzenSupplyChain.Infrastructure --startup-project BuntzenSupplyChain.Api

# Apply pending migrations
dotnet ef database update --project BuntzenSupplyChain.Infrastructure --startup-project BuntzenSupplyChain.Api

# Generate SQL script for DBA review
dotnet ef migrations script --output ./migrations.sql
```

---

### Dapper — Micro-ORM for Raw SQL Performance

```csharp
using Dapper;
using Microsoft.Data.SqlClient;

public async Task<IEnumerable<ParDeficitDto>> GetTopDeficitsByAuthorityAsync(string authority)
{
    const string sql = @"
        SELECT s.SiteCode, i.ItemNumber, i.Name AS ItemName,
               inv.QuantityOnHand, inv.ParLevel,
               (inv.ParLevel - inv.QuantityOnHand) AS DeficitAmount
        FROM SiteInventories inv
        INNER JOIN HealthAuthoritySites s ON inv.SiteId = s.Id
        INNER JOIN SupplyItems i ON inv.ItemId = i.Id
        WHERE inv.QuantityOnHand < inv.ReorderThreshold
          AND s.Authority = @Authority
        ORDER BY DeficitAmount DESC";

    await using var connection = new SqlConnection(_connectionString);
    return await connection.QueryAsync<ParDeficitDto>(sql, new { Authority = authority });
}

// Calling a Stored Procedure with Dapper
public async Task<IEnumerable<ParDeficitDto>> GetHospitalParDeficitsAsync(string siteCode)
{
    await using var connection = new SqlConnection(_connectionString);
    return await connection.QueryAsync<ParDeficitDto>(
        "sp_GetHospitalParDeficits",
        new { SiteCode = siteCode },
        commandType: CommandType.StoredProcedure);
}
```

#### EF Core vs Dapper — When to Use Which

| Scenario | Use EF Core | Use Dapper |
| :--- | :--- | :--- |
| Simple CRUD operations | Yes | |
| LINQ-expressible queries | Yes | |
| Schema migrations | Yes | |
| Complex multi-table joins/aggregates | | Yes |
| Stored procedure calls | Both work | Yes (simpler) |
| Read-heavy reporting queries | | Yes (faster) |

---
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

## 10. Interview STAR Stories — Behavioral Coaching

STAR format: **S**ituation → **T**ask → **A**ction → **R**esult

### STAR Story 1: Performance Optimization Under Pressure

> "Tell me about a time you improved application performance."

**Situation**: At Brigham and Women's Hospital / Philips, the cardiac monitor data pipeline experienced 8–12 second delays in alarm propagation during peak ICU hours (7am–9am, 3pm–5pm shift changes). This was clinically unacceptable — cardiac alarms need sub-second response.

**Task**: Profile and optimize the data ingestion pipeline that received telemetry from 40+ bedside monitors.

**Action**:
1. Application-level profiling identified 73% of latency was in database queries
2. Discovered N+1 query patterns — for each patient record, 1+N queries loaded device readings separately
3. Rewrote queries to use eager loading (JOIN FETCH in JPA, equivalent to EF Core's `.Include()`)
4. Added covering indexes on high-frequency query columns (PatientId + Timestamp)
5. Implemented `AsNoTracking()` equivalent for all display-path queries

**Result**: Alarm propagation latency dropped from 8–12 seconds to under 400 milliseconds — a 20–30x improvement, validated in pre-production UAT before go-live.

---

### STAR Story 2: Integrating with External Systems

> "Describe a time you had to integrate with a third-party system or external data source."

**Situation**: The Philips cardiac monitoring system needed to receive patient demographic data from the hospital's Epic EHR system to correlate monitor readings with patient identities.

**Task**: Build a real-time integration service consuming HL7 ADT (Admit, Discharge, Transfer) messages from Epic and updating the monitoring system's patient registry.

**Action**:
1. Designed an XML/HL7 message parser (same pattern as the XSLT transformation engine in this Buntzen project)
2. Implemented the canonical message pattern — transformed Epic's HL7 format into our internal patient model
3. Used connection retry logic and dead-letter queuing for failed message processing

**Result**: Successfully integrated with Epic in the test environment, enabling real-time patient-monitor association that eliminated manual reconciliation estimated at 2+ hours per day per unit.

---

### STAR Story 3: Code Quality / Technical Debt

> "Tell me about a time you improved code maintainability or reduced technical debt."

**Situation**: Inherited a C# data access layer that used raw ADO.NET `SqlConnection` and `SqlDataReader` with hardcoded SQL strings scattered across 15+ files — no parameterization, vulnerable to SQL injection.

**Task**: Modernize the data access layer to use EF Core while maintaining zero downtime.

**Action**:
1. Introduced EF Core `DbContext` alongside existing code (not a big-bang replacement)
2. Migrated one entity at a time, starting with read-only reporting queries
3. Added integration tests for each migrated entity before removing old ADO.NET code
4. Used Dapper for complex stored procedure calls that didn't map cleanly to LINQ

**Result**: Eliminated all raw string SQL. Zero SQL injection vulnerabilities. New features took 60% less time to implement.

---

### STAR Story 4: Real-Time System Work

> "Tell me about a real-time system you've built or worked with."

**Situation**: The cardiac monitor dashboard at Brigham needed to display live vital signs (ECG, SPO2, blood pressure) updating every second per patient without page refreshes.

**Task**: Implement real-time data streaming from server to the clinical dashboard browser.

**Action**: Used WebSockets for bidirectional real-time communication:
```csharp
app.UseWebSockets();
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/vitals" && context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        await StreamVitalSignsAsync(webSocket, cancellationToken);
    }
    else await next(context);
});
```

**Result**: Eliminated polling (40+ requests/second for a 20-bed ICU). WebSocket reduced server load by 85% while delivering data 5x faster.

---

## 11. What's New in .NET 8 / .NET 9 / .NET 10 (Must-Know for 2026)

### .NET 8 (LTS — November 2023) — Key Features

#### 1. Primary Constructors (C# 12)
```csharp
// OLD way
public class InventoryService
{
    private readonly BuntzenDbContext _db;
    public InventoryService(BuntzenDbContext db) { _db = db; }
}

// NEW: Primary Constructor (C# 12 / .NET 8)
public class InventoryService(BuntzenDbContext db)
{
    // 'db' available throughout the class — no backing field needed
    public async Task<List<SiteInventory>> GetAllAsync() =>
        await db.Inventories.AsNoTracking().ToListAsync();
}
```

#### 2. `TimeProvider` — Testable Time (new in .NET 8)
```csharp
// OLD: DateTime.UtcNow is not mockable in tests
transaction.CreatedAt = DateTime.UtcNow;

// NEW: TimeProvider (injectable, mockable)
public class EdiTransactionService(TimeProvider timeProvider)
{
    public EdiTransaction Create() => new()
    {
        CreatedAt = timeProvider.GetUtcNow().UtcDateTime
    };
}
// In tests: TimeProvider.Fixed(new DateTimeOffset(2026, 8, 10, ...))
```

#### 3. Frozen Collections
```csharp
using System.Collections.Frozen;

// FrozenDictionary has faster read performance for static/constant data
var xsltTemplates = new Dictionary<string, string> { ... }
    .ToFrozenDictionary(); // Perfect for XmlXsltTransformationService!
```

#### 4. `System.Text.Json` Source Generation (Zero Reflection)
```csharp
[JsonSerializable(typeof(List<SupplyItemDto>))]
public partial class SupplyChainJsonContext : JsonSerializerContext { }

// No runtime reflection — AOT-compatible, 2-5x faster serialization
string json = JsonSerializer.Serialize(items, SupplyChainJsonContext.Default.ListSupplyItemDto);
```

---

### .NET 9 (STS — November 2024) — Key Features

#### 1. LINQ `CountBy` and `AggregateBy` (New Methods!)
```csharp
// CountBy
var countByCategory = items.CountBy(i => i.Category);

// AggregateBy
var totalCostByCategory = items.AggregateBy(
    i => i.Category,
    seed: 0m,
    (totalCost, item) => totalCost + item.UnitPrice);
```

#### 2. `Span<T>` — Zero-Allocation Data Processing
```csharp
// Slice data without allocating new arrays
ReadOnlySpan<byte> hl7Message = rawBytes.AsSpan(0, messageLength);
// Processes bytes in-place — zero heap allocation
```

---

### .NET 10 (LTS — November 2025) — Current Version

1. **Native AOT**: Compile to native binary — faster startup, smaller memory, no JIT. Key for containerized microservices.
2. **Aspire** (distributed app orchestration): .NET's answer to Spring Cloud. Wires up containers, databases, services with health checks and dashboards.
3. **OpenAPI built-in**: Native OpenAPI document generation without external libraries like Swashbuckle.
4. **HybridCache**: New caching API that unifies in-memory and distributed caches with stampede protection.

---

## 12. 30 Likely PHSA Interview Questions with Model Answers

### Technical Questions

**Q1: What is Clean Architecture and why is it important in healthcare systems?**
Clean Architecture separates the application into concentric layers where dependencies always point inward. Domain entities have zero dependencies on infrastructure. Critical in healthcare because: (1) regulatory compliance requires separation of concerns; (2) database technology changes should not require rewriting business rules; (3) unit testing business logic without a live database is required for CI/CD in regulated environments.

---

**Q2: What is the difference between `IEnumerable<T>`, `IQueryable<T>`, and `IList<T>`?**
- `IEnumerable<T>`: In-memory iteration. LINQ operations run in C#.
- `IQueryable<T>`: Expression tree that translates to SQL. EF Core returns `IQueryable<T>` from `DbSet<T>` — adding `.Where()` modifies the SQL query.
- `IList<T>`: In-memory collection supporting indexing, `Count`, `Add`, `Remove`.

Calling `.ToList()` on an `IQueryable<T>` sends the SQL to the database. Never call methods inside a loop on an `IQueryable<T>` — this causes N+1 queries.

---

**Q3: What is `async/await` and how does it differ from creating a new thread?**
`async/await` does NOT create new threads. When a method hits `await`, it suspends and **returns the current thread to the thread pool**. When the awaited I/O completes, any available thread pool thread picks up execution after the `await`. Creating a new thread (`Task.Run()`) takes a thread pool thread and keeps it busy for the entire duration.

---

**Q4: What is SARGability and how does it affect query performance?**
SARGability means the query optimizer can use an index to directly seek rows matching the WHERE predicate. Non-SARGable queries wrap indexed columns in functions (UPPER, CAST, YEAR), forcing a full index scan. Always use direct column comparisons with parameterized values and date ranges instead of casting.

---

**Q5: What is the N+1 query problem in EF Core and how do you prevent it?**
N+1 occurs when loading N parent entities, then making 1 additional database query per parent to load related children — N+1 total queries. Fix: use `.Include(r => r.LineItems)` to load all data in a single SQL JOIN query.

---

**Q6: Explain the DI container service lifetimes (Scoped, Transient, Singleton).**
- **Scoped**: One instance per HTTP request. `BuntzenDbContext` is Scoped. Disposed at end of request.
- **Transient**: New instance every time it's requested from the DI container.
- **Singleton**: One instance for the entire application lifetime.

Never inject a Scoped service into a Singleton — the "captive dependency" trap. `DbContext` is Scoped; if injected into a Singleton it becomes a thread-safety risk and potential data leak.

---

**Q7: What is the difference between `Controller` and `ControllerBase` in ASP.NET Core?**
`ControllerBase` is the base class for Web API controllers. `Controller` inherits from `ControllerBase` and adds Razor View support (`View()`, `PartialView()`, `ViewBag`). This project uses both: `HomeController : Controller` (HTML views) and `InventoryController : ControllerBase` (JSON). Use `ControllerBase` for pure API controllers — it's lighter weight.

---

**Q8: What does `[ValidateAntiForgeryToken]` do?**
Prevents Cross-Site Request Forgery (CSRF) attacks. ASP.NET Core generates a cryptographic token per session and embeds it in HTML forms. On POST, `[ValidateAntiForgeryToken]` validates the token — ensuring the form was submitted from your own app, not a malicious site.

---

**Q9: When would you use Dapper instead of EF Core?**
Use Dapper when: (1) you need maximum query performance for complex reporting queries that LINQ cannot express cleanly; (2) you're calling stored procedures with complex output; (3) you need database views or proprietary SQL features. Use EF Core for standard CRUD, migrations, and type-safe LINQ queries.

---

**Q10: What is parameter sniffing and how do you mitigate it?**
SQL Server caches an execution plan based on the first parameter values a stored procedure receives. If initial call had atypical data, all future calls use that suboptimal plan. Mitigations: (1) `OPTION (RECOMPILE)` — fresh plan per execution; (2) local variable masking; (3) `OPTION (OPTIMIZE FOR UNKNOWN)`; (4) separate stored procedures for different data distributions.

---

**Q11: What is `AsNoTracking()` and when should you use it?**
`AsNoTracking()` tells EF Core not to add returned entities to the Change Tracker. For read-only operations (reports, API GET endpoints, Razor view data), tracking is unnecessary overhead. Use for all read-only queries; use tracked queries only when you intend to modify and save the entity.

---

**Q12: Explain the differences between `First()`, `FirstOrDefault()`, `Single()`, `SingleOrDefault()`.**
- `First()`: Returns first element. Throws if no elements.
- `FirstOrDefault()`: Returns first element or null/default if none. Safe.
- `Single()`: Returns exactly one element. Throws if zero OR more than one.
- `SingleOrDefault()`: Returns the one element or null. Throws if more than one.

Use `Single()` for guaranteed unique results (primary key lookup). Use `FirstOrDefault()` for optional results.

---

**Q13: How does EF Core compile LINQ to SQL?**
EF Core builds an **Expression Tree** from LINQ method chains. When you call `.Where(x => x.UnitPrice > 20m)`, EF Core analyzes the lambda as data (not code) and translates it into parameterized SQL. Parameterization prevents SQL injection automatically — this is why EF Core is inherently more secure than raw string SQL.

---

**Q14: What is a covering index?**
A covering index includes all columns needed to satisfy a query without returning to the clustered index. When a non-clustered index lacks requested columns, SQL Server does a "Key Lookup" — following a pointer from the index to the main table for each row. Adding extra columns to the `INCLUDE (...)` clause eliminates Key Lookups. See `IX_SupplyChainAuditLogs_Action_Entity` in this project.

---

**Q15: What is the difference between `Task.Run()` and `async/await`?**
`Task.Run()` offloads work to a **thread pool thread** — for CPU-bound work (heavy computation, encryption). `async/await` is for **I/O-bound work** (database queries, HTTP calls, file I/O) — it doesn't use extra threads. NEVER do `Task.Run(() => db.Items.ToListAsync()).Result` — this can deadlock by blocking a thread pool thread while waiting for another thread pool thread.

---

**Q16: What is the `record` type in C# and how does it differ from `class`?**
`record` is a C# 9+ type that generates value-equality semantics, immutability patterns, and a built-in `with` expression for non-destructive mutation. Unlike `class` (reference equality), two `record` instances with the same property values are considered equal. Records are ideal for DTOs and Value Objects in Domain-Driven Design.

---

**Q17: What is the middleware pipeline in ASP.NET Core and how is it different from Java filters?**
Middleware is a chain of `RequestDelegate` functions that each receive an `HttpContext` and call the next middleware via `await next(context)`. Unlike Java servlet filters (which are framework-specific), ASP.NET Core middleware is first-class: order is explicitly defined in `Program.cs`, and it's the same mechanism used by the framework itself for routing, authentication, and static files.

---

**Q18: How do you handle database transactions in EF Core?**
EF Core wraps `SaveChangesAsync()` in a transaction by default — all pending entity changes are atomic. For explicit multi-step transactions:
```csharp
await using var transaction = await _db.Database.BeginTransactionAsync();
try {
    _db.Requisitions.Add(req);
    _db.AuditLogs.Add(audit);
    await _db.SaveChangesAsync();
    await transaction.CommitAsync();
} catch {
    await transaction.RollbackAsync();
    throw;
}
```

---

**Q19: What is `decimal` and why is it used for money in C#?**
`decimal` uses base-10 floating-point arithmetic with 28–29 significant digits. `double` and `float` use base-2, causing rounding errors: `0.1 + 0.2 = 0.30000000000000004` in double. In C#: `0.1m + 0.2m = 0.3m` exactly. Java equivalent: `BigDecimal`. Always use `decimal` for financial calculations in healthcare supply chain (medication costs, invoice totals).

---

**Q20: What is string interpolation and what are raw string literals?**
String interpolation (`$"..."`) embeds expressions directly: `$"Site: {site.SiteCode} at {DateTime.UtcNow:yyyy-MM-dd}"`. Raw string literals (C# 11, `"""..."""`) allow embedded quotes and newlines without escape sequences — equivalent to Java text blocks.

---

### Behavioral / Situational Questions

**Q21**: Tell me about a challenging technical problem you solved.
→ Use STAR Story 1 (performance optimization — 20–30x improvement in alarm latency)

**Q22**: Describe how you handle integration with external systems.
→ Use STAR Story 2 (HL7/Epic ADT integration at Brigham)

**Q23**: How do you approach technical debt in a legacy codebase?
→ Use STAR Story 3 (ADO.NET → EF Core migration, zero downtime, incremental approach)

**Q24**: Tell me about a time you worked with real-time data.
→ Use STAR Story 4 (WebSocket cardiac monitor dashboard, 85% server load reduction)

**Q25**: How do you handle disagreements with colleagues on technical approach?
→ "I prototype both approaches, measure with benchmarks, then present data. Performance numbers and concrete trade-off tables resolve most technical disagreements quickly."

**Q26**: How do you stay current with technology?
→ "Microsoft's .NET blog, DotNetConf (annual conference), building projects like this Buntzen platform, reading the ASP.NET Core changelog for each release."

**Q27**: Describe your experience with healthcare data privacy and compliance.
→ "At Brigham/Philips, patient data was classified, encrypted at rest (AES-256) and in transit (TLS 1.2+), access was role-based. Every data access was logged to an immutable audit trail — similar to the `SupplyChainAuditLog` table in this project."

---

### System Design / Architecture Questions

**Q28**: How would you design a real-time inventory alert system for PHSA?
1. Background service (`IHostedService` / Worker Service) polling inventory every 5 minutes
2. When `QuantityOnHand < ReorderThreshold`, publish an alert event
3. Use SignalR (ASP.NET Core's real-time library) to push alerts to browser dashboards
4. Store alert history in `SupplyChainAuditLog` with `Action = "PAR_DEFICIT_ALERT"`
5. Send email notifications via SendGrid/SMTP for critical items

**Q29**: How would you secure the API endpoints?
1. JWT Bearer authentication (`builder.Services.AddAuthentication().AddJwtBearer(...)`)
2. `[Authorize]` attribute on controllers/actions
3. Role-based authorization: `[Authorize(Roles = "SupplyManager,Administrator")]`
4. Rate limiting (`builder.Services.AddRateLimiter(...)` — new in .NET 7)
5. Audit all API calls to `SupplyChainAuditLog`

**Q30**: How is EF Core's DbContext similar to and different from Spring's EntityManager?

**Similar**: Both are the "Unit of Work" — they track changes to entities in memory and flush them to the database in a single transaction. Both support LINQ/JPQL queries, eager loading with joins, and relationship management.

**Different**: EF Core's `DbContext` is designed to be scoped per HTTP request (thread-unsafe by design). Spring's `EntityManager` uses persistence context with container-managed transactions (`@Transactional`). EF Core uses `SaveChangesAsync()` as an explicit commit; Spring commits at the end of a `@Transactional` boundary. EF Core migrations are C# code files; Spring typically uses Flyway/Liquibase SQL scripts.

---

## Summary Checklist
* Clean Architecture layer boundaries enforced — can explain Dependency Inversion Principle.
* C# Auto-Properties, Nullable Types, LINQ methods, `async/await`, records, delegates mastered.
* Java-to-C# mapping table memorized — ready to answer "how would you do X in C#?" questions.
* ASP.NET Core Kestrel server, Middleware chain, Model Binding, Action Results understood.
* EF Core Change Tracker, `AsNoTracking()`, Eager Loading, N+1 prevention, Fluent API mastered.
* T-SQL SARGability, Covering Indexes, Window Functions, Parameter Sniffing mastered.
* XML/XSLT/EDI transformation pipeline understood in context of healthcare data integration.
* Docker, `Program.cs` composition root, DI lifetimes, Azure DevOps CI/CD pipeline understood.
* STAR stories prepared for Brigham/Philips experience — performance, integration, refactoring, real-time.
* All 30 interview Q&A reviewed and can speak to each confidently.
* What's New in .NET 8/9/10 — primary constructors, TimeProvider, LINQ additions, source generation.
* `decimal` vs `double` for money: **always `decimal` in healthcare systems**.
* All project files committed and pushed to GitHub repository. Good luck! You've got this. 🎯

