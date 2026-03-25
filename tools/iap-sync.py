#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
iap-sync.py — Single source of truth for IAP product definitions.

Usage:
    python iap-sync.py unity       — update IAPProductCatalog.asset + IAPMockConfig.asset
    python iap-sync.py playfab     — update PlayFab V1 catalog + Remote Config
    python iap-sync.py appstore    — print App Store Connect instructions
    python iap-sync.py googleplay  — print Google Play Console instructions
    python iap-sync.py all         — run all targets

Config file: iap-products.json (in the same directory as this script)

Secrets (never in the config file — set as env vars or in a local .env):
    PLAYFAB_TITLE_ID        e.g. 17F2B3
    PLAYFAB_SECRET_KEY      Developer secret key from PlayFab Game Manager

Dependencies:
    pip install requests python-dotenv
"""

import json
import os
import re
import sys
import io

# Force UTF-8 output on Windows (avoids cp1252 encode errors for arrow chars etc.)
if sys.stdout.encoding and sys.stdout.encoding.lower() != "utf-8":
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding="utf-8", errors="replace")

from pathlib import Path

# ---------------------------------------------------------------------------
# Config loading
# ---------------------------------------------------------------------------

SCRIPT_DIR   = Path(__file__).parent
CONFIG_FILE  = SCRIPT_DIR / "iap-products.json"
REPO_DIR     = SCRIPT_DIR.parent

CATALOG_ASSET   = REPO_DIR / "Assets/Resources/IAPProductCatalog.asset"
MOCK_CONFIG_ASSET = REPO_DIR / "Assets/Resources/IAPMockConfig.asset"


def load_config() -> dict:
    if not CONFIG_FILE.exists():
        sys.exit(f"[ERROR] Config file not found: {CONFIG_FILE}")
    with open(CONFIG_FILE) as f:
        return json.load(f)


def load_secrets() -> dict:
    # Try .env file next to this script first, then environment.
    # .env lives next to this script (Tools/.env) or one level up (repo root).
    # Neither should be committed — both are in .gitignore.
    env_file = SCRIPT_DIR / ".env"
    if not env_file.exists():
        env_file = SCRIPT_DIR.parent / ".env"
    secrets = {}
    if env_file.exists():
        with open(env_file) as f:
            for line in f:
                line = line.strip()
                if line and not line.startswith("#") and "=" in line:
                    k, _, v = line.partition("=")
                    secrets[k.strip()] = v.strip().strip('"').strip("'")

    for key in ("PLAYFAB_TITLE_ID", "PLAYFAB_SECRET_KEY"):
        if key not in secrets and key in os.environ:
            secrets[key] = os.environ[key]

    return secrets


# ---------------------------------------------------------------------------
# Target: unity
# ---------------------------------------------------------------------------

def sync_unity(products: list):
    """Rewrite IAPProductCatalog.asset and IAPMockConfig.asset in-place."""

    print("\n=== Unity assets ===")

    # --- IAPProductCatalog.asset ---
    if not CATALOG_ASSET.exists():
        print(f"[SKIP] {CATALOG_ASSET} not found — run Tools/Setup/Create IAP Assets in Unity first.")
    else:
        text = CATALOG_ASSET.read_text(encoding="utf-8")

        # Build replacement Products block.
        lines = ["  Products:"]
        for p in products:
            lines.append(f"  - ProductId: {p['id']}")
            lines.append(f"    CoinsAmount: {p['coins']}")
            lines.append(f"    DisplayName: {p['display_name']}")
        new_block = "\n".join(lines)

        # Replace existing Products block (from "  Products:" to next top-level key or EOF).
        updated = re.sub(
            r"  Products:.*?(?=\n  \w|\Z)",
            new_block,
            text,
            flags=re.DOTALL,
        )

        if updated == text:
            print(f"[OK]   IAPProductCatalog.asset — no changes needed")
        else:
            CATALOG_ASSET.write_text(updated + "\n", encoding="utf-8")
            print(f"[DONE] IAPProductCatalog.asset — {len(products)} products written")

    # --- IAPMockConfig.asset ---
    # CoinsGranted = first product's coins (mock tests use a single grant value).
    if not MOCK_CONFIG_ASSET.exists():
        print(f"[SKIP] {MOCK_CONFIG_ASSET} not found — run Tools/Setup/Create IAP Assets in Unity first.")
    else:
        first_coins = products[0]["coins"] if products else 0
        text = MOCK_CONFIG_ASSET.read_text(encoding="utf-8")
        updated = re.sub(r"(  CoinsGranted:) \d+", rf"\1 {first_coins}", text)

        if updated == text:
            print(f"[OK]   IAPMockConfig.asset — no changes needed")
        else:
            MOCK_CONFIG_ASSET.write_text(updated, encoding="utf-8")
            print(f"[DONE] IAPMockConfig.asset — CoinsGranted set to {first_coins}")


# ---------------------------------------------------------------------------
# Target: playfab
# ---------------------------------------------------------------------------

PLAYFAB_API = "https://{title_id}.playfabapi.com"


def playfab_request(title_id: str, secret_key: str, endpoint: str, body: dict) -> dict:
    import urllib.request

    url = f"{PLAYFAB_API.format(title_id=title_id)}{endpoint}"
    data = json.dumps(body).encode("utf-8")
    req = urllib.request.Request(
        url,
        data=data,
        headers={
            "Content-Type": "application/json",
            "X-SecretKey": secret_key,
        },
    )
    try:
        with urllib.request.urlopen(req, timeout=15) as resp:
            return json.loads(resp.read())
    except Exception as e:
        sys.exit(f"[ERROR] PlayFab API call to {endpoint} failed: {e}")


def sync_playfab(config: dict, secrets: dict):
    """Update PlayFab V1 catalog items and Remote Config coins blob."""

    print("\n=== PlayFab ===")

    title_id   = secrets.get("PLAYFAB_TITLE_ID")
    secret_key = secrets.get("PLAYFAB_SECRET_KEY")

    if not title_id or not secret_key:
        print("[SKIP] PLAYFAB_TITLE_ID or PLAYFAB_SECRET_KEY not set.")
        print("       Add them to .env or export as environment variables.")
        return

    products        = config["products"]
    catalog_version = config.get("catalog_version", "main")

    # --- V1 Catalog items ---
    # SetCatalogItems replaces the entire catalog version in one call — idempotent.
    catalog_items = []
    for p in products:
        catalog_items.append({
            "ItemId":      p["id"],
            "DisplayName": p["display_name"],
            "Description": p["description"],
            "CustomData":  json.dumps({"coins": p["coins"]}),
            "Consumable":  {"UsageCount": 1},
            "CanBecomeCharacter": False,
            "IsStackable":       False,
            "IsTradable":        False,
            "IsLimitedEdition":  False,
        })

    body = {
        "CatalogVersion": catalog_version,
        "Catalog":        catalog_items,
        "SetAsDefaultCatalog": True,
    }
    resp = playfab_request(title_id, secret_key, "/Admin/SetCatalogItems", body)
    if resp.get("code") == 200:
        print(f"[DONE] PlayFab catalog '{catalog_version}' — {len(catalog_items)} items written")
    else:
        print(f"[ERROR] SetCatalogItems failed: {resp}")

    # --- Remote Config ---
    # Store coins as a single JSON key so the client can fetch all values in one call.
    # Key: "iap_coins"  Value: '{"com.simplegame.coins.500":500,"com.simplegame.coins.1200":1200,...}'
    coins_map = {p["id"]: p["coins"] for p in products}
    remote_config_body = {
        "CreateOrUpdateConfigs": [
            {
                "ConfigGroup": "",
                "Configs": [
                    {"Name": "iap_coins", "Value": json.dumps(coins_map)}
                ],
            }
        ]
    }
    resp = playfab_request(title_id, secret_key, "/GameData/CreateOrUpdateRemoteConfiguration", remote_config_body)
    if resp.get("code") == 200:
        print(f"[DONE] PlayFab Remote Config 'iap_coins' — {len(coins_map)} entries written")
    else:
        # Remote Config API may vary by SDK version — print what we got.
        print(f"[WARN] Remote Config response (check manually): {json.dumps(resp, indent=2)}")


# ---------------------------------------------------------------------------
# Target: appstore
# ---------------------------------------------------------------------------

def sync_appstore(config: dict):
    print("\n=== App Store Connect ===")
    print("Automated update via App Store Connect API requires an Apple Developer account")
    print("and a private key (.p8). Automating this is possible but needs initial setup.")
    print("For now, follow these steps in App Store Connect:\n")
    print("  1. Go to https://appstoreconnect.apple.com")
    print("  2. Select your app → Monetization → In-App Purchases")
    print("  3. For each product below, create or verify the listing:\n")

    for p in config["products"]:
        print(f"  Product: {p['id']}")
        print(f"    Type:         Consumable")
        print(f"    Reference:    {p['display_name']}")
        print(f"    Price tier:   ~ ${p['price_usd']} USD")
        print(f"    Display name: {p['display_name']}")
        print(f"    Description:  {p['description']}")
        print()

    print("  NOTE: Product IDs cannot be changed after creation.")
    print("        Price tiers are approximate — select the closest tier in App Store Connect.")
    print("\n  To automate this later: https://developer.apple.com/documentation/appstoreconnectapi/in-app_purchases")


# ---------------------------------------------------------------------------
# Target: googleplay
# ---------------------------------------------------------------------------

def sync_googleplay(config: dict):
    print("\n=== Google Play Console ===")
    print("Automated update via Google Play Developer API requires a service account.")
    print("For now, follow these steps in Google Play Console:\n")
    print("  1. Go to https://play.google.com/console")
    print("  2. Select your app → Monetise → In-app products")
    print("  3. For each product below, create or verify the listing:\n")

    for p in config["products"]:
        print(f"  Product: {p['id']}")
        print(f"    Product type: Consumable (Managed product)")
        print(f"    Name:         {p['display_name']}")
        print(f"    Description:  {p['description']}")
        print(f"    Price:        ${p['price_usd']} USD (set local prices per market as needed)")
        print(f"    Status:       Active")
        print()

    print("  NOTE: Product IDs cannot be changed after creation.")
    print("\n  To automate this later: https://developers.google.com/android-publisher/api-ref/rest/v3/inappproducts")


# ---------------------------------------------------------------------------
# Entry point
# ---------------------------------------------------------------------------

TARGETS = {
    "unity":       lambda c, s: sync_unity(c["products"]),
    "playfab":     lambda c, s: sync_playfab(c, s),
    "appstore":    lambda c, s: sync_appstore(c),
    "googleplay":  lambda c, s: sync_googleplay(c),
}


def main():
    if len(sys.argv) < 2 or sys.argv[1] not in (*TARGETS, "all"):
        print(__doc__)
        print(f"Available targets: {', '.join(TARGETS)} or all")
        sys.exit(1)

    target  = sys.argv[1]
    config  = load_config()
    secrets = load_secrets()

    targets_to_run = list(TARGETS.keys()) if target == "all" else [target]

    for t in targets_to_run:
        TARGETS[t](config, secrets)

    print("\nDone.")


if __name__ == "__main__":
    main()
