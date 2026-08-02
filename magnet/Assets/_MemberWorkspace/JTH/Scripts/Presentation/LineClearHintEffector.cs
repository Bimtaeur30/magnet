using System.Collections.Generic;
using JTH.Scripts.Data;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 클리어 예정 줄의 Place된 칸만 알파로 깜빡인다. 프리뷰는 건드리지 않는다.
    /// </summary>
    public sealed class LineClearHintEffector : MonoBehaviour
    {
        [SerializeField] private PlacedBlocksView placedBlocksView;

        private readonly HashSet<Block> _hintedBlocks = new HashSet<Block>();
        private readonly HashSet<Block> _desiredPlaced = new HashSet<Block>();
        private readonly List<Block> _removeBuffer = new List<Block>(32);

        private void Awake()
        {
            if (placedBlocksView == null)
            {
                placedBlocksView = GetComponent<PlacedBlocksView>();
            }

            Debug.Assert(placedBlocksView != null, "[LineClearHintEffector] PlacedBlocksView is missing.", this);
        }

        public void SetHints(IReadOnlyCollection<Vector2Int> clearedCells, LineClearPreviewConfigSO config)
        {
            if (clearedCells == null || clearedCells.Count == 0 || config == null)
            {
                ClearHints();
                return;
            }

            _desiredPlaced.Clear();
            foreach (Vector2Int cell in clearedCells)
            {
                if (placedBlocksView.TryGetBlock(cell, out Block placed))
                {
                    _desiredPlaced.Add(placed);
                }
            }

            SyncSet(
                _hintedBlocks,
                _desiredPlaced,
                enable: block => block.SetClearHint(
                    true,
                    1f,
                    1f,
                    config.PulseMinAlpha,
                    1f,
                    config.PulsePeriod));
        }

        public void ClearHints()
        {
            foreach (Block block in _hintedBlocks)
            {
                if (block != null)
                {
                    block.SetClearHint(false);
                }
            }

            _hintedBlocks.Clear();
            _desiredPlaced.Clear();
        }

        private void SyncSet(HashSet<Block> current, HashSet<Block> desired, System.Action<Block> enable)
        {
            _removeBuffer.Clear();
            foreach (Block hinted in current)
            {
                if (hinted == null || !desired.Contains(hinted))
                {
                    _removeBuffer.Add(hinted);
                }
            }

            for (int i = 0; i < _removeBuffer.Count; ++i)
            {
                Block block = _removeBuffer[i];
                if (block != null)
                {
                    block.SetClearHint(false);
                }

                current.Remove(block);
            }

            foreach (Block block in desired)
            {
                if (current.Contains(block))
                {
                    continue;
                }

                enable(block);
                current.Add(block);
            }
        }
    }
}
