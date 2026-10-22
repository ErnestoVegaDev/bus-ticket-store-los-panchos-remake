# AGENTS.md

## What this is

Monorepo with **two deployables**:

- **Vue 3 SPA** at the repository root — the customer-facing site where passengers buy
  bus tickets.
- **ASP.NET Core 10 API** under `backend/` — the domain backend.

The backend is **Clean Architecture, basic and non-hexagonal**: four .NET projects, a
**domain-focused** core, and **CQRS**. The HTTP surface is **Minimal APIs** — there are no
MVC controllers.

The frontend is **plain JavaScript** (no TypeScript, no JSX) with **Pinia** for shared
state and **Prettier** as the only formatter.

Current state: the frontend is the `create-vue` baseline (counter store, placeholder
views) and the backend is a single ASP.NET Core project created from the Visual Studio
template. The four-project split described under *Architecture* is the target shape, not
the current one. Persistence, authentication and deployment are **not defined yet** — do
not assume an ORM, a database or an auth scheme that this document does not name.

## Domain

**Autobuses Los Panchos** sells electronic tickets for **route 644**, the
**Puerto Jiménez – La Palma – Puerto Escondido** corridor, which runs in both
directions.

| Fare tier | Amount |
| --- | --- |
| Standard passenger | ₡825.00 (825.00 CRC) |
| Gold Citizen (`Ciudadanos de Oro`) | ₡415.00 (415.00 CRC) |

Rules that follow from the fare table:

- **Fares are domain data, not configuration.** The two amounts live in the Domain
  project. They do not belong in `appsettings.json`, in an environment variable, or in a
  frontend constant.
- **A passenger's tier decides the fare.** Standard unless the passenger qualifies as a
  Gold Citizen; the discount is per passenger, not per order.
- **Route 644 has one fare table in both directions.** A trip is the same product
  whether it runs Puerto Jiménez → Puerto Escondido or the reverse.
- **Amounts are `decimal`.** Never `double`, never `float`. The currency is CRC.

## Behavioral guidelines

Hard rules — apply them on every task, no exceptions.

- **Ask before any file-modifying terminal command.** Never run a command that creates,
  edits, moves, or deletes files without asking the user for permission first.
- **Ask before querying data outside the repository.** Any command that reads or inspects
  data outside this repository's path requires explicit permission before it runs.
- **Never modify files outside the repository.** Creating, editing, or deleting anything
  outside this repository is strictly forbidden under any circumstance — this rule has no
  exception.

## Architecture

### Four projects, one dependency direction

| Project | May reference | Owns |
| --- | --- | --- |
| `BusTicketStore.Domain` | **nothing** | Entities, value objects, domain services, invariants, domain events, fare rules |
| `BusTicketStore.Application` | Domain | CQRS commands and queries, handlers, DTOs, port interfaces (e.g. `ITripRepository`) |
| `BusTicketStore.Infrastructure` | Application, Domain | Persistence, external adapters, `MediatR` registration, `AddInfrastructure` |
| `BusTicketStore.Api` | Application, Infrastructure | Minimal API endpoints, middleware, `Program.cs` composition root, OpenAPI |

The **compiler enforces the project reference graph**: a `using` that points inward-out
does not compile. If a change seems to require a backward reference, the design is
wrong, not the rule — move the concept instead of widening the reference.

**Package-level discipline inside `Domain` is a code-review rule, not a compiler rule.**
`Domain` must carry **zero package references**: no EF Core, no ASP.NET Core, no
MediatR. A `Domain` type that needs `IRequest` does not belong in `Domain`.

### CQRS

- A **command** changes state. A **query** reads it. They live in separate namespaces
  and never mix in one handler.
- Commands and queries are `record` types implementing `IRequest<T>` / `IRequest`.
  Handlers implement `IRequestHandler<TRequest, TResponse>`.
- **Handlers are thin.** A handler orchestrates — load the aggregate, ask the domain,
  persist, publish — it does not hold the business rules. The rules live in `Domain`.
- **Queries return DTOs, never domain entities.** A read model is not an aggregate, and
  the HTTP contract must not leak one.
- **Commands dispatch through `ISender` / `IMediator`.** Endpoints never call a handler
  class directly.
- Domain events, when they exist, are raised by the domain and dispatched after the
  unit of work completes — never from inside the handler's own transaction.

### Minimal APIs

- Endpoints are route groups, e.g. `app.MapGroup("/api/tickets")`, with one extension
  method per area.
- **No controllers, no `AddControllers()`, no `[ApiController]`.** The current
  `Program.cs` still registers MVC from the Visual Studio template; the API surface is
  Minimal API.
- **`Program.cs` is the composition root.** It wires DI (`AddMediatR`,
  `AddApplication`, `AddInfrastructure`) and contains no business logic.
- Endpoints validate input and translate to a command or query. They do not contain
  domain logic either.

## Layout

Target shape:

```text
AGENTS.md
README.md
package.json                        Vue app manifest (npm)
package-lock.json                  Resolved dependency tree (committed for reproducibility)
vite.config.js                      Vite config, '@' alias -> ./src
jsconfig.json                       editor path mapping for '@/*'
index.html                          Vite entry document
.prettierrc.json                    Prettier settings (the formatter contract)
public/                             static assets (favicon)
src/                                Vue app
  main.js                             bootstrap: createApp + Pinia + router
  App.vue                             root component
  assets/                             css + logo
  components/                         reusable components
  router/index.js                     routes (vue-router 5)
  stores/                             Pinia stores
  views/                              route-level components
backend/
  backend.slnx                        solution (new XML solution format)
  BusTicketStore/                    current single project; becomes Api/ after the split
    Program.cs                          composition root
    Controllers/                        Visual Studio template leftovers
    WeatherForecast.cs                   Visual Studio template leftovers
  src/                                target: the four layer projects
    BusTicketStore.Domain/
    BusTicketStore.Application/
    BusTicketStore.Infrastructure/
    BusTicketStore.Api/
  tests/                              target: one test project per testable layer
.opencode/skills/                   project skills, tracked in the repository
odd/tasks/<feature-name>.md         feature documents
```

## Critical gotchas

- **The compiler enforces project references, not package purity.** The four-project
  split guarantees the *direction* of dependencies. It does **not** stop
  `Microsoft.EntityFrameworkCore` from being added to `Domain` — that compiles fine and
  breaks the architecture silently. Review it.
- **MediatR changed license at v13.** v12.4.1 is the last Apache-2.0 line and needs no
  license key. v13+ (current 14.2.0) is **RPL-1.5** by default, with a commercial License
  Agreement as the alternative when you do not want the reciprocal obligation. v13+ reads
  a key from `cfg.LicenseKey` or `MEDIATR_LICENSE_KEY` and logs a license warning when it
  is absent. Pin the version deliberately and record which regime applies — do not let
  `dotnet add package MediatR` silently pick the latest.
- **Minimal API means no controllers.** `Program.cs` currently calls `AddControllers()`
  and `MapControllers()` from the Visual Studio template. Do not add a controller
  "just for one endpoint".
- **Prices are domain data.** 825.00 and 415.00 CRC live in `Domain`, not in settings.
  `decimal`, never `double`.
- **The SPA and the API are different origins.** Vite serves on `http://localhost:5173`;
  the API serves on `http://localhost:5098` / `https://localhost:7005` (from
  `launchSettings.json`). **No CORS policy is configured**, so browser calls from the
  SPA to the API are blocked until one is added — expect this the first time you wire
  the two halves together.
- **The `package-lock.json` is committed.** It is the resolved dependency tree, so
  `npm ci` reproduces the exact tree the build was verified against. Prefer `npm ci` on a
  fresh clone and in CI; reach for `npm install` only when intentionally changing
  dependencies.
- **Node is below the declared floor.** `package.json` declares
  `engines.node: ^22.18.0 || >=24.12.0`; the installed Node is v24.8.0, so `npm install`
  prints `EBADENGINE` warnings. They are non-fatal — build and dev server both work. Do
  not "fix" it by editing `engines` unless you intend to raise the floor on purpose.
- **Plain JavaScript only.** No TypeScript, no JSX. `jsconfig.json` gives editors the
  `@/*` alias; Vite resolves `@` through `vite.config.js`. Pinia is mandatory for shared
  state; state that only one component uses stays local to that component.
- **Prettier's settings are not the defaults.** `.prettierrc.json` is `semi: false`,
  `singleQuote: true`, `printWidth: 100`, and `formatOnSave` is on in
  `.vscode/settings.json`. Run `npm run format`; do not hand-format around it.
- **No test, lint, or typecheck tooling is configured** on either side — no ESLint, no
  Vitest, no xUnit, no Roslyn analyzer. Do not assume a runner exists; if you add one,
  document the exact command here.
- **`backend/backend.slnx` is the solution.** `.slnx` is the new XML solution format
  (.NET 10). A project that is not listed there is not built by `dotnet build` on the
  solution.
- **Only `appsettings.*.local.json` is ignored.** `appsettings.json` and
  `appsettings.Development.json` are tracked by ASP.NET Core convention. Secrets go in
  User Secrets or environment variables, never in tracked JSON.
- **The shell is Windows PowerShell 5.1.** `&&` is a parse error — separate commands with
  `;`. Set a one-off variable with `$env:NAME='value'`.

## Environment

- **Node** v24.8.0, **npm** 11.8.0. See the `engines.node` gotcha above.
- **.NET SDK** 10.0.402 (10.0.301 also installed). Target framework **`net10.0`**,
  ASP.NET Core runtime 10.0.13. `backend` currently builds with 0 warnings, 0 errors.
- **OS**: Windows. Shell is Windows PowerShell 5.1.
- Frontend packages installed: `vue` 3.5.44, `vite` 8.3.5, `@vitejs/plugin-vue` 6.0.10,
  `pinia` 4.1.0, `vue-router` 5.4.0, `prettier` 3.9.6.
- Backend packages referenced: `Microsoft.AspNetCore.OpenApi` 10.0.12. **MediatR is not
  referenced yet** — add it deliberately, see the license gotcha.
- `node_modules/` and `dist/` exist locally and are git-ignored.

## Commands

Frontend:

```powershell
npm ci               # reproducible install from the committed lockfile
npm install          # only when intentionally changing dependencies
npm run dev          # Vite dev server, http://localhost:5173
npm run build        # production build into dist/
npm run preview      # serve the production build
npm run format       # Prettier over src/
```

Backend:

```powershell
dotnet build backend\backend.slnx
dotnet run --project backend\BusTicketStore     # http://localhost:5098
```

Both processes run side by side in development. Once the layer split lands, the runnable
project becomes `backend\src\BusTicketStore.Api` and the build command stays the same.

Verify a backend change with `dotnet build`; there is no test runner to call. Verify a
frontend change with `npm run build`; there is no test runner either.

## Project skills

These skills live under `.opencode/skills/` and are **tracked in this repository** — a
fresh clone gets them. Load the matching one before starting the related task.

- **api-design** — REST principles, HTTP methods, status codes, error handling,
  versioning, pagination, rate limiting. Use when designing or reviewing the HTTP
  contract.
- **code** — software-engineering meta-router that auto-selects a framework (debugging,
  architecture, resilience, CI/CD, API design). Use when a task does not name its own
  framework.
- **conventional-commits** — Conventional Commits v1.0.0 plus kernel-style commit
  quality. Use when writing, reviewing, or validating commit messages.
- **ddd** — Domain-Driven Design (Evans): strategic design (bounded contexts, context
  map, ubiquitous language) and tactical design (entities, value objects, aggregates,
  domain events). Use when shaping the domain model.
- **dotnet-patterns** — idiomatic C#/.NET patterns: dependency injection, async/await,
  LINQ, resource management, maintainability. Use when writing or reviewing C#.
- **event-sourcing-cqrs** — event sourcing and CQRS in practice. Use when a command
  needs to produce events or a query needs a dedicated read model.
- **modular-monolith** — module boundaries inside one deployment. **Not the current
  target** — this backend is a layered Clean Architecture app. Use only if the backend
  later splits into independently deployable modules.
- **owasp-security** — OWASP Top 10 checklist. Use when touching input handling,
  authentication, or anything that crosses the network boundary.
- **readme-writer** — professional `README.md` structure (title, stack badges, About,
  concept table, architecture, user vs. developer instructions). Use when creating or
  rewriting the README.
- **solid** — SOLID principles (SRP, OCP, LSP, ISP, DIP), with warnings against
  over-application. Use when designing or refactoring object-oriented code.

## Git and repo conventions

- Default branch `main`; remote `origin` → `ErnestoVegaDev/bus-ticket-store-los-panchos-remake`.
- Commits follow **Conventional Commits** (e.g. `feat(tickets): ...`,
  `chore(gitignore): ...`).
- **Never add `Co-Authored-By` or AI attribution to a commit.**
- Tracked: `src/`, `public/`, `backend/` sources, `index.html`, `package.json`,
  `package-lock.json`, `vite.config.js`, `jsconfig.json`, `.prettierrc.json`,
  `.vscode/extensions.json`, `.vscode/settings.json`, `README.md`, `AGENTS.md`,
  `.opencode/skills/**`.
- `.gitignore` excludes: `node_modules/`, `dist/`, `.atl/`, `opencode.json`, `.idea`,
  `docs/`, `.vscode/*` (except `extensions.json` and `settings.json`), `[Bb]in/`, `[Oo]bj/`,
  `.vs/`, `*.user`, `TestResults/`, `*.db`, `appsettings.*.local.json`, `*.pfx`, `*.snk`,
  `*.key`, `.azure/`.
- Feature work is tracked in `odd/tasks/<feature-name>.md`. Run `git status` before
  assuming a file is under version control — the base structure of both solutions is
  committed on `main`, but feature work lands on its own branches.
