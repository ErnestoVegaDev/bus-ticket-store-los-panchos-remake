---
name: code-router-prompt
version: "1.0.0"
description: "LLM-as-Router prompt template for selecting the optimal software engineering framework given a situation. Replaces keyword-matching Detection Matrix."
type: router_prompt
target_repo: software-framework
---

# Code Router — LLM Selection Prompt

This file is consumed by `pag_pipeline.py::route(repo="software", question, router_llm)`.
It lists every framework in this repo with a one-line "when to use" description and a concrete example scenario. The router LLM is asked to return **exactly one framework name**.

---

## System Prompt

```
You are a software architecture expert acting as a framework selector.
Given a situation, pick the SINGLE framework that best fits.

Output ONLY the framework name (lowercase, exactly as listed below). No explanation, no punctuation, no quotes.

If the situation is ambiguous, prefer the framework whose Example most closely matches the scenario's CORE problem, not just surface keywords.
```

---

## Framework Catalog

Each entry: `name` — **when to use** (one line) / **example** (one concrete scenario).

### Debugging & Operations (live problems, incidents, regressions)

**scientific-debugging** — Bug is mysterious or intermittent; need hypothesis-driven Observation→Hypothesis→Prediction→Experiment loop.
Example: "Checkout randomly fails for ~2% of users and we can't reproduce it. How do we structure the investigation instead of guessing?"

**bisection** — Behavior regressed between two known points in time; need to isolate the offending commit/config in log N steps.
Example: "Deploy on Tuesday was fine, Friday's is broken. 300 commits between them — how do we find the one that caused it?"

**observability** — Live system is slow or opaque and we lack signals to explain internal state (USE/RED/Four Golden Signals).
Example: "p99 latency tripled last week, no errors in logs. We don't have enough metrics/traces to know where time is spent."

**resilience-patterns** — External dependency or downstream failure cascades through our system; need circuit breaker, bulkhead, retry-with-jitter, timeout.
Example: "When the payment gateway slows down, our entire checkout thread pool gets starved and every other request dies. How do we contain the blast radius?"

### Architecture & Design (structuring systems)

**ddd** — Domain is complex or business experts and developers talk past each other; need ubiquitous language, bounded contexts, strategic design.
Example: "Our insurance-claims code has 15 years of accreted logic nobody fully understands, and product owners keep saying we 'don't get' the domain."

**hexagonal** — Business logic is entangled with I/O (DB, HTTP, queues), making testing painful and frameworks hard to swap.
Example: "Every unit test spins up Postgres and Kafka because our domain objects directly import ORM and clients. Can't test logic in isolation."

**event-sourcing-cqrs** — Domain requires audit trail, time-travel, replay, or wildly different read vs write shapes (ledgers, settlements).
Example: "Regulators want to reconstruct any account's state at any past timestamp, and our current CRUD tables overwrite history."

**modular-monolith** — Team is debating microservices vs monolith, or a naive split has caused distributed-transaction pain; want enforced module boundaries in one deploy.
Example: "We split into 12 microservices last year and now every feature needs coordination across 4 of them. Should we collapse back but keep boundaries?"

**solid** — Class/function-level design smells (God objects, switch explosions, fragile inheritance); need principled OO refactoring without redesigning the domain.
Example: "This 3,000-line OrderService class is terrifying to touch and has a 40-case switch on order type. How do we refactor it class by class?"

**api-design** — Designing or critiquing an HTTP/REST API surface — resource modeling, status codes, versioning, pagination, errors.
Example: "We're exposing a public v1 API next month. Review the endpoint naming, status code usage, and how we handle breaking changes."

### Operations Hygiene & Security

**twelve-factor** — App is hard to deploy across environments, config is hardcoded, or preparing for container/K8s migration.
Example: "Dev, staging, and prod each have a different config file baked into the image. Getting ready to move to ECS — what do we fix first?"

**cicd-basics** — No automated pipeline, or current deploy is manual/fragile; need build-test-deploy hygiene.
Example: "We deploy by SSHing in and running a shell script on Friday afternoons. Set up GitHub Actions to build, test, and ship automatically."

**owasp-security** — Code or API may be exposed to common web vulnerabilities (injection, XSS, broken auth, exposed secrets).
Example: "We're about to launch a public signup flow that takes passwords and stores files. Audit against OWASP Top 10 before go-live."

### Evolution & Organization (legacy, teams, change)

**strangler-fig** — Legacy system must be replaced but big-bang rewrite is too risky; need incremental route-by-route migration.
Example: "15-year-old PHP monolith still runs core billing. We can't rewrite it in one shot — how do we route traffic off it over 12 months?"

**team-topologies** — Team-level problem: growing headcount with dropping throughput, silos, bottlenecks, or unclear team boundaries.
Example: "We went from 15 to 80 engineers in a year. Now every cross-team change takes 3 weeks. Do we need a platform team? How should teams be shaped?"

---

## User Prompt Template

```
## Situation
{scenario}

## Task
From the catalog above, output the SINGLE framework name that best fits.

Answer (one word, lowercase):
```

---

## Routing Notes (for maintainers, not shown to LLM)

- **`scientific-debugging` vs `bisection`**: bisection needs a known good→bad delta (version, commit, config); scientific-debugging is for mysteries without a clear regression range.
- **`hexagonal` vs `solid` vs `ddd`**: SOLID is class-level; Hexagonal is I/O-boundary-level; DDD is domain/context-level. Route to the smallest scope that explains the pain. SOLID should NOT win when the real problem is boundary or deployment scope.
- **`modular-monolith` vs `team-topologies`**: Modular Monolith when the dispute is about code/deployment structure; Team Topologies when the dispute is about who owns what team-wise. Conway's Law means both often appear together, but route by the *stated* complaint.
- **`event-sourcing-cqrs` bar is high** — only route here when audit/replay/time-travel or strongly divergent read/write shapes are core. For generic "complex domain", prefer `ddd`.
- **`resilience-patterns` must pair with `observability`** — if the user asks only for circuit breakers with no metrics, the selected skill's SKILL.md will escalate. Router still picks one.
- **`owasp-security` vs `twelve-factor`**: Twelve-Factor covers config-in-env and secret-management hygiene; OWASP covers injection/XSS/auth vulnerabilities. Route by whether the concern is operational hygiene or attacker-facing surface.
- **Exclusive routing**: If the situation fits multiple frameworks, the router picks ONE. Pipeline combination is handled in Layer 3 (the selected framework's SKILL.md may invoke others).
- **Not included here**: `code` itself (this IS code).

---

## Maintenance Protocol

Adding a new framework requires:
1. Add an entry to Framework Catalog above (name + when + example).
2. Choose an example that is **unambiguous** — if it could be confused with another listed framework, rewrite.
3. Run the evaluation set in `scripts/experiment/` to check no regression on existing scenarios.
