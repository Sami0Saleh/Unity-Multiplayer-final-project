using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;
using static Game.Board.BoardMask;
using Game.Player.Visuals;
using System.Collections.Generic;

namespace Game.Player
{
	public class Pawn : MonoBehaviourPun
	{
		public InputActions InputActions { get; private set; }
		public Cursor Cursor { get; set; }
		[field: SerializeField] public PawnMovement Movement { get; private set; }
		[field: SerializeField] public Hammer Hammer { get; private set; }
		public Position Position { get; set; }
		public Vector2Int PositionIndex
		{
			get => Position.ToIndex();
			set => Position = new Position(value);
		}
		public bool IsOnBoard => Board.Instance.CurrentBoardState.Contains(Position);
		public bool CanAct => Movement.AbleToMove || Hammer.AbleToHammer;
		public static Pawn Mine { get; private set; }

		public static event UnityAction<Pawn> PlayerJoined;
		public static event UnityAction<Pawn> PlayerEliminated;
		public event UnityAction<Pawn> TurnStart;
		public event UnityAction<Pawn> TurnEnd;

        public Dictionary<PunPlayer, Pawn> EliminatedPlayers { get; private set; }

        public Photon.Realtime.Player Owner => photonView.Owner;

		private void Awake()
		{
			gameObject.name = Owner.NickName;
			SetPosition();
			if (!photonView.AmOwner)
				return;
			RegisterMine();
			InputActions = new();

			void RegisterMine()
			{
				if (Mine == null)
					Mine = this;
			}

			void SetPosition()
			{
				transform.SetParent(Board.Instance.PawnsParent);
				var board = Board.Instance;
				PositionIndex = Board.CellToIndex(board.Grid.WorldToCell(transform.position));
			}
		}

		private void OnDestroy() => InputActions?.Dispose();

		private void OnEnable()
		{
			PlayerJoined?.Invoke(this);
			TurnIterator.Instance.OnTurnChange += OnTurnChange;
			Movement.OnPawnMoved += OnPawnMoved;
			Hammer.OnHammered += OnHammered;
			if (photonView.AmController)
				InputActions.Cursor.Enable();
		}

		private void OnDisable()
		{
			TurnIterator.Instance.OnTurnChange -= OnTurnChange;
			Movement.OnPawnMoved -= OnPawnMoved;
			Hammer.OnHammered -= OnHammered;
			if (photonView.AmController)
				InputActions.Cursor.Disable();
		}

		private void OnTurnChange(TurnIterator.TurnChangeEvent turnChangeEvent)
		{
			CheckForPawnElimination(turnChangeEvent);
			if (turnChangeEvent.lastPlayer == Owner)
				TurnEnd?.Invoke(this);
			if (turnChangeEvent.currentPlayer == Owner)
			{
				TurnStart?.Invoke(this);
				CheckForStartOfTurnPawnElimination();
			}
		}

		private void OnPawnMoved(PawnMovement.PawnMovementEvent movementEvent)
		{
			if (movementEvent.player != Owner)
				return;
			Position = movementEvent.steps.Last();
			var tileTransform = Board.Instance.PositionToTile(Position).transform;
			transform.SetPositionAndRotation(tileTransform.position, tileTransform.rotation);
			CheckEndTurn();
		}

		private void OnHammered(Position _) => CheckEndTurn();

		private void CheckEndTurn()
		{
			if (!CanAct)
				TurnEnd?.Invoke(this);
		}

		private void CheckForPawnElimination(TurnIterator.TurnChangeEvent turnChangeEvent)
		{
			if (PhotonNetwork.IsMasterClient && EliminatePawnCondition())
				EliminatePawn();

			bool EliminatePawnCondition() => InstantPawnEliminationCondition() || EndOfTurnPawnEliminationCondition();

			bool InstantPawnEliminationCondition() => Pathfinding.GetArea(Position, 1).Empty();

			bool EndOfTurnPawnEliminationCondition() => LastPawnIsMe() && !Movement.HasMoved && Movement.ReachableArea.Empty();

			bool LastPawnIsMe() => turnChangeEvent.lastPlayer != null && GameManager.Instance.ActivePlayers.TryGetValue(turnChangeEvent.lastPlayer, out var lastPawn) && lastPawn == this;
		}

		public void CheckForStartOfTurnPawnElimination()
		{
			if (!CanAct)
			{
                EliminatedPlayers.Add(Owner, this);
                UIManager.Instance.UpdateRanks($"{EliminatedPlayers}\n");
                EliminatePawn();
			}
		}

		void EliminatePawn()
		{
			const string ELIMINATE_PAWN = nameof(EliminatePawnRPC);
            
            photonView.RPC(ELIMINATE_PAWN, RpcTarget.All);
		}

		[PunRPC]
		private void EliminatePawnRPC(PhotonMessageInfo info)
		{
			if (info.Sender != PhotonNetwork.MasterClient)
				return;
			PlayerEliminated?.Invoke(this);
			if (photonView.IsMine)
			{
				PhotonNetwork.Destroy(Cursor.gameObject);
				PhotonNetwork.Destroy(gameObject);
			}
		}

		public override string ToString() => gameObject.name;
	}
}
