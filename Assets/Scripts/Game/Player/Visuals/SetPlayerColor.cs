using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using PunPlayer = Photon.Realtime.Player;

namespace Game.Player.Visuals
{
	public class SetPlayerColor : MonoBehaviourPun
	{
		[SerializeField] private PlayerColors _colorConfig;
		[SerializeField] private List<Renderer> _renderers;

		public PunPlayer ThisPlayer => photonView.Owner;

		private void Awake()
		{
			SetMaterial();

			void SetMaterial()
			{
				if (_colorConfig.TryGetMaterial(ThisPlayer, out var profile))
					SetOutline(profile.outlineMaterial);
			}
			Destroy(this);
		}

		public void SetOutline(Material mat)
		{
			foreach (Renderer renderer in _renderers)
				renderer.SetMaterials(renderer.materials.Append(mat).ToList());
		}
	}
}