using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class Player : MonoBehaviour
	{
		[SerializeField] private GameObject _cursorPrefab;
		[SerializeField, HideInInspector] private PhotonView _photonView;

		[SerializeField] private MeshRenderer _renderer;
		[SerializeField] private Material[] _colors;
        private const int RED_MAT_INDEX = 0;
        private const int GREEN_MAT_INDEX = 1;
        private const int BLUE_MAT_INDEX = 2;
        private const int YELLOW_MAT_INDEX = 3;

		private void OnValidate()
		{
			_photonView = GetComponent<PhotonView>();
		}

		private void Awake()
		{
			SetColor();
			if (!_photonView.IsMine)
			{
				enabled = false;
				return;
			}
			PhotonNetwork.Instantiate(_cursorPrefab.name, transform.position, transform.rotation);
		}

        private void SetColor()
        {
            var _hashedProperties = _photonView.Owner.CustomProperties;
			switch (_hashedProperties["PlayerColor"])
			{
                case "Red":
                    _renderer.material = _colors[RED_MAT_INDEX];
                    break;
                case "Green":
                    _renderer.material = _colors[GREEN_MAT_INDEX];
                    break;
                case "Blue":
                    _renderer.material = _colors[BLUE_MAT_INDEX];
                    break;
                case "Yellow": _renderer.material = _colors[YELLOW_MAT_INDEX];
					break;

            }
        }
    }
}
