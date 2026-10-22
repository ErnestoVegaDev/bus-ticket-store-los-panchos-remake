---
name: api-design
version: "0.2.0"
description: "API Design — REST principles, HTTP methods, status codes, error handling, versioning, pagination, rate limiting. Clean API contracts that other developers (and your future self) can actually understand."
---

# API Design

> **Background and theory**: Read [references/foundation.md](references/foundation.md)


## One-Line Summary

**APIs are contracts. A well-designed API is predictable, consistent, and self-explanatory — a bad one is a source of constant confusion for everyone, including you six months later.**


## HTTP Status Codes

Use status codes accurately. Returning `200 OK` with `{ "success": false }` in the body is a lie — use the right status code.

### Success (2xx)
| Code | Name | When to use |
|---|---|---|
| **200 OK** | Everything worked | GET, PUT, PATCH success |
| **201 Created** | Resource created | POST success (return the created resource + Location header) |
| **204 No Content** | Success, nothing to return | DELETE success |

### Client Errors (4xx — it's the caller's fault)
| Code | Name | When to use |
|---|---|---|
| **400 Bad Request** | Invalid input | Missing required field, wrong data type, validation failure |
| **401 Unauthorized** | Not authenticated | No token, expired token (despite the name, this is about authentication) |
| **403 Forbidden** | Not authorized | Authenticated but lacks permission for this resource |
| **404 Not Found** | Resource doesn't exist | Wrong ID, or hiding existence for security reasons |
| **409 Conflict** | State conflict | Creating a duplicate, version mismatch (optimistic locking) |
| **422 Unprocessable Entity** | Semantic validation failure | Request is well-formed but business rules fail |
| **429 Too Many Requests** | Rate limited | Include `Retry-After` header |

### Server Errors (5xx — it's the server's fault)
| Code | Name | When to use |
|---|---|---|
| **500 Internal Server Error** | Unexpected server crash | Unhandled exception, bug in the code |
| **502 Bad Gateway** | Upstream service down | Your server can't reach the DB or external service |
| **503 Service Unavailable** | Intentional unavailability | Maintenance mode, overloaded |

**Common mistake**: Using 500 for all errors. Discipline yourself to return 400-level for client errors — it helps consumers of your API debug much faster.

---


## Pagination

Never return unbounded lists. An endpoint that returns all orders eventually times out or OOMs.

### Offset Pagination (simpler, but has issues)
```
GET /orders?page=3&limit=20

Response:
{
  "data": [...],
  "pagination": {
    "page": 3,
    "limit": 20,
    "total": 847,
    "total_pages": 43
  }
}
```

**Problem**: If items are inserted between page 1 and page 2 being fetched, some items are skipped or duplicated.

### Cursor Pagination (more robust)
```
GET /orders?cursor=eyJpZCI6MTAwfQ==&limit=20

Response:
{
  "data": [...],
  "pagination": {
    "next_cursor": "eyJpZCI6MTIwfQ==",
    "has_more": true
  }
}
```

The cursor encodes the position in the result set (often a Base64-encoded ID or timestamp). Insertions don't cause skips.

**When to use which**:
- Offset: admin tables, reporting, when "jump to page 42" is needed
- Cursor: infinite scroll, real-time feeds, large datasets, sorted by timestamp

---


## Versioning

APIs change. Without versioning, you can't change your API without breaking existing clients.

### URL Path Versioning (most common)
```
GET /v1/users
GET /v2/users
```

**Pros**: Obvious, simple, easy to route, easy to document separately.
**Cons**: URL "should" represent a resource, not a version (philosophical).

### Header Versioning
```
GET /users
Accept: application/vnd.myapi.v2+json
```

**Pros**: Cleaner URLs.
**Cons**: Hidden — hard to test in browser, easy to forget.

**Recommendation**: Use URL versioning (`/v1/`) unless you have a specific reason not to. It's the most discoverable and widely understood approach.

### Versioning policy
- Increment major version (`v1` → `v2`) for breaking changes
- Breaking changes: removing fields, changing field types, changing behavior
- Non-breaking additions (new optional fields, new endpoints) don't require a new version
- Maintain previous version for at least 6–12 months with deprecation notice
- Add `Deprecation: true` and `Sunset: <date>` headers to deprecated endpoints

---


## Rate Limiting

Protect your API from abuse and overload. Include headers so clients can self-regulate.

```
# Response headers
X-RateLimit-Limit: 100        # requests allowed per window
X-RateLimit-Remaining: 47     # requests left in current window
X-RateLimit-Reset: 1714300800 # Unix timestamp when window resets

# When limit exceeded
HTTP 429 Too Many Requests
Retry-After: 30               # seconds until retry is safe
```

**Common limits by tier**:
- Free/unauthenticated: 60 req/hour
- Authenticated users: 1,000 req/hour
- Premium/API key: 10,000 req/hour

---


## API Documentation with OpenAPI

OpenAPI (Swagger) is the standard. Tools generate docs, client SDKs, and test scaffolding from it.

```yaml
openapi: 3.0.3
info:
  title: My App API
  version: 1.0.0

paths:
  /users/{id}:
    get:
      summary: Get a user by ID
      parameters:
        - name: id
          in: path
          required: true
          schema:
            type: integer
      responses:
        '200':
          description: User found
          content:
            application/json:
              schema:
                $ref: '#/components/schemas/User'
        '404':
          description: User not found
```

**Tooling**:
- **Swagger UI** — interactive browser-based API explorer
- **Redoc** — clean static documentation
- **Postman** — import OpenAPI to generate a collection
- **FastAPI / NestJS** — generate OpenAPI automatically from code annotations

---


## GraphQL vs REST — When to Choose

| Dimension | REST | GraphQL |
|---|---|---|
| Learning curve | Low | Medium-high |
| Over-fetching | Common | Eliminated (client specifies fields) |
| Under-fetching / N+1 | Common (multiple requests) | Handled in one query |
| Caching | Easy (HTTP caching) | Harder (all POST to /graphql) |
| Tooling maturity | Excellent | Good, improving |
| Type safety | Via OpenAPI/TypeScript | Built-in |
| Best for | Public APIs, simple CRUD, mobile with predictable data needs | Complex queries, many client types, dashboard-heavy apps |

**Rule of thumb**:
- < 5 client consumers with predictable data needs → REST
- Multiple client types (web, mobile, third-party) with divergent data needs → GraphQL
- AI-generated CRUD app → REST (simpler, more AI training data)

---


## Vibe Coder Consistency Checklist

AI-generated APIs frequently have consistency problems. Check for:

- [ ] All endpoints return the same JSON structure for errors (same `error` field shape)
- [ ] HTTP methods match semantics (GET for fetches, POST for creates, etc.)
- [ ] Status codes are accurate (not returning 200 for errors)
- [ ] Collection endpoints have pagination (not returning 10,000 records)
- [ ] All timestamps use the same format (ISO 8601: `2026-04-16T09:30:00Z`)
- [ ] IDs are the same type everywhere (all integers, or all strings — not mixed)
- [ ] URL naming is consistent (all plural, all kebab-case, or all snake_case — choose one)
- [ ] Authentication scheme is consistent (Bearer token on all endpoints, not just some)
- [ ] API versioned before going public (changing `/users` to `/v1/users` after clients exist is painful)

---


## Anti-Patterns

- **Chatty APIs** — 10 requests to build one page. Design endpoints around use cases, not data models.
- **God endpoints** — `POST /do-stuff` that does different things based on a `type` parameter. Split into real resources.
- **Exposing your database schema** — if your API is 1:1 with your DB tables, changes to your DB break your clients. Add a translation layer.
- **Boolean parameters everywhere** — `?format=true&expand=true&verbose=true` is unclear. Use explicit resource paths or fields parameters.
- **Ignoring backwards compatibility** — removing a field breaks every client using it. Add fields freely; remove carefully and with versioning.
- **Inconsistent auth** — some endpoints need auth, some don't, with no clear pattern. Document and make it predictable.


## Over-Application Warning

API design principles exist for long-lived APIs with multiple consumers. Calibrate accordingly:

- **Personal project**: RESTful URLs and correct status codes are enough. Don't write OpenAPI docs for an app only you use.
- **Early MVP**: Consistency > perfection. Pick a convention and stick to it. Don't redesign before you know what your clients actually need.
- **Public API**: Versioning, rate limiting, and full OpenAPI docs are non-negotiable before launch.
- **Internal microservices**: gRPC or tRPC may be more appropriate than REST — especially if all consumers are in the same language.

---


## Limitations

1. **REST is architectural style, not a standard** — implementations vary widely, leading to "REST-ish" APIs that everyone calls REST
2. **HTTP caching is powerful but ignored** — most REST APIs don't leverage ETags or Cache-Control effectively
3. **Hypermedia (HATEOAS) rarely adopted** — Roy Fielding's Level 3 REST (links in responses) is rarely practical in real-world APIs
4. **File uploads are awkward** — REST + JSON doesn't handle multipart/form-data elegantly; often requires a separate pattern
5. **Real-time data** — REST is request/response. For real-time, add WebSockets or SSE alongside REST (not instead of it)


## Related Frameworks

- `hexagonal` — API layer is an "inbound port". Hexagonal helps keep domain logic out of controllers.
- `twelve-factor` — Factor VII (Port Binding) and Factor III (Config for API keys/tokens)
- `owasp-security` — A01 (Broken Access Control) and A03 (Injection) are the most common API vulnerabilities
