---
name: ddd
version: "0.2.0"
description: "Domain-Driven Design (Eric Evans) — tackles complex business domains through language, boundaries, and models. Strategic (bounded context, context map, ubiquitous language) + Tactical (entity, value object, aggregate, domain event)."
---

# Domain-Driven Design

> **Background and theory**: Read [references/foundation.md](references/foundation.md)


## One-Line Summary

**The essence of complex business lies in the domain model.** Organize code around *domain language and boundaries*, not technical structure. The core is Strategic; Tactical is merely the means of expression.


## When to Use

- Business logic is complex and frequently changing
- Constant communication with domain experts
- Multiple teams working on the same product (service partitioning needed)
- Long-lived products (5+ years of maintenance)


## Practical Application

### Starting Point for Service Partitioning
1. Identify domain events through an Event Storming workshop (Alberto Brandolini)
2. Event clusters → bounded context candidates
3. Draw the Context Map
4. Select the Core Domain → concentrate investment
5. Service boundaries = bounded contexts (aligned with team structure)

### Aggregate Design Rules
- Keep small (large aggregates = concurrency bottleneck)
- Reference by ID, not object graphs
- Separate if consistency requirements differ
- Eventually consistent between aggregates


## Antipatterns

- **Anemic Domain Model** — Entities with only getters/setters, logic concentrated in Services. Not DDD.
- **"DDD" = just a collection of Tactical patterns** — adopting only Tactical without Strategic leads to failure
- **Core illusion** — everyone thinks their domain is Core
- **Bounded Context abuse** — splitting a simple domain into 10 contexts
- **Absence of Ubiquitous Language** — mixing "고객", "사용자", "회원", "이용자" (customer, user, member, patron)


## Limitations

1. **High cost** — time for domain expert participation, workshops, and modeling
2. **Overkill for simple domains** — Rails-style is faster for CRUD and simple brokerage
3. **Learning difficulty** — Evans's original book is hard, and Vernon is needed for practical connection
4. **Tactical temptation** — teams follow only Aggregates without reading the front portion (Strategic)


## Common Failures in the Korean Context

- **"고객 vs 사용자 vs 회원 vs 이용자"** — legal, marketing, and development each use the 4 terms differently. A representative case of Ubiquitous Language failure.
- **Blurred boundaries between payment, order, and settlement** — only "order calls payment" is visible, with no bounded context separation. A problem even Toss, Coupang, and Naver Pay experienced early on.
- **Boundaries created by regulation** — the Electronic Financial Business Act, MyData, and the Credit Information Act each have different constraints (data retention, access logs, consent management). These are bounded context candidates.
- **"The team lead / part lead read a DDD book"** — introducing only Tactical (Aggregate, Repository) without Strategic discussion → Anemic Model + boilerplate. A common failure pattern in Korean conglomerate transitions.
- **Absence of domain experts** — mistaking planners for domain experts. Without talking to actual operational personnel (CS, settlement, tax accounting), the Ubiquitous Language is an illusion.


## What to Use With This Framework

- `hexagonal` — structure separating domain from I/O
- `event-sourcing-cqrs` — treat domain events as true "events"
- `modular-monolith` — bounded context = module
- `team-topologies` — bounded context = team boundary


## When This Framework Is *Wrong*

- CRUD-centric simple apps → skip full-stack DDD; use a thin layered approach + `solid`
- MVP with unclear domain → full `ddd` comes after PMF
- Tech debt cleanup → `strangler-fig` first
