# Phase 2 — Deferred Items

This file documents items that were considered during the test coverage plan (`.kilo/plans/1788183319410-audioinfo-test-coverage-plan.md`) but explicitly deferred. They are not forgotten — they are tracked here for a future phase.

## 1. Implement ID3v2.2 Commercial and Encryption frame types

**Context**: The 4 ignored tests in `Tests/IdSharp.Tagging.Tests/ID3v22_FrameTest.cs` are marked `[Ignore("Not supported")]` because `IdSharp.Tagging` does not implement these ID3v2.2 frame types:
- `COM` / `CRA` / `EQU` / `CRM` (Commercial frame family)
- `ENC` (Encryption method and registered crypto data)

**What's needed**:
- Implement the missing frame classes in `IdSharp.Tagging` (read/write logic for the binary representation).
- Add tests in `ID3v22_FrameTest.cs`, `ID3v23_FrameTest.cs`, `ID3v24_FrameTest.cs` as appropriate (the same frames may need different handling across ID3v2 versions).
- Remove the `[Ignore]` attributes and verify the tests pass.

**Effort note**: This is a feature, not a test, and requires understanding the ID3v2.2 spec sections for these frame types.

## 2. Performance / benchmark tests

**Context**: The IdSharp library has hot paths (e.g., `MpegAudio` constructor reads the file multiple times, `Mpeg4Tag.ParseAtom` recurses through atoms, binary parsers in `IdSharp.AudioInfo.Mpeg.Inspection`). There are no benchmarks, so performance regressions can land silently.

**What's needed**:
- Add `BenchmarkDotNet` (or similar) to a new `Tests/IdSharp.AudioInfo.Benchmarks` project (or under `Tests/IdSharp.AudioInfo.Tests/Benchmarks/`).
- Benchmark `MpegAudio(path)`, `BasicLameTagReader(path)`, `DescriptiveLameTagReader(path)`, `Mpeg4Tag.ReadStream(stream)` on the committed MP3 fixture.
- Track results over time (e.g., a `benchmarks/` output directory committed for historical comparison, or a CI workflow that posts results as PR comments).

**Effort note**: Adding BenchmarkDotNet is straightforward; interpreting results and setting up CI reporting is the larger piece.

## 3. Property-based / fuzz tests for the MPEG header parser

**Context**: `MpegAudio.FindFrame` and related methods parse untrusted binary input (the MP3 file). They contain tight loops, arithmetic shifts, and bounds checks. A fuzz test would catch edge cases that hand-crafted tests miss.

**What's needed**:
- Add a fuzz harness using `FsCheck` (property-based) or a .NET-native fuzzer like `SharpFuzz` / `OneFuzz`.
- Generate random byte arrays of varying lengths, ensure no exception escapes (`CatchAll` style) and no infinite loop occurs (use a timeout).
- Optionally: integration with [oss-fuzz](https://github.com/google/oss-fuzz) for continuous fuzzing in the cloud.

**Effort note**: A basic FsCheck property test is small. Production-grade fuzzing in CI is a larger commitment.

## 4. Round-trip tests on frame encoders

**Context**: `IdSharp.Tagging` writes ID3v2 frames. The round-trip property is: write a frame, read it back, assert equality. Today the tests cover read paths and write paths separately, not the combination.

**What's needed**:
- For each frame type that supports both read and write (e.g., text frames, attached picture, etc.), write a frame with known data, read it back, assert the read data matches the written data.
- Use in-memory streams so no file I/O is required.
- Cover edge cases: empty values, maximum-length values, multi-byte encodings (UTF-16, UTF-8).

**Effort note**: Doable per-frame. The work scales with the number of frame types.

---

## How to promote items from phase2

When ready to start one of these items:
1. Create a new plan file (e.g., `.kilo/plans/<timestamp>-<item-name>.md`) using the standard planning workflow.
2. Reference this file to maintain continuity (so reviewers can see why the item was deferred).
3. Remove the item from this file once it lands in its own plan.
