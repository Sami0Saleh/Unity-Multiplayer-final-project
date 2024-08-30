using TMPro;
using UnityEngine;

namespace UI
{
	public class ChatMessage : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI _textMeshPro;

		public string Text { get => _textMeshPro.text; set => _textMeshPro.text = value; }

		public Color Color { get => _textMeshPro.color; set => _textMeshPro.color = value; }
	}
}
