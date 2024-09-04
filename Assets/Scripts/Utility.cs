using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public static class Utility
{
	/// <returns>The player number for this player. (0-3)</returns>
	public static int GetPlayerNumber(Player player) => PhotonNetwork.PlayerList.AsEnumerable().OrderBy(p => p.ActorNumber).ToList().IndexOf(player);

	/// <returns>Like the '%' operator, but can account for negative integers.</returns>
	public static int Modulo(int a, int b) => ((a %= b) < 0) ? a + b : a;

	/// <summary>
	/// Calls <see cref="Object.Destroy"/> on all of <paramref name="parent"/>'s children.
	/// </summary>
	public static void DestroyAllChildren(this Transform parent)
	{
        foreach (var item in parent.GetAllChildren())
			UnityEngine.Object.Destroy(item.gameObject);
    }

	/// <summary>
	/// Calls <see cref="Object.DestroyImmediate"/> on all of <paramref name="parent"/>'s children.
	/// </summary>
	public static void DestroyAllChildrenImmediate(this Transform parent)
	{
		foreach (var item in parent.GetAllChildren())
			UnityEngine.Object.DestroyImmediate(item.gameObject);
	}

	/// <returns>All children transforms of <paramref name="parent"/>.</returns>
	public static IEnumerable<Transform> GetAllChildren(this Transform parent)
	{
		for (int n = parent.childCount - 1; n >= 0; n--)
			yield return parent.GetChild(n);
	}

	public static bool AllUnique<TSource>(this IEnumerable<TSource> source) => source.Distinct().Count() == source.Count();
}
