using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JTH.Scripts.Data;
using JTH.Scripts.Domain;
using JTH.Scripts.Domain.Clear;
using LitMotion;
using UnityEngine;

namespace JTH.Scripts.Presentation
{
    /// <summary>
    /// 보드에 붙은 칸 1개의 View. 재배치·회전 연출을 담당한다.
    /// </summary>
    public sealed class OccupiedCellView : MonoBehaviour
    {
        private Block _block;
        private Vector2Int _gridPosition;
        private readonly List<MotionHandle> _motions = new();
        private BoardView _boardView;

        public Vector2Int GridPosition => _gridPosition;

        private BoardView BoardView
        {
            get
            {
                if (_boardView == null)
                {
                    _boardView = GetComponentInParent<BoardView>();
                }

                return _boardView;
            }
        }

        public void Bind(Block block, Vector2Int gridPosition, float cellSize, float fill)
        {
            _block = block;
            _gridPosition = gridPosition;
            if (_block != null)
            {
                _block.transform.SetParent(transform, worldPositionStays: true);
            }

            SnapToGrid(gridPosition, cellSize, fill);
            transform.localEulerAngles = Vector3.zero;
        }

        public void SnapToGrid(Vector2Int gridPosition, float cellSize, float fill)
        {
            _gridPosition = gridPosition;
            Vector2 boardLocal = BoardCoordinates.GridToWorld(gridPosition.x, gridPosition.y, cellSize);
            SetBoardLocalPosition(boardLocal);
            if (_block != null)
            {
                _block.SetLocalPosition(Vector3.zero);
                _block.SetLocalScale(new Vector3(cellSize * fill, cellSize * fill, 1f));
                _block.SetSortingOrder(0);
                _block.SetActive(true);
            }
        }

        public void AnimateMoveTo(
            Vector2Int gridPosition,
            float cellSize,
            float fill,
            float duration,
            Ease ease,
            Action onComplete)
        {
            CancelMotions();
            Vector2 targetBoardLocal = BoardCoordinates.GridToWorld(gridPosition.x, gridPosition.y, cellSize);
            Vector2 startBoardLocal = GetBoardLocalPosition();
            _gridPosition = gridPosition;

            MotionHandle handle = LMotion.Create(0f, 1f, duration)
                .WithEase(ease)
                .WithOnComplete(() =>
                {
                    SnapToGrid(gridPosition, cellSize, fill);
                    onComplete?.Invoke();
                })
                .Bind(t => SetBoardLocalPosition(Vector2.Lerp(startBoardLocal, targetBoardLocal, t)));
            _motions.Add(handle);
        }

        /// <summary>
        /// 링 스태거 딜레이 후 튕김 → 목표 칸에 바로 착지. 공전 1바퀴 없음.
        /// </summary>
        public async UniTask PlayRelocationAsync(
            CellRelocation relocation,
            BoardConfigSO boardConfig,
            PlacementConfigSO placementConfig,
            float delaySeconds)
        {
            if (delaySeconds > 0f)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
            }

            CancelMotions();

            float cellSize = boardConfig.CellSize;
            float fill = placementConfig.CellFill;
            Vector2 fromBoardLocal = BoardCoordinates.GridToWorld(relocation.From.x, relocation.From.y, cellSize);
            Vector2 toBoardLocal = BoardCoordinates.GridToWorld(relocation.To.x, relocation.To.y, cellSize);

            Vector2 radial = fromBoardLocal.sqrMagnitude > 0.0001f ? fromBoardLocal.normalized : Vector2.up;
            Vector2 bounceEnd = fromBoardLocal + radial * (placementConfig.BounceCells * cellSize);

            await TweenPositionWithSpin(
                GetBoardLocalPosition(),
                bounceEnd,
                placementConfig.BounceDuration,
                placementConfig.BounceEase,
                placementConfig.SpinDegreesPerSecond);

            Vector2 landStart = GetBoardLocalPosition();
            Vector2 landEnd = toBoardLocal;
            float landDuration = placementConfig.LandDuration;
            float spinSpeed = placementConfig.SpinDegreesPerSecond;

            var landCompletion = new UniTaskCompletionSource();
            float spinZ = transform.localEulerAngles.z;
            MotionHandle landHandle = LMotion.Create(0f, 1f, landDuration)
                .WithEase(placementConfig.LandEase)
                .WithOnComplete(() => landCompletion.TrySetResult())
                .Bind(t =>
                {
                    SetBoardLocalPosition(Vector2.Lerp(landStart, landEnd, t));
                    spinZ += spinSpeed * Time.deltaTime;
                    transform.localEulerAngles = new Vector3(0f, 0f, spinZ);
                });
            _motions.Add(landHandle);
            await landCompletion.Task;

            SnapToGrid(relocation.To, cellSize, fill);
            transform.localEulerAngles = Vector3.zero;
        }

        private async UniTask TweenPositionWithSpin(
            Vector2 startBoardLocal,
            Vector2 endBoardLocal,
            float duration,
            Ease ease,
            float spinDegreesPerSecond)
        {
            var completion = new UniTaskCompletionSource();
            float spinZ = transform.localEulerAngles.z;
            MotionHandle handle = LMotion.Create(0f, 1f, duration)
                .WithEase(ease)
                .WithOnComplete(() => completion.TrySetResult())
                .Bind(t =>
                {
                    SetBoardLocalPosition(Vector2.Lerp(startBoardLocal, endBoardLocal, t));
                    spinZ += spinDegreesPerSecond * Time.deltaTime;
                    transform.localEulerAngles = new Vector3(0f, 0f, spinZ);
                });
            _motions.Add(handle);
            await completion.Task;
        }

        private Vector2 GetBoardLocalPosition()
        {
            BoardView boardView = BoardView;
            if (boardView == null)
            {
                Vector3 local = transform.localPosition;
                return new Vector2(local.x, local.y);
            }

            return boardView.WorldToBoardLocal(transform.position);
        }

        private void SetBoardLocalPosition(Vector2 boardLocal)
        {
            BoardView boardView = BoardView;
            if (boardView == null)
            {
                transform.localPosition = new Vector3(boardLocal.x, boardLocal.y, transform.localPosition.z);
                return;
            }

            boardView.SetAtBoardLocal(transform, boardLocal);
        }

        public void CancelMotions()
        {
            for (int i = 0; i < _motions.Count; i++)
            {
                if (_motions[i].IsActive())
                {
                    _motions[i].Cancel();
                }
            }

            _motions.Clear();
        }

        private void OnDestroy()
        {
            CancelMotions();
        }
    }
}
