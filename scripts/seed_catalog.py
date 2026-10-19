#!/usr/bin/env python3
"""Приводить каталог товарів API у відповідність до JSON-файлу.

Використання:
    NBS_ADMIN_EMAIL=... NBS_ADMIN_PASSWORD=... \\
        python3 scripts/seed_catalog.py catalog.json [base_url] [--dry-run]

base_url за замовчуванням http://localhost:5299. Пароль ніколи не виводиться.

Файл: JSON-масив товарів з полями code, name, brand, gender, categoryId,
productType, price, oldPrice, size, color, stock, imageUrl, extraImages,
description. size і extraImages можуть бути рядком через кому або масивом.
Поле code лише ідентифікує запис у логах і в API не передається.

Алгоритм (ідемпотентний, лише stdlib):
  1. вхід адміністратором через POST /api/Auth/login (сесійна кука);
  2. товари, що вже є в БД з тими самими name + imageUrl, пропускаються
     (оновлюються через PUT, лише якщо відрізняються поля, окрім stock;
     залишок на складі не чіпаємо, бо його змінюють замовлення);
  3. решта записів файлу: спершу PUT у наявні товари, що не збіглися, за
     зростанням id (щоб id у старих замовленнях лишилися чинними), далі POST;
  4. зайві товари в БД: DELETE, якщо в них немає замовлень, інакше stock = 0.
"""
from __future__ import annotations

import argparse
import http.cookiejar
import json
import os
import sys
import urllib.error
import urllib.parse
import urllib.request
from decimal import Decimal, InvalidOperation
from pathlib import Path
from typing import Any

DEFAULT_BASE_URL = "http://localhost:5299"
REQUEST_TIMEOUT_SECONDS = 90  # безкоштовний Render «прокидається» до хвилини
GENDERS = ("men", "women", "kids", "unisex")
LOCAL_HOSTS = ("localhost", "127.0.0.1", "::1")

# поля, які порівнюємо, щоб зрозуміти, чи потрібен PUT для товару, що вже є в БД
COMPARED_FIELDS = (
    "name", "brand", "gender", "categoryId", "productType", "price", "oldPrice",
    "size", "color", "imageUrl", "extraImages", "description",
)


class SeedError(Exception):
    """Помилка вхідних даних або відповіді API; завершує скрипт із повідомленням."""


def _csv(value: Any) -> str:
    if isinstance(value, (list, tuple)):
        return ",".join(str(v).strip() for v in value if str(v).strip())
    return "" if value is None else str(value).strip()


def _money(value: Any, label: str) -> Decimal | None:
    if value is None or value == "":
        return None
    try:
        return Decimal(str(value))
    except InvalidOperation as exc:
        raise SeedError(f"{label}: некоректне число {value!r}") from exc


def normalize_entry(raw: dict[str, Any], index: int) -> dict[str, Any]:
    """Перетворює запис файлу на тіло ProductDto (без code) і перевіряє його."""
    code = raw.get("code") or f"#{index + 1}"
    name = _csv(raw.get("name"))
    if not name:
        raise SeedError(f"{code}: порожня назва")

    gender = _csv(raw.get("gender")) or "unisex"
    if gender not in GENDERS:
        raise SeedError(f"{code}: gender має бути одним із {', '.join(GENDERS)}, а не {gender!r}")

    price = _money(raw.get("price"), f"{code}: price")
    if price is None or price < 0:
        raise SeedError(f"{code}: price обов'язкова й не може бути від'ємною")
    old_price = _money(raw.get("oldPrice"), f"{code}: oldPrice")
    if old_price is not None and old_price <= price:
        raise SeedError(f"{code}: oldPrice ({old_price}) має бути більшою за price ({price})")

    try:
        category_id = int(raw["categoryId"])
        stock = int(raw.get("stock", 0))
    except (KeyError, TypeError, ValueError) as exc:
        raise SeedError(f"{code}: categoryId обов'язковий, stock має бути цілим") from exc
    if stock < 0:
        raise SeedError(f"{code}: stock не може бути від'ємним")

    return {
        "name": name,
        "brand": _csv(raw.get("brand")) or "New Balance",
        "gender": gender,
        "categoryId": category_id,
        "productType": _csv(raw.get("productType")),
        "price": price,
        "oldPrice": old_price,
        "size": _csv(raw.get("size")),
        "color": _csv(raw.get("color")),
        "stock": stock,
        "imageUrl": _csv(raw.get("imageUrl")),
        "extraImages": _csv(raw.get("extraImages")),
        "description": _csv(raw.get("description")),
    }


def load_catalog(path: Path) -> list[tuple[str, dict[str, Any]]]:
    """Читає файл і повертає пари (code, тіло товару); code унікальний, якщо заданий."""
    try:
        data = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, json.JSONDecodeError) as exc:
        raise SeedError(f"не вдалося прочитати {path}: {exc}") from exc
    if not isinstance(data, list) or not data:
        raise SeedError(f"{path}: очікується непорожній JSON-масив товарів")

    entries: list[tuple[str, dict[str, Any]]] = []
    seen_codes: set[str] = set()
    for index, raw in enumerate(data):
        if not isinstance(raw, dict):
            raise SeedError(f"запис #{index + 1} не є об'єктом")
        code = str(raw.get("code") or f"#{index + 1}")
        if raw.get("code") and code in seen_codes:
            raise SeedError(f"code {code!r} зустрічається кілька разів")
        seen_codes.add(code)
        entries.append((code, normalize_entry(raw, index)))
    return entries


def _comparable(value: Any) -> Any:
    if value is None:
        return None
    if isinstance(value, (int, float, Decimal)) and not isinstance(value, bool):
        return Decimal(str(value)).normalize()
    return value


def same_fields(wanted: dict[str, Any], existing: dict[str, Any]) -> bool:
    """Чи збігаються всі порівнювані поля (stock не враховується)."""
    return all(_comparable(wanted.get(f)) == _comparable(existing.get(f)) for f in COMPARED_FIELDS)


def to_json_body(product: dict[str, Any]) -> bytes:
    body = {k: (float(v) if isinstance(v, Decimal) else v) for k, v in product.items()}
    return json.dumps(body, ensure_ascii=False).encode("utf-8")


class ApiClient:
    """Мінімальний HTTP-клієнт із сесійною кукою (http.cookiejar)."""

    def __init__(self, base_url: str) -> None:
        self.base_url = base_url.rstrip("/")
        host = urllib.parse.urlparse(self.base_url).hostname or ""
        # кука сесії має прапор Secure; на локальному http:// її теж треба відправляти
        policy = http.cookiejar.DefaultCookiePolicy(
            secure_protocols=("https", "wss", "http") if host in LOCAL_HOSTS else ("https", "wss")
        )
        self.opener = urllib.request.build_opener(
            urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar(policy))
        )

    def request(self, method: str, path: str, body: bytes | None = None) -> Any:
        req = urllib.request.Request(
            self.base_url + path, data=body, method=method,
            headers={"Content-Type": "application/json", "Accept": "application/json"},
        )
        try:
            with self.opener.open(req, timeout=REQUEST_TIMEOUT_SECONDS) as resp:
                payload = resp.read()
        except urllib.error.HTTPError as exc:
            detail = exc.read().decode("utf-8", "replace")[:300]
            raise SeedError(f"{method} {path} -> HTTP {exc.code}: {detail}") from exc
        except urllib.error.URLError as exc:
            raise SeedError(f"{method} {path}: немає зв'язку з {self.base_url} ({exc.reason})") from exc
        return json.loads(payload) if payload else None

    def login(self, email: str, password: str) -> None:
        body = json.dumps({"email": email, "password": password}).encode("utf-8")
        try:
            self.request("POST", "/api/Auth/login", body)
        except SeedError as exc:
            # у повідомленні немає пароля, але й відповідь сервера не потрібна — лише причина
            raise SeedError(f"вхід адміністратора не вдався: {exc}") from exc


def plan_changes(
    wanted: list[tuple[str, dict[str, Any]]], existing: list[dict[str, Any]]
) -> dict[str, Any]:
    """Складає план: що пропустити, оновити, перезаписати, створити та прибрати."""
    existing = sorted(existing, key=lambda p: p["id"])
    free = {p["id"]: p for p in existing}
    unmatched: list[tuple[str, dict[str, Any]]] = []
    skip: list[str] = []
    patch: list[tuple[int, str, dict[str, Any]]] = []

    for code, entry in wanted:
        match = next(
            (p for p in free.values() if p["name"] == entry["name"] and (p.get("imageUrl") or "") == entry["imageUrl"]),
            None,
        )
        if match is None:
            unmatched.append((code, entry))
            continue
        del free[match["id"]]
        if same_fields(entry, match):
            skip.append(code)
        else:
            patch.append((match["id"], code, {**entry, "stock": match["stock"]}))

    spare_ids = sorted(free)
    overwrite = [(spare_ids[i], code, entry) for i, (code, entry) in enumerate(unmatched[: len(spare_ids)])]
    create = unmatched[len(spare_ids):]
    surplus = [free[i] for i in spare_ids[len(unmatched):]]
    return {"skip": skip, "patch": patch, "overwrite": overwrite, "create": create, "surplus": surplus}


def apply_plan(client: ApiClient, plan: dict[str, Any], dry_run: bool) -> dict[str, int]:
    done = {"skipped": len(plan["skip"]), "updated": 0, "overwritten": 0, "created": 0, "deleted": 0, "zeroed": 0}
    tag = "[dry-run] " if dry_run else ""

    for product_id, code, body in plan["patch"]:
        print(f"{tag}update  id={product_id} {code}")
        if not dry_run:
            client.request("PUT", f"/api/Products/{product_id}", to_json_body(body))
        done["updated"] += 1

    for product_id, code, body in plan["overwrite"]:
        print(f"{tag}replace id={product_id} {code}")
        if not dry_run:
            client.request("PUT", f"/api/Products/{product_id}", to_json_body(body))
        done["overwritten"] += 1

    for code, body in plan["create"]:
        print(f"{tag}create  {code}")
        if not dry_run:
            client.request("POST", "/api/Products", to_json_body(body))
        done["created"] += 1

    if plan["surplus"]:
        ordered = _ordered_product_ids(client)
        for product in plan["surplus"]:
            if product["id"] not in ordered:
                print(f"{tag}delete  id={product['id']} {product['name']}")
                if not dry_run:
                    client.request("DELETE", f"/api/Products/{product['id']}")
                done["deleted"] += 1
            elif product["stock"] != 0:
                print(f"{tag}zero stock id={product['id']} {product['name']} (є замовлення)")
                if not dry_run:
                    body = {f: product.get(f) for f in (*COMPARED_FIELDS, "stock")}
                    body["stock"] = 0
                    client.request("PUT", f"/api/Products/{product['id']}", to_json_body(body))
                done["zeroed"] += 1
    return done


def _ordered_product_ids(client: ApiClient) -> set[int]:
    orders = client.request("GET", "/api/Orders/all") or []
    return {item["productId"] for order in orders for item in order.get("items", [])}


def parse_args(argv: list[str]) -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Синхронізує каталог NewBalanceShop із JSON-файлом.")
    parser.add_argument("catalog", type=Path, help="JSON-файл із масивом товарів")
    parser.add_argument("base_url", nargs="?", default=DEFAULT_BASE_URL, help=f"адреса API (за замовчуванням {DEFAULT_BASE_URL})")
    parser.add_argument("--dry-run", action="store_true", help="лише показати, що буде зроблено")
    return parser.parse_args(argv)


def main(argv: list[str] | None = None) -> int:
    args = parse_args(sys.argv[1:] if argv is None else argv)
    email = os.environ.get("NBS_ADMIN_EMAIL", "")
    password = os.environ.get("NBS_ADMIN_PASSWORD", "")
    try:
        if not email or not password:
            raise SeedError("задайте змінні середовища NBS_ADMIN_EMAIL і NBS_ADMIN_PASSWORD")
        wanted = load_catalog(args.catalog)
        client = ApiClient(args.base_url)
        client.login(email, password)
        existing = client.request("GET", "/api/Products") or []
        plan = plan_changes(wanted, existing)
        done = apply_plan(client, plan, args.dry_run)
    except SeedError as exc:
        print(f"Помилка: {exc}", file=sys.stderr)
        return 1

    print(
        f"Готово{' (dry-run)' if args.dry_run else ''}: у файлі {len(wanted)}, у БД було {len(existing)}; "
        f"без змін {done['skipped']}, оновлено {done['updated']}, замінено {done['overwritten']}, "
        f"створено {done['created']}, видалено {done['deleted']}, обнулено залишок {done['zeroed']}"
    )
    return 0


if __name__ == "__main__":
    sys.exit(main())
