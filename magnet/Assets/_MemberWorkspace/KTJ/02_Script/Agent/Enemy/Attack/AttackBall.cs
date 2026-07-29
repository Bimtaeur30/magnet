using System.Collections;
using GameLib.EventChannelSystem;
using UnityEngine;

public sealed class AttackBall : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float moveDuration = 0.5f;
    [SerializeField] private float curveHeight = 2f;

    private EventChannelSO _enemyEventChannel;
    private Vector3 _attackStartWorldPosition;
    private Vector3 _attackEndWorldPosition;
    private int _damage;

    public void Initialize(
        EventChannelSO enemyEventChannel,
        Vector3 attackStartWorldPosition,
        Vector3 attackEndWorldPosition,
        int damage)
    {
        _enemyEventChannel = enemyEventChannel;
        _attackStartWorldPosition = attackStartWorldPosition;
        _attackEndWorldPosition = attackEndWorldPosition;
        _damage = damage;

        transform.position = _attackStartWorldPosition;
        StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        Vector3 middle = (_attackStartWorldPosition + _attackEndWorldPosition) * 0.5f;
        Vector3 controlPoint = middle + new Vector3(Random.Range(-1f, 1f), 0, 0) * curveHeight;
        float elapsedTime = 0f;

        while (elapsedTime < moveDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / moveDuration);

            transform.position = EvaluateQuadraticBezier(
                _attackStartWorldPosition,
                controlPoint,
                _attackEndWorldPosition,
                t);

            yield return null;
        }

        transform.position = _attackEndWorldPosition;

        _enemyEventChannel.RaiseEvent(
            EnemyEvents.EnemyAttackEvent.Init(_attackEndWorldPosition, _damage));

        Destroy(gameObject);
    }

    private static Vector3 EvaluateQuadraticBezier(
        Vector3 start,
        Vector3 control,
        Vector3 end,
        float t)
    {
        Vector3 startToControl = Vector3.Lerp(start, control, t);
        Vector3 controlToEnd = Vector3.Lerp(control, end, t);

        return Vector3.Lerp(startToControl, controlToEnd, t);
    }
}
