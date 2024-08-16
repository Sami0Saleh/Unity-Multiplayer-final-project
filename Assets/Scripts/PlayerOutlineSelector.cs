using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerOutlineSelector : MonoBehaviourPun
{
	[SerializeField] private PlayerColors _colorConfig;
	[SerializeField] private Material knightMat;
    [SerializeField] private Renderer bodyTopHalf;
    [SerializeField] private Renderer bodyBottomHalf;

	public Player ThisPlayer => photonView.Owner;

	private void Awake()
	{
		SetMaterial();

		void SetMaterial()
		{
			if (_colorConfig.TryGetMaterial(ThisPlayer, out var mat))
				SetOutline(mat);
		}
		Destroy(this);
	}

	public void SetOutline(Material mat)
    {
		bodyTopHalf.SetMaterials(new List<Material>() {knightMat, mat});
        bodyBottomHalf.SetMaterials(new List<Material>() {knightMat, mat});
    }
}
