using System.Collections.Generic;
using _Shared.Magnet.Core.Events;
using GameLib.EventChannelSystem;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Domain.Clear;
using JTH.Scripts.Domain.Placement;
using JTH.Scripts.Events;
using Magnet.Core.Events;
using Magnet.Core.SO.Skin;
using Reflex.Attributes;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    public sealed class LineClearExplosionPresenter : MonoBehaviour
    {
        [SerializeField] private EventChannelSO inGameChannel;
        [SerializeField] private EventChannelSO presentationChannel;
        [SerializeField] private EventChannelSO skinChannel;

        [Inject] private readonly GameBoard _gameBoard;

        private SkinDataSO _currentSkin;

        private void Awake()
        {
            Debug.Assert(inGameChannel != null, "[LineClearExplosionPresenter] inGameChannel is not assigned.", this);
            Debug.Assert(presentationChannel != null, "[LineClearExplosionPresenter] presentationChannel is not assigned.", this);
            Debug.Assert(skinChannel != null, "[LineClearExplosionPresenter] skinChannel is not assigned.", this);
            Debug.Assert(_gameBoard != null, "[LineClearExplosionPresenter] GameBoard was not injected.", this);
        }

        private void OnEnable()
        {
            inGameChannel.AddListener<BlockPlacedEvent>(OnBlockPlaced);
            skinChannel.AddListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.AddListener<SkinInitializedEvent>(OnSkinInitialized);
        }

        private void OnDisable()
        {
            inGameChannel.RemoveListener<BlockPlacedEvent>(OnBlockPlaced);
            skinChannel.RemoveListener<SkinChangedEvent>(OnSkinChanged);
            skinChannel.RemoveListener<SkinInitializedEvent>(OnSkinInitialized);
        }

        private void OnSkinChanged(SkinChangedEvent evt)
        {
            _currentSkin = evt.CurrentSkin;
        }

        private void OnSkinInitialized(SkinInitializedEvent evt)
        {
            _currentSkin = evt.Skin;
        }

        private void OnBlockPlaced(BlockPlacedEvent evt)
        {
            PlacementResult result = evt.PlacementResult;
            if (result == null || _currentSkin == null || result.ClearedLineResult.ClearedLineCount <= 0)
            {
                return;
            }

            if (_currentSkin.FireCenteredLineClear)
            {
                PlayCenteredEffects(result.ClearedLineResult);
            }
        }

        private void PlayCenteredEffects(ClearedLineResult cleared)
        {
            PoolItemSO effect = _currentSkin.CenterLineClearEffect;
            if (effect == null)
            {
                return;
            }

            int boardSize = _gameBoard.Grid.BoardSize;
            for (int i = 0; i < cleared.ClearedLines.Count; ++i)
            {
                Line line = cleared.ClearedLines[i];
                Vector3 center = ResolveLineCenter(line, boardSize);
                Quaternion rotation = line.Orientation == Line.Axis.Row
                    ? Quaternion.identity
                    : Quaternion.Euler(0f, 0f, 90f);
                PlayEffect(effect, center, rotation);
            }
        }

        private Vector3 ResolveLineCenter(Line line, int boardSize)
        {
            List<Vector2Int> cells = line.GetCells(boardSize);
            Vector3 first = _gameBoard.GridToWorldCenter(cells[0]);
            Vector3 last = _gameBoard.GridToWorldCenter(cells[cells.Count - 1]);
            return (first + last) * 0.5f;
        }

        private void PlayEffect(PoolItemSO effect, Vector3 position, Quaternion rotation)
        {
            presentationChannel.RaiseEvent(
                PresentationEvents.PlayParticleEffectEvent.Init(effect, position, rotation));
        }
    }
}
