using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class PasswordVisibleToggle : MonoBehaviour
{
    // Unity Inspectors
    [Header("★ Bindings")]
    [SerializeField] private TMP_InputField passwordTMP = null;

    // Unity Messages
    private void Awake()
    {
        GetComponent<Toggle>().onValueChanged.AddListener((value) => {
            if (value)
                passwordTMP.inputType = TMP_InputField.InputType.Password;
            else
                passwordTMP.inputType = TMP_InputField.InputType.Standard;
            passwordTMP.ForceLabelUpdate();
        });
    }
}
