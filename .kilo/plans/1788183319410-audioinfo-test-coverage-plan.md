# Test Coverage Plan for IdSharp

## Goal
Expand test coverage across the IdSharp solution, focusing on the severely under-tested `IdSharp.AudioInfo` project, add fixture-based tests for the format readers, document the 4 skipped Tagging tests, and add a CI workflow that builds and runs all tests on every push/PR.

## Final deliverable (when implementation is complete)
- **6 audio fixture files** committed to git under `Tests/IdSharp.AudioInfo.Tests/TestData/` (sample.flac, sample.ogg, sample.wav, sample.mpc, sample.ape, sample_vbr.mp3), plus the existing `file_example_MP3_700KB.mp3`.
- **4 new test files** in `Tests/IdSharp.AudioInfo.Tests/`:
  - `Mpeg4/Mpeg4TagTests.cs` — 8 tests using synthetic byte-array fixtures.
  - `Mpeg/DescriptiveLameTagReaderTests.cs` — 5 tests against the existing MP3.
  - `Mpeg/PresetGuesserTests.cs` — 8 unit tests (no fixtures).
  - `Flac/FlacTests.cs`, `OggVorbis/OggVorbisTests.cs`, `RiffWave/RiffWaveTests.cs`, `Musepack/MusepackTests.cs`, `MonkeysAudio/MonkeysAudioTests.cs` — 3–4 tests each against the new audio fixtures.
- **1 comment block** added to `Tests/IdSharp.Tagging.Tests/ID3v22_FrameTest.cs` documenting the 4 ignored tests.
- **1 CI workflow** at `.github/workflows/build-and-test.yml` running on push and PR for `ubuntu-latest` and `windows-latest`, with no ffmpeg dependency.
- **1 `phase2.md`** at the **repo root** (`phase2.md`) documenting the 4 explicitly deferred items.
- **1 `AGENTS/INSTRUCTIONS.md`** at the **repo root** (`AGENTS/INSTRUCTIONS.md`) capturing the user's tool and permission rules (see Task 0).
- **Test count target**: from 60 to roughly 85–95 passing tests, plus 4 still-ignored Tagging tests (documented).

## Resolved decisions (with user)
- **"Fixture" terminology**:
  - **Audio fixture** = a real audio file committed to git under `TestData/`, used to exercise format readers.
  - **Synthetic fixture** = a `byte[]` built in-memory by test helpers, passed via `MemoryStream`, used for parser unit tests (Mpeg4Tag).
- **Audio fixtures are committed to git**, not regenerated on CI. ffmpeg is used **only once** to generate them now; it is not a CI or local-dev dependency.
- **No tests deleted**. The 4 Ignored Tagging tests stay ignored (they require unimplemented features) but are now documented.
- **Mpeg4Tag `meta` test stays in the plan** (Task 2 item 8). If the parser has a bug, the implementation should fix it, not mark the test as a known limitation. Only mark as limitation after a real fix attempt and consultation.
- **All 4 out-of-scope items go to `phase2.md`** (at the repo root): implementing ID3v2.2 Commercial/Encryption frames, performance/benchmark tests, property-based/fuzz tests, round-trip tests on frame encoders.

---

## Task 1: Generate and commit audio fixtures

**Why**: The format-reader tests need real audio files. None of the 5 required formats (flac/ogg/wav/mpc/ape) exist in the repo today. ffmpeg is used once to generate them, then they are committed.

**Do**:
- Verify ffmpeg is available locally (`ffmpeg -version`).
- Run ffmpeg once to generate 6 small files (1 second of 440Hz mono sine unless otherwise noted) into `Tests/IdSharp.AudioInfo.Tests/TestData/`:
  - `sample.flac`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a flac sample.flac`
  - `sample.ogg`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a libvorbis sample.ogg`
  - `sample.wav`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a pcm_s16le sample.wav`
  - `sample.mpc`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a musepack sample.mpc` (if `libmpcenc` not available in local ffmpeg, try `ffmpeg -encoders | grep mpc` and fall back; if truly unavailable, note in the task completion message)
  - `sample.ape`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a ape sample.ape` (if `libmac` not available locally, note in completion message)
  - `sample_vbr.mp3`: `ffmpeg -f lavfi -i "sine=frequency=440:duration=1" -c:a libmp3lame -q:a 4 sample_vbr.mp3`
- Commit all 6 generated files to git.
- Also commit a one-time regeneration script `Tests/scripts/regenerate-fixtures.sh` (for future maintainers who need different fixtures) but it is **not** invoked by CI.
- Update `Tests/IdSharp.AudioInfo.Tests/IdSharp.AudioInfo.Tests.csproj` to add `<Content Include="TestData\sample.*" CopyToOutputDirectory="Always"/>` and `<Content Include="TestData\sample_vbr.mp3" CopyToOutputDirectory="Always"/>` entries next to the existing MP3 entry.

**Validation**: All 6 files exist in `TestData/` and are committed; the csproj copies them to the test output directory; running `dotnet test` finds them at `TestContext.CurrentContext.TestDirectory/TestData/`.

---

## Task 2: Mpeg4Tag tests (synthetic byte-array fixtures)

**Why**: `IdSharp.AudioInfo/TagSizes/Mpeg4Tag.cs` is internal (InternalsVisibleTo already set for the test project) and has zero test coverage. Synthetic byte arrays give deterministic, fast tests with no external dependencies.

**Do** — create `Tests/IdSharp.AudioInfo.Tests/Mpeg4/Mpeg4TagTests.cs`:
- Helpers in the test class:
  - `MakeAtom(string type, byte[] payload)` — builds big-endian atom bytes `[4B size BE][4 ASCII type][payload]`.
  - `WriteBE32(byte[] buf, int offset, int value)` — writes a 32-bit big-endian int.
- Test cases:
  1. `ReadStream_Empty` — empty stream, no crash, all properties at default.
  2. `ReadStream_FtypOnly` — a single `ftyp` atom (not in `ATOM_TYPES`, so recursion stops). No crash.
  3. `ReadStream_MdatSetsMdatAtomSize` — a `mdat` atom of size 1000. Assert `MdatAtomSize == 1000`.
  4. `ReadStream_MdhdPopulatesFrequencyAndSamples` — `mdhd` payload with version byte 0, flags 0, creation_time 0, modification_time 0, timescale = 44100, duration = 88200. Assert `Frequency == 44100` and `Samples == 88200`.
  5. `ReadStream_Stsd_Aac` — `stsd` with one entry whose data_format is `"mp4a"` and `encoder_vendor[0] == 0`, with sample-rate/channel bytes in the expected offsets. Assert `Codec == "AAC"`, `Channels == 2`, `Frequency == 48000`.
  6. `ReadStream_Stsd_NonAac` — same but data_format `"alac"`. Assert `Codec == "alac"`.
  7. `ReadStream_CorruptedAtomSize_Throws` — atom with size larger than stream length. Assert `InvalidDataException`.
  8. `ReadStream_MetaWithIlst` — `meta` atom whose payload contains the bytes `ilst` at some offset. Assert no crash. **If the parser hangs or throws**, the implementation should investigate and fix the underlying bug in `Mpeg4Tag.cs` (this is preferred over marking as a known limitation). Only mark as a known limitation if a real fix attempt is infeasible.

**Key implementation notes** (for the implementer):
- `Mpeg4Tag.ReadStream(Stream)` is the entry point. Use `new MemoryStream(bytes)`.
- `Mpeg4Tag` constructor takes a `string path`; use `ReadStream` via reflection or add a tiny wrapper. Prefer the `ReadStream` method directly.
- Atom size field is **big-endian** (MP4 standard). The existing `ReadInt32` helper in the class is big-endian.
- The `mdhd` parser does `Seek(12, SeekOrigin.Current)` after reading the atom header (size+type = 8 bytes), then reads `Frequency` and `Samples`. With version 0 the payload layout is `[1B version + 3B flags][4B creation][4B modification][4B timescale][4B duration]...`. After seeking 12 bytes, position is at the timescale field.
- The `stsd` parser expects `[4B version+flags][4B num_entries]` then per-entry `[4B size][4B data_format][6B reserved][2B data_ref_index]...`. The code skips 12 bytes after `data_format` (i.e. 6B reserved + 2B data_ref + ...). Carefully follow the offsets in `ParseStsdAtom` (Mpeg4Tag.cs:55–88).

**Validation**: `dotnet test IdSharp.AudioInfo.Tests` passes all new Mpeg4Tag tests; existing 3 tests still pass.

---

## Task 3: DescriptiveLameTagReader tests

**Why**: `DescriptiveLameTagReader` is public, has 0 tests, and wraps the same MP3 fixture that `BasicLameTagReader` already uses.

**Do** — create `Tests/IdSharp.AudioInfo.Tests/Mpeg/DescriptiveLameTagReaderTests.cs`:
- Extract the `MP3TestFilePath` from `MpegTests.cs` into a shared `TestPaths` static class (create it if not present) to avoid duplication.
- Cases:
  1. `Constructor_ValidFile` — build a reader against the existing MP3, assert `IsLameTagFound == true`, `VersionString == "3.10"`, `Preset == "480"`. (Verify these values against the file's actual contents by reading `BasicLameTagReaderTests` and the file; the existing `MpegTests.BasicLameTagReader` test pins these values, so they should be the same.)
  2. `Constructor_NullOrEmptyPath_Throws` — `Assert.Throws<ArgumentNullException>` for `null` and `string.Empty`.
  3. `Constructor_WhitespacePath_Throws` — `Assert.Throws<ArgumentException>` for `"   "`. (Note: `DescriptiveLameTagReader` only checks `IsNullOrEmpty`, not whitespace — verify the actual behavior and pin the test to the actual exception type. If it only throws `ArgumentNullException` for null/empty and no exception for whitespace, adjust the test accordingly and document.)
  4. `Constructor_MissingFile_Throws` — `Assert.Throws<FileNotFoundException>` for a non-existent path.
  5. `PresetGuess_PropertiesPopulated` — assert `PresetGuess`, `IsPresetGuessNonBitrate`, `UsePresetGuess` are populated (don't pin exact values, just non-default/non-null).

**Validation**: All 5 new tests pass; total AudioInfo tests go from 3 → 8+.

---

## Task 4: PresetGuesser unit tests

**Why**: Just refactored to return `PresetGuessResult` record struct. Now is the ideal time to add tests. All inputs/outputs are pure — no fixtures needed.

**Do** — create `Tests/IdSharp.AudioInfo.Tests/Mpeg/PresetGuesserTests.cs`:
- `PresetGuesser` is `internal sealed`; `InternalsVisibleTo` covers it.
- The public surface is `GuessPreset(string versionString, byte bitrate, byte quality, byte encodingMethod, byte noiseShaping, byte stereoMode, byte athType, byte lowpassDiv100)`.
- Cases:
  1. `GuessPreset_UnknownVersion_ReturnsUnknown` — version `"1.00"`, expect `Preset == LamePreset.Unknown` and `NonBitrate == false`.
  2. `GuessPreset_390Version_3.90_TrailingDot_ReturnsUnknown` — `"3.90"` → exercises the `3.90 && != "3.90."` branch.
  3. `GuessPreset_390TrailingDot_TriesTwoVersions` — `"3.90."` → exercises `BestGuessTwoVersions`. With no matching row, expect `Unknown`.
  4. `GuessPreset_391Version` — `"3.91"` → exercises `lvg3902_391` branch.
  5. `GuessPreset_393Version` — `"3.93"` → exercises `BestGuessTwoVersions` between `lvg3931_3903up` and `lvg393`.
  6. `GuessPreset_394OrLater` — `"3.94"` and `"3.95"` → `lvg394up` branch.
  7. `GuessPreset_BitrateMatch_NotNonBitrate` — pick concrete inputs that match a bitrate row in `PresetGuessTable`, assert `NonBitrate == false`.
  8. `GuessPreset_NonBitrateMatch_OnlyVbr` — pick inputs that match no bitrate row but match a non-bitrate row with `encodingMethod == 3` or `4`, assert `NonBitrate == true`.

**How to find matching rows** (at implementation time):
- Read `PresetGuesser.cs` and `PresetGuessRow.cs` to determine the field order in `PresetGuessRow`.
- Pick a concrete row, reverse-engineer the 8 input arguments, and pin them in the test. The `PresetGuessTable` has ~40 rows, so multiple options exist for both bitrate-match and non-bitrate-match cases.

**Validation**: All 8 tests pass; PresetGuesser has full unit coverage of all version-string branches and both bitrate/non-bitrate paths.

---

## Task 5: Format reader tests (Flac, OggVorbis, RiffWave, Musepack, MonkeysAudio)

**Why**: Five entire format namespaces have zero tests. Each format reader is a public class taking a `string path`. The audio fixtures from Task 1 make this doable.

**Do** — add one test file per format, all using the committed fixtures from Task 1:
- `Tests/IdSharp.AudioInfo.Tests/Flac/FlacTests.cs`
- `Tests/IdSharp.AudioInfo.Tests/OggVorbis/OggVorbisTests.cs`
- `Tests/IdSharp.AudioInfo.Tests/RiffWave/RiffWaveTests.cs`
- `Tests/IdSharp.AudioInfo.Tests/Musepack/MusepackTests.cs` (only if `sample.mpc` was generated successfully)
- `Tests/IdSharp.AudioInfo.Tests/MonkeysAudio/MonkeysAudioTests.cs` (only if `sample.ape` was generated successfully)

**Common structure per file**:
1. `Constructor_ValidFile_DoesNotThrow` — fixture exists, constructor returns without throwing.
2. `Constructor_NullPath_Throws` — `Assert.Throws<ArgumentNullException>`.
3. `Constructor_MissingFile_Throws` — assert the specific exception each reader throws for a missing file (verify each by reading the constructor).
4. `Properties_Populated` — sample rate, channels, duration are read from the fixture (where the reader exposes those properties).

**For codecs where fixture generation failed** (e.g., mpc or ape encoders not available locally):
- The test file should still be created but all its `[Test]` methods are gated with `Assume.That(File.Exists(FixturePath), "Fixture not generated")` at the top, or the entire fixture is omitted.
- Alternatively, if a fixture is genuinely unavailable, the corresponding test file can be skipped (not created) and noted in the task completion message. The user will be informed which format readers ended up without tests.

**File format details for the implementer** (verify against each reader's source):
- `Flac(string path)` — `IdSharp.AudioInfo/Flac/Flac.cs`
- `OggVorbis(string path)` — `IdSharp.AudioInfo/OggVorbis/OggVorbis.cs`
- `RiffWave(string path)` — `IdSharp.AudioInfo/RiffWave/RiffWave.cs`
- `Musepack(string path)` — `IdSharp.AudioInfo/Musepack/Musepack.cs`
- `MonkeysAudio(string path)` — `IdSharp.AudioInfo/MonkeysAudio/MonkeysAudio.cs`

For each, read the constructor to find:
- The exact exception type for null/empty/missing path.
- The public properties that get populated.

**Validation**: All five format readers (or as many as have fixtures) have at least 3 passing tests each. The user is informed of any skipped-due-to-missing-fixture cases.

---

## Task 6: Tagging tests — document the 4 skipped

**Why**: Skipped tests rot. The 4 skipped tests in `Tests/IdSharp.Tagging.Tests/ID3v22_FrameTest.cs` are marked `[Ignore("Not supported")]` because the library doesn't implement Commercial/Encryption ID3v2.2 frames. They are kept (not deleted) but should be self-explanatory.

**Do**:
- **Do not un-skip** (requires implementing features).
- **Do not delete** (per user).
- Add a single `//` comment block at the top of `ID3v22_FrameTest.cs` (above the `[TestFixture]` attribute) explaining:
  > The 4 ignored tests below correspond to ID3v2.2 frame types (COM — Commercial, ENC — Encryption, and related) that are not implemented in `IdSharp.Tagging`. See the `FrameTest` base class. They are kept as `[Ignore]` to document the gap, not to mask failures. Un-skipping requires implementing the missing frame types in `IdSharp.Tagging`.
- **Verify the comment is accurate** at implementation time by reading `FrameTest.cs` and the base methods. If the actual reason is different, adjust the comment to match.
- Note: the plan originally listed `CRA, EQU, CRM` as Commercial frame types — these were guesses. The implementer must read the actual `FrameTest` base class to confirm the correct frame types before writing the comment.

**Validation**: No new passing tests, but the 4 Ignored tests are now documented and the file is self-explanatory.

---

## Task 7: CI workflow (GitHub Actions)

**Why**: The user explicitly requested CI/CD. Today there is no `.github/workflows/` directory, so builds and tests are not validated on push. No ffmpeg dependency in CI (fixtures are committed).

**Do** — create `.github/workflows/build-and-test.yml`:
- Trigger: `on: [push, pull_request]` to all branches.
- Matrix: `ubuntu-latest` and `windows-latest` (the library targets both via the csproj `Condition="'$(OS)' == 'Windows_NT'"` blocks).
- Steps per matrix:
  1. `actions/checkout@v4`
  2. `actions/setup-dotnet@v4` with `dotnet-version: ['8.0.x', '10.0.x']` (the solution targets both)
  3. `dotnet restore IdSharp.sln`
  4. `dotnet build IdSharp.sln --no-restore --configuration Release`
  5. `dotnet test IdSharp.sln --no-build --configuration Release --logger "trx;LogFileName=test-results.trx"`
  6. Upload test results: `actions/upload-artifact@v4` with `if: always()` so failures are still uploaded.
- Add a brief comment at the top of the workflow explaining: "Test fixtures are committed to git; no external dependencies required. ffmpeg is not installed in CI."

**Validation**: Workflow file is syntactically valid YAML; the user can push to a branch and observe green/red status.

---

## Resolved risks and questions

1. **ffmpeg codec availability** — **RESOLVED**: ffmpeg is used only at implementation time (one shot, local) to generate fixtures that are then committed. No ffmpeg in CI or local dev workflow.
2. **`PresetGuesser` non-bitrate path test** — **RESOLVED**: the `PresetGuessTable` has multiple rows that can be matched on `TVs[1..6]` with `AEncodingMethod == 3 || 4`; the implementer picks concrete inputs at implementation time.
3. **CI Windows ffmpeg** — **RESOLVED**: no ffmpeg in CI.
4. **Mpeg4Tag `meta` atom test** — **RESOLVED**: stays in the plan, implementation should fix the parser if the test reveals a bug. Only mark as limitation after a real fix attempt.

---

## Execution order

**Task 0 — Pre-implementation setup (mandatory before any other task)**

This task must be done before Task 1. It produces the working agreement between the user and the implementation agent.

0.1. **Move `phase2.md` from `.kilo/plans/phase2.md` to the repo root** (`/mnt/DATA/Ohad/Projects/IdSharp/phase2.md`). The file's content is already finalized; just move it.

0.2. **Create `AGENTS/INSTRUCTIONS.md` at the repo root** with the following content (verbatim — these are the user's standing rules for all agents working in this repo):

   ```markdown
   # Agent Instructions

   ## Tool installation
   - If a required tool is missing, **ask the user** before installing. Do not install tools on your own.
   - **No `npm`**.
   - **No `pip`** (the Python package manager).
   - `uv` and `pnpm` are **allowed only if the user explicitly grants permission** for the specific use. Do not assume permission.

   ## Plan files
   - Plan files live in `.kilo/plans/`.
   - `phase2.md` (deferred work) lives at the repo root.

   ## Scope discipline
   - Do not delete or un-skip tests. Adding a test or making one pass is fine.
   - Fix bugs revealed by new tests when feasible. Only mark `[Ignore]` after a real fix attempt and consultation with the user.
   ```

0.3. **Commit** the moved `phase2.md`, the new `AGENTS/INSTRUCTIONS.md`, and this plan file (`.kilo/plans/1788183319410-audioinfo-test-coverage-plan.md`) together as a single setup commit. Use message: `Add phase2.md, AGENTS/INSTRUCTIONS.md, and test coverage plan`. **Do not push.**

0.4. **Confirm** the setup commit landed and the three files are at the right paths before starting Task 1.

**Task 1 through Task 7** (unchanged from before):

1. Task 1 (generate and commit audio fixtures) — done first so all subsequent tasks can rely on fixtures being present.
2. Task 7 (CI workflow) — set up CI so we get automated validation for everything below.
3. Task 2 (Mpeg4Tag — synthetic fixtures, no external deps) — can run immediately.
4. Task 3 (DescriptiveLameTagReader — uses existing MP3).
5. Task 4 (PresetGuesser — pure unit).
6. Task 5 (format readers — needs Task 1 fixtures).
7. Task 6 (Tagging — just a comment).
8. Commit each task as a separate commit, push, observe CI.

---

## Out of scope (deferred to `phase2.md` at the repo root)

The following items are explicitly out of scope for this plan and are documented in `phase2.md` (at the repo root) for future work:

1. **Implementing the ID3v2.2 Commercial/Encryption frame types** (would un-skip the 4 ignored tests, but is a feature, not a test).
2. **Performance/benchmark tests** (e.g., using BenchmarkDotNet).
3. **Property-based or fuzz tests for the MPEG header parser**.
4. **Round-trip tests on frame encoders** (encode a frame, decode it, assert equality).
