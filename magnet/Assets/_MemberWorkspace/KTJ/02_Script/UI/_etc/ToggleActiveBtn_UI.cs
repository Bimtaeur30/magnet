using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ToggleActiveBtn_UI : MonoBehaviour
{
    [SerializeField] private GameObject ToggleActiveObj;
    [SerializeField] private bool active;
    [SerializeField] private UnityEvent<bool> onToggleChanged = new UnityEvent<bool>();

    private Button btn;

    public bool IsOn => active;
    public UnityEvent<bool> OnToggleChanged => onToggleChanged;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        btn.onClick.AddListener(ToggleObj);
        ApplyState(false);
    }

    private void OnDisable()
    {
        btn.onClick.RemoveListener(ToggleObj);
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
