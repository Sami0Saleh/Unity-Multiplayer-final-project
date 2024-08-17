using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

namespace Game
{
    public class PlayerCharacter : MonoBehaviourPun
    {
        public static Dictionary<Player, PlayerCharacter> PlayerDictionary = new(4);

        private const string RECEIVE_DAMAGE = nameof(ReceiveDamage);

        [SerializeField] private PlayerColors _colorConfig;
        [SerializeField] private GameObject _cursorPrefab;
        [SerializeField, HideInInspector] private MeshRenderer _renderer;
        [SerializeField] private int _baseDamage = 1;

        private int _currentDamage;
        private bool _isInactive;

        private void OnValidate() => _renderer = GetComponentInChildren<MeshRenderer>();

        public static event UnityAction<PlayerCharacter> PlayerJoined;
        public static event UnityAction<PlayerCharacter> PlayerDamaged;
        public static event UnityAction<PlayerCharacter> PlayerDied;

        public Player ThisPlayer => photonView.Owner;
        public bool IsDead => HP <= 0;
        [field: SerializeField] public int HP { get; private set; } = 10;

        private void Awake()
        {
            SetMaterial();
            gameObject.name = ThisPlayer.NickName;
            _currentDamage = _baseDamage;
            PlayerDictionary.TryAdd(ThisPlayer, this);

            if (!photonView.IsMine)
                return;

            //PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);

            void SetMaterial()
            {
                if (_colorConfig.TryGetMaterial(ThisPlayer, out var mat))
                    _renderer.material = mat;
            }
        }

		private void OnDestroy()
		{
			PlayerDictionary.Remove(ThisPlayer);
		}

		private void Start()
        {
            PlayerJoined?.Invoke(this);
        }

        public void ReceiveDamage(int damage)
        {
            photonView.RPC(RECEIVE_DAMAGE, RpcTarget.All, damage, photonView.Owner.ActorNumber);
        }

        public void ReassignOwnership(Player newOwner)
        {
            photonView.TransferOwnership(newOwner);
        }

        public void SetInactive(bool isInactive)
        {
            _isInactive = isInactive;

            GetComponent<Collider>().enabled = !_isInactive;
        }

        public void AddHealth(int amount)
        {
            HP += amount;
        }

        public void AddDamage(int amount)
        {
            _currentDamage += amount;
        }

        public int GetDamage()
        {
            return _currentDamage;
        }

        [PunRPC]
        private void ReceiveDamage(int damage, int damagedActorNumber)
        {
            if (damagedActorNumber != ThisPlayer.ActorNumber)
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

        public void OnEnable()
        {
            if (_isInactive && photonView.IsMine)
            {
                SetInactive(false);
            }
        }

        public void OnDisable()
        {
            if (photonView.IsMine)
            {
                SetInactive(true);
            }
        }

        private void ClearDictionary() => PlayerDictionary.Clear();
    }
}
