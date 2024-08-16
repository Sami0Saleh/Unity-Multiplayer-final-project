using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

public class PlayerShaderSelector : MonoBehaviour
{
    [SerializeField] private Material knightMat;
    [SerializeField] private Material[] shaders;
    [SerializeField] private Renderer bodyTopHalf;
    [SerializeField] private Renderer bodyBottomHalf;

    private void Update()
    {
        //This is for testing
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("Pressed Z");
            SetMatColor("Red");
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Pressed X");
            SetMatColor("Green");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("Pressed C");
            SetMatColor("Blue");
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("Pressed V");
            SetMatColor("Yellow");
        }
    }

    public void SetMatColor(string color)
    {
        switch (color)
        {
            case "Red": SetMat(0);
                break;
            case "Green": SetMat(1);
                break;
            case "Blue": SetMat(2);
                break;
            case "Yellow": SetMat(3);
                break;
            default: SetMat(0);
                break;
        }
    }

    private void SetMat(int index)
    {
        bodyTopHalf.SetMaterials(new List<Material>() { knightMat, shaders[index] });
        bodyBottomHalf.SetMaterials(new List<Material>() { knightMat, shaders[index] });
    }
}
