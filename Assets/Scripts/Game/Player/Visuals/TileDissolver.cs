using UnityEngine;

public class TileDissolver : MonoBehaviour
{
    private const int DISSOLVE_MAT_INDEX = 0;
    private const int OUTLINE_MAT_INDEX = 1;

    [SerializeField] private MeshRenderer _renderer;

    private float value = 1;
    private bool _isDissolving = false;

    private void Start()
    {
        value = 1f;
        _renderer.materials[DISSOLVE_MAT_INDEX].SetFloat("_DissolveValue", value);
        _renderer.materials[OUTLINE_MAT_INDEX].SetFloat("_Opacity", 1f);
    }

    private void Update()
    {
        if (_isDissolving && value >= 0)
        {
            value -= 0.01f;
            _renderer.materials[DISSOLVE_MAT_INDEX].SetFloat("_DissolveValue", value);
        }
        else if (value <= 0)
        {
            //Destroy me
        }
    }

    [ContextMenu("StartDissolving")]
    public void StartDissolving()
    {
        _isDissolving = true;
        _renderer.materials[OUTLINE_MAT_INDEX].SetFloat("_Opacity", 0f);
    }
}
