"""Death% vs Unique pattern correlation on Unique folder screenshots."""
from __future__ import annotations

import json
import statistics
from pathlib import Path

import numpy as np

import _analyze_unique_pattern as ap
import _extract_hands as eh

OUT = Path(__file__).resolve().parent / "unique_death_correlation.json"
BUDGET = 48


def signature(offs: list[tuple[int, int]]) -> str:
    return "".join(f"{x},{y};" for x, y in offs)


def remaining(pieces: list[list[tuple[int, int]]], used: list[bool]) -> list[list[tuple[int, int]]]:
    return [p for i, p in enumerate(pieces) if not used[i]]


def full_sequence_exists(board: np.ndarray, pieces: list[list[tuple[int, int]]]) -> bool:
    if not pieces:
        return True
    sigs = [signature(p) for p in pieces]
    used = [False] * len(pieces)

    def search(b: np.ndarray) -> bool:
        rem = sum(1 for u in used if not u)
        if rem == 0:
            return True
        tried: set[str] = set()
        for i, piece in enumerate(pieces):
            if used[i] or sigs[i] in tried:
                continue
            tried.add(sigs[i])
            for px, py in ap.placements(b, piece):
                nxt, _ = ap.place_and_clear(b, piece, px, py)
                used[i] = True
                ok = search(nxt)
                used[i] = False
                if ok:
                    return True
        return False

    return search(board)


def count_death_percent(
    board: np.ndarray,
    pieces: list[list[tuple[int, int]]],
    budget: int = BUDGET,
) -> tuple[float, int, int, bool]:
    """Returns death%, deaths, branches, budget_exceeded."""
    if not pieces:
        return 0.0, 0, 0, False
    sigs = [signature(p) for p in pieces]
    used = [False] * len(pieces)
    deaths = 0
    branches = 0
    exceeded = False

    def accumulate(b: np.ndarray) -> None:
        nonlocal deaths, branches, exceeded
        if exceeded:
            return
        rem = sum(1 for u in used if not u)
        if rem <= 1:
            return
        tried: set[str] = set()
        for i, piece in enumerate(pieces):
            if exceeded:
                return
            if used[i] or sigs[i] in tried:
                continue
            tried.add(sigs[i])
            for px, py in ap.placements(b, piece):
                if exceeded:
                    return
                nxt, _ = ap.place_and_clear(b, piece, px, py)
                used[i] = True
                branches += 1
                if budget > 0 and branches > budget:
                    exceeded = True
                    used[i] = False
                    return
                rest = remaining(pieces, used)
                if not full_sequence_exists(nxt, rest):
                    deaths += 1
                else:
                    accumulate(nxt)
                used[i] = False
                if exceeded:
                    return

    accumulate(board)
    if branches <= 0:
        return 0.0, deaths, branches, exceeded
    return 100.0 * deaths / branches, deaths, branches, exceeded


def bucket_of(h: dict) -> str:
    if h.get("matches_user_rule"):
        return "match_strong_A"
    if h.get("pattern") == "all_placeable":
        return "all_placeable"
    if h.get("pattern") == "multi_blocked":
        return "multi_blocked"
    if h.get("pattern") == "one_blocked_two_free":
        if h.get("alone0") or h.get("alone1"):
            return "single_unlock"
        if not h.get("both_unlock"):
            return "both_fail"
    return "other"


def main() -> None:
    prev = json.loads(ap.OUT.read_text(encoding="utf-8"))
    rows = []
    for h in prev["hands"]:
        if "ids" not in h or "pattern" not in h:
            continue
        name = h["file"]
        board = ap.extract_board(ap.SRC / name)
        pieces = [ap.offsets(i) for i in h["ids"]]
        death_pct, deaths, branches, exceeded = count_death_percent(board, pieces)
        bucket = bucket_of(h)
        row = {
            "file": name,
            "ids": h["ids"],
            "bucket": bucket,
            "pattern": h["pattern"],
            "death_pct": round(death_pct, 2),
            "deaths": deaths,
            "branches": branches,
            "budget_exceeded": exceeded,
            "occupied": int(board.sum()),
            "has_duplicate": h.get("has_duplicate", False),
        }
        rows.append(row)
        print(
            f"{name} {bucket} death={death_pct:.1f}% br={branches}"
            f"{' BUDGET' if exceeded else ''}",
            flush=True,
        )

    by_bucket: dict[str, list[float]] = {}
    for r in rows:
        by_bucket.setdefault(r["bucket"], []).append(r["death_pct"])

    summary = {}
    for k, vals in sorted(by_bucket.items()):
        summary[k] = {
            "n": len(vals),
            "mean": round(statistics.mean(vals), 2),
            "median": round(statistics.median(vals), 2),
            "min": round(min(vals), 2),
            "max": round(max(vals), 2),
        }

    # Also: sloppy = everything except match_strong_A
    strict = [r["death_pct"] for r in rows if r["bucket"] == "match_strong_A"]
    sloppy = [r["death_pct"] for r in rows if r["bucket"] != "match_strong_A"]
    compare = {
        "strict_match_A": {
            "n": len(strict),
            "mean": round(statistics.mean(strict), 2) if strict else None,
            "median": round(statistics.median(strict), 2) if strict else None,
        },
        "sloppy_non_match": {
            "n": len(sloppy),
            "mean": round(statistics.mean(sloppy), 2) if sloppy else None,
            "median": round(statistics.median(sloppy), 2) if sloppy else None,
        },
    }

    out = {
        "budget": BUDGET,
        "by_bucket": summary,
        "compare": compare,
        "rows": sorted(rows, key=lambda r: (-r["death_pct"], r["file"])),
    }
    OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({"by_bucket": summary, "compare": compare}, indent=2), flush=True)


if __name__ == "__main__":
    main()
