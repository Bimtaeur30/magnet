using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleActiveBtn_UI : MonoBehaviour
{
    [SerializeField] private GameObject ToggleActiveObj;
    [SerializeField] private bool active;
    [SerializeField] private UnityEvent<bool> onToggleChanged = new UnityEvent<bool>();

    [Tooltip("토글을 실행할 버튼. 비워 두면 같은 오브젝트의 Button을 사용합니다.")]
    [SerializeField] private Button btn;

    [Tooltip("같은 대상을 토글할 추가 버튼 목록. 빈 항목과 중복 버튼은 무시합니다.")]
    [SerializeField] private List<Button> additionalButtons = new List<Button>();

    private readonly HashSet<Button> registeredButtons = new HashSet<Button>();

    public bool IsOn => active;
    public UnityEvent<bool> OnToggleChanged => onToggleChanged;

    private void Awake()
    {
        if (btn == null)
        {
            btn = GetComponent<Button>();
        }
    }

    private void OnEnable()
    {
        RegisterButton(btn);
        if (additionalButtons != null)
        {
            foreach (Button button in additionalButtons)
            {
                RegisterButton(button);
            }
        }
        ApplyState(false);
    }

    private void OnDisable()
    {
        foreach (Button button in registeredButtons)
        {
            if (button != null)
            {
                button.onClick.RemoveListener(ToggleObj);
            }
        }
        registeredButtons.Clear();
    }

    private void RegisterButton(Button button)
    {
        if (button != null && registeredButtons.Add(button))
        {
            button.onClick.AddListener(ToggleObj);
        }
    }

    public void SetState(bool value)
    {
        if (active == value)
        {
            return;
        }

        active = value;
        ApplyState(true);
    }

    private void ToggleObj()
    {
        SetState(!active);
    }

    private void ApplyState(bool invokeEvent)
    {
        if (ToggleActiveObj != null)
        {
            ToggleActiveObj.SetActive(active);
        }

        if (invokeEvent)
        {
            onToggleChanged?.Invoke(active);
        }
    }
}
