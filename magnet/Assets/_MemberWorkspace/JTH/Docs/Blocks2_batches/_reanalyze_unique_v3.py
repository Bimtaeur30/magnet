"""Full Unique re-analysis using visually labeled hands."""
from __future__ import annotations

import json
import statistics
from collections import Counter
from pathlib import Path

import numpy as np

import _analyze_unique_pattern as ap

VISUAL = Path(__file__).resolve().parent / "unique_hands_visual.json"
OUT = Path(__file__).resolve().parent / "unique_reanalysis_v3.json"
BUDGET = 48


def signature(offs: list[tuple[int, int]]) -> str:
    return "".join(f"{x},{y};" for x, y in offs)


def both_unlock(board, a, b, blocked) -> bool:
    for first, second in ((a, b), (b, a)):
        for p1 in ap.placements(board, first):
            after1, c1 = ap.place_and_clear(board, first, *p1)
            for p2 in ap.placements(after1, second):
                after2, c2 = ap.place_and_clear(after1, second, *p2)
                if c1 + c2 < 1:
                    continue
                if ap.can_place_anywhere(after2, blocked):
                    return True
    return False


def full_sequence_exists(board, pieces) -> bool:
    if not pieces:
        return True
    sigs = [signature(p) for p in pieces]
    used = [False] * len(pieces)

    def search(b):
        if sum(1 for u in used if not u) == 0:
            return True
        tried = set()
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


def count_death_percent(board, pieces, budget=BUDGET):
    if not pieces:
        return 0.0, 0, 0, False
    sigs = [signature(p) for p in pieces]
    used = [False] * len(pieces)
    deaths = 0
    branches = 0
    exceeded = False

    def remaining():
        return [p for i, p in enumerate(pieces) if not used[i]]

    def accumulate(b):
        nonlocal deaths, branches, exceeded
        if exceeded:
            return
        if sum(1 for u in used if not u) <= 1:
            return
        tried = set()
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
                if not full_sequence_exists(nxt, remaining()):
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


def classify(board, ids):
    offs = [ap.offsets(i) for i in ids]
    placeable = [ap.can_place_anywhere(board, o) for o in offs]
    blocked_idxs = [i for i, ok in enumerate(placeable) if not ok]
    free_idxs = [i for i, ok in enumerate(placeable) if ok]
    info = {
        "ids": ids,
        "placeable": placeable,
        "blocked_count": len(blocked_idxs),
        "has_duplicate": len(set(ids)) < 3,
        "legal_counts": [len(ap.placements(board, o)) for o in offs],
    }
    if len(blocked_idxs) == 1 and len(free_idxs) == 2:
        bi = blocked_idxs[0]
        u0, u1 = free_idxs
        blocked = offs[bi]
        a, b = offs[u0], offs[u1]
        alone0 = ap.alone_unlocks(board, a, blocked)
        alone1 = ap.alone_unlocks(board, b, blocked)
        both = both_unlock(board, a, b, blocked)
        info.update(
            {
                "pattern": "one_blocked_two_free",
                "blocked_id": ids[bi],
                "unlock_ids": [ids[u0], ids[u1]],
                "unlock_legal": [info["legal_counts"][u0], info["legal_counts"][u1]],
                "alone0": alone0,
                "alone1": alone1,
                "alone_any": alone0 or alone1,
                "both_unlock": both,
                "matches_strong_A": (not alone0) and (not alone1) and both,
                "matches_weak": both,
            }
        )
    elif len(blocked_idxs) == 0:
        info["pattern"] = "all_placeable"
        info["matches_strong_A"] = False
        info["matches_weak"] = False
        info["alone_any"] = False
        info["both_unlock"] = False
    elif len(blocked_idxs) >= 2:
        info["pattern"] = "multi_blocked"
        info["matches_strong_A"] = False
        info["matches_weak"] = False
        info["alone_any"] = False
        info["both_unlock"] = False
    else:
        info["pattern"] = "other"
        info["matches_strong_A"] = False
        info["matches_weak"] = False
    return info


def bucket_of(h):
    if h.get("matches_strong_A"):
        return "match_strong_A"
    if h["pattern"] == "all_placeable":
        return "all_placeable"
    if h["pattern"] == "multi_blocked":
        return "multi_blocked"
    if h["pattern"] == "one_blocked_two_free" and h.get("alone_any"):
        return "single_unlock"
    if h["pattern"] == "one_blocked_two_free" and not h.get("both_unlock"):
        return "obtf_no_unlock"
    return "other"


def main():
    visual = json.loads(VISUAL.read_text(encoding="utf-8"))
    rows = []
    for entry in visual["hands"]:
        name = entry["file"]
        ids = entry["ids"]
        board = ap.extract_board(ap.SRC / name)
        info = classify(board, ids)
        pieces = [ap.offsets(i) for i in ids]
        death, deaths, branches, exceeded = count_death_percent(board, pieces)
        info.update(
            {
                "file": name,
                "occupied": int(board.sum()),
                "death_pct": round(death, 2),
                "branches": branches,
                "budget_exceeded": exceeded,
                "bucket": bucket_of(info),
                "notes": entry.get("notes"),
            }
        )
        rows.append(info)
        print(
            name,
            ids,
            info["pattern"],
            info["bucket"],
            f"death={death:.1f}%",
            "ALONE" if info.get("alone_any") else "",
            "MATCH" if info.get("matches_strong_A") else "",
            flush=True,
        )

    patterns = Counter(r["pattern"] for r in rows)
    buckets = Counter(r["bucket"] for r in rows)
    by_bucket_death = {}
    for b in buckets:
        vals = [r["death_pct"] for r in rows if r["bucket"] == b]
        by_bucket_death[b] = {
            "n": len(vals),
            "mean": round(statistics.mean(vals), 2),
            "median": round(statistics.median(vals), 2),
        }

    obtf = [r for r in rows if r["pattern"] == "one_blocked_two_free"]
    legal_both1 = sum(1 for r in obtf if r.get("unlock_legal") == [1, 1] or (r.get("unlock_legal") and min(r["unlock_legal"]) == 1 and max(r["unlock_legal"]) == 1))
    # fix: unlock_legal both == 1
    legal_both1 = sum(1 for r in obtf if r.get("unlock_legal") and r["unlock_legal"][0] == 1 and r["unlock_legal"][1] == 1)

    summary = {
        "n": len(rows),
        "patterns": dict(patterns),
        "buckets": dict(buckets),
        "match_strong_A": sum(1 for r in rows if r.get("matches_strong_A")),
        "match_weak_both_unlock": sum(1 for r in rows if r.get("matches_weak")),
        "single_unlock_files": [r["file"] for r in rows if r["bucket"] == "single_unlock"],
        "duplicate_files": [r["file"] for r in rows if r["has_duplicate"]],
        "legal_forced_both_among_obtf": legal_both1,
        "obtf_n": len(obtf),
        "death_by_bucket": by_bucket_death,
        "compare_death": {
            "strict_A": {
                "n": by_bucket_death.get("match_strong_A", {}).get("n", 0),
                "mean": by_bucket_death.get("match_strong_A", {}).get("mean"),
                "median": by_bucket_death.get("match_strong_A", {}).get("median"),
            },
            "sloppy": {
                "n": sum(v["n"] for k, v in by_bucket_death.items() if k != "match_strong_A"),
                "mean": round(
                    statistics.mean([r["death_pct"] for r in rows if r["bucket"] != "match_strong_A"]),
                    2,
                )
                if any(r["bucket"] != "match_strong_A" for r in rows)
                else None,
                "median": round(
                    statistics.median([r["death_pct"] for r in rows if r["bucket"] != "match_strong_A"]),
                    2,
                )
                if any(r["bucket"] != "match_strong_A" for r in rows)
                else None,
            },
        },
    }
    OUT.write_text(
        json.dumps({"summary": summary, "hands": rows}, indent=2, ensure_ascii=False),
        encoding="utf-8",
    )
    print(json.dumps(summary, indent=2), flush=True)


if __name__ == "__main__":
    main()
