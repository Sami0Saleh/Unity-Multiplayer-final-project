using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class PlayerController : MonoBehaviourPun
	{
		private const string PROJECTILE_PREFAB = "Prefabs\\Projectile";

		[Header("Projectile")]
		[SerializeField] private Transform ProjectileSpawnTransform;

		private Camera _cachedCamera;
		private Vector3 raycastPos;

		private void Awake()
		{
			if (!photonView.IsMine)
			{
				enabled = false;
				return;
			}
		}

		void Start()
		{
			_cachedCamera = Camera.main;
		}

		void Update()
		{
			Ray ray = _cachedCamera.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(ray, out RaycastHit hit))
				raycastPos = hit.point;

			if (Input.GetKeyDown(KeyCode.Mouse0))
				Shoot();

			Vector3 directionToFace = raycastPos - transform.position;
			Quaternion lookAtRotatin = Quaternion.LookRotation(directionToFace);
			Vector3 eulerRotation = lookAtRotatin.eulerAngles;
			eulerRotation.x = 0f;
			eulerRotation.z = 0f;
			transform.eulerAngles = eulerRotation;
		}

        private void Shoot()
        {
            if (Physics.Raycast(_cachedCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                var projectile = PhotonNetwork.Instantiate(PROJECTILE_PREFAB, ProjectileSpawnTransform.position, ProjectileSpawnTransform.rotation);
                // Apply initial direction to projectile
            }
            else
            {
                Debug.Log("No valid target, not shooting.");
            }
        }
    }
}
