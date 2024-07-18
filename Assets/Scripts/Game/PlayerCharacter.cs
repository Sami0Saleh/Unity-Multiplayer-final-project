using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
	public class PlayerCharacter : MonoBehaviourPun
	{
		private const string PROJECTILE_TAG = "Projectile";
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
				if (ThisPlayer.TryGetColorProperty(out var color) && _colorConfig.TryGetValue(color, out var mat))
					_renderer.material = mat;
			}
		}

		private void Start()
		{
			PlayerJoined?.Invoke(this);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!other.CompareTag(PROJECTILE_TAG))
				return;
			Projectile otherProjectile = other.GetComponent<Projectile>();
			if (otherProjectile.photonView.Owner.ActorNumber == photonView.Owner.ActorNumber || otherProjectile.Damage <= 0 || !otherProjectile.photonView.IsMine)
				return;

			StartCoroutine(DestroyDelay(otherProjectile.gameObject, 2f));
			photonView.RPC(RECEIVE_DAMAGE, RpcTarget.All, otherProjectile.Damage, photonView.Owner.ActorNumber);
			otherProjectile.SetVisibility(false);
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

		IEnumerator DestroyDelay(GameObject projectileObject, float delay = 1f)
		{
			yield return new WaitForSeconds(delay);
			Destroy(projectileObject);
		}
	}
}
