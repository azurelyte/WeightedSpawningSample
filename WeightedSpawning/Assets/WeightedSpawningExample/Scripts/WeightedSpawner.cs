using UnityEngine;
using System.Collections.Generic;

namespace WeightedSpawning
{
	/// <summary>
	/// Periodically spawns groups of <see cref="SpawnableEnemy"/>'s in a flat circle around the world origin.
	/// </summary>
	public class WeightedSpawner : MonoBehaviour
	{
		const string LOG_HEADER = "[" + nameof(WeightedSpawner) + "]";
		const int MAX_CAPACITY = 128;
		const int MAX_ITEMS = 128;

		[Tooltip("Think of this as the difficulty ceiling for your level. The spawner will do it's best to reach an enemy value/difficulty that matches this number.")]
		[Range(1, MAX_CAPACITY)]
		public int Capacity;
		[Tooltip("Rate of spawning.")]
		[Range(1, 16)]
		public float SpawnEveryXSeconds = 16;
		private float m_Timer = 0;
		[Tooltip("How far away from the center of the world should enemies be spawned?")]
		public float SpawnRange = 10;

		/// <summary>
		/// Array of items that will be used to spawn enemies in the world. Although private, this is assignable from the inspector and is assummed to be mutable at
		/// any time. Item ordering does not matter.
		/// </summary>
		[SerializeField]
		private SpawnableEnemy[] m_Items;
		/// <summary>
		/// Internal solver for the spawner. Items are added to the solver on each solve call. Use <see cref="Solver"/> for public access.
		/// </summary>
		private Knapsack.Solver<SpawnableEnemy> m_Solver;
		/// <summary>
		/// Pointer to the <see cref="Knapsack.Solver{T}"/> used to compute the resulting enemy waves calculated by this class. Note that
		/// you shouldn't call solve on this directly and should use the <see cref="WeightedSpawner"/>.<see cref="Solve"/> function to properly
		/// capture the list of items that this class is relying on. You should, however, access the solver if you want to look at the resulting values.
		/// </summary>
		public Knapsack.Solver<SpawnableEnemy> Solver { get { return m_Solver == null ? m_Solver = new Knapsack.Solver<SpawnableEnemy>(MAX_CAPACITY, MAX_ITEMS) : m_Solver; } }
		/// <summary> Object pools for all the enemies in the example. </summary>
		private Dictionary<SpawnableEnemy, Stack<GameObject>> m_EnemyPools = new();

		private void OnValidate()
		{
			if (m_Items != null && m_Items.Length > MAX_ITEMS)
			{
				Debug.LogWarning($"{LOG_HEADER} You have assigned more items than is supportted by the weighted spawner. A limit of {MAX_ITEMS} is enforced in code to make sure things don't" +
					$" get out of hand. If you need more, you can increase this by editing the value {nameof(MAX_ITEMS)} in {nameof(WeightedSpawner)}.cs");
				SpawnableEnemy[] arr = new SpawnableEnemy[MAX_ITEMS];
				for(int i = 0; i < MAX_ITEMS; i++) arr[i] = m_Items[i];
				m_Items = arr;
			}
		}

		private void Update()
		{
			m_Timer -= Time.deltaTime;
			if (m_Timer > 0) return;
			m_Timer = Mathf.Max(SpawnEveryXSeconds, 1);
			Solve();
			for (int i = 0; i < m_Solver.Length; i++)
			{
				float piVal = (Random.value * Mathf.PI * 2);
				Vector3 spawnPos = new Vector3(Mathf.Sin(piVal) * SpawnRange, 0, Mathf.Cos(piVal) * SpawnRange);
				SpawnableEnemy enemyData = m_Solver[i];
				if (!m_EnemyPools.TryGetValue(enemyData, out Stack<GameObject> pool)) m_EnemyPools.Add(enemyData, pool = new Stack<GameObject>());
				GameObject spawnedEnemy = pool.Count > 0 ? pool.Pop() : enemyData.Spawn(spawnPos, pool);
				spawnedEnemy.transform.position = spawnPos;
				spawnedEnemy.SetActive(true);
			}
		}

		public void Solve()
		{
			Solver.Clear();
			// It is assumed that the items array may change it's contents between frames/Solve calls. So we must rebuild the
			// items array in the solver each time.
			foreach (SpawnableEnemy item in m_Items) if (item && item.IsValid()) m_Solver.AddItem(item.Weight, item.Value, item);
			m_Solver.Solve(Capacity);
			// Could also introduce some degree of randomness into the capacity to make solve calls varried. Knapsack is deterministic.
			//m_Solver.Solve(Random.Range(Mathf.Max(Capacity, 2) / 2, Capacity));
		}

		[ContextMenu("Test")]
		void Test() => Solve();
	}
}
