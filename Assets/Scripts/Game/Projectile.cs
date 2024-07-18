using System.Collections;
using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class Projectile : MonoBehaviourPun
	{
		[SerializeField] private GameObject _visuals;
		[SerializeField] private Collider _collider;
		[SerializeField] private float _velocity = 20f;
		[field: SerializeField] public int Damage { get; private set; } = 1;

		private void Awake()
		{
			if (!photonView.IsMine)
				enabled = false;
		}

		private void FixedUpdate()
		{
			transform.Translate(Vector3.forward * (Time.fixedDeltaTime * _velocity));
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.TryGetComponent<PlayerCharacter>(out var player) && player.ThisPlayer.ActorNumber != photonView.Owner.ActorNumber)
			{
				_collider.enabled = false;
				SetVisibility(false);
				if (PhotonNetwork.IsMasterClient && Damage > 0)
				{
					player.ReceiveDamage(Damage);
					StartCoroutine(DestroyDelay());
				}
			}
		}

		IEnumerator DestroyDelay(float delay = 1f)
		{
			yield return new WaitForSeconds(delay);
			PhotonNetwork.Destroy(gameObject);
		}

		public void SetVisibility(bool visible) => _visuals.SetActive(visible);
	}
}
