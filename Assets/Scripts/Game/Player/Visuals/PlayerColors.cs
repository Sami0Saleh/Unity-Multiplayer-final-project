using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using static Game.Player.Visuals.PlayerColorsExtension;

namespace Game.Player.Visuals
{
	/// <summary>
	/// Contains data pertaining to associating <see cref="Material"/>s and <see cref="Color"/>s to <see cref="Game.Player.Pawn"/> and <see cref="Photon.Realtime.Player"/>s.
	/// </summary>
	[CreateAssetMenu(fileName = "PlayerColors", menuName = "Scriptable Objects/PlayerColors")]
	public class PlayerColors : ScriptableObject, IReadOnlyDictionary<string, ColorProfile>
	{
		public const string COLOR_PROPERTY = "PlayerColor";
		public const string DEFAULT_COLOR = "None";

		[SerializeField] private ColorProfile _defaultProfile;
		[SerializeField] private List<ColorProfile> _profiles;
		private Dictionary<string, ColorProfile> _dict;

		private void OnEnable()
		{
			InitDictionary();

			void InitDictionary()
			{
				_dict = new(_profiles.Count + 1)
		{ { DEFAULT_COLOR, _defaultProfile } };
				foreach (var profile in _profiles)
					_dict.Add(profile.outlineMaterial.name, profile);
			}
		}

		public ColorProfile this[string key] { get => _dict[key]; set => _dict[key] = value; }

		#region DICTIONARY_OVERRIDES
		public IEnumerable<string> Keys => _dict.Keys;

		public IEnumerable<ColorProfile> Values => _dict.Values;

		public int Count => _dict.Count - 1;

		public bool ContainsKey(string key) => _dict.ContainsKey(key);

		public IEnumerator<KeyValuePair<string, ColorProfile>> GetEnumerator() => _dict.GetEnumerator();

		public bool TryGetValue(string key, out ColorProfile value) => _dict.TryGetValue(key, out value);

		public string this[int index] => _dict.ElementAt(index + 1).Key;

		public int IndexOf(string color)
		{
			int index = -1;
			foreach (var key in Keys)
			{
				if (key.Equals(color))
					return index;
				index++;
			}
			return -1;
		}

		IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_dict).GetEnumerator();
		#endregion
	}

	/// <summary>
	/// Assortment of convenience functions extending <see cref="PlayerColors"/>.
	/// </summary>
	public static class PlayerColorsExtension
	{
		public static bool HasColorProperty(this Hashtable properties) => properties.ContainsKey(PlayerColors.COLOR_PROPERTY);

		public static bool TryGetColorProperty(this Photon.Realtime.Player player, out string color) => player.CustomProperties.TryGetColorProperty(out color);

		public static void SetColorProperty(this Photon.Realtime.Player player, string color)
		{
			var hashedProperties = new Hashtable
			{
				[PlayerColors.COLOR_PROPERTY] = color
			};
			player.SetCustomProperties(hashedProperties);
		}

		public static bool TryGetColorProperty(this Hashtable properties, out string color)
		{
			if (properties.TryGetValue(PlayerColors.COLOR_PROPERTY, out var colorObj))
			{
				color = colorObj.ToString();
				return true;
			}
			color = null;
			return false;
		}

		public static bool TryGetMaterial(this PlayerColors playerColors, Photon.Realtime.Player player, out ColorProfile profile)
		{
			profile = default;
			return player.TryGetColorProperty(out var color) && playerColors.TryGetValue(color, out profile);
		}

		[Serializable]
		public struct ColorProfile
		{
			public Material outlineMaterial;
			public Material lineMaterial;

			public Color Color => outlineMaterial.color;
		}
	}
}