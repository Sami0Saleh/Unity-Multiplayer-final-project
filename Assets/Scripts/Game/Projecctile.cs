using UnityEngine;
using Photon.Pun;

public class Projecctile : MonoBehaviourPun
{
    public GameObject VisualPanel;
    [SerializeField] private float speed = 20f;

    void Update()
    {
        if(photonView.IsMine)
            transform.Translate(Vector3.forward * (Time.deltaTime * speed));
    }

   
}
