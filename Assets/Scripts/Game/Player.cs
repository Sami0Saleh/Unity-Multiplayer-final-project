using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class Player : MonoBehaviourPun
	{
		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private GameObject _cursorPrefab;
		[SerializeField, HideInInspector] private MeshRenderer _renderer;

		public Photon.Realtime.Player ThisPlayer => photonView.Owner;

		private void OnValidate() => _renderer = GetComponentInChildren<MeshRenderer>();

		private void Awake()
		{
			SetMaterial();
			if (!photonView.IsMine)
			{
				enabled = false;
				return;
			}
			PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);

			void SetMaterial()
			{
				if (ThisPlayer.TryGetColorProperty(out var color) && _colorConfig.TryGetValue(color, out var mat))
					_renderer.material = mat;
			}
		}
	}
}
