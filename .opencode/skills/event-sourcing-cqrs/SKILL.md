---
name: event-sourcing-cqrs
version: "0.2.0"
description: "Event Sourcing + CQRS (Greg Young) — store events instead of state; state is a fold of events. Separate write and read models. Powerful for financial ledgers, auditing, time travel, and offloading read workloads."
---

# Event Sourcing + CQRS

> **Background and theory**: Read [references/foundation.md](references/foundation.md)


## One-Line Summary

**State = Fold(Events)**. Don't store current state — store the *events that changed state*. Reads are projected into separate, optimized models (projections).


## When to Use

- **Domains where audit and regulation are mandatory** — financial ledgers, medical records, legal transactions
- **State history itself has value** — "when did what change?"
- **Read load >> write load** — separate read models
- **Rich domain events** — the business naturally speaks in event language
- **Time travel required** — simulating past points in time, what-if analysis

### Triggers in the Korean Financial Context

- **Electronic financial business supervisory reporting** — when reporting electronic financial incidents to the FSS/FSC, transaction flow must be reconstructable → ES is a natural fit
- **PG / card company settlement cycles** — T+2 / T+3 settlement constantly requires reconstructing balances at specific points in time
- **MyData APIs** — statutory requirement for audit trails of personal credit information provision history
- **Virtual Asset Service Providers (VASPs)** — the Specific Financial Information Act mandates transaction record retention and submission
- **Insurance and securities sales logs** — record retention obligations under the Capital Markets Act and the Insurance Business Act
- **Toss, KakaoPay, Naver Pay case studies** — user balance and transaction queries are projections; the ledger is append-only


## Applied in Practice

### Payment Systems like Toss and KakaoPay
```
Command: ChargeRequested
Event: PaymentInitiated
      → PaymentAuthorized (by bank)
      → PaymentCaptured
      → PaymentSettled

Read models (each projected independently):
  - user-transaction-history (personal view)
  - merchant-daily-settlement (merchant settlement)
  - compliance-audit-log (regulatory reporting)
  - fraud-ml-features (ML pipeline)
```

### Choosing an Event Store
- Purpose-built: EventStoreDB, Marten
- General messaging: Kafka + KSQL (limited for strict ES)
- On top of an RDB: the outbox pattern

### Creating Projections (Read Models)
- Subscribe to event streams
- Maintain an optimized schema
- *Replayable* — if the schema changes, rebuild from scratch
- Eventually consistent (lag of a few ms to seconds)


## Concurrency and Consistency Models

- **Optimistic concurrency** — append per Aggregate with an expected version. On conflict, retry or reject the command. Default in EventStoreDB and Marten.
- **Stream per Aggregate** — events for a single Aggregate live in a single stream (e.g., `order-12345`). Order within a stream is guaranteed.
- **Causal consistency** (within a stream) vs. **Eventual consistency** (across streams and projections)
- **Exactly-once processing** — especially hard in Kafka-based ES. Requires idempotent consumer + dedup key design.
- **Snapshot policy** — only when Aggregate reconstruction cost exceeds query latency tolerance. Typically every N events (N = 50–1000) or time-based.
- **Projection rebuild** — full event replay on schema change. At scale, can take hours to days. Requires parallel projectors + partitioning.
- **From a CAP perspective** — ES leans AP (writes are available, read projections are eventually consistent).


## Anti-Patterns

- **CQRS without Event Sourcing** — common. Simple read-model separation is fine as "CQRS-lite."
- **"Event-sourcing everything"** — overkill for most CRUD.
- **Events named like CRUD operations**: meaningless events like `UserUpdated` → not domain occurrences, just DB change logs.
- **Aggregates that are too large** — replay latency on the event stream explodes.
- **Query models that also write** — violates CQRS, leads to consistency hell.
- **Long histories without snapshots** — Aggregate reconstruction cost balloons.


## Limitations

1. **Steep learning curve** — the entire team's mental model must shift
2. **Operational complexity** — monitoring projection lag, replay for disaster recovery
3. **Politics of schema evolution** — event contracts become cross-team boundaries
4. **Eventually consistent UX** — handling users who say "I just paid but my balance hasn't changed"
5. **Overkill for small teams and simple domains**


## What to Use Alongside This Framework

- `ddd` — domain events are ES events. They pair naturally.
- `resilience-patterns` — managing the side effects of async and eventual consistency
- `observability` — monitoring projection lag and event backlog


## When This Framework Is *Wrong*

- Simple CRUD → over-application
- Strict immediate consistency required (e.g., medical dosing records) → ES is possible, but requires synchronous projections
- Low-load, early-stage MVP → ES's audit value < complexity cost
