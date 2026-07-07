using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace MVP.Visual
{
    /// <summary>
    /// RectTransform을 일정 속도로 무한 Z회전시키는 로딩 인디케이터.
    /// 아무 GO에 부착하면 기본값으로 동작. InteractionFeedback과 무관.
    /// </summary>
    public class Spinner : MonoBehaviour
    {
        [SerializeField] private float degPerSec = 180f;
        [SerializeField] private bool clockwise = true;
        [SerializeField] private bool ignoreTimeScale = true;

        private MotionHandle _handle;

        private void OnEnable()
        {
            Play();
        }

        private void OnDisable()
        {
            if (_handle.IsActive()) _handle.Cancel();
            _handle = default;
        }

        private void Play()
        {
            if (_handle.IsActive()) _handle.Cancel();

            float speed = Mathf.Max(1f, degPerSec);
            float dir = clockwise ? -1f : 1f; // UI에서 Z- 가 시계방향
            float durationFor360 = 360f / speed;
            float startZ = transform.localEulerAngles.z;

            var motion = LMotion.Create(startZ, startZ + dir * 360f, durationFor360)
                .WithEase(Ease.Linear)
                .WithLoops(-1, LoopType.Incremental);

            if (ignoreTimeScale)
                motion = motion.WithScheduler(MotionScheduler.UpdateIgnoreTimeScale);

            _handle = motion.BindToLocalEulerAnglesZ(transform);
        }
    }
}
