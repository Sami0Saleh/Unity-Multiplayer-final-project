using System.Linq;
using Photon.Pun;
using Photon.Realtime;

public class Utility
{
	/// <returns>The player number for this player. (0-3)</returns>
	public static int GetPlayerNumber(Player player)
	{
		return PhotonNetwork.PlayerList.AsEnumerable().OrderBy(p => p.ActorNumber).ToList().IndexOf(player);
	}
}
