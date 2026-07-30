using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public sealed class EnemyShatterFragment : MonoBehaviour
{
    private Vector3 _velocity;
    private Vector3 _rotationVelocity;
    private float _gravity;
    private float _duration;
    private float _elapsedTime;
    private Vector3 _initialScale;
    private float _startScaleMultiplier;

    public void Initialize(
        Vector3 velocity,
        Vector3 rotationVelocity,
        float gravity,
        float duration,
        float startScaleMultiplier)
    {
        _velocity = velocity;
        _rotationVelocity = rotationVelocity;
        _gravity = gravity;
        _duration = duration;
        _initialScale = transform.localScale;
        _startScaleMultiplier = startScaleMultiplier;
        transform.localScale = _initialScale * _startScaleMultiplier;
    }

    private void Update()
    {
        _elapsedTime += Time.deltaTime;
        _velocity += Vector3.down * (_gravity * Time.deltaTime);

        transform.position += _velocity * Time.deltaTime;
        transform.Rotate(_rotationVelocity * Time.deltaTime, Space.Self);

        float normalizedTime = Mathf.Clamp01(_elapsedTime / _duration);
        float popAmount = Mathf.InverseLerp(0f, 0.2f, normalizedTime);
        float popScale = Mathf.Lerp(_startScaleMultiplier, 1f, popAmount);
        float shrinkAmount = Mathf.InverseLerp(0.72f, 1f, normalizedTime);
        transform.localScale =
            _initialScale * popScale * (1f - shrinkAmount);
    }

    private void OnDestroy()
    {
        Mesh mesh = GetComponent<MeshFilter>().sharedMesh;

        if (mesh != null)
            Destroy(mesh);
    }
}
