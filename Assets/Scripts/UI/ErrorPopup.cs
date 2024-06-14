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
        MainMenuManager.Instance.ToggleButtonState(false);
	}

	private void OnDisable()
	{
		MainMenuManager.Instance.ToggleButtonState(true);
	}

	public void EditErrorText(string error)
    {
        _errorText.text = error;
    }

    public void OkButton()
    {
        gameObject.SetActive(false);
    }
}
