# Domain-Driven Design -- Background & Theory

> This document contains the theoretical foundations, evidence base, and references for this framework.
> For practical application and execution, see [SKILL.md](../SKILL.md).

## Theoretical Origins

- **Eric Evans** — *Domain-Driven Design: Tackling Complexity in the Heart of Software* (Addison-Wesley, 2003). The original source and origin of the "Strategic vs Tactical" distinction.
- **Vaughn Vernon** — *Implementing Domain-Driven Design* (Addison-Wesley, 2013). Translates Evans's book to practical code-level implementation.
- **Alberto Brandolini** — *Introducing EventStorming* (Leanpub, 2015~, unfinished, continuously updated). A domain-event-based modeling workshop technique.
- **Vlad Khononov** — *Learning Domain-Driven Design* (O'Reilly, 2021). The current standard introduction.
- Merged with the 2014–2018 microservices movement (Newman, Fowler) — bounded context formalized as service boundary.
- Roots: Booch (1994), Fowler's *Analysis Patterns* (1996) domain modeling lineage.


## Strategic — The Most Important

### 1. Ubiquitous Language
- Domain experts and developers use the *same terminology*
- Identical in code, documents, and conversation
- Needing translation = failure of domain understanding

### 2. Bounded Context
- The scope within which a model is *consistently* valid
- "Customer" may differ between the *order* context and *support* context
- The theoretical basis for setting microservice boundaries

### 3. Context Map
- Types of relationships between bounded contexts:
  - **Partnership** — bidirectional coordination
  - **Customer-Supplier** — one side determines requirements
  - **Conformist** — the dependent side adapts
  - **Anti-Corruption Layer (ACL)** — a translation protection layer for external models
  - **Shared Kernel** — a small amount of shared code
  - **Open Host Service + Published Language** — an externally published protocol

### 4. Subdomain Classification
- **Core Domain** — the competitive advantage area. Top talent and dedication.
- **Supporting** — necessary but not differentiating.
- **Generic** — a solved problem. Replace with packages or SaaS.

Investment priority: Core > Supporting > Generic. *Do not waste internal development on Generic*.


## Tactical

### Entity
- Identified by a unique identifier (ID). The same object even when attributes change.

### Value Object
- Judged by attributes only. Immutable. Compared by value.
- Examples: Money, Address, DateRange.

### Aggregate
- A consistency unit. The Aggregate Root controls internal access.
- Transaction boundary = Aggregate boundary.
- **Principle: modify one Aggregate per transaction** (Vernon 2013, Ch.10)
- **Keep small** — large Aggregates cause surges in optimistic lock conflicts. Vernon's four rules of thumb:
  1. Keep only true invariants within the boundary
  2. Prefer small Aggregates
  3. Reference other Aggregates *by ID*
  4. Consistency between Aggregates is *eventually consistent* (via domain events)

### Repository
- Load and save only by Aggregate unit.
- A domain-language interface. The Adapter is a secondary port.

### Domain Event
- Something that *has already happened* in the domain.
- An asynchronous medium of communication between bounded contexts.

### Domain Service
- Domain logic that does not belong to an Entity or Value Object.
- Examples: PaymentRouter, PricingCalculator.


## Further Learning

- Evans, E. *Domain-Driven Design.* (classic, difficult)
- Vernon, V. *Implementing DDD.* (practical connection)
- Brandolini, A. *Introducing EventStorming.*
- Contemporary: Khononov, V. *Learning Domain-Driven Design.* (2021, current standard introduction)