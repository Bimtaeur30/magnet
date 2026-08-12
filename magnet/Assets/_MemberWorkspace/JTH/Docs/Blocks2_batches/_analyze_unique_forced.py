"""How often Unique hands have forced 1st/2nd moves under several definitions."""
from __future__ import annotations

import json
from collections import Counter
from pathlib import Path

import numpy as np

import _analyze_unique_pattern as ap

OUT = Path(__file__).resolve().parent / "unique_forced_move_analysis.json"


def placement_count(board: np.ndarray, piece: list[tuple[int, int]]) -> int:
    return len(ap.placements(board, piece))


def unlock_paths(
    board: np.ndarray,
    a: list[tuple[int, int]],
    b: list[tuple[int, int]],
    blocked: list[tuple[int, int]],
) -> list[tuple[str, tuple[int, int], tuple[int, int], int, int]]:
    """All (order, p1, p2, c1, c2) that unlock blocked with clear>=1."""
    paths = []
    for order, first, second in (("ab", a, b), ("ba", b, a)):
        for p1 in ap.placements(board, first):
            after1, c1 = ap.place_and_clear(board, first, *p1)
            for p2 in ap.placements(after1, second):
                after2, c2 = ap.place_and_clear(after1, second, *p2)
                if c1 + c2 < 1:
                    continue
                if ap.can_place_anywhere(after2, blocked):
                    paths.append((order, p1, p2, c1, c2))
    return paths


def analyze_obtf(board: np.ndarray, ids: list[int], placeable: list[bool]) -> dict | None:
    blocked_idxs = [i for i, ok in enumerate(placeable) if not ok]
    free_idxs = [i for i, ok in enumerate(placeable) if ok]
    if len(blocked_idxs) != 1 or len(free_idxs) != 2:
        return None
    bi = blocked_idxs[0]
    u0, u1 = free_idxs
    blocked = ap.offsets(ids[bi])
    a = ap.offsets(ids[u0])
    b = ap.offsets(ids[u1])

    legal0 = placement_count(board, a)
    legal1 = placement_count(board, b)
    paths = unlock_paths(board, a, b, blocked)
    orders = {p[0] for p in paths}
    # Distinct first placements that appear in some unlock path
    first_ab = {p[1] for p in paths if p[0] == "ab"}
    first_ba = {p[1] for p in paths if p[0] == "ba"}
    # After a specific forced-first sense: for each order, is first move unique among unlocking paths?
    forced_first_ab = len(first_ab) == 1
    forced_first_ba = len(first_ba) == 1
    # Either order has uniquely forced first placement among unlocking paths
    any_order_forced_first = (forced_first_ab and "ab" in orders) or (forced_first_ba and "ba" in orders)

    alone0 = ap.alone_unlocks(board, a, blocked)
    alone1 = ap.alone_unlocks(board, b, blocked)

    return {
        "blocked_id": ids[bi],
        "unlock_ids": [ids[u0], ids[u1]],
        "legal_placements": [legal0, legal1],
        "legal_forced_both": legal0 == 1 and legal1 == 1,
        "legal_forced_either": legal0 == 1 or legal1 == 1,
        "unlock_path_count": len(paths),
        "unlock_orders": sorted(orders),
        "order_count": len(orders),
        "forced_order": len(orders) == 1,
        "forced_first_in_some_order": any_order_forced_first,
        "first_choices_ab": len(first_ab),
        "first_choices_ba": len(first_ba),
        "alone_unlock": alone0 or alone1,
        "both_unlock": len(paths) > 0,
        "user_strong_A": (not alone0) and (not alone1) and len(paths) > 0,
    }


def main() -> None:
    prev = json.loads(ap.OUT.read_text(encoding="utf-8"))
    rows = []
    for h in prev["hands"]:
        if "ids" not in h:
            continue
        board = ap.extract_board(ap.SRC / h["file"])
        ids = h["ids"]
        placeable = [ap.can_place_anywhere(board, ap.offsets(i)) for i in ids]
        info = {
            "file": h["file"],
            "ids": ids,
            "pattern": h.get("pattern"),
            "user_match": bool(h.get("matches_user_rule")),
        }
        obtf = analyze_obtf(board, ids, placeable)
        if obtf:
            info.update(obtf)
            info["bucket"] = (
                "match_A"
                if obtf["user_strong_A"]
                else ("single_unlock" if obtf["alone_unlock"] else ("both_ok" if obtf["both_unlock"] else "no_unlock"))
            )
        else:
            info["bucket"] = h.get("pattern")
        rows.append(info)

    obtf_rows = [r for r in rows if "legal_placements" in r]
    summary = {
        "obtf_n": len(obtf_rows),
        "legal_forced_both_pieces": sum(1 for r in obtf_rows if r["legal_forced_both"]),
        "legal_forced_either_piece": sum(1 for r in obtf_rows if r["legal_forced_either"]),
        "forced_order_only_one_order_unlocks": sum(1 for r in obtf_rows if r["forced_order"]),
        "forced_first_placement_in_some_order": sum(1 for r in obtf_rows if r["forced_first_in_some_order"]),
        "by_bucket": {},
    }
    for bucket in ("match_A", "single_unlock", "both_ok", "no_unlock"):
        sub = [r for r in obtf_rows if r.get("bucket") == bucket]
        if not sub:
            continue
        summary["by_bucket"][bucket] = {
            "n": len(sub),
            "legal_forced_both": sum(1 for r in sub if r["legal_forced_both"]),
            "legal_forced_either": sum(1 for r in sub if r["legal_forced_either"]),
            "forced_order": sum(1 for r in sub if r["forced_order"]),
            "forced_first_some_order": sum(1 for r in sub if r["forced_first_in_some_order"]),
            "avg_legal0": round(sum(r["legal_placements"][0] for r in sub) / len(sub), 2),
            "avg_legal1": round(sum(r["legal_placements"][1] for r in sub) / len(sub), 2),
            "avg_unlock_paths": round(sum(r["unlock_path_count"] for r in sub) / len(sub), 2),
        }

    # Intersection: if we REPLACE alone-forbid with forced-1st-2nd (legal==1 both)
    replace_with_legal_forced = sum(
        1 for r in obtf_rows if r["both_unlock"] and r["legal_forced_both"]
    )
    # Soft: both unlock + forced order
    soft_forced_order = sum(1 for r in obtf_rows if r["both_unlock"] and r["forced_order"])
    # both unlock + forced first in the unlocking order(s)
    soft_forced_first = sum(1 for r in obtf_rows if r["both_unlock"] and r["forced_first_in_some_order"])

    summary["if_require_legal_forced_both_plus_both_unlock"] = replace_with_legal_forced
    summary["if_require_forced_order_plus_both_unlock"] = soft_forced_order
    summary["if_require_forced_first_some_order_plus_both_unlock"] = soft_forced_first
    summary["current_user_strong_A"] = sum(1 for r in obtf_rows if r["user_strong_A"])
    summary["both_unlock_only"] = sum(1 for r in obtf_rows if r["both_unlock"])

    out = {"summary": summary, "rows": rows}
    OUT.write_text(json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print(json.dumps(summary, indent=2), flush=True)


if __name__ == "__main__":
    main()
