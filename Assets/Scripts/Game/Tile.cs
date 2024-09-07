using UnityEngine;

namespace Game
{
	public class Tile : MonoBehaviour
	{
		[field: SerializeField] public MeshRenderer Renderer { get; private set; }
	}
}
