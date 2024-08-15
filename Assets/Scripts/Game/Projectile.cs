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
            if (other.TryGetComponent<PlayerCharacter>(out var player) && player.ThisPlayer.ActorNumber != photonView.OwnerActorNr)
            {
                _collider.enabled = false;
                SetVisibility(false);

                if (PhotonNetwork.IsMasterClient)
                {
                    player.ReceiveDamage(Damage);
                    StartCoroutine(DestroyDelay());
                }
            }
        }

        IEnumerator DestroyDelay(float delay = 0.5f)
        {
            yield return new WaitForSeconds(delay);

            if (photonView.AmController || PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.Destroy(gameObject);
            }
            else
            {
                Debug.LogWarning($"Cannot destroy object. Not the owner nor the MasterClient. ViewID: {photonView.ViewID}");
            }
        }

        public void SetVisibility(bool visible) => _visuals.SetActive(visible);
    }
}

