using UnityEngine;

public class Dissolve : MonoBehaviour
{
    private const int DISSOLVE_MAT_INDEX = 0;
    private const int OUTLINE_MAT_INDEX = 1;

    [SerializeField] private MeshRenderer _renderer;

    private float _value = 1;

	private void Start()
    {
        _value = 1f;
        _renderer.materials[DISSOLVE_MAT_INDEX].SetFloat("_DissolveValue", _value);
		_renderer.materials[OUTLINE_MAT_INDEX].SetFloat("_Opacity", 0f);
	}

    private void Update()
    {
        _value -= 0.01f;
        _renderer.materials[DISSOLVE_MAT_INDEX].SetFloat("_DissolveValue", _value);
    }
}
