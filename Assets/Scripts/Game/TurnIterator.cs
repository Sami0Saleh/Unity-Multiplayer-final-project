using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using Game.Player;

namespace Game
{
	/// <summary>
	/// Keeps track of the current turn order and current acting <see cref="PunPlayer"/>.
	/// Iterate this to advance the turn order.
	/// </summary>
	public class TurnIterator : MonoBehaviourPun, IEnumerator<PunPlayer>, IEnumerable<PunPlayer>
	{
		private void Awake()
		{
			_currentStable = GameManager.Instance.ActivePlayers.Last().Key;
		}

		public event UnityAction<PunPlayer, PunPlayer> OnTurnChange;

		public int CurrentTurn { get; private set; } = -1;
		public PunPlayer Current => _currentTemp ?? _currentStable;
		object IEnumerator.Current => Current;

		private PunPlayer _currentStable;
		private PunPlayer _currentTemp;

		public bool MoveNext()
		{
			var activePlayers = GameManager.Instance.ActivePlayers;
			PunPlayer lastPlayer = Current;
			if (activePlayers.Count == 1)
			{
				_currentStable = activePlayers.Single().Key;
				_currentTemp = default;
				return false;
			}
			else if (TryGetOutOfBoardPawn(out var pawn))
			{
				_currentTemp = pawn.ThisPlayer;
				InvokeOnTurnChange();
				return true;
			}
			else if (Current == activePlayers.Last().Key)
			{
				_currentStable = activePlayers.First().Key;
				_currentTemp = default;
				CurrentTurn++;
				InvokeOnTurnChange();
				return true;
			}
			else
			{
				_currentStable = activePlayers.ElementAt(GetIndexOf(_currentStable)+1).Key;
				_currentTemp = default;
				InvokeOnTurnChange();
				return true;
			}

			void InvokeOnTurnChange() => OnTurnChange?.Invoke(lastPlayer, Current);

			int GetIndexOf(PunPlayer player)
			{
				int index = 0;
				foreach (var kvp in activePlayers)
				{
					if (kvp.Key == player)
						break;
					index++;
				}
				return index;
			}

			bool TryGetOutOfBoardPawn(out Pawn pawn)
			{
				throw new System.NotImplementedException(); // TODO Implement
			}
		}

		public void Reset() { }

		public void Dispose() { }

		public IEnumerator<PunPlayer> GetEnumerator() => this;

		IEnumerator IEnumerable.GetEnumerator() => this;
	}
}
