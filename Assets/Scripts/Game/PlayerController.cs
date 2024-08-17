using UnityEngine;
using UnityEngine.InputSystem;
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


		private void Awake()
		{
			if (!photonView.AmController)
			{
				Destroy(this);
				return;
			}
			_cachedCamera = Camera.main;
		}

        private void FixedUpdate()
		{
            movementVector = new Vector3();
            if (Keyboard.current.wKey.IsPressed())
                movementVector.z += 1;
            if (Keyboard.current.sKey.IsPressed())
                movementVector.z += -1;
            if (Keyboard.current.dKey.IsPressed())
                movementVector.x += 1;
            if (Keyboard.current.aKey.IsPressed())
                movementVector.x += -1;
            transform.Translate(speed * Time.fixedDeltaTime * movementVector);

			if (Pointer.current.press.wasPressedThisFrame)
				Shoot();
		}

        private void Shoot()
        {
            var hit = GetWorldPositionOnPlane(Pointer.current.position.ReadValue());
            hit.y = ProjectileSpawnTransform.position.y;
			var projectile = PhotonNetwork.Instantiate(PROJECTILE_PREFAB, ProjectileSpawnTransform.position, Quaternion.LookRotation(hit - ProjectileSpawnTransform.position, Vector3.up)).GetComponent<Projectile>();
            projectile.photonView.TransferOwnership(PhotonNetwork.MasterClient);

			Vector3 GetWorldPositionOnPlane(Vector2 screenPosition, float y = 0)
			{
				Ray ray = _cachedCamera.ScreenPointToRay(screenPosition);
				Plane xz = new(Vector3.up, new Vector3(0, y, 0));
				xz.Raycast(ray, out float distance);
				return ray.GetPoint(distance);
			}
		}
    }
}
