using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Player
{
	public class MousePositionTracker : MonoBehaviour
	{
		[SerializeField] private Camera _camera;
		private Vector2 _screenPosition = Vector2.zero;

		[field: SerializeField] public Transform Target { get; private set; }

		private void OnEnable() => Pawn.Mine.InputActions.Cursor.Look.performed += OnLook;

		private void OnDisable() => Pawn.Mine.InputActions.Cursor.Look.performed -= OnLook;

		private void Start()
		{
			if (_camera == null)
				_camera = Camera.main;
		}

		public void Update() => Target.position = GetWorldPositionOnPlane(_screenPosition);

		private void OnLook(InputAction.CallbackContext context) => _screenPosition = context.ReadValue<Vector2>();

		private Vector3 GetWorldPositionOnPlane(Vector2 screenPosition, float y = 0)
		{
			Ray ray = _camera.ScreenPointToRay(screenPosition);
			Plane xz = new(Vector3.up, new Vector3(0, y, 0));
			xz.Raycast(ray, out float distance);
			return ray.GetPoint(distance);
		}
	}
}
