using System;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class ToggleActiveBtn_UI : MonoBehaviour
{
    [SerializeField] private GameObject ToggleActiveObj;
    [SerializeField] private bool active;
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
        btn.onClick.AddListener(() => ToggleObj());
    }

    private void ToggleObj()
    {
        active = !active;
        ToggleActiveObj.gameObject.SetActive(active);
    }
}
