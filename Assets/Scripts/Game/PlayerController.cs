using UnityEngine;
using Photon.Pun;

namespace Game
{
	public class PlayerController : MonoBehaviourPun
	{
		private const string PROJECTILE_PREFAB = "Prefabs\\Projectile";

		[Header("Projectile")]
		[SerializeField] private Transform ProjectileSpawnTransform;

        [SerializeField] private float speed = 10;
        private Vector3 movementVector = new Vector3();

        private Camera _cachedCamera;
		private Vector3 raycastPos;

        private PlayerCharacter _character;


		private void Awake()
		{
			if (!photonView.IsMine)
			{
				enabled = false;
				return;
			}
            _character = GetComponent<PlayerCharacter>();
		}

        private void Start()
		{
			_cachedCamera = Camera.main;
		}

        private void Update()
		{
			if (photonView.AmOwner)
            {
                Ray ray = _cachedCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                    raycastPos = hit.point;

                movementVector = new Vector3();
                if (Input.GetKey(KeyCode.W))
                    movementVector.z = 1;
                if (Input.GetKey(KeyCode.S))
                    movementVector.z = -1;
                if (Input.GetKey(KeyCode.D))
                    movementVector.x = 1;
                if (Input.GetKey(KeyCode.A))
                    movementVector.x = -1;

                if (Input.GetKeyDown(KeyCode.Mouse0))
                    Shoot();

                Vector3 directionToFace = raycastPos - gameObject.transform.position;
                Quaternion lookAtRotation = Quaternion.LookRotation(directionToFace);
                Vector3 eulerRotation = lookAtRotation.eulerAngles;
                eulerRotation.x = 0;
                eulerRotation.z = 0;
                transform.eulerAngles = eulerRotation;
                transform.Translate(movementVector * (Time.deltaTime * speed));
            }
        
		}

        private void Shoot()
        {
            if (Physics.Raycast(_cachedCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                if (hit.collider.tag != "Player")
                    return;
                var projectile = PhotonNetwork.Instantiate(PROJECTILE_PREFAB, ProjectileSpawnTransform.position, ProjectileSpawnTransform.rotation).GetComponent<Projectile>();
                projectile.ownerPlayer = _character;
            }
            else
            {
                Debug.Log("No valid target, not shooting.");
            }
        }
    }
}
