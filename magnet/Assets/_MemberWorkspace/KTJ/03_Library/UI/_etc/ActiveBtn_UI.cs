using LitMotion;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class ActiveBtn_UI : MonoBehaviour
{
    [SerializeField] private GameObject ActiveObj;
    [SerializeField] private bool active;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => Active());
    }

    private void Active()
    {
        ActiveObj.gameObject.SetActive(active);
    }
}
