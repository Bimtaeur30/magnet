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

        public Vector2Int GridPosition => _gridPosition;

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
            Vector2 world = BoardCoordinates.GridToWorld(gridPosition.x, gridPosition.y, cellSize);
            transform.localPosition = new Vector3(world.x, world.y, 0f);
            if (_block != null)
            {
                _block.SetLocalPosition(Vector3.zero);
                _block.SetLocalScale(new Vector3(cellSize * fill, cellSize * fill, 1f));
                _block.SetSortingOrder(0);
                _block.SetActive(true);
            }
        }

        public void AnimateMoveTo(Vector2Int gridPosition, float cellSize, float fill, float duration, Action onComplete)
        {
            CancelMotions();
            Vector2 target = BoardCoordinates.GridToWorld(gridPosition.x, gridPosition.y, cellSize);
            Vector3 start = transform.localPosition;
            Vector3 end = new Vector3(target.x, target.y, start.z);
            _gridPosition = gridPosition;

            MotionHandle handle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() =>
                {
                    SnapToGrid(gridPosition, cellSize, fill);
                    onComplete?.Invoke();
                })
                .Bind(t => transform.localPosition = Vector3.Lerp(start, end, t));
            _motions.Add(handle);
        }

        /// <summary>
        /// 시계방향 스태거 딜레이 후 튕김 → 목표 칸에 바로 착지. 공전 1바퀴 없음.
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
            Vector2 fromWorld = BoardCoordinates.GridToWorld(relocation.From.x, relocation.From.y, cellSize);
            Vector2 toWorld = BoardCoordinates.GridToWorld(relocation.To.x, relocation.To.y, cellSize);

            Vector2 radial = fromWorld.sqrMagnitude > 0.0001f ? fromWorld.normalized : Vector2.up;
            Vector2 bounceEnd = fromWorld + radial * (placementConfig.BounceCells * cellSize);

            await TweenPositionWithSpin(
                transform.localPosition,
                new Vector3(bounceEnd.x, bounceEnd.y, 0f),
                placementConfig.BounceDuration,
                placementConfig.SpinDegreesPerSecond);

            Vector3 landStart = transform.localPosition;
            Vector3 landEnd = new Vector3(toWorld.x, toWorld.y, 0f);
            float landDuration = placementConfig.LandDuration;
            float spinSpeed = placementConfig.SpinDegreesPerSecond;

            var landCompletion = new UniTaskCompletionSource();
            float spinZ = transform.localEulerAngles.z;
            MotionHandle landHandle = LMotion.Create(0f, 1f, landDuration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => landCompletion.TrySetResult())
                .Bind(t =>
                {
                    transform.localPosition = Vector3.Lerp(landStart, landEnd, t);
                    spinZ += spinSpeed * Time.deltaTime;
                    transform.localEulerAngles = new Vector3(0f, 0f, spinZ);
                });
            _motions.Add(landHandle);
            await landCompletion.Task;

            SnapToGrid(relocation.To, cellSize, fill);
            transform.localEulerAngles = Vector3.zero;
        }

        private async UniTask TweenPositionWithSpin(Vector3 start, Vector3 end, float duration, float spinDegreesPerSecond)
        {
            var completion = new UniTaskCompletionSource();
            float spinZ = transform.localEulerAngles.z;
            MotionHandle handle = LMotion.Create(0f, 1f, duration)
                .WithEase(Ease.OutQuad)
                .WithOnComplete(() => completion.TrySetResult())
                .Bind(t =>
                {
                    transform.localPosition = Vector3.Lerp(start, end, t);
                    spinZ += spinDegreesPerSecond * Time.deltaTime;
                    transform.localEulerAngles = new Vector3(0f, 0f, spinZ);
                });
            _motions.Add(handle);
            await completion.Task;
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
