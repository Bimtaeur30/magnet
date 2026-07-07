namespace HwanLib.MVP.UIData
{
    public class UIStringParam : UIParam
    {
        public string Value;

        public UIStringParam Init(string value)
        {
            Value = value;

            return this;
        }
    }
}