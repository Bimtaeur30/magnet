"""Compare user Unique rule vs observed Unique screenshot properties."""
from __future__ import annotations

import json
from collections import Counter
from pathlib import Path

import numpy as np

import _analyze_unique_pattern as ap

OUT = Path(__file__).resolve().parent / "unique_condition_diff.json"


def signature(offs: list[tuple[int, int]]) -> str:
    return "".join(f"{x},{y};" for x, y in offs)


def count_full_sequences(board: np.ndarray, pieces: list[list[tuple[int, int]]], cap: int = 2) -> int:
    if not pieces:
        return 0
    sigs = [signature(p) for p in pieces]
    used = [False] * len(pieces)
    found = 0

    def search(b: np.ndarray) -> None:
        nonlocal found
        if found >= cap:
            return
        rem = sum(1 for u in used if not u)
        if rem == 0:
            found += 1
            return
        tried: set[str] = set()
        for i, piece in enumerate(pieces):
            if found >= cap:
                return
            if used[i] or sigs[i] in tried:
                continue
            tried.add(sigs[i])
            for px, py in ap.placements(b, piece):
                if found >= cap:
                    return
                nxt, _ = ap.place_and_clear(b, piece, px, py)
                used[i] = True
                search(nxt)
                used[i] = False

    search(board)
    return found


def alone_clears(board: np.ndarray, piece: list[tuple[int, int]]) -> bool:
    for px, py in ap.placements(board, piece):
        _, c = ap.place_and_clear(board, piece, px, py)
        if c >= 1:
            return True
    return False


def main() -> None:
    prev = json.loads(ap.OUT.read_text(encoding="utf-8"))
    rows = []
    for h in prev["hands"]:
        if "ids" not in h:
            continue
        board = ap.extract_board(ap.SRC / h["file"])
        pieces = [ap.offsets(i) for i in h["ids"]]
        seq = count_full_sequences(board, pieces, cap=2)
        placeable = [ap.can_place_anywhere(board, p) for p in pieces]
        blocked = [i for i, ok in enumerate(placeable) if not ok]
        free = [i for i, ok in enumerate(placeable) if ok]

        flags = {
            "file": h["file"],
            "ids": h["ids"],
            "user_match": bool(h.get("matches_user_rule")),
            "seq_count_cap2": seq,
            "unique_solution": seq == 1,
            "solvable": seq >= 1,
            "blocked_count": len(blocked),
            "has_duplicate": h.get("has_duplicate", False),
            "pattern": h.get("pattern"),
            "alone_unlock": bool(h.get("alone0") or h.get("alone1")),
            "both_unlock": bool(h.get("both_unlock")),
        }

        # Extra: among free pieces, can any single piece clear a line on current board?
        if len(free) >= 1:
            flags["any_free_alone_clears"] = any(alone_clears(board, pieces[i]) for i in free)
        else:
            flags["any_free_alone_clears"] = False

        rows.append(flags)

    def rate(pred):
        hit = [r for r in rows if pred(r)]
        return len(hit), round(100 * len(hit) / max(1, len(rows)), 1)

    # Condition checklist coverage on real Unique set
    checklist = {
        "solvable_full_sequence": rate(lambda r: r["solvable"]),
        "unique_solution_exactly_1": rate(lambda r: r["unique_solution"]),
        "exactly_1_blocked": rate(lambda r: r["blocked_count"] == 1),
        "0_blocked_all_free": rate(lambda r: r["blocked_count"] == 0),
        "2plus_blocked": rate(lambda r: r["blocked_count"] >= 2),
        "user_strong_A": rate(lambda r: r["user_match"]),
        "has_duplicate": rate(lambda r: r["has_duplicate"]),
        "among_obtf_alone_unlocks": (
            sum(1 for r in rows if r["pattern"] == "one_blocked_two_free" and r["alone_unlock"]),
            None,
        ),
    }

    # Cross: user mismatch reasons vs unique_solution
    mismatch = [r for r in rows if not r["user_match"]]
    cross = Counter()
    for r in mismatch:
        key = r["pattern"]
        if r["unique_solution"]:
            key += "+uniqueSol"
        elif r["solvable"]:
            key += "+multiSol"
        else:
            key += "+unsolvable"
        cross[key] += 1

    # Among all-placeable, how many unique solution?
    all_free = [r for r in rows if r["blocked_count"] == 0]
    all_free_unique = sum(1 for r in all_free if r["unique_solution"])
    alone = [r for r in rows if r["pattern"] == "one_blocked_two_free" and r["alone_unlock"]]
    alone_unique = sum(1 for r in alone if r["unique_solution"])

    out = {
        "n": len(rows),
        "checklist": {
            k: {"count": v[0], "pct": v[1]} if v[1] is not None else {"count": v[0]}
            for k, v in checklist.items()
        },
        "mismatch_cross": dict(cross),
        "all_placeable_unique_sol": {"n": len(all_free), "unique_sol": all_free_unique},
        "single_unlock_unique_sol": {"n": len(alone), "unique_sol": alone_unique},
        "rows": rows,
    }
    OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps({k: out[k] for k in out if k != "rows"}, indent=2), flush=True)


if __name__ == "__main__":
    main()
