using System.Collections.Generic;

using JTH.Scripts.Domain.Placement;

using UnityEngine;



namespace JTH.Scripts.Domain.Clear

{

    /// <summary>

    /// 최내곽 테두리 파괴 → 바깥 칸 재배치를 Domain에서 연쇄 끝까지 선확정한다.

    /// 이젝터는 ring &gt; half만. 목표는 폭발로 비워진 안쪽(ring ≤ half)도 허용한다.

    /// </summary>

    public sealed class ClearReassemblyService

    {

        private const int MaxWaves = 64;



        public ClearReassemblyResult ResolveAllWaves(BoardSession session, float corridorHalfWidth)

        {

            var waves = new List<ClearWave>();



            for (int waveIndex = 0; waveIndex < MaxWaves; waveIndex++)

            {

                if (!SquareClearDetector.TryDetectInnermost(session.Grid, out ClearedSquareInfo square))

                {

                    break;

                }



                int half = (square.SquareSize - 1) / 2;

                List<OccupiedCell> ejectors = CollectEjectors(session, half);

                CellRelocationOrder.Sort(ejectors);



                IReadOnlyList<int> destroyedIds = session.RemoveCellsAt(square.ClearedCells);



                for (int i = 0; i < ejectors.Count; i++)

                {

                    session.ReleaseOccupancy(ejectors[i].CellId);

                }



                var relocations = new List<CellRelocation>(ejectors.Count);



                for (int i = 0; i < ejectors.Count; i++)

                {

                    OccupiedCell ejector = ejectors[i];

                    if (!session.TryGetCell(ejector.CellId, out OccupiedCell live))

                    {

                        continue;

                    }



                    Vector2Int from = live.Position;



                    // 안전장치: 보존 구역 잔존 칸은 절대 재배치하지 않음

                    if (CellRelocationOrder.Chebyshev(from) < half)

                    {

                        session.MoveCell(live.CellId, from);

                        continue;

                    }



                    if (!CellRelocationTargetFinder.TryFind(

                            session.Grid,

                            from,

                            corridorHalfWidth,

                            out Vector2Int to)

                        || from == to)

                    {

                        session.MoveCell(live.CellId, from);

                        continue;

                    }



                    session.MoveCell(live.CellId, to);

                    relocations.Add(new CellRelocation(live.CellId, from, to));

                }



                waves.Add(new ClearWave(

                    square.SquareSize,

                    square.ClearedCells,

                    destroyedIds,

                    relocations));

            }



            bool outside = BlockPlacementCells.HasAnyCellOutsideBounds(session.Grid);

            // wave가 없어도 경계 밖 칸이 있으면 플래그를 살려야 GameOver 분기가 탄다.
            if (waves.Count == 0 && !outside)
            {
                return ClearReassemblyResult.None;
            }

            return new ClearReassemblyResult(waves, outside);

        }



        /// <summary>폭발 N의 바깥(chebyshev &gt; half)만 이젝터. 테두리·내부 잔존은 제외.</summary>

        private static List<OccupiedCell> CollectEjectors(BoardSession session, int half)

        {

            var ejectors = new List<OccupiedCell>();

            IReadOnlyList<OccupiedCell> cells = session.Cells;

            for (int i = 0; i < cells.Count; i++)

            {

                OccupiedCell cell = cells[i];

                int ring = CellRelocationOrder.Chebyshev(cell.Position);

                if (ring > half)

                {

                    ejectors.Add(cell);

                }

            }



            return ejectors;

        }

    }

}


