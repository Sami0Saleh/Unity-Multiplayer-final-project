using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using static Game.Player.Pawn;
using static Game.Pathfinding;
using UnityEditor;

namespace Game.Player.Visuals
{
    public class TileColor : MonoBehaviour
    {
        // get pawn from cursor
        [SerializeField] private Cursor _cursor;
        [SerializeField] private Material _tileMaterial;
        private Board _board;
        private void Start() => _board = Board.Instance;
        private void OnEnable()
        {
            _cursor.StateChanged += OnChangeState;
        }

        private void OnDisable()
        {
            _cursor.StateChanged -= OnChangeState;
        }

        private void OnChangeState(Cursor.State state)
        {
            if (state == Cursor.State.Move)
            {

                foreach (var tile in Pathfinding.GetTraversableArea(Pawn.Mine.Position, PawnMovement.MAX_STEPS, _board.TraversableArea))
                {
                    Debug.Log("Change color");
                    _tileMaterial.color = Color.black;
                }
            }
        }
    }
}