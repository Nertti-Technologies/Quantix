# Changelog

All notable changes to **Quantix** are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
Package versions are derived from git tags by [MinVer](https://github.com/adamralph/minver):
tagging a commit `v1.0.0` produces package version `1.0.0`.

## [Unreleased]

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

### Performance

- `Send` for a command or query with a reference-type result no longer
  allocates (previously 40 bytes per call from boxing the `ValueTask<TResult>`).
- The navigation analyzer builds its handler map only on the first mediator
  call site in a compilation, instead of at every compilation start.

### Changed

- The repository builds with the .NET 11 SDK (`global.json` pins 11.0.100,
  rolling forward to later feature bands).
- Build tooling updated: Microsoft.NET.Test.Sdk 18.12.0, xunit 2.9.3,
  BenchmarkDotNet 0.15.8, MinVer 8.0.0.

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

[Unreleased]: https://github.com/isureshsubramanian/Quantix/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/isureshsubramanian/Quantix/releases/tag/v1.0.1
[1.0.0]: https://github.com/isureshsubramanian/Quantix/releases/tag/v1.0.0
