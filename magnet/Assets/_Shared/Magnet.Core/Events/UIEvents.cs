using GameLib.EventChannelSystem;
using Magnet.Contracts.BlockShapes;
using Magnet.Contracts.BlockSkins;
using UnityEngine;
using UnityEngine.UI;

public static class UIEvents
{
    public static readonly UIBackgroundRequestEvent BackgroundRequestEvent = new();
    public static readonly UIPlayNewSkinEvent UIPlayNewSkinEvent = new();


    //public static readonly BlockSlotSetEvent BlockSlotSetEvent = new(); // 폐기됨, 사용하지 않음
}

public sealed class UIPlayNewSkinEvent : GameEvent { }
public sealed class UIBackgroundRequestEvent : GameEvent
{
    public Object Requester { get; private set; }
    public bool IsUsing { get; private set; }
    public Color Color { get; private set; }
    public float Alpha { get; private set; }
    public float FadeDuration { get; private set; }
    public bool RaycastTarget { get; private set; }

    public UIBackgroundRequestEvent Init(
        Object requester,
        bool isUsing,
        Color color,
        float alpha,
        float fadeDuration,
        bool raycastTarget = true)
    {
        Requester = requester;
        IsUsing = isUsing;
        Color = color;
        Alpha = Mathf.Clamp01(alpha);
        FadeDuration = Mathf.Max(0f, fadeDuration);
        RaycastTarget = raycastTarget;
        return this;
    }
}

//public class BlockSlotSetEvent : GameEvent
//{
//    public int Index;
//    public IBlockShape Shape;
//    public IBlockSkin Skin;

//    public BlockSlotSetEvent Init(int index, IBlockShape shape, IBlockSkin skin)
//    {
//        Index = index;
//        Shape = shape;
//        Skin = skin;
//        return this;
//    }
//}
