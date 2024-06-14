using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ErrorPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _errorText;
    [SerializeField] private Button _okButton;

    private void Start()
    {
        _okButton.onClick.AddListener(OkButton);
    }

    private void OnEnable()
    {
        _okButton.interactable = true;
    }

    public void EditErrorText(string error)
    {
        _errorText.text = error;
    }

    public void OkButton()
    {
        _okButton.interactable = false;
        MainMenuManager.Instance.ResolveErrorPopup();
    }
}
