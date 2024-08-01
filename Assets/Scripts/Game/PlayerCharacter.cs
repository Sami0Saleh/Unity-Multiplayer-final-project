using UnityEngine;
using UnityEngine.Events;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
	public class PlayerCharacter : MonoBehaviourPun
	{
		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private GameObject _cursorPrefab;
		[SerializeField, HideInInspector] private MeshRenderer _renderer;

		public static event UnityAction<PlayerCharacter> PlayerJoined;
		public static event UnityAction<PlayerCharacter> PlayerEliminated;

		public Player ThisPlayer => photonView.Owner;

		private void OnValidate() => _renderer = GetComponentInChildren<MeshRenderer>();

		private void Awake()
		{
			SetMaterial();
			gameObject.name = ThisPlayer.NickName;
			transform.SetParent(Board.Instance.PlayerParent);
			if (!photonView.AmOwner)
				return;
			PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);

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
	}
}
