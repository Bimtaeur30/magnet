using UnityEngine;

public class SkinBoxContainer : MonoBehaviour
{
    [SerializeField] private SkinBox SkinBoxPrefab;
    [SerializeField] private RectTransform SkinBoxParent;

    private void Awake()
    {
        // 스킨 로딩 이벤트 구독
    }
}
