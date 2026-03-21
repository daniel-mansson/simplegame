# T01: Gemfile, Appfile, Matchfile Scaffold

**Slice:** S01
**Milestone:** M015

## Goal
Create the core Ruby/Fastlane project files that let `bundle exec fastlane` resolve and run: Gemfile (pinned deps), Appfile (app identifiers), Matchfile (cert repo stub), and Pluginfile.

## Must-Haves

### Truths
- `bundle install` completes without error (Gemfile.lock generated)
- `bundle exec fastlane --version` prints a version string
- `fastlane/Appfile` contains iOS bundle ID `com.simplemagicstudios.puzzletap`, Android package name `com.SimpleMagicStudios.PuzzleTap`, team ID placeholder
- `fastlane/Matchfile` present with `git_url`, `type`, `app_identifier` stubs
- `fastlane/Pluginfile` present (empty or minimal)

### Artifacts
- `Gemfile` — pins `fastlane` gem, `min ruby version`
- `fastlane/Appfile` — app_identifier (iOS), package_name (Android), team_id, apple_id
- `fastlane/Matchfile` — git_url, type, app_identifier, api_key_path references
- `fastlane/Pluginfile` — gemfile for fastlane plugins (empty source block)

### Key Links
- `Gemfile` → `fastlane` gem → resolved via `bundle install`
- `Appfile` → read by all Fastlane actions via `CredentialsManager::AppfileConfig.try_fetch_value`

## Steps
1. Write `Gemfile` at project root — pin fastlane ~> 2.225, ruby ~> 3.0
2. Write `fastlane/Appfile` with both iOS and Android identifiers and env-var references for team_id and apple_id
3. Write `fastlane/Matchfile` with placeholder git_url env var, type defaults, api_key_path env var
4. Write `fastlane/Pluginfile` (minimal — just the source declaration)
5. Run `bundle install` to generate Gemfile.lock
6. Run `bundle exec fastlane --version` to verify resolution
7. Add `Gemfile.lock` to git (Fastlane convention: lock file should be committed)

## Context
- fastlane 2.225+ supports App Store Connect API key auth for match, pilot, deliver
- Appfile should use `ENV[]` for secrets — never hardcode team IDs or Apple IDs
- Matchfile `git_url` must be an env var since the cert repo URL is a secret/per-environment config
- Ruby version: check what's available on the dev machine; target ~> 3.0 minimum
