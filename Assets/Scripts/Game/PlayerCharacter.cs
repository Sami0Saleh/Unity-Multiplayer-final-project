using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
	public class PlayerCharacter : MonoBehaviourPun
	{
		private const string RECEIVE_DAMAGE = nameof(ReceiveDamage);

		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private GameObject _cursorPrefab;
		[SerializeField, HideInInspector] private MeshRenderer _renderer;

		public static event UnityAction<PlayerCharacter> PlayerJoined;
		public static event UnityAction<PlayerCharacter> PlayerDamaged;
		public static event UnityAction<PlayerCharacter> PlayerDied;

		public Player ThisPlayer => photonView.Owner;
		public bool IsDead => HP <= 0;
		[field: SerializeField] public int HP { get; private set; } = 10;

		private void OnValidate() => _renderer = GetComponentInChildren<MeshRenderer>();

		private void Awake()
		{
			SetMaterial();
			gameObject.name = ThisPlayer.NickName;
			if (!photonView.IsMine)
				return;
			//PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);

			void SetMaterial()
			{
				if (_colorConfig.TryGetMaterial(ThisPlayer, out var mat))
					_renderer.material = mat;
			}
		}

		private void Start()
		{
			PlayerJoined?.Invoke(this);
		}

		public void ReceiveDamage(int damage)
		{
			photonView.RPC(RECEIVE_DAMAGE, RpcTarget.All, damage, photonView.Owner.ActorNumber);
		}

		[PunRPC]
		private void ReceiveDamage(int damage, int damagedActorNumber)
		{
			if (damagedActorNumber != photonView.Owner.ActorNumber)
				return;
			HP -= damage;
			PlayerDamaged?.Invoke(this);
			Debug.Log(photonView.Owner + " HP Left: " + HP);
			if (IsDead)
			{
				HP = 0;
				PlayerDied?.Invoke(this);
				if (photonView.IsMine)
					PhotonNetwork.Destroy(gameObject);
			}
		}
	}
}
