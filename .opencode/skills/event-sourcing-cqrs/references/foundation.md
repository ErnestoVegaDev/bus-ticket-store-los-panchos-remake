# Event Sourcing + CQRS -- Background & Theory

> This document contains the theoretical foundations, evidence base, and references for this framework.
> For practical application and execution, see [SKILL.md](../SKILL.md).

## Theoretical Origins

- **Greg Young** — formalized CQRS (2010s). ES itself has older roots (accounting ledgers, SCM).
- **Martin Fowler** — articulated Event Sourcing.
- **Udi Dahan** — combined it with SOA and message-based design.
- Roots: double-entry bookkeeping in accounting. Record "transactions," not "balances."


## Two Concepts (Usable Independently)

### Event Sourcing (ES)
- Writes = append events
- State = fold over events
- Event Store = immutable, append-only log
- Pros: complete audit trail, time travel (rebuild to any point), natural fit for domain events
- Cons: schema evolution is complex, querying is hard

### CQRS (Command Query Responsibility Segregation)
- Separate Command (write) model vs. Query (read) model
- Each can use different DBs, schemas, and consistency models
- Pros: read optimization, independent scaling, team separation
- Cons: eventual consistency, synchronization complexity

### Combining Both
- ES is the source of truth
- CQRS read models subscribe to events and stay up to date
- Extremely powerful — extremely complex


## Schema Evolution

- Events are *facts that already happened* → immutable
- To add new fields: versioning (`OrderPlacedV1`, `V2`)
- Upcasting: transform old events into the latest shape when reading
- *Never rename or delete events* — manage via a compatibility layer


## Further Reading

- Young, G. "CQRS Documents" (PDF, free).
- Dahan, U. — blog and NServiceBus materials.
- Vernon, V. *Implementing Domain-Driven Design.* Ch. 8.
- EventStoreDB documentation (eventstore.com).