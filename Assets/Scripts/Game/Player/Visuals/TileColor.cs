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
        [SerializeField] private MeshRenderer _tileRenderer;
        [SerializeField] private Material _oldMaterial;
        [SerializeField] private Material _newMaterial;

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
            if (state == Cursor.State.Neutral)
            {

                foreach (var tile in Pathfinding.GetTraversableArea(Pawn.Mine.Position, PawnMovement.MAX_STEPS, _board.TraversableArea))
                {
                    _tileRenderer = _board.Tiles1[tile].GetComponentInChildren<MeshRenderer>(true);
                    _tileRenderer.materials[1] = _newMaterial;
                }
            }
            else
            {
                foreach (var tile in _board.TraversableArea)
                {
                    _tileRenderer = _board.Tiles1[tile].GetComponentInChildren<MeshRenderer>(true);
                    _tileRenderer.materials[1] = _oldMaterial;
                }

            }
        }
    }
}