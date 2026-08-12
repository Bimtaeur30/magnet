using System.Collections.Generic;
using System.Text;
using GameLib.EventChannelSystem;
using JTH.Scripts.Data;
using JTH.Scripts.Domain.AreaBundleSpawn;
using JTH.Scripts.Domain.BlockSelection.Simulation;
using JTH.Scripts.Domain.Board;
using JTH.Scripts.Domain.Skin;
using JTH.Scripts.Domain.Spawn;
using JTH.Scripts.Events;
using JTH.Scripts.Input;
using JTH.Scripts.Presentation;
using Magnet.Contracts;
using Magnet.Core.Events;
using Magnet.Core.SO.Block;
using Magnet.Core.SO.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Bootstrap
{
    public sealed class BlockSpawnBootstrap : MonoBehaviour
    {
        [SerializeField] private EventChannelSO magnetGameChannel;
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private MagnetInputSO inputSO;
        [SerializeField] private SkinDataListSO skinDataListSO;
        [SerializeField] private BlockShapeSourceSO shapeSourceSO;
        [SerializeField] private AreaBundlePoolSO areaBundlePoolSO;

        [Inject] private GameBoard _gameBoard;

        private BlockSupply _supply;
        private AreaBundleDrawer _drawer;
        private int _turnIndex;
        private BoardGrid _handStartBoard;
        private readonly List<PlayerHandMove> _playerMoves = new(BlockSupply.SlotCount);

        public IReadOnlyList<ShapeBlockData> Candidates => _supply.Candidates;

        public AreaBundleSelectionResult LastSelection => _drawer?.LastResult;

        private void Awake()
        {
            Debug.Assert(magnetGameChannel != null, "[BlockSpawnBootstrap] magnetGameChannel is not assigned.", this);
            Debug.Assert(inputSO != null, "[BlockSpawnBootstrap] inputSO is not assigned.", this);
            Debug.Assert(areaBundlePoolSO != null, "[BlockSpawnBootstrap] areaBundlePoolSO is not assigned.", this);

            #if UNITY_EDITOR
            inputSO.OnSlotSelected += OnBlockSelected;
            #endif
            magnetGameChannel.AddListener<BlockSelectedOnUIEvent>(OnBlockSelected);

            _drawer = new AreaBundleDrawer(new AreaBundleOrchestrator(areaBundlePoolSO));
            _supply = new BlockSupply(_drawer, new SkinSession(skinDataListSO));
        }

        private void Start()
        {
            Fill();
        }

        private void OnDestroy()
        {
            #if UNITY_EDITOR
            inputSO.OnSlotSelected -= OnBlockSelected;
            #endif
            magnetGameChannel.RemoveListener<BlockSelectedOnUIEvent>(OnBlockSelected);
        }

        public void Consume(int slotIndex)
        {
            _supply.Consume(slotIndex);
            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        /// <summary>
        /// 플레이어 한 수 기록. 손 3개 모두 두면 추천 Explain과 Area를 비교 출력한다.
        /// </summary>
        public void RecordPlayerMove(int slotIndex, IReadOnlyList<Vector2Int> cells, bool lastDrop)
        {
            Vector2Int[] copy = new Vector2Int[cells.Count];
            for (int i = 0; i < cells.Count; ++i)
            {
                copy[i] = cells[i];
            }

            _playerMoves.Add(new PlayerHandMove(slotIndex, copy));
            if (!lastDrop)
            {
                return;
            }

            LogHandCompare();
            _playerMoves.Clear();
        }

        private void OnBlockSelected(BlockSelectedOnUIEvent data)
            => OnBlockSelected(data.Index);

        private void OnBlockSelected(int index)
        {
            if (index < 0 || index >= BlockSupply.SlotCount)
            {
                return;
            }

            if (_supply.Candidates == null || index >= _supply.Candidates.Count)
            {
                return;
            }

            ShapeBlockData block = _supply.Candidates[index];
            if (block == null)
            {
                return;
            }

            inGameChannel.RaiseEvent(InGameEvents.BlockSelectedEvent.Init(index, block));
        }

        public void Fill()
        {
            _playerMoves.Clear();

            BoardGrid grid = _gameBoard.Grid;
            _handStartBoard = grid?.Clone();
            BlockSpawnContext context = new(shapeSourceSO, grid, 0)
            {
                TurnIndex = _turnIndex,
                IsRetrySession = false,
            };
            ++_turnIndex;

            _supply.Fill(context);
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        private void LogHandCompare()
        {
            AreaBundleSelectionResult selection = LastSelection;
            IReadOnlyList<AreaBundleExplainStep> recommend =
                selection?.ExplainSteps ?? System.Array.Empty<AreaBundleExplainStep>();
            AreaScoreTuning tuning = areaBundlePoolSO.AreaScore;

            float startArea = _handStartBoard != null
                ? AreaScoreCalculator.ScoreTotal(_handStartBoard, tuning)
                : float.NaN;
            bool recOk = TryScoreRecommendPath(_handStartBoard, recommend, tuning, out float recArea);
            bool actOk = TryScorePlayerPath(_handStartBoard, _playerMoves, tuning, out float actArea);

            int cellMatch = 0;
            int compared = Mathf.Min(_playerMoves.Count, recommend.Count);
            bool orderMatch = compared > 0 && _playerMoves.Count == recommend.Count;
            var sb = new StringBuilder(512);

            string tier = selection != null ? selection.Tier.ToString() : "none";
            string bundle = selection != null ? selection.BundleId : "-";
            sb.Append("[AreaBundle:HandCompare] turn=").Append(_turnIndex - 1)
                .Append(" tier=").Append(tier)
                .Append(" bundle=").Append(bundle)
                .Append('\n');

            sb.Append("  area start=").Append(FormatArea(startArea))
                .Append(" rec=").Append(recOk ? FormatArea(recArea) : "FAIL")
                .Append(" act=").Append(actOk ? FormatArea(actArea) : "FAIL");
            if (recOk && actOk)
            {
                float delta = actArea - recArea;
                string vs = delta > 0.01f
                    ? "HIGHER"
                    : delta < -0.01f
                        ? "LOWER"
                        : "SAME";
                sb.Append(" delta=").Append(delta.ToString("+0.0;-0.0;0.0"))
                    .Append(" actVsRec=").Append(vs);
            }

            sb.Append('\n');

            sb.Append("  recOrder=");
            AppendRecommendSlots(sb, recommend);
            sb.Append("  actOrder=");
            AppendPlayerSlots(sb, _playerMoves);
            sb.Append('\n');

            for (int i = 0; i < compared; ++i)
            {
                AreaBundleExplainStep step = recommend[i];
                PlayerHandMove act = _playerMoves[i];
                bool sameSlot = act.SlotIndex == step.PieceSlotIndex;
                bool sameCells = SameCellSet(step.Cells, act.Cells);
                if (sameCells)
                {
                    ++cellMatch;
                }

                if (!sameSlot)
                {
                    orderMatch = false;
                }

                sb.Append("  #").Append(i + 1)
                    .Append(" rec(slot=").Append(step.PieceSlotIndex)
                    .Append(' ').Append(FormatCells(step.Cells)).Append(')')
                    .Append(" act(slot=").Append(act.SlotIndex)
                    .Append(' ').Append(FormatCells(act.Cells)).Append(')')
                    .Append(sameCells ? " CELLS_OK" : " CELLS_DIFF")
                    .Append(sameSlot ? " SLOT_OK" : " SLOT_DIFF")
                    .Append('\n');
            }

            for (int i = compared; i < _playerMoves.Count; ++i)
            {
                PlayerHandMove act = _playerMoves[i];
                sb.Append("  actExtra #").Append(i + 1)
                    .Append(" slot=").Append(act.SlotIndex)
                    .Append(' ').Append(FormatCells(act.Cells))
                    .Append('\n');
                orderMatch = false;
            }

            for (int i = compared; i < recommend.Count; ++i)
            {
                AreaBundleExplainStep step = recommend[i];
                sb.Append("  recExtra #").Append(i + 1)
                    .Append(" slot=").Append(step.PieceSlotIndex)
                    .Append(' ').Append(FormatCells(step.Cells))
                    .Append('\n');
                orderMatch = false;
            }

            bool allMatch = orderMatch
                && cellMatch == recommend.Count
                && _playerMoves.Count == recommend.Count
                && recommend.Count > 0;
            string verdict = allMatch
                ? "MATCH"
                : recommend.Count == 0
                    ? "NO_RECOMMEND"
                    : "DIFF";

            string color = "#FF8A80";
            if (recOk && actOk)
            {
                if (actArea > recArea + 0.01f)
                {
                    color = "#69F0AE";
                }
                else if (Mathf.Abs(actArea - recArea) <= 0.01f)
                {
                    color = allMatch ? "#69F0AE" : "#FFD54F";
                }
            }
            else if (allMatch)
            {
                color = "#69F0AE";
            }

            sb.Append("  result=").Append(verdict)
                .Append(" cells=").Append(cellMatch).Append('/').Append(recommend.Count)
                .Append(" orderMatch=").Append(orderMatch);

            Debug.Log($"<color={color}><b>{sb}</b></color>");
        }

        private static bool TryScoreRecommendPath(
            BoardGrid start,
            IReadOnlyList<AreaBundleExplainStep> steps,
            AreaScoreTuning tuning,
            out float area)
        {
            area = float.NaN;
            if (start == null)
            {
                return false;
            }

            if (steps == null || steps.Count == 0)
            {
                area = AreaScoreCalculator.ScoreTotal(start, tuning);
                return true;
            }

            BoardGrid sim = start.Clone();
            for (int i = 0; i < steps.Count; ++i)
            {
                if (!TryApplyCells(sim, steps[i].Cells))
                {
                    return false;
                }
            }

            area = AreaScoreCalculator.ScoreTotal(sim, tuning);
            return true;
        }

        private static bool TryScorePlayerPath(
            BoardGrid start,
            List<PlayerHandMove> moves,
            AreaScoreTuning tuning,
            out float area)
        {
            area = float.NaN;
            if (start == null)
            {
                return false;
            }

            if (moves == null || moves.Count == 0)
            {
                area = AreaScoreCalculator.ScoreTotal(start, tuning);
                return true;
            }

            BoardGrid sim = start.Clone();
            for (int i = 0; i < moves.Count; ++i)
            {
                if (!TryApplyCells(sim, moves[i].Cells))
                {
                    return false;
                }
            }

            area = AreaScoreCalculator.ScoreTotal(sim, tuning);
            return true;
        }

        private static bool TryApplyCells(BoardGrid board, IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return false;
            }

            for (int i = 0; i < cells.Count; ++i)
            {
                Vector2Int cell = cells[i];
                if (!board.IsInBounds(cell) || board.IsOccupied(cell))
                {
                    return false;
                }
            }

            Vector2Int pivot = cells[0];
            Vector2Int[] offsets = new Vector2Int[cells.Count];
            for (int i = 0; i < cells.Count; ++i)
            {
                offsets[i] = cells[i] - pivot;
            }

            PlacementSimulator.PlaceAndClear(board, offsets, pivot);
            return true;
        }

        private static string FormatArea(float area) =>
            float.IsNaN(area) ? "NaN" : area.ToString("0.0");

        private static void AppendRecommendSlots(StringBuilder sb, IReadOnlyList<AreaBundleExplainStep> steps)
        {
            sb.Append('[');
            for (int i = 0; i < steps.Count; ++i)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(steps[i].PieceSlotIndex);
            }

            sb.Append(']');
        }

        private static void AppendPlayerSlots(StringBuilder sb, List<PlayerHandMove> moves)
        {
            sb.Append('[');
            for (int i = 0; i < moves.Count; ++i)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append(moves[i].SlotIndex);
            }

            sb.Append(']');
        }

        private static bool SameCellSet(IReadOnlyList<Vector2Int> a, IReadOnlyList<Vector2Int> b)
        {
            if (a == null || b == null || a.Count != b.Count)
            {
                return false;
            }

            var set = new HashSet<Vector2Int>(a.Count);
            for (int i = 0; i < a.Count; ++i)
            {
                set.Add(a[i]);
            }

            for (int i = 0; i < b.Count; ++i)
            {
                if (!set.Contains(b[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static string FormatCells(IReadOnlyList<Vector2Int> cells)
        {
            if (cells == null || cells.Count == 0)
            {
                return "[]";
            }

            var sb = new StringBuilder(cells.Count * 8);
            sb.Append('[');
            for (int i = 0; i < cells.Count; ++i)
            {
                if (i > 0)
                {
                    sb.Append(',');
                }

                sb.Append('(').Append(cells[i].x).Append(',').Append(cells[i].y).Append(')');
            }

            sb.Append(']');
            return sb.ToString();
        }

        private readonly struct PlayerHandMove
        {
            public PlayerHandMove(int slotIndex, Vector2Int[] cells)
            {
                SlotIndex = slotIndex;
                Cells = cells;
            }

            public int SlotIndex { get; }
            public Vector2Int[] Cells { get; }
        }
    }
}
