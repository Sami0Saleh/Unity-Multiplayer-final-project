using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviourPun
{
	[SerializeField] private GameObject _visuals;
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

	public void SetVisibility(bool visible) => _visuals.SetActive(visible);
}
