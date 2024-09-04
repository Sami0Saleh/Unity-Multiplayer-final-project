using System;
using UnityEngine;
using ExitGames.Client.Photon;
using static Game.TurnIterator;
using static Game.Player.PawnMovement;

public static class PunRegisterCustomTypes
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Register()
	{
		if (!PhotonPeer.RegisterType(typeof(TurnChangeEvent), (byte)'T', TurnChangeEvent.Serialize, TurnChangeEvent.Deserialize))
			LogError(typeof(TurnChangeEvent));
		if (!PhotonPeer.RegisterType(typeof(PawnMovementEvent), (byte)'M', PawnMovementEvent.Serialize, PawnMovementEvent.Deserialize))
			LogError(typeof(PawnMovementEvent));

		static void LogError(Type type) => Debug.LogWarning($"Couldn't register custom type {type} for serialization.");
	}
}
