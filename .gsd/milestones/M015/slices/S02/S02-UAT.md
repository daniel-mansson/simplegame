# S02: Cert & Provisioning — UAT

**Slice:** S02 — Cert & Provisioning
**Milestone:** M015 — Fastlane Distribution Pipeline

## Prerequisites

- macOS with Ruby ~> 3.0 and `bundle install` complete
- `ASC_KEY_ID`, `ASC_ISSUER_ID`, `ASC_KEY_FILEPATH` set (or skip to dry-run only tests)

## Test Cases

### UAT-01: Dry-run appstore certs

```bash
bundle exec fastlane ios certs dry_run:true
```

**Expected:** Exits 0. Prints `[DRY RUN] Would: Fetch/create appstore certificate and provisioning profile for com.simplemagicstudios.puzzletap`.

### UAT-02: Dry-run development certs

```bash
bundle exec fastlane ios certs type:development dry_run:true
```

**Expected:** Exits 0. DRY RUN log shows `development` type.

### UAT-03: Dry-run adhoc certs

```bash
bundle exec fastlane ios certs type:adhoc dry_run:true
```

**Expected:** Exits 0. DRY RUN log shows `adhoc` type.

### UAT-04 (live, optional): Real match fetch

```bash
export MATCH_GIT_URL=<your-cert-repo>
export MATCH_PASSWORD=<your-passphrase>
bundle exec fastlane ios certs type:development
```

**Expected:** match fetches or creates development certificate and profile. No 2FA prompt.

## Pass Criteria

UAT-01 through UAT-03 pass. UAT-04 optional for now — requires provisioned match repo.
