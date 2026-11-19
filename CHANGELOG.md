# Changelog

All notable changes to **Quantix** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Package versions are derived from git tags by [MinVer](https://github.com/adamralph/minver):
tagging a commit `v1.0.0` produces package version `1.0.0`.

## [Unreleased]

### Fixed

- The NuGet package page showed the README's header image as raw HTML: nuget.org
  renders neither HTML nor relative image paths. The header is now a Markdown
  image with an absolute URL.

## [2.0.0] - 2026-11-20

### Fixed

- A handler class implementing several handler interfaces (for example
  `INotificationHandler<A>, INotificationHandler<B>`) was discovered for the
  first interface only; the others were silently dropped. Every implemented
  handler interface is now discovered.
- A message deriving from another message could be routed to the base
  message's handler by the type-pattern chain (used for up to 32 messages).
  Derived messages are now tested before their bases.
- A generic message created with a target-typed `new(...)` was not discovered,
  so its open-generic handler was never closed and `Send` failed at run time.
- `[QuantixIgnore]` was documented but had no effect; it now excludes the
  handler or behavior from discovery.
- Passing a `null` message to `Send`, `Publish` or `Stream` now throws
  `ArgumentNullException` instead of `NullReferenceException` — or, for
  `Publish`, silently doing nothing.
- A behavior class implementing several behavior interfaces (for example
  request and stream) wrapped only the first; and a class that was both a
  handler and a behavior lost its behavior role. Every implemented behavior
  interface is now discovered.
- The sample's behaviors logged whole messages and results, which carry user
  input; they now log the message type only. The sample also length-limits
  its free-text route values and lowers ASP.NET request logging to Warning.
- The sample's `/visit/{page}` count was shared across all pages, so a first
  visit to one page could report "Visit #3"; it is now counted per page, with
  at most 1,000 distinct pages tracked. A route value longer than 64
  characters now returns 400 Bad Request instead of 404 Not Found.
- The package's project and repository URLs now point at the current
  repository (`Nertti-Technologies/Quantix`).

### Performance

- `Send` for a command or query with a reference-type result no longer
  allocates (previously 40 bytes per call from boxing the `ValueTask<TResult>`).
- The navigation analyzer builds its handler map only on the first mediator
  call site in a compilation, instead of at every compilation start.

### Removed

- **Breaking:** the `net8.0` target. The package now targets `net10.0` only;
  consumers need the .NET 10 SDK (10.0.100) or later.
- **Breaking:** the `[QuantixHandler]`, `[QuantixModule]` and
  `[ScanQuantixHandlersFrom]` attributes. They were never read by the generator,
  so they had no effect; delete any usages. Discovery covers the compilation
  that calls `AddQuantix()` only.

### Changed

- **Licence:** Quantix is licensed under Apache-2.0. The package metadata
  (previously MIT) and the README now match the `LICENSE` file.
- The package now ships `LICENSE` and a generated `THIRD-PARTY-NOTICES.txt`,
  crediting the MIT-licensed polyfill code (PolySharp and .NET runtime
  sources) compiled into the generator. CI fails if the notices drift from the
  pinned packages (`eng/notices.py --check`).
- The repository builds with the .NET 11 SDK (`global.json` pins 11.0.100,
  rolling forward to later feature bands).
- Build tooling updated: PolySharp 1.17.0, Microsoft.NET.Test.Sdk 18.12.0,
  xunit 2.9.3, BenchmarkDotNet 0.15.8, MinVer 8.0.0.
- CI workflows run with read-only token permissions and pin every action to a
  commit SHA; the release workflow passes the tag name through an environment
  variable and publishes to NuGet with Trusted Publishing (a short-lived key
  issued per release) instead of a stored API key. Dependabot is configured
  for NuGet and GitHub Actions, excluding the deliberate version floors.
- The CI floor check now builds a `net10.0` consumer with the oldest supported
  SDK (10.0.100).

## [1.0.1] - 2026-05-26

### Fixed

- The generated dispatcher emitted an illegal direct cast in the frozen
  (large-message-set) routing path, breaking compilation for consumers whose
  generic-interface messages (`ICommand<TResult>`, `IQuery<TResult>`,
  `IStreamRequest<TResult>`) were declared `sealed` — the standard C# message
  shape. The discriminator already proves the runtime type at that point, so
  the cast now goes through `object` (zero cost on reference types, identical
  IL). Pattern-chain routing (used below the message-count threshold) was
  unaffected. A regression test in `AdaptiveDispatchTests` now compiles the
  emitted source for the frozen path so this class of mistake cannot recur.

## [1.0.0] - 2026-05-23

The first release of Quantix — a source-generated, AOT-friendly mediator for .NET.
Handler discovery, generic closing and dispatch all happen at compile time, so there
is no startup scan, no per-call reflection, and no trim or AOT warnings.

### Added

- Source-generated mediator: an incremental Roslyn generator emits the dispatcher and
  the dependency-injection registration into the consuming assembly.
- Message kinds: void commands, result commands, queries, notifications and stream
  requests — `ICommand`, `ICommand<TResult>`, `IQuery<TResult>`, `INotification`,
  `IStreamRequest<TResult>`.
- `IMediator` with `Send`, `Publish` and `Stream`.
- Pipeline behaviors, including constraint-aware open-generic behaviors, ordered with
  `[PipelineOrder]`.
- Notification handlers run sequentially, ordered with `[NotificationOrder]`.
- Generic message types, discovered from their construction sites and closed at
  compile time.
- Adaptive dispatch: a type-pattern chain for small message sets and a
  `FrozenDictionary` jump-table above a benchmark-tuned threshold.
- Twelve compile-time diagnostics, `QTX0001`–`QTX0012`, that turn missing handlers,
  duplicate handlers and signature mismatches into build errors.
- Navigation analyzer: `QTX0012` names the handler at each `Send` / `Publish` /
  `Stream` call site.
- A single NuGet package, `Quantix`, bundling the abstractions and the generator.
- Native AOT and trimming support, with zero trim or AOT warnings.

[Unreleased]: https://github.com/Nertti-Technologies/Quantix/compare/v2.0.0...HEAD
[2.0.0]: https://github.com/Nertti-Technologies/Quantix/compare/v1.0.1...v2.0.0
[1.0.1]: https://github.com/Nertti-Technologies/Quantix/releases/tag/v1.0.1
[1.0.0]: https://github.com/Nertti-Technologies/Quantix/releases/tag/v1.0.0
