using System.Collections.Generic;
using UnityEngine;

namespace Game.Player.Visuals
{
	public class ChangeCursorShapeOnTurnChange : MonoBehaviour
	{
		[SerializeField] private Cursor _cursor;
		[SerializeField] private GameObject _neutralCursor;
		[SerializeField] private GameObject _walkCursor;
		[SerializeField] private GameObject _destroyCursor;

		private IEnumerable<GameObject> Cursors
		{
			get
			{
				yield return _neutralCursor;
				yield return _walkCursor;
				yield return _destroyCursor;
			}
		}

		private void OnEnable()
		{
			var pawn = _cursor.OwnerPawn;
			pawn.TurnEnd += OnTurnEnd;
			pawn.MoveTurnStart += OnTurnStart;
			pawn.HammerTurnStart += OnHammerStart;
		}

		private void OnDisable()
		{
			var pawn = _cursor.OwnerPawn;
			pawn.TurnEnd -= OnTurnEnd;
			pawn.MoveTurnStart -= OnTurnStart;
			pawn.HammerTurnStart -= OnHammerStart;
		}

		private void OnTurnEnd(Pawn _) => SetCursor(_neutralCursor);

		private void OnTurnStart(Pawn _) => SetCursor(_walkCursor);

		private void OnHammerStart(Pawn _) => SetCursor(_destroyCursor);

		private void SetCursor(GameObject renderer)
		{
			foreach (var r in Cursors)
				r.SetActive(false);
			renderer.SetActive(true);
		}
	}
}
