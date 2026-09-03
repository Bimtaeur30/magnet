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
        [Tooltip("새 게임 시작 보드 프리필 설정. 비우거나 Enabled=false면 빈 보드로 시작")]
        [SerializeField] private BoardPrefillConfigSO boardPrefillConfigSO;

        [Tooltip("퍼펙트 판정용 핸드 최적 탐색을 돌릴 보드 점유율 하한(0~1). 이 값 미만이면 탐색을 건너뛴다 " +
                 "— 빈 보드일수록 탐색 공간이 커 프레임 히칭 위험. 0이면 항상 탐색")]
        [SerializeField, Range(0f, 1f)] private float perfectSolveMinOccupancy = 0.4f;

        [Inject] private GameBoard _gameBoard;

        private BlockSupply _supply;
        private SkinSession _skinSession;
        private AreaBundleDrawer _drawer;
        private int _turnIndex;
        private int _handStartScore;
        private BoardGrid _handStartBoard;
        private readonly List<PlayerHandMove> _playerMoves = new(BlockSupply.SlotCount);

        private HandOptimalResult _handOptimal = HandOptimalResult.Unsolved;
        private int _handClearedLines;

        /// <summary>
        /// 직전 핸드(3피스)를 마지막 배치까지 끝냈을 때, 누적 클리어 라인 수가 그 핸드의 최적값과 같았는지.
        /// 마지막 배치 시점에만 갱신된다.
        /// </summary>
        public bool LastHandWasPerfect { get; private set; }

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
            _skinSession = new SkinSession(skinDataListSO);
            _supply = new BlockSupply(_drawer, _skinSession);
        }

        private void Start()
        {
            PrefillBoard();
            Fill();
        }

        /// <summary>
        /// 새 게임 시작 보드를 미리 채운다. 첫 손 3개는 이 보드를 기준으로 뽑히므로 Fill() 전에 돌아야 한다.
        /// 이어하기가 구현되면 "새 게임일 때만" 조건을 여기에 건다.
        /// </summary>
        private void PrefillBoard()
        {
            if (boardPrefillConfigSO == null || !boardPrefillConfigSO.Enabled)
            {
                return;
            }

            BoardGrid grid = _gameBoard != null ? _gameBoard.Grid : null;
            if (grid == null)
            {
                return;
            }

            System.Random rng = boardPrefillConfigSO.Seed >= 0
                ? new System.Random(boardPrefillConfigSO.Seed)
                : new System.Random();

            List<Vector2Int> cells = BoardPrefillGenerator.Generate(
                grid.BoardSize,
                boardPrefillConfigSO,
                areaBundlePoolSO != null ? areaBundlePoolSO.NormalBundles : null,
                rng);

            if (cells.Count == 0)
            {
                return;
            }

            int variantCount = _skinSession != null ? _skinSession.MaxVariant : 1;
            List<int> skinIds = new List<int>(cells.Count);
            for (int i = 0; i < cells.Count; ++i)
            {
                skinIds.Add(rng.Next(variantCount));
            }

            _gameBoard.PrefillCells(cells, skinIds);
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
        /// 플레이어 한 수 기록. 손 3개 모두 두면 추천 Explain과 Area를 비교 출력하고,
        /// 누적 클리어 수를 핸드 최적값과 비교해 <see cref="LastHandWasPerfect"/>를 갱신한다.
        /// </summary>
        public void RecordPlayerMove(
            int slotIndex,
            IReadOnlyList<Vector2Int> cells,
            bool lastDrop,
            int clearedLineCount)
        {
            Vector2Int[] copy = new Vector2Int[cells.Count];
            for (int i = 0; i < cells.Count; ++i)
            {
                copy[i] = cells[i];
            }

            _playerMoves.Add(new PlayerHandMove(slotIndex, copy));
            _handClearedLines += clearedLineCount;
            RaiseUniqueCorrectPlacementIfMatched(slotIndex, copy);
            if (!lastDrop)
            {
                return;
            }

            LastHandWasPerfect = _handOptimal.IsValid && _handClearedLines == _handOptimal.MaxClearedLines;
            LogHandCompare();
            _playerMoves.Clear();
        }

        private void RaiseUniqueCorrectPlacementIfMatched(int slotIndex, IReadOnlyList<Vector2Int> cells)
        {
            AreaBundleSelectionResult selection = LastSelection;
            if (selection == null || !selection.IsUniqueCorrectPlacement(slotIndex, cells))
            {
                return;
            }

            Vector3[] worldPositions = new Vector3[cells.Count];
            for (int i = 0; i < cells.Count; ++i)
            {
                worldPositions[i] = _gameBoard.GridToWorldCenter(cells[i]);
            }

            magnetGameChannel.RaiseEvent(
                MagnetGameEvents.UniqueCorrectPlacementEvent.Init(worldPositions));
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

        public void Fill(int currentScore = 0)
        {
            _playerMoves.Clear();

            BoardGrid grid = _gameBoard.Grid;
            _handStartBoard = grid?.Clone();
            _handStartScore = currentScore;
            BlockSpawnContext context = new(shapeSourceSO, grid, currentScore)
            {
                TurnIndex = _turnIndex,
                IsRetrySession = false,
            };
            ++_turnIndex;

            _supply.Fill(context);
            SolveHandOptimal();
            LogDeal();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        public IReadOnlyList<IReadOnlyList<Vector2Int>> DrawEasy(int currentScore)
        {
            BoardGrid grid = _gameBoard.Grid;
            BlockSpawnContext context = new(shapeSourceSO, grid, currentScore)
            {
                TurnIndex = _turnIndex,
                IsRetrySession = true,
            };
            List<IReadOnlyList<Vector2Int>> pieces = _drawer.DrawEasy(context);
            return CopyPieces(pieces);
        }

        public void FillPrepared(IReadOnlyList<IReadOnlyList<Vector2Int>> cellOffsetsList, int currentScore)
        {
            _playerMoves.Clear();
            BoardGrid grid = _gameBoard.Grid;
            _handStartBoard = grid?.Clone();
            _handStartScore = currentScore;
            _supply.FillFrom(cellOffsetsList);
            SolveHandOptimal();
            LogDeal();
            magnetGameChannel.RaiseEvent(MagnetGameEvents.BlockCandidatesUpdatedEvent.Init(_supply.Candidates));
        }

        /// <summary>
        /// 핸드가 확정된 직후 1회. 이 3피스를 전부 놓았을 때 지울 수 있는 최대 라인 수를 미리 구해둔다.
        /// 마지막 배치에서 플레이어 누적 클리어 수와 비교해 퍼펙트를 판정한다.
        /// 보드 점유율이 <see cref="perfectSolveMinOccupancy"/> 미만이면 탐색 자체를 건너뛴다
        /// (빈 보드는 합법 배치가 폭증해 완전탐색이 무거움 + 퍼펙트도 사실상 의미 없음).
        /// </summary>
        private void SolveHandOptimal()
        {
            _handClearedLines = 0;
            LastHandWasPerfect = false;
            _handOptimal = HandOptimalResult.Unsolved;

            if (_handStartBoard == null)
            {
                return;
            }

            int cellTotal = _handStartBoard.BoardSize * _handStartBoard.BoardSize;
            if (cellTotal <= 0 || _handStartBoard.CountOccupied() < cellTotal * perfectSolveMinOccupancy)
            {
                return;
            }

            _handOptimal = HandOptimalSolver.Solve(_handStartBoard, _supply.Candidates);
        }

        private static List<IReadOnlyList<Vector2Int>> CopyPieces(IReadOnlyList<IReadOnlyList<Vector2Int>> source)
        {
            List<IReadOnlyList<Vector2Int>> copy = new List<IReadOnlyList<Vector2Int>>(source.Count);
            for (int i = 0; i < source.Count; ++i)
            {
                IReadOnlyList<Vector2Int> piece = source[i];
                Vector2Int[] cells = new Vector2Int[piece.Count];
                for (int j = 0; j < piece.Count; ++j)
                {
                    cells[j] = piece[j];
                }

                copy.Add(cells);
            }

            return copy;
        }

        private void LogDeal()
        {
            AreaBundleSelectionResult result = LastSelection;
            if (result == null)
            {
                return;
            }

            (string label, string color) = ResolveDealStyle(result);
            Debug.Log($"<color={color}><b>[AreaBundle] {label}</b>"
                + $" turn={_turnIndex - 1}"
                + $" heat={result.HeatScore:F0}"
                + $" bundle={result.BundleId}"
                + $" blocks=[{string.Join(",", result.BlockIds)}]</color>");
        }

        private static (string label, string color) ResolveDealStyle(AreaBundleSelectionResult result)
        {
            if (result.IsKillHand)
            {
                return result.Tier == AreaBundleTier.Easy
                    ? ("Easy-랜덤", "#FFAB40")
                    : ("Kill", "#FFAB40");
            }

            if (result.Reason != null && result.Reason.Contains("AllClear"))
            {
                return ("올클리어", "#FFD54F");
            }

            return result.Tier switch
            {
                AreaBundleTier.Unique => ("유일수", "#B388FF"),
                AreaBundleTier.Easy => ("Easy", "#4FC3F7"),
                AreaBundleTier.Normal => ("Normal", "#66BB6A"),
                _ => (result.Tier.ToString(), "#A5D6A7"),
            };
        }

        private void LogHandCompare()
        {
            AreaBundleSelectionResult selection = LastSelection;
            IReadOnlyList<AreaBundleExplainStep> recommend =
                selection?.ExplainSteps ?? System.Array.Empty<AreaBundleExplainStep>();

            float emptyPenalty = areaBundlePoolSO != null
                ? areaBundlePoolSO.ResolveEmptyHeatPenalty(_handStartScore)
                : 2f;
            bool recOk = TryScoreHeatPath(_handStartBoard, recommend, emptyPenalty, out float recHeat);
            bool actOk = TryScorePlayerHeatPath(_handStartBoard, _playerMoves, emptyPenalty, out float actHeat);

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

            sb.Append("  heat rec=").Append(recOk ? FormatHeat(recHeat) : "FAIL")
                .Append(" act=").Append(actOk ? FormatHeat(actHeat) : "FAIL");
            if (recOk && actOk)
            {
                float delta = actHeat - recHeat;
                string vs = delta > 0.01f
                    ? "HIGHER"
                    : delta < -0.01f
                        ? "LOWER"
                        : "SAME";
                sb.Append(" delta=").Append(delta.ToString("+0;-0;0"))
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
                if (actHeat > recHeat + 0.01f)
                {
                    color = "#69F0AE";
                }
                else if (Mathf.Abs(actHeat - recHeat) <= 0.01f)
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

        private static bool TryScoreHeatPath(
            BoardGrid start,
            IReadOnlyList<AreaBundleExplainStep> steps,
            float emptyPenalty,
            out float heat)
        {
            heat = float.NaN;
            if (start == null)
            {
                return false;
            }

            BoardGrid sim = start.Clone();
            float total = 0f;
            if (steps != null)
            {
                for (int i = 0; i < steps.Count; ++i)
                {
                    if (!TryApplyCellsWithHeat(sim, steps[i].Cells, emptyPenalty, out float gain))
                    {
                        return false;
                    }

                    total += gain;
                }
            }

            heat = total;
            return true;
        }

        private static bool TryScorePlayerHeatPath(
            BoardGrid start,
            List<PlayerHandMove> moves,
            float emptyPenalty,
            out float heat)
        {
            heat = float.NaN;
            if (start == null)
            {
                return false;
            }

            BoardGrid sim = start.Clone();
            float total = 0f;
            if (moves != null)
            {
                for (int i = 0; i < moves.Count; ++i)
                {
                    if (!TryApplyCellsWithHeat(sim, moves[i].Cells, emptyPenalty, out float gain))
                    {
                        return false;
                    }

                    total += gain;
                }
            }

            heat = total;
            return true;
        }

        private static bool TryApplyCellsWithHeat(
            BoardGrid board,
            IReadOnlyList<Vector2Int> cells,
            float emptyPenalty,
            out float gain)
        {
            gain = 0f;
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

            int[,] heatMap = LineFillHeatmap.Build(board);
            gain = LineFillHeatmap.ScoreCells(heatMap, cells, emptyPenalty);

            Vector2Int pivot = cells[0];
            Vector2Int[] offsets = new Vector2Int[cells.Count];
            for (int i = 0; i < cells.Count; ++i)
            {
                offsets[i] = cells[i] - pivot;
            }

            PlacementSimulator.PlaceAndClear(board, offsets, pivot);
            return true;
        }

        private static string FormatHeat(float heat) =>
            float.IsNaN(heat) ? "NaN" : heat.ToString("0");

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
