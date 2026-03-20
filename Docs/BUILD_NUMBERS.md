# Build Number Conventions

## Version Number Format

The app version is a three-part string: `a.b.c`

- `a` — major version (breaking changes, major content milestones)
- `b` — minor version (feature releases, significant updates)
- `c` — patch version (bug fixes, small updates)

Example: `0.1.0`, `1.2.3`, `2.0.0`

This string is set in:
- Unity `ProjectSettings/ProjectSettings.asset` → `bundleVersion`
- iOS: displayed as the **Version** string in App Store Connect
- Android: displayed as the **Version name** in Play Console

---

## Bundle Number / Build Number Format

Bundle numbers encode both the version and a per-version build counter into a single integer, making them strictly increasing across all builds and uniquely traceable back to a version.

### Encoding scheme

```
aabbccdd
```

| Field | Digits | Range  | Meaning                        |
|-------|--------|--------|--------------------------------|
| `aa`  | 2      | 00–99  | Major version component (`a`)  |
| `bb`  | 2      | 00–99  | Minor version component (`b`)  |
| `cc`  | 2      | 00–99  | Patch version component (`c`)  |
| `dd`  | 2      | 01–99  | Build counter for this version |

The result is an 8-digit integer. Build counter starts at `01` (never `00`).

### Examples

| Version | Build | Bundle number | Notes                    |
|---------|-------|---------------|--------------------------|
| `0.1.0` | 1     | `01000101` → `1000101` | Leading zero dropped by stores |
| `0.1.0` | 2     | `01000102` → `1000102` | Second build of 0.1.0    |
| `0.1.1` | 1     | `01000201` → `1000201` | Patch bump, reset build counter |
| `1.0.0` | 1     | `01000001` → wait — see below |

> **Note on leading zeros:** Integer fields drop leading zeros. `00.01.00` build `01` → `00010001` = `10001` as an integer. This is fine — the value is still strictly increasing and unambiguous when decoded. The *encoding* is always 8 digits conceptually; the stored integer is just that value in decimal.

### Decoding

Given bundle number `N`:

```
dd = N % 100
cc = (N / 100) % 100
bb = (N / 10000) % 100
aa = (N / 1000000) % 100
```

So `1000201` → `dd=01`, `cc=02`, `bb=00`, `aa=01` → version `1.0.2` build `1`.

### Where to set it

| Platform | Field                                              |
|----------|----------------------------------------------------|
| Android  | `ProjectSettings.asset` → `AndroidBundleVersionCode` |
| iOS      | `ProjectSettings.asset` → `buildNumber.iPhone`     |

Both must be updated together whenever a version or build counter changes.

---

## Rules

1. **Version string and bundle number must stay in sync.** When `bundleVersion` changes, recalculate the bundle number.
2. **Build counter increments for every build submitted to TestFlight or Play Console.** Local development builds do not need a new counter.
3. **Bundle numbers must be strictly increasing.** Stores reject builds with a lower or equal number to what was previously submitted.
4. **Major, minor, and patch each cap at 99.** If any component would exceed 99, that's a signal to rethink versioning.
5. **Reset the build counter to `01` whenever the version string changes.**

---

## Current Values

| Field            | Value    |
|------------------|----------|
| `bundleVersion`  | `0.1.0`  |
| Build counter    | `1`      |
| Bundle number    | `10001`  |

Encoding: `aa=00`, `bb=01`, `cc=00`, `dd=01` → `00010001` → integer `10001`.
