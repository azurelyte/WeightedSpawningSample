using UnityEngine;

namespace WeightedSpawning
{
	[CreateAssetMenu(fileName = "SpawnableEnemy", menuName = "Scriptable Objects/SpawnableEnemy")]
	public class SpawnableEnemy : ScriptableObject
	{
		[Tooltip("When the game wants to spawn this enemy in, this is how valuable/difficult the enemy is. " +
			"Enemies with a high value are prioritized over those with a lower value when their weights are equal.")]
		public int Value;
		[Tooltip("When trying to fill the level think of this as the space this enemy will take up. Enemies with " +
			"a higher value/difficulty should (probably) have a greater weight.")]
		public int Weight;
		[Tooltip("Prefab to create when spawning this enemy.")]
		public GameObject Prefab;

		/// <returns>True when a valid gameobject would be created from the <see cref="Spawn(Vector3, System.Collections.Generic.Stack{GameObject})"/> function.</returns>
		public bool IsValid()
		{
			return Value > 0 && Weight > 0 && Prefab;
		}

		/// <param name="worldPosition">Where in the world the <see cref="Prefab"/> should be spawned.</param>
		/// <param name="objectPool">Optional. When provided, the prefab will be provided with a object pool to avoid having to Destroy/Instantiate in the future.</param>
		/// <returns>Instance of <see cref="Prefab"/></returns>
		public GameObject Spawn(Vector3 worldPosition, System.Collections.Generic.Stack<GameObject> objectPool = null)
		{
			if (!Application.isPlaying) return null;
			GameObject inst = GameObject.Instantiate(Prefab, worldPosition, Quaternion.identity);
			if (objectPool != null) inst.GetComponent<Cubie>()?.SetObjectPool(objectPool);
			return inst;
		}
	}
}