using HwanLib.MVP.System.BaseMVP;
using HwanLib.MVP.UIData;

namespace TitleUI.TitleUI
{
    public class TitleUIModel : IModel
    {
        public UIParam UpdateSlider()
        {
            return UIParams.UIFloatParam.Init(0.5f);
        }
        int gold;
        public void BuyBUttonClickHandler(UIParam value)
        {
            gold -= ((UIIntParam)value).Value;
        }
    }
}
