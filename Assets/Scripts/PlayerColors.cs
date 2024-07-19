using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

[CreateAssetMenu(fileName = "PlayerColors", menuName = "Scriptable Objects/PlayerColors")]
public class PlayerColors : ScriptableObject, IDictionary<string, Material>
{
	public const string COLOR_PROPERTY = "PlayerColor";
	public const string DEFAULT_COLOR = "None";

	[SerializeField] private Material _defaultColor;
    [SerializeField] private List<Material> _materials;
	private Dictionary<string, Material> _dict;

	private void OnEnable()
	{
		InitDictionary();

		void InitDictionary()
		{
			_dict = new(_materials.Count + 1)
			{ { DEFAULT_COLOR, _defaultColor } };
			foreach (var mat in _materials)
				_dict.Add(mat.name, mat);
		}
	}

	public Material this[string key] { get => ((IDictionary<string, Material>)_dict)[key]; set => ((IDictionary<string, Material>)_dict)[key] = value; }

	#region DICTIONARY_OVERRIDES
	public ICollection<string> Keys => ((IDictionary<string, Material>)_dict).Keys;

	public ICollection<Material> Values => ((IDictionary<string, Material>)_dict).Values;

	public int Count => ((ICollection<KeyValuePair<string, Material>>)_dict).Count-1;

	public bool IsReadOnly => true;

	public void Add(string key, Material value) => throw new InvalidOperationException("PlayerColors dictionary is readonly.");

	public void Add(KeyValuePair<string, Material> item) => throw new InvalidOperationException("PlayerColors dictionary is readonly.");

	public void Clear() => throw new InvalidOperationException("PlayerColors dictionary is readonly.");

	public bool Contains(KeyValuePair<string, Material> item) => (_dict as ICollection<KeyValuePair<string, Material>>).Contains(item);

	public bool ContainsKey(string key) => (_dict as IDictionary<string, Material>).ContainsKey(key);

	public void CopyTo(KeyValuePair<string, Material>[] array, int arrayIndex) => (_dict as ICollection<KeyValuePair<string, Material>>).CopyTo(array, arrayIndex);

	public IEnumerator<KeyValuePair<string, Material>> GetEnumerator() => (_dict as IEnumerable<KeyValuePair<string, Material>>).GetEnumerator();

	public bool Remove(string key) => throw new InvalidOperationException("PlayerColors dictionary is readonly.");

	public bool Remove(KeyValuePair<string, Material> item) => throw new InvalidOperationException("PlayerColors dictionary is readonly.");

	public bool TryGetValue(string key, out Material value) => (_dict as IDictionary<string, Material>).TryGetValue(key, out value);

	public string this[int index] => _dict.ElementAt(index+1).Key;

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

public static class PlayerColorsExtension
{
	public static bool HasColorProperty(this Hashtable properties) => properties.ContainsKey(PlayerColors.COLOR_PROPERTY);

	public static bool TryGetColorProperty(this Player player, out string color) => player.CustomProperties.TryGetColorProperty(out color);

	public static void SetColorProperty(this Player player, string color)
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

	public static bool TryGetMaterial(this PlayerColors playerColors, Player player, out Material material)
	{
		material = null;
		return player.TryGetColorProperty(out var color) && playerColors.TryGetValue(color, out material);
	}
}
