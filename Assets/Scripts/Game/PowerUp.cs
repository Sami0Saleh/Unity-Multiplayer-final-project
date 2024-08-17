using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
    public class PowerUp : MonoBehaviourPun
    {
        [SerializeField] private int _healthBonus = 2;
        [SerializeField] private int _damageBonus = 1;
        [SerializeField] private PowerUpType _powerUpType;


        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<PlayerCharacter>();

            if (player == null)
            {
                Debug.LogWarning($"The object that triggered the power-up does not have a PlayerCharacter component. Object name: {other.gameObject.name}");
                return;
            }

            if (photonView.IsMine || PhotonNetwork.IsMasterClient)
            {
                ApplyPowerUp(player);

                photonView.RPC(nameof(OnPowerUpPicked), RpcTarget.MasterClient, player.ThisPlayer, _powerUpType);

                PhotonNetwork.Destroy(gameObject);
            }
            
        }

        private void ApplyPowerUp(PlayerCharacter player)
        {
            switch (_powerUpType)
            {
                case PowerUpType.Health:
                    player.AddHealth(_healthBonus);
                    break;
                case PowerUpType.Damage:
                    player.AddDamage(_damageBonus);
                    break;
            }
        }

        [PunRPC]
        private void OnPowerUpPicked(Player player, PowerUpType type, PhotonMessageInfo info)
        {

            string powerUpName = type == PowerUpType.Health ? "Health" : "Damage";
            string playerName = player.NickName;

            photonView.RPC(nameof(BroadcastPowerUpPickup), RpcTarget.All, playerName, powerUpName);
        }

        [PunRPC]
        private void BroadcastPowerUpPickup(string playerName, string powerUpName)
        {
            UIManager.Instance.UpdateText($"{playerName} collected {powerUpName} power-up.");
        }
    }

    public enum PowerUpType
    {
        Health,
        Damage
    }
}
