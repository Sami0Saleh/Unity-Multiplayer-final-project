using System;
using UnityEngine;
using ExitGames.Client.Photon;
using static Game.TurnIterator;

public static class PunRegisterCustomTypes
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Register()
	{
		if (!PhotonPeer.RegisterType(typeof(TurnChangeEvent), (byte)'T', TurnChangeEvent.Serialize, TurnChangeEvent.Deserialize))
			LogError(typeof(TurnChangeEvent));

		static void LogError(Type type) => Debug.LogWarning($"Couldn't register custom type {type} for serialization.");
	}
}
