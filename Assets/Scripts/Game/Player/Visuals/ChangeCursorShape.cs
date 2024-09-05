using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Visuals
{
	public class ChangeCursorShape : MonoBehaviour
	{
		[SerializeField] private Cursor _cursor;
		[SerializeField] private GameObject _neutralCursor;
		[SerializeField] private GameObject _moveCursor;
		[SerializeField] private GameObject _hammerCursor;

		private IEnumerable<GameObject> Cursors
		{
			get
			{
				yield return _neutralCursor;
				yield return _moveCursor;
				yield return _hammerCursor;
			}
		}

		private void OnEnable() => _cursor.StateChanged += OnStateChanged;

		private void OnDisable() => _cursor.StateChanged -= OnStateChanged;

		private void OnStateChanged(Cursor.State newState)
		{
			var cursor = newState switch
			{
				Cursor.State.Move => _moveCursor,
				Cursor.State.Hammer => _hammerCursor,
				_ => _neutralCursor,
			};
			SetCursor(cursor);
		}

		private void SetCursor(GameObject cursor)
		{
			foreach (var r in Cursors)
				r.SetActive(false);
			cursor.SetActive(true);
		}
	}
}
