# API Design -- Background & Theory

> This document contains the theoretical foundations, evidence base, and references for this framework.
> For practical application and execution, see [SKILL.md](../SKILL.md).

## Theoretical Origin

- **REST (Representational State Transfer)** — Roy Fielding's PhD dissertation, UC Irvine, 2000. Defined as an architectural style for distributed hypermedia systems.
- **Richardson Maturity Model** (Leonard Richardson, 2008) — 4 levels of REST "maturity" (0: plain HTTP, 1: Resources, 2: HTTP Verbs, 3: Hypermedia).
- *RESTful Web APIs* (Richardson & Ruby, 2013) — practical REST guide.
- *API Design Patterns* (JJ Geewax, Google, 2021) — patterns from building APIs at scale.
- **OpenAPI Specification** (formerly Swagger) — the industry standard for documenting REST APIs, maintained by the OpenAPI Initiative.

---


## Core Concepts

### Resources, Not Actions

REST is about *things* (resources), not *verbs* (actions). The HTTP method is the verb; the URL is the noun.

**Wrong** (action-based):
```
POST /getUser
POST /createOrder
POST /deleteProduct
```

**Right** (resource-based):
```
GET    /users/123
POST   /orders
DELETE /products/456
```

---

### HTTP Methods and What They Mean

| Method | Meaning | Idempotent? | Safe? |
|---|---|---|---|
| **GET** | Fetch a resource | Yes | Yes |
| **POST** | Create a new resource | No | No |
| **PUT** | Replace a resource entirely | Yes | No |
| **PATCH** | Update part of a resource | No | No |
| **DELETE** | Remove a resource | Yes | No |

**Idempotent** = calling it multiple times has the same result as calling it once.
**Safe** = doesn't change server state.

Why this matters: browsers, proxies, and caches treat GET differently from POST. Misusing methods breaks these expectations.

---

### URL Design Patterns

**Collections and items**:
```
GET    /users           → list all users
POST   /users           → create a user
GET    /users/123       → get user 123
PUT    /users/123       → replace user 123
PATCH  /users/123       → update user 123 partially
DELETE /users/123       → delete user 123
```

**Nested resources** (when the relationship is strong):
```
GET /users/123/orders        → orders belonging to user 123
GET /users/123/orders/456    → specific order of user 123
```

**Flat relationships** (when the nested resource exists independently):
```
# Prefer flat if orders can exist without a specific user context
GET /orders?userId=123       → orders filtered by user
```

**Rule of thumb**: Nest at most 2 levels deep. `/a/:id/b/:id` is acceptable. `/a/:id/b/:id/c/:id/d/:id` is a smell.

### Health & Status Endpoints

| Endpoint | Purpose |
|---|---|
| `GET /health` | Basic liveness check (returns 200 if server is running) |
| `GET /health/ready` | Readiness check (returns 200 if all dependencies are connected) |
| `GET /version` | Returns app version, git commit hash (useful for debugging deployments) |

These are not resources but operational endpoints. Every API should have at least `GET /health`.

**Naming conventions**:
```
# Use plural nouns for collections
/users, /orders, /products    ✓
/user, /order, /product       ✗ (singular is inconsistent)

# Use kebab-case for multi-word resources
/user-profiles                ✓
/userProfiles, /user_profiles ✗ (both work, but pick one and be consistent)

# No verbs in URLs (the HTTP method is the verb)
GET /users/123/activate       ✗ (action as URL)
POST /users/123/activations   ✓ (activation is a resource)
POST /users/123?action=activate ✗ (also wrong)
```

---


## Error Response Structure

Consistent error responses are as important as consistent success responses. Pick a format and stick to it everywhere.

**Recommended structure**:
```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Request validation failed",
    "details": [
      {
        "field": "email",
        "message": "Must be a valid email address"
      },
      {
        "field": "age",
        "message": "Must be a positive integer"
      }
    ],
    "request_id": "req_abc123"
  }
}
```

**Key fields**:
- `code` — machine-readable string (for programmatic handling)
- `message` — human-readable string (for developers debugging)
- `details` — array for validation errors with per-field context
- `request_id` — correlates with server logs (invaluable for support)

**What not to do**:
```json
// Too vague — what went wrong?
{ "error": "Error" }

// Stack trace exposed to client
{ "error": "NullPointerException at line 234 in UserService.java" }

// Inconsistent — sometimes it's "error", sometimes "message", sometimes "err"
{ "message": "not found" }  // on one endpoint
{ "error": "User not found" }  // on another
```

---


## Further Reading

- Richardson, L. & Ruby, S. *RESTful Web APIs.* (O'Reilly, 2013)
- Geewax, JJ. *API Design Patterns.* (Manning, 2021) — patterns from Google's API design
- **Google API Design Guide**: cloud.google.com/apis/design (free, opinionated, excellent)
- **OpenAPI Specification**: spec.openapis.org
- Fielding, R. T. *Architectural Styles and the Design of Network-based Software Architectures.* (2000) — original REST dissertation (free online)