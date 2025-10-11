# **Sinch .NET SDK — Developer Experience Design Document**

## Assessment Requirements

The goal of this assessment is to **model access to the provided REST API** through a .NET SDK that delivers an **exceptional developer experience (DX)** and a smooth onboarding path for first-time users.

The SDK must provide a clear, discoverable, and intuitive interface that hides HTTP complexity while exposing the API’s core capabilities.

> Note: This SDK prioritizes developer experience by abstracting HTTP details behind expressive APIs, ensuring discoverability, minimal ceremony, and safe defaults.

---

## API Scope

The SDK must cover the following operations:

### Contacts

* Get
* Pagination
* Create
* Update
* Delete

### Messages

* Get
* Pagination
* Send

> **Authentication:**
> All API calls require a Bearer token header:
> `Authorization: Bearer there-is-no-key`

---

## SDK Requirements

### Musts

* **Discoverable and expressive:**
  Well-named, self-explanatory methods and models.

* **Idiomatic .NET design:**
  Async-first, `CancellationToken` support, and standard Task-based signatures.

* **Automatic `HttpClient` management:**
  Internalized HTTP handling with minimal configuration.

* **Delegating handler for authentication:**
  Automatically injects the Bearer token into every request.
  Token retrieval should be configurable via DI or a delegate:

  ```csharp
  options.ConfigureAuth(ctx => ctx.UseBearerToken(() => "my-token"));
  ```

* **Resiliency policies:**
  Basic retries (for idempotent requests) and timeouts built-in, opt-in via configuration.

* **Dependency Injection integration:**
  Provide a fluent registration method with clear extension points:

  ```csharp
  services.AddSinchSdk(options => { ... });
  ```

* **Canonical request/response models:**
  Use parameters for simple cases and request objects for complex or evolving ones.

* **Unified error abstraction:**
  Map API/HTTP errors (400, 404, etc.) to SDK-specific exceptions.
  Example:

  ```csharp
  try
  {
      await client.Messages.SendAsync(...);
  }
  catch (NotFoundException) { ... }
  ```

* **Testing coverage:**
  Unit and integration tests validating the main API flows and error scenarios. Unit tests will validate error mapping and request construction; integration tests will hit the Dockerized API locally.

---

### Nice-to-Haves

* **Fluent DI API:**
  Extension-method syntax for advanced features:

  ```csharp
  options.EnableRetries(p => p.MaxRetries = 3)
         .EnableTracing()
         .ConfigurePagination(defaultSize: 50);
  ```

* **Pagination configuration:**
  Global defaults at startup with per-method overrides.

* **Hooks for power users:**
  Customizable handlers or interceptors (e.g., logging, diagnostics, or telemetry).

* **Webhook signature validation:**
  Optional helper for validating HMAC signatures.

* **Granular service registration:**
  Allow injecting individual components (e.g., `IContactsApi`, `IMessagesApi`)
  without needing the entire façade.

* **SDK instrumentation support:**
  Traces and metrics recording to optionally support SDK instrumentation.

---

## Ideal SDK Usage

### Sending a message

```csharp
public class MyService(ISinchClient sinch)
{
    public async Task SendMessageAsync(string from, string to, string content, CancellationToken ct = default)
    {
        var response = await sinch.Messages.SendAsync(from, to, content, ct);
        // Handle response...
    }
}
```

### Paginated retrieval

```csharp
public class MyService(ISinchClient sinch)
{
    public async Task FetchPagedMessagesAsync(int page = 0, int size = 50, CancellationToken ct = default)
    {
        var response = await sinch.Messages.GetPagedAsync(
            new PaginationOptions(page, size),
            cancellationToken: ct
        );
        // Handle paged response...
    }
}
```

Where:

```csharp
public record PaginationOptions(int Page = 0, int Size = 50);
```

---

## Public API Surface (Consumption Level)

### Entrypoints

| Component      | Responsibility                                              |
| -------------- | ----------------------------------------------------------- |
| `ISinchClient` | Root façade providing access to all SDK features.           |
| `IMessagesApi` | Handles message-related operations (Send, Get, Pagination). |
| `IContactsApi` | Handles contact management operations (CRUD, Pagination).   |

### DI Registration

```csharp
services.AddSinchSdk(options =>
{
    options.ConfigureAuth(ctx => ctx.UseBearerToken(() => "there-is-no-key"));
    options.EnableRetries(p => p.MaxRetries = 3);
});
```

Or granular registration:

```csharp
services.AddSinchMessaging(options => { ... });
services.AddSinchContacts(options => { ... });
```

### Options Object

Configurable settings include:

* Base URL
* Auth configuration
* Retry/timeout policies
* Default pagination
* Optional diagnostics hooks

Example:

```csharp
public class SinchOptions
{
    public Uri BaseAddress { get; set; } = new("https://api.localhost/");
    public AuthOptions Auth { get; set; } = new();
    public RetryOptions Retry { get; set; } = new();
    public PaginationOptions Pagination { get; set; } = new();
}
```

---

## Error Contract

Define a unified hierarchy to encapsulate all API errors:

```csharp
public abstract class SinchException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
}

public class NotFoundException : SinchException { }
public class ValidationException : SinchException { }
public class UnauthorizedException : SinchException { }
```

No raw HTTP exceptions should bubble up.

---

## Response Envelope

To ensure consistent data models across endpoints:

```csharp
public sealed record ApiResponse<T>(T Data, ApiMeta Meta);

public sealed record ApiMeta(
    string RequestId,
    string TimestampUtc,
    ApiPagination? Pagination = null,
    IReadOnlyList<string>? Warnings = null
);

public sealed record ApiPagination(
    int Page,
    int PageSize,
    long? Total = null,
    bool? HasNext = null
);
```

---

## ✅ Summary

| Goal                         | Achieved Through                            |
| ---------------------------- | ------------------------------------------- |
| Simplicity & discoverability | `SinchClient` façade with intuitive methods |
| Configurability              | Fluent DI and options pattern               |
| Robustness                   | Built-in retries, timeouts, auth handler    |
| Consistency                  | Canonical models, unified error contract    |
| DX excellence                | Minimal ceremony, maximum expressiveness    |
