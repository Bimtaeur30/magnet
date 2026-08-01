using JTH.Scripts.Data;
using JTH.Scripts.Domain.BlockSelection.Health;
using JTH.Scripts.Domain.Board;
using UnityEngine;

namespace JTH.Scripts.Domain.BlockSelection.Blame
{
    public sealed class BlameTracker
    {
        private readonly BlockSelectionTuningSO _tuning;

        public float Total { get; private set; }
        public float LastTurnDelta { get; private set; }

        public BlameTracker(BlockSelectionTuningSO tuning)
        {
            _tuning = tuning;
        }

        public TurnFeedback OnTurnEnded(
            BoardGrid boardBefore,
            BoardGrid boardAfter,
            BoardHealthResult healthBefore,
            BoardHealthResult healthAfter,
            bool allPiecesPlaced)
        {
            float delta = 0f;

            int newDeadZones = healthAfter.DeadZoneCount - healthBefore.DeadZoneCount;
            if (newDeadZones > 0)
            {
                delta += newDeadZones * _tuning.BlamePerDeadZone;
            }
            else
            {
                newDeadZones = 0;
            }

            int centerCellsGained = CountCenterCellsGained(boardBefore, boardAfter);
            delta += centerCellsGained * _tuning.BlamePerCenterCell;

            bool bigSlotLost = healthAfter.BigPieceSlots < healthBefore.BigPieceSlots;
            if (bigSlotLost)
            {
                delta += _tuning.BlamePerBigSlotLost;
            }

            float freedomDrop = healthBefore.PlacementFreedom - healthAfter.PlacementFreedom;
            if (freedomDrop > 0f)
            {
                delta += freedomDrop * _tuning.BlamePerFreedomDrop;
            }
            else
            {
                freedomDrop = 0f;
            }

            float decayLoss = Total * (1f - _tuning.BlameDecayRate);

            LastTurnDelta = delta;
            Total = Total * _tuning.BlameDecayRate + delta;

            bool isGoodTurn = allPiecesPlaced && delta <= _tuning.GoodTurnBlameDeltaMax;
            return new TurnFeedback(
                isGoodTurn, delta, Total, newDeadZones, centerCellsGained, bigSlotLost, freedomDrop, decayLoss);
        }

        public void Reset()
        {
            Total = 0f;
            LastTurnDelta = 0f;
        }

        private static int CountCenterCellsGained(BoardGrid before, BoardGrid after)
        {
            int min = after.BoardSize / 2 - 1;
            int max = after.BoardSize / 2;
            int gained = 0;
            Vector2Int cell = Vector2Int.zero;

            for (int x = min; x <= max; ++x)
            {
                for (int y = min; y <= max; ++y)
                {
                    cell.x = x;
                    cell.y = y;

                    if (after.IsOccupied(cell) && !before.IsOccupied(cell))
                    {
                        ++gained;
                    }
                }
            }

            return gained;
        }
    }
}
