# Sinch .NET SDK (Assessment Edition)

> **Developer-Experience-first SDK for Sinch REST API** — modern async design, DI-friendly setup, policy-based resiliency, and simple, expressive API surface for **Contacts** and **Messages**.

[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

> **Status:** Assessment build (pre-v1). Intended to demonstrate production-grade SDK structure and developer ergonomics.

---

## Table of Contents

* [About](#about)
* [Features](#features)
* [Install](#install)
* [Quickstart](#quickstart)
* [Usage](#usage)
  * [Contacts API](#contacts-api)
  * [Messages API](#messages-api)
* [Configuration & DI](#configuration--di)
* [Resiliency](#resiliency)
* [Error Handling](#error-handling)
* [Signature Verification (Webhook HMAC)](#signature-verification-webhook-hmac)
* [Testing](#testing)
* [Roadmap](#roadmap)
* [License](#license)

---

## About

This SDK wraps the **Sinch REST API** behind a clean, discoverable .NET interface that feels native to modern .NET developers.
It was built from scratch to showcase:

* **Strong Developer Experience** (DX) focus — discoverable, fluent, and testable.
* **Async-first design** with `CancellationToken` support everywhere.
* **Automatic Bearer authentication** through a pluggable token delegate.
* **Built-in resiliency** (timeouts, retries) powered by Polly.
* **Canonical responses** that unify Sinch’s API results into consistent `.Data` + `.Meta` envelopes.

---

## Features

- Auto-discovered, DI-ready SDK entrypoints
- Clean async API: `await client.Contacts.CreateAsync(...)`
- Canonical result models (`SinchResponse<T>` with `Meta`)
- Built-in retry and timeout policies
- Configurable token provider (sync/async)
- Friendly error mapping (400/401/404/500 → typed exceptions)
- Integration-test coverage for Contacts & Messages

---

## Install

> For now: just a project reference or local NuGet feed.

```bash
dotnet add package Sinch.Sdk
```

---

## Quickstart

```csharp
// 1. Register the SDK in DI
services.AddSinchSdk(opts =>
{
    opts.BaseAddress = new Uri("http://localhost:3000");
    opts.Auth.UseBearerToken(() => "there-is-no-key");
});

// 2. Resolve and use 
var provider = services.BuildServiceProvider();
var sinch = provider.GetRequiredService<ISinchClient>();

or inject ISinchClient into your classes

public class MyService
{
    private readonly ISinchClient _sinch;
    public MyService(ISinchClient sinch)
    {
        _sinch = sinch;
    }
}

// 3. Use it!
var created = await sinch.Contacts.CreateAsync("Alice", "+34988208897");
Console.WriteLine($"Created contact {created.Data.Id}");

// Fetch it again
var fetched = await sinch.Contacts.GetAsync(created.Data.Id);
Console.WriteLine($"Fetched: {fetched.Data.Name} ({fetched.Data.Phone})");
```

---

## Configuration & DI

The SDK is designed for seamless integration into .NET.

```csharp
builder.Services.AddSinchSdk(opts =>
{
    opts.BaseAddress = new Uri("http://localhost:3000");
    opts.Auth.UseBearerTokenAsync(async ct => await tokenService.GetAccessToken(ct));

    opts.Resilience.Timeout.Overall = TimeSpan.FromSeconds(15);
    opts.Resilience.Retry.Attempts = 3;
});
```

Internally this registers:

* A named `HttpClient` (`"SinchSdk"`)
* Delegating handlers:
  * **`AuthHandler`** — adds the `Authorization: Bearer` header.
  * **`ResilienceHandler`** — applies retry/timeout policies.
  > The order is important: auth first, then resiliency.
  > Can be extended with custom or different handlers as needed later on.
* Core services: `IHttpGateway`, `IContactsApi`, `IMessagesApi`, and `ISinchClient`.

---

## Usage

The SDK needs to be resolved from the DI container:
```csharp
var sinch = provider.GetRequiredService<ISinchClient>();

// or inject ISinchClient into your classes

public class MyService
{
    private readonly ISinchClient _sinch;
    public MyService(ISinchClient sinch)
    {
        _sinch = sinch;
    }
}
```

### Contacts API

```csharp
// Create
var created = await sinch.Contacts.CreateAsync("John", "+15551111");

// Get single
var contact = await sinch.Contacts.GetAsync(created.Data.Id);

// Update
var updated = await sinch.Contacts.UpdateAsync(created.Data.Id, "John B", "+15552222");

// List (paged)
var page = await sinch.Contacts.GetPagedAsync();
var pageWithConcreteSize = await sinch.Contacts.GetPagedAsync(new PaginationOptions(0, 10)); // page 0, size 10

// Delete
await sinch.Contacts.DeleteAsync(created.Data.Id);
```

> The SDK automatically wraps all responses in:
>
> ```csharp
> public sealed record SinchResponse<T>(T Data, SinchMeta Meta);
> ```
> Where `Meta` contains request metadata like timestamps or pagination info.
> This makes it easy to extend in the future without breaking changes and also offers a consistent experience across all APIs.

### Messages API

```csharp
// Send a message
var msg = await sinch.Messages.SendAsync(
    to: {Recipiend Id},
    from: "service",
    content: "Hello from the SDK!"
);

// Get message status
var status = await sinch.Messages.GetAsync(msg.Data.Id);
Console.WriteLine($"Message {status.Data.Id} -> {status.Data.Status}");

// Paged messages
var page = await sinch.Messages.GetPagedAsync();
var pageWithConcreteSize = await sinch.Messages.GetPagedAsync(new PaginationOptions(0, 10)); // page 0, size 10
```

---

## Resiliency

The SDK ships with **opinionated defaults**:

* Timeout: 15 seconds (overall, optimistic)
* Retries: 3 attempts for idempotent requests (`GET`, `HEAD`, `OPTIONS`)
* Jittered exponential backoff

You can override these through options when registering the SDK in the DI container:

```csharp
opts.Resilience.Retry.Attempts = 5;
opts.Resilience.Retry.BaseDelay = TimeSpan.FromMilliseconds(200);
opts.Resilience.Timeout.Overall = TimeSpan.FromSeconds(20);
```

---

## Error Handling

All HTTP and network errors are mapped to canonical exceptions:

| Status Code       | SDK Exception             | Example Message                |
| ----------------- | ------------------------- | ------------------------------ |
| 400               | `ValidationException`     | `Bad request.`                 |
| 401 / 403         | `UnauthorizedException`   | `Unauthorized.` / `Forbidden.` |
| 404               | `NotFoundException`       | `Resource not found.`          |
| ≥500              | `InternalServerException` | `Server error.`                |

```csharp
try
{
    await sinch.Contacts.GetAsync("nonexistent");
}
catch (NotFoundException ex)
{
    Console.WriteLine($"Contact not found: {ex.Message}");
}
```

---

## Signature Verification (Webhook HMAC)

> Enables verifying Sinch webhook authenticity using HMAC-SHA256 signatures.

When Sinch’s API server sends a webhook event, it includes an Authorization header in the format:

```
Authorization: Signature <hex_value>
```

where `<hex_value>` is computed as:

```
HMAC_SHA256(secret, body)
```

The SDK provides a ready-to-use helper to verify that signature before processing the webhook payload.

### Registering the verifier

```csharp
services.AddSinchSignatureVerification(opts =>
{
    opts.Secret = Environment.GetEnvironmentVariable("WEBHOOK_SECRET") ?? "mySecret";
});
```


This registers an injectable `ISinchSignatureVerifier` that you can use in your application or .NET controller.

### Using it in a webhook controller

```csharp
[ApiController]
[Route("webhooks")]
[AllowAnonymous]
public class WebhooksController : ControllerBase
{
    private readonly ISinchSignatureVerifier _verifier;
    public WebhooksController(ISinchSignatureVerifier verifier) => _verifier = verifier;

    [HttpPost]
    public async Task<IActionResult> Post()
    {
        // Read exact raw body (important for signature match)
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var payload = ms.ToArray();

        var header = Request.Headers["Authorization"].ToString();

        if (!_verifier.Verify(header, payload))
            return Unauthorized();

        Console.WriteLine($"[WEBHOOK] {Encoding.UTF8.GetString(payload)}");
        return Ok();
    }
}
```

> The verification works only if the body is read exactly as it was received.
> This is why we copy the `Request.Body` stream into a `MemoryStream` first.

---

## Testing

### Unit Tests

Each component (auth, resiliency, gateway, APIs) is fully covered by unit tests. For the scope of this assessment, only the happy paths were tested.

### Integration Tests

Integration tests run against the local Docker stub (`http://localhost:3000`).

Example:

```csharp
[Fact(DisplayName = "Contacts: Create returns contact with Id and expected fields")]
public async Task Create_returns_contact_with_id()
{
    // Arrange
    var name = NewName();
    var phone = NewPhone();

    // Act
    var created = await _api.CreateAsync(name, phone);

    // Assert
    created.Should().NotBeNull();
    created.Data.Id.Should().NotBeNullOrWhiteSpace();
    created.Data.Name.Should().Be(name);
    created.Data.Phone.Should().Be(phone);
    created.Meta.TimestampUtc.Should().NotBeNullOrWhiteSpace();
}
```

---

## Roadmap

* ✅ Contacts + Messages SDKs (MVP)
* ✅ Typed error mapping
* ✅ DI + Resilience
* ✅ Webhook signature validation (future nice-to-have)
* ❌ FluentAPI for SDK DI setup (e.g. `services.AddSinchSdk().WithBearerToken(...).WithResilience(...)`)
* ❌ Use pagination interface marker (`IPaginatedResult<T>`) for pagination meta extraction. Also remove `PaginationOptions` class from public API surface and use simple `(int page, int size)` tuples instead.
* ❌ Paginated helpers (`IAsyncEnumerable` support)
* ❌ Diagnostics hooks for OpenTelemetry

---

## License

MIT — see [LICENSE](LICENSE).