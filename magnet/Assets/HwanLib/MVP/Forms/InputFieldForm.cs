using HwanLib.MVP.System;
using HwanLib.MVP.System.AbstractMVP.Form;
using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.System.BaseMVP.Form;
using HwanLib.MVP.UIData;
using TMPro;
using UnityEngine;

namespace HwanLib.MVP.Forms
{
    /// <summary>
    /// TMP_InputField 텍스트 입력 위젯. onEndEdit 시점에 문자열을 발행한다(매 글자 발행 안 함).
    /// 같은 GameObject의 TMP_InputField를 사용한다.
    /// </summary>
    [RequireComponent(typeof(TMP_InputField))]
    public class InputFieldForm : AbstractVisualForm, IInteractable, IInitializable
    {
        public event FormInteracted OnFormInteracted;

        private TMP_InputField _input;

        public void Initialize()
        {
            _input = GetComponent<TMP_InputField>();
            _input.onEndEdit.AddListener(EndEditHandler);
        }

        private void EndEditHandler(string value)
        {
            OnFormInteracted?.Invoke(UIParams.UIStringParam.Init(value));
        }

        protected override void UpdateVisual(UIParam data)
        {
            // 재진입 루프 방지: notify 없이 텍스트만 설정
            _input.SetTextWithoutNotify(((UIStringParam)data).Value);
        }
    }
}
