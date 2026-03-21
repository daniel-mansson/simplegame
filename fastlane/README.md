# Fastlane — Puzzle Tap Distribution Pipeline

CLI-based build and distribution automation for iOS and Android.
Run all commands from the **project root** with `bundle exec fastlane`.

---

## Quick Start

```bash
# Install dependencies (first time, or after Gemfile changes)
bundle install

# Validate your setup without touching real APIs
bundle exec fastlane ios register dry_run:true
bundle exec fastlane android bootstrap dry_run:true
bundle exec fastlane ios build dry_run:true
bundle exec fastlane android build dry_run:true
```

---

## Environment Variables

All secrets are read from environment variables — never committed. Set them in your shell or in a `.env.local` file at the project root (gitignored).

### Apple / App Store Connect

| Variable | Required for | Description |
|---|---|---|
| `ASC_KEY_ID` | All Apple lanes | App Store Connect API key ID (e.g. `D383SF739`) |
| `ASC_ISSUER_ID` | All Apple lanes | ASC API issuer ID (UUID) |
| `ASC_KEY_FILEPATH` | All Apple lanes | Absolute path to the downloaded `.p8` key file |
| `APPLE_ID` | `register` only | Apple ID email — only needed for `produce` app creation |
| `APPLE_TEAM_ID` | `register`, `certs` | 10-character team ID from developer.apple.com |
| `MATCH_GIT_URL` | `certs` | SSH or HTTPS URL of the private cert git repo |
| `MATCH_PASSWORD` | `certs` | Passphrase for encrypting/decrypting the cert repo |

**Getting ASC API Keys:**
1. Log in to [appstoreconnect.apple.com](https://appstoreconnect.apple.com/access/users)
2. Go to Keys tab → click + to generate a new key
3. Select "Developer" access
4. Download the `.p8` file (only downloadable once)
5. Note the Key ID and Issuer ID

### Android / Google Play

| Variable | Required for | Description |
|---|---|---|
| `GOOGLE_PLAY_KEY_FILE` | All Android lanes | Absolute path to the service account JSON key file |
| `ANDROID_KEYSTORE_PATH` | `android build` | Absolute path to the `.keystore` file |
| `ANDROID_KEY_ALIAS` | `android build` | Key alias within the keystore |
| `ANDROID_KEY_PASSWORD` | `android build` | Password for the key alias |
| `ANDROID_STORE_PASSWORD` | `android build` | Password for the keystore file |

### Unity

| Variable | Required for | Description |
|---|---|---|
| `UNITY_PATH` | `ios build`, `android build` | Absolute path to the Unity executable |

**Unity path examples:**
- macOS: `/Applications/Unity/Hub/Editor/6000.0.30f1/Unity.app/Contents/MacOS/Unity`
- Windows: `C:\Program Files\Unity\Hub\Editor\6000.0.30f1\Editor\Unity.exe`

---

## First-Time Setup

### iOS Bootstrap

1. Set all Apple env vars (see table above)
2. Create a private git repo for match certificates (e.g. `github.com/yourorg/puzzletap-certs`)
3. Set `MATCH_GIT_URL` to its SSH URL
4. Set `MATCH_PASSWORD` to a secure passphrase
5. Register the app:
   ```bash
   bundle exec fastlane ios register
   ```
6. Create certificates and provisioning profiles:
   ```bash
   bundle exec fastlane ios certs type:development
   bundle exec fastlane ios certs type:appstore
   ```

### Android Bootstrap

Google Play API cannot create new apps programmatically. One-time manual steps are required:

```bash
bundle exec fastlane android bootstrap
```

This prints step-by-step instructions. Follow them, then validate:

```bash
bundle exec fastlane run validate_play_store_json_key json_key:$GOOGLE_PLAY_KEY_FILE
```

---

## Available Lanes

### iOS

```bash
bundle exec fastlane ios register [dry_run:true]     # Register app on App Store Connect
bundle exec fastlane ios certs [type:appstore] [dry_run:true]   # Fetch/create certs via match
bundle exec fastlane ios build [dry_run:true]         # Build .ipa from Unity
bundle exec fastlane ios beta [dry_run:true]          # Upload to TestFlight
bundle exec fastlane ios metadata [dry_run:true]      # Push store text to App Store Connect
```

### Android

```bash
bundle exec fastlane android bootstrap [dry_run:true] # Print one-time manual setup instructions
bundle exec fastlane android build [dry_run:true]     # Build .aab from Unity
bundle exec fastlane android beta [track:internal] [dry_run:true]  # Upload to Play Store track
bundle exec fastlane android metadata [dry_run:true]  # Push store text to Play Console
```

### Query

```bash
bundle exec fastlane status    # Query both platforms, return JSON
```

---

## Build Numbers

Build numbers follow the `aabbccdd` encoding convention documented in `Docs/BUILD_NUMBERS.md`.
The build lane automatically reads the current number, increments the counter, and writes it back
to `ProjectSettings/ProjectSettings.asset` before building.

Current: version `0.1.0`, build number `10001`.

---

## Folder Structure

```
fastlane/
  Appfile             — app identifiers (env var refs only)
  Fastfile            — platform lanes: ios, android, status
  Matchfile           — match cert repo config (env var refs only)
  Pluginfile          — fastlane plugin dependencies
  config/
    app.json          — app metadata and env var name mapping
    build.json        — build environment config
    testers.json      — TestFlight tester groups
  metadata/
    default/          — iOS store text (deliver format)
    android/en-US/    — Android store text (supply format)

tools/
  fastlane/
    Fastfile          — bootstrap_ios, bootstrap_android lanes
    lib/
      dry_run.rb      — DryRun module (with_dry_run wrapper)
      build_number.rb — BuildNumber module (aabbccdd encoding)
      config.rb       — Config module (JSON config loader)
```

---

## Dry-Run Mode

Every lane accepts `dry_run:true`. In dry-run mode:
- All API calls and file mutations are **skipped**
- Intent is logged: `[DRY RUN] Would: <description>`
- Config file validation **still runs** — catches missing files early
- Environment variable checks warn instead of failing
- Exit code is 0 on success

Use dry-run to validate your configuration before committing to real API calls.

---

## Secrets Checklist

Before running any live lane, confirm:

- [ ] `ASC_KEY_ID`, `ASC_ISSUER_ID`, `ASC_KEY_FILEPATH` set and `.p8` file exists
- [ ] `MATCH_GIT_URL` points to your private cert repo
- [ ] `MATCH_PASSWORD` set to your repo encryption passphrase
- [ ] `GOOGLE_PLAY_KEY_FILE` set and JSON key file exists
- [ ] `ANDROID_KEYSTORE_PATH` set and keystore file exists
- [ ] `UNITY_PATH` set to correct Unity version executable

No secrets should ever appear in committed files. If you accidentally commit a secret, rotate it immediately.
