#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace WeightedSpawning
{
	/// <summary>
	/// Lets us see the result of a spawner without running the game. Nothing special.
	/// </summary>
	[CustomEditor(typeof(WeightedSpawner))]
	public class WeightedSpawnerInspector : Editor
	{
		private string m_Result = "";
		private string m_EnemyCount = "";
		private System.Collections.Generic.Dictionary<SpawnableEnemy, int> m_EnemyCounts = new();

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI(); // Default inspector
			if (GUILayout.Button("Solve"))
			{
				try
				{
					WeightedSpawner spawner = target as WeightedSpawner;
					if (spawner)
					{
						spawner.Solve();
						Knapsack.Solver<SpawnableEnemy> solver = spawner.Solver;
						m_Result = solver.ToString();
						m_EnemyCounts.Clear();
						for (int i = 0; i < solver.Length; i++)
						{
							if (m_EnemyCounts.TryGetValue(solver[i], out int count)) m_EnemyCounts[solver[i]] = count + 1;
							else m_EnemyCounts.Add(solver[i], 1);
						}
						System.Text.StringBuilder sb = new System.Text.StringBuilder();
						foreach(var pair in  m_EnemyCounts)
						{
							sb.Append(pair.Key.name);
							sb.Append(" * ");
							sb.Append(pair.Value);
							sb.Append(System.Environment.NewLine);
						}
						m_EnemyCount = sb.ToString();
					}
					else m_Result = m_EnemyCount = "?";
				}
				catch (System.Exception e)
				{
					m_Result = "Err: " + e.ToString();
					m_EnemyCount = "";
				}
			}
			GUILayout.Label(m_Result);
			GUILayout.Label(m_EnemyCount);
		}
	}
}
#endif