using UnityEngine;

namespace WeightedSpawning
{
	[CreateAssetMenu(fileName = "WeightedSpawningItem", menuName = "Scriptable Objects/WeightedSpawningItem")]
	public class WeightedSpawningItem : ScriptableObject
	{
		public int Weight;
		public int Value;
		public Color Color;
	}
}