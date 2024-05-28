using UnityEngine;
using Photon.Pun;

public class NetworkedObject : MonoBehaviourPunCallbacks
{
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected");
	}
}
