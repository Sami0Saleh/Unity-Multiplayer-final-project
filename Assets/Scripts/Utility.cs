using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class Utility
{
	/// <returns>The player number for this player. (0-3)</returns>
	public static int GetPlayerNumber(Player player) => PhotonNetwork.PlayerList.AsEnumerable().OrderBy(p => p.ActorNumber).ToList().IndexOf(player);

	/// <returns>Like the '%' operator, but can account for negative integers.</returns>
	public static int Modulo(int a, int b) => ((a %= b) < 0) ? a + b : a;
}
