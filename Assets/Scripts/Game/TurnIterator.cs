using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using Game.Player;

namespace Game
{
	/// <summary>
	/// Keeps track of the current turn order and current acting <see cref="PunPlayer"/>.
	/// Iterate this to advance the turn order.
	/// </summary>
	public class TurnIterator : MonoBehaviourPun, IEnumerator<PunPlayer>, IEnumerator<Pawn>
	{
		private void Awake()
		{
			_currentStable = GameManager.Instance.ActivePlayers.Last();
		}

		public int CurrentTurn { get; private set; } = -1;
		KeyValuePair<PunPlayer, Pawn> Current => _currentTemp.Key == null ? _currentStable : _currentTemp;
		PunPlayer IEnumerator<PunPlayer>.Current => Current.Key;
		Player.Pawn IEnumerator<Pawn>.Current => Current.Value;
		object IEnumerator.Current => Current;

		private KeyValuePair<PunPlayer, Pawn> _currentStable;
		private KeyValuePair<PunPlayer, Pawn> _currentTemp;

		public void Dispose() => Destroy(gameObject);

		public bool MoveNext()
		{
			var activePlayers = GameManager.Instance.ActivePlayers;
			bool lastPlayer = activePlayers.Count == 1;
			if (lastPlayer)
			{
				_currentStable = activePlayers.Single();
				_currentTemp = default;
				return false;
			}
			else if (TryGetOutOfBoardPawn(out var pawn))
			{
				_currentTemp = activePlayers.Where(kvp => kvp.Key == pawn.ThisPlayer).Single();
				return true;
			}
			else if (Current.Key == activePlayers.Last().Key)
			{
				_currentStable = activePlayers.First();
				_currentTemp = default;
				return true;
			}
			else
			{
				_currentStable = activePlayers.ElementAt(GetIndexOf(_currentStable.Key)+1);
				_currentTemp = default;
				return true;
			}

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
	}
}
