# Modular Monolith -- Background & Theory

> This document contains the theoretical foundations, evidence base, and references for this framework.
> For practical application and execution, see [SKILL.md](../SKILL.md).

## Theoretical Origins

- **DHH / Basecamp** — "The Majestic Monolith" (Signal v. Noise blog, 2016). The origin of the term "Majestic Monolith."
- **Shopify (Kirsten Westeinde)** — "Deconstructing the Monolith" (shopify.engineering, 2019). A case study of transitioning a Rails monolith into a modular mono.
- **Simon Brown** — Popularized the term "Modular Monolith."
- **Sam Newman** — *Monolith to Microservices* (O'Reilly, 2019). A strategy for incremental separation.


## Core Principles

1. **One process, one deployment** — Preserve the simplicity of a monolith
2. **Enforce module boundaries** — Expose only externally facing APIs, hide internal implementation
3. **Shared DB allowed, shared tables forbidden** — Clear per-module schema/ownership
4. **Inter-module communication = explicit interfaces** — Function calls, but contracts at the service level
5. **Splittable later** — Boundaries such that each module can later be extracted into a separate service


## Why Before Microservices

| Item | Mono | Modular Mono | Microservices |
|---|---|---|---|
| Deployment complexity | Low | Low | High |
| Transactions | Simple | Simple | Distributed (hard) |
| Debugging | Local stack | Local stack | Distributed tracing required |
| Boundary discipline | Loose | Enforceable | Enforced |
| Team independence | Low | Medium | High |
| Operational cost | Low | Low | High |
| Independent scaling | Not possible | Not possible | Possible |

*Cost-benefit for early and mid-stage startups*: Modular Monolith wins most of the time.


## Further Reading

- Shopify Engineering. "Deconstructing the Monolith" (blog).
- Newman, S. *Monolith to Microservices.* (2019)
- Brown, S. *Software Architecture for Developers.* — modular monolith section.
- Packwerk (github.com/Shopify/packwerk) — Ruby module boundary inspection tool.