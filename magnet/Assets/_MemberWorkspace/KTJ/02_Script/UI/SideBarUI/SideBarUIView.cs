using Mvvm;
using UnityEngine;

namespace Game.UI
{
    public sealed partial class SideBarUIView : MvvmView<SideBarUIViewModel>
    {
        [SerializeField] private ToggleActiveBtn_UI settingToggle;
        [SerializeField] private ToggleActiveBtn_UI skinToggle;

        private bool isChangingToggleState;

        protected override void Awake()
        {
            ResolveToggleReferences();
            base.Awake();
        }

        protected override void OnEnable()
        {
            ResolveToggleReferences();
            base.OnEnable();

            if (settingToggle != null)
            {
                settingToggle.OnToggleChanged.AddListener(OnSettingToggleChanged);
            }

            if (skinToggle != null)
            {
                skinToggle.OnToggleChanged.AddListener(OnSkinToggleChanged);
            }

            InitializeToggleState();
        }

        protected override void OnDisable()
        {
            if (settingToggle != null)
            {
                settingToggle.OnToggleChanged.RemoveListener(OnSettingToggleChanged);
            }

            if (skinToggle != null)
            {
                skinToggle.OnToggleChanged.RemoveListener(OnSkinToggleChanged);
            }

            base.OnDisable();
        }

        private void Reset()
        {
            ResolveToggleReferences();
        }

        private void OnValidate()
        {
            ResolveToggleReferences();
        }

        private void ResolveToggleReferences()
        {
            if (settingToggle == null)
            {
                settingToggle = FindToggleByName("Button_Setting");
            }

            if (skinToggle == null)
            {
                skinToggle = FindToggleByName("Button_Skin");
            }
        }

        private ToggleActiveBtn_UI FindToggleByName(params string[] names)
        {
            foreach (string objectName in names)
            {
                Transform child = transform.Find(objectName);

                if (child != null && child.TryGetComponent(out ToggleActiveBtn_UI toggle))
                {
                    return toggle;
                }
            }

            return null;
        }

        private void InitializeToggleState()
        {
            if (settingToggle == null || skinToggle == null)
            {
                return;
            }

            if (settingToggle.IsOn)
            {
                SelectToggle(settingToggle, skinToggle);
            }
        }

        private void OnSettingToggleChanged(bool isOn)
        {
            HandleToggleChanged(settingToggle, skinToggle, isOn);
        }

        private void OnSkinToggleChanged(bool isOn)
        {
            HandleToggleChanged(skinToggle, settingToggle, isOn);
        }

        private void HandleToggleChanged(ToggleActiveBtn_UI selectedToggle, ToggleActiveBtn_UI otherToggle, bool isOn)
        {
            if (isChangingToggleState)
            {
                return;
            }

            if (isOn)
            {
                SelectToggle(selectedToggle, otherToggle);
            }
        }

        private void SelectToggle(ToggleActiveBtn_UI selectedToggle, ToggleActiveBtn_UI otherToggle)
        {
            if (selectedToggle == null)
            {
                return;
            }

            isChangingToggleState = true;
            selectedToggle.SetState(true);

            if (otherToggle != null)
            {
                otherToggle.SetState(false);
            }

            isChangingToggleState = false;
        }
    }
}
