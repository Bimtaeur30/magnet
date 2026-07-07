using System.Collections.Generic;
using LitMotion;
using LitMotion.Extensions;
using MVP.System.AbstractMVP.Form;
using MVP.System.BaseMVP;
using MVP.UIData;
using UnityEngine;
using UnityEngine.UI;

namespace MVP.Forms
{
    public class SwapForm : AbstractVisualForm, IInitializable
    {
        [SerializeField] private float swapDuration = 0.3f;

        private Dictionary<int, int> _childDict;
        private RectTransform[] _currentChildren;
        private MotionHandle _sequenceHandle;

        public void Initialize()
        {
            _childDict = new Dictionary<int, int>();
            _currentChildren = new RectTransform[transform.childCount];
            
            for (int i = 0; i < transform.childCount; ++i)
            {
                RectTransform rectTrm = transform.GetChild(i).GetComponent<RectTransform>();
                _childDict.Add(i, i);
                _currentChildren[i] = rectTrm;
            }
            
            // LayoutGroup은 위치랑 모양만 잡고 끄기
            SetOffLayoutGroup();
        }
        
        //그냥 enable을 끄면 Layout 깨짐.
        private void SetOffLayoutGroup()
        {
            VerticalLayoutGroup layoutGroup = GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null || layoutGroup.enabled == false)
                return;
            
            layoutGroup.CalculateLayoutInputHorizontal();
            layoutGroup.CalculateLayoutInputVertical();
            layoutGroup.SetLayoutHorizontal();
            layoutGroup.SetLayoutVertical();
            
            layoutGroup.enabled = false;
        }

        protected override void UpdateVisual(UIParam data)
        {
            UISwapParam swapData = (UISwapParam)data;
            
            SwapItem(swapData.ItemEnum, swapData.TargetIndex);
        }

        private void SwapItem(int itemEnum, int targetIndex)
        {
            int targetItemIdx = _childDict[itemEnum];
            if (targetItemIdx == targetIndex)
                return;

            DoMove(targetItemIdx, targetIndex);

            //Swap
            (_currentChildren[targetIndex], _currentChildren[targetItemIdx]) 
                = (_currentChildren[targetItemIdx], _currentChildren[targetIndex]);
            (_childDict[targetIndex], _childDict[targetItemIdx]) 
                = (_childDict[targetItemIdx], _childDict[targetIndex]);
        }

        private void DoMove(int item1Idx, int item2Idx)
        {
            if (_sequenceHandle.IsActive())
            {
                _sequenceHandle.Complete();
                _sequenceHandle.Cancel();
            }
                
            Vector2 item1Pos = _currentChildren[item1Idx].anchoredPosition;
            Vector2 item2Pos = _currentChildren[item2Idx].anchoredPosition;
            
            Vector2 item1Size = _currentChildren[item1Idx].sizeDelta;
            Vector2 item2Size = _currentChildren[item2Idx].sizeDelta;

            var item1 = _currentChildren[item1Idx];
            var item2 = _currentChildren[item2Idx];
            
            _sequenceHandle = LSequence.Create()
                .Join(LMotion.Create(item1Pos, item2Pos, swapDuration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToAnchoredPosition(item1))
                .Join(LMotion.Create(item1Size, item2Size, swapDuration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToSizeDelta(item1))
                .Join(LMotion.Create(item2Pos, item1Pos, swapDuration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToAnchoredPosition(item2))
                .Join(LMotion.Create(item2Size, item1Size, swapDuration)
                    .WithEase(Ease.OutBack)
                    .WithScheduler(MotionScheduler.UpdateIgnoreTimeScale)
                    .BindToSizeDelta(item2))
                .Run();
        }
        
        private void OnDestroy()
        {
            if (_sequenceHandle.IsActive())
            {
                _sequenceHandle.Complete();
                _sequenceHandle.Cancel();
            }
        }
    }
}
