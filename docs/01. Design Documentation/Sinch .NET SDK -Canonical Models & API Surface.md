
# **Sinch SDK Assessment – Canonical Models & API Surface**

---

## General

### Response Envelope

To ensure consistent data models across endpoints:

```csharp
public sealed record ApiResponse<T>(
    T Data,
    ApiMeta Meta
);

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

## Contacts

### Get

**Request:**
None — `id` obtained from method argument.

**Response:**

```csharp
public sealed record ContactResponse(string Name, string Phone, string Id);
```

> Enveloped in `ApiResponse<ContactResponse>`

**Sample usage:**

```csharp
await client.Contacts.GetAsync(id, cancellationToken: ct);
```

**Endpoint:**
`GET /contacts/{id}`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 400       | Bad Request  | `ValidationException`     |
| 401       | Unauthorized | `UnauthorizedException`   |
| 404       | Not Found    | `NotFoundException`       |
| 500       | Server Error | `InternalServerException` |

---

### List (Paged)

**Request (internally mapped from arguments):**

```csharp
public sealed record PagedOptions(int Page, int Size);
```

**Response:**

```csharp
public sealed record ContactResponse(string Name, string Phone, string Id);
```

> Enveloped in `ApiResponse<IEnumerable<ContactResponse>>`
> `ApiMeta.Pagination` populated from API response.

**Sample usage:**

```csharp
await client.Contacts.GetPagedAsync(pagedOptionsOrNull, cancellationToken: ct); // Fallbacks to defaults
```

**Endpoint:**
`GET /contacts`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 401       | Unauthorized | `UnauthorizedException`   |
| 500       | Server Error | `InternalServerException` |

---

### Create

**Request (internally mapped from arguments):**

```csharp
public sealed record CreateContactRequest(string Name, string Phone);
```

**Response:**

```csharp
public sealed record ContactResponse(string Name, string Phone, string Id);
```

> Enveloped in `ApiResponse<ContactResponse>`

**Sample usage:**

```csharp
await client.Contacts.CreateAsync(name, phone, cancellationToken: ct);
```

**Endpoint:**
`POST /contacts`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 400       | Bad Request  | `ValidationException`     |
| 401       | Unauthorized | `UnauthorizedException`   |
| 500       | Server Error | `InternalServerException` |

---

### Update

**Request (internally mapped from arguments):**

```csharp
public sealed record UpdateContactRequest(string Name, string Phone);
```

**Response:**

```csharp
public sealed record ContactResponse(string Name, string Phone, string Id);
```

> Enveloped in `ApiResponse<ContactResponse>`

**Sample usage:**

```csharp
await client.Contacts.UpdateAsync(id, name, phone, cancellationToken: ct);
```

**Endpoint:**
`PUT /contacts/{id}`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 401       | Unauthorized | `UnauthorizedException`   |
| 404       | Not Found    | `NotFoundException`       |
| 500       | Server Error | `InternalServerException` |

---

### Delete

**Request:**
None — `id` obtained from method argument.

**Response:**
No content.

**Sample usage:**

```csharp
await client.Contacts.DeleteAsync(id, cancellationToken: ct);
```

**Endpoint:**
`DELETE /contacts/{id}`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 401       | Unauthorized | `UnauthorizedException`   |
| 404       | Not Found    | `NotFoundException`       |
| 500       | Server Error | `InternalServerException` |

---

## Messages

### Get

**Request:**
None — `id` obtained from method argument.

**Response:**

```csharp
public sealed record MessageResponse(
    string From,
    string To,
    string Content,
    string Id,
    MessageStatus Status,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);

public enum MessageStatus
{
    Queued,
    Acknowledged,
    Failed
}
```

> Enveloped in `ApiResponse<MessageResponse>`

**Sample usage:**

```csharp
await client.Messages.GetAsync(id, cancellationToken: ct);
```

**Endpoint:**
`GET /messages/{id}`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 401       | Unauthorized | `UnauthorizedException`   |
| 404       | Not Found    | `NotFoundException`       |
| 500       | Server Error | `InternalServerException` |

---

### List (Paged)

**Request:**

```csharp
public sealed record PagedOptions(int Page, int Size);
```

**Response:**

```csharp
public sealed record MessagePagedResponse(
    IEnumerable<MessageResponse> Messages,
    MessageContactsExtraInfo ContactsInfo
);

public sealed record MessageContactsExtraInfo(
    MessageContactData Additional1,
    MessageContactData Additional2,
    MessageContactData Additional3
);

public sealed record MessageContactData(string Name, string Phone);
```

> Enveloped in `ApiResponse<MessagePagedResponse>`
> `ApiMeta.Pagination` populated from API response.

**Sample usage:**

```csharp
await client.Messages.GetPagedAsync(pagedOptionsOrNull, cancellationToken: ct);
```

**Endpoint:**
`GET /messages`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 401       | Unauthorized | `UnauthorizedException`   |
| 500       | Server Error | `InternalServerException` |

---

### Create

**Request:**

```csharp
public sealed record CreateMessageRequest(string From, string Content, CreateMessageReceiverRequest To);

public sealed record CreateMessageReceiverRequest(string To);
```

**Response:**

```csharp
public sealed record MessageResponse(
    string From,
    string To,
    string Content,
    string Id,
    MessageStatus Status,
    DateTime CreatedAt,
    DateTime? DeliveredAt
);
```

> Enveloped in `ApiResponse<MessageResponse>`

**Sample usage:**

```csharp
await client.Messages.CreateAsync(from, to, content, cancellationToken: ct);
```

**Endpoint:**
`POST /messages`

**Errors:**

| HTTP Code | Description  | SDK Exception             |
| :-------- | :----------- | :------------------------ |
| 400       | Bad Request  | `ValidationException`     |
| 401       | Unauthorized | `UnauthorizedException`   |
| 500       | Server Error | `InternalServerException` |

---

## Error Contract

```csharp
public abstract class SinchException : Exception
{
    public int StatusCode { get; }
    public string? ErrorCode { get; }
}

public sealed class NotFoundException : SinchException;
public sealed class ValidationException : SinchException;
public sealed class UnauthorizedException : SinchException;
public sealed class InternalServerException : SinchException;
```

---

## Summary

* Consistent API envelope across all endpoints.
* Canonical, strongly typed request and response records.
* Predictable, mapped exception hierarchy.
* Async, cancellation-friendly SDK surface.
* Clean DX: parameter-based methods for simple ops, typed requests for complex ones.