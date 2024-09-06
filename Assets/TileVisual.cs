using Game;
using Game.Player;
using System;
using UnityEngine;

public class TileVisual : MonoBehaviour
{
    [SerializeField] MeshRenderer mRenderer;
    [SerializeField] Material NewMaterial;
    [SerializeField] Material OldMaterial;
    private void Awake()
    {
        TurnIterator.Instance.OnTurnChange += PaintTile;
    }

    private void PaintTile(TurnIterator.TurnChangeEvent arg0)
    {
        mRenderer.material = NewMaterial;
       // throw new NotImplementedException();
    }
}



