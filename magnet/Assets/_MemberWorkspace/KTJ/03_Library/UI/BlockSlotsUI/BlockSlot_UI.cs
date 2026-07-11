using UnityEngine;
using UnityEngine.UI;

public class BlockSlot_UI : MonoBehaviour
{
    [SerializeField] private Image BlockImage;

    public void SetSlot(Sprite sprite)
    {
        SetBlockImageAlpha(1f);
        BlockImage.sprite = sprite;
    }

    public void EmptySlot()
    {
        SetBlockImageAlpha(0f);
        BlockImage.sprite = null;
    }

    private void SetBlockImageAlpha(float alpha)
    {
        Color c = BlockImage.color;
        c.a = alpha;
        BlockImage.color = c;
    }
}
