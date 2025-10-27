#if UNITY_64 || UNITY_32 // In case we aren't in a unity project.
#define KNAPSACK_LOGS // Undefine to remove logging to the unity console.
#define UNITY
using UnityEngine;
#endif

namespace WeightedSpawning
{
	/// <summary>
	/// The Knapsack problem is a common dynamic programming problem. Where by given a knapsack of a fixed size 
	/// and a set of items with associated values we fill the knapsack with the items that result in the highest
	/// resulting value without exceeding the knapsacks capacity.
	/// </summary>
	public static class Knapsack
	{
		/// <summary>
		/// Helper class. Use this for repeated calls to the knapsack algo. Uses fixed size arrays. For the sake of making
		/// iteration easier, you can treat this as an array with the Length property and int property indexer to get the
		/// items out of the solution.
		/// </summary>
		public class Solver<T>
		{
			private const string SOLVER_LOG_HEADER = "[" + LOG_HEADER + "." + nameof(Solver<T>) + "]";
			public const int DEFAULT_MAX_CAPACITY = 64;
			public const int DEFAULT_ITEM_LIMIT = 64;

			private int m_ItemIndex;
			private int[] m_Weights;
			private int[] m_Values;
			private T[] m_Items;
			private int[][] m_DpArray;
			private int m_NumItemsInSolution;
			private int[] m_Solution;
			private int m_ResultingValue;
			/// <summary>When true, the solver has a result.</summary>
			public bool HasResult { get => m_ResultingValue >= 0; }
			/// <summary>Result of <see cref="Solve(int)"/></summary>
			public int Result { get => m_ResultingValue; }
			/// <summary>The number of items in the solver's solution buffer. See also <seealso cref="this[int]"/></summary>
			public int Length { get => m_NumItemsInSolution; }
			/// <summary>Get an item out of the solution buffer.</summary>
			/// <returns><see cref="default"/> if index is out of range, and a valid item otherwise.</returns>
			public T this[int i] { get => i < 0 || i >= m_NumItemsInSolution ? default : m_Items[m_Solution[i]]; }
			/// <summary>Solver uses fixed size arrays. This is the maximum weight supportted by the current arrangement of those arrays</summary>
			public int MaxCapacity { get; private set; }
			/// <summary>Solver uses fixed size arrays. This is the maximum number of items supportted by the current arrangement of those arrays</summary>
			public int MaxNumberOfItems { get; private set; }

			public Solver(int capacityLimit = DEFAULT_MAX_CAPACITY, int itemLimit = DEFAULT_ITEM_LIMIT)
			{
				m_NumItemsInSolution = m_ItemIndex = 0;
				m_ResultingValue = -1;
				MaxCapacity = capacityLimit;
				MaxNumberOfItems = itemLimit;
				m_Items = new T[itemLimit];
				m_Weights = new int[itemLimit];
				m_Values = new int[itemLimit];
				m_Solution = new int[itemLimit];
				m_DpArray = new int[itemLimit][];
				for (int i = 0; i < itemLimit; i++) m_DpArray[i] = new int[capacityLimit + 1];
			}

			/// <summary>
			/// Adds an item to the solver.
			/// </summary>
			/// <param name="weight">Must be greater than 0.</param>
			/// <param name="value">Must be greater than 0.</param>
			public void AddItem(int weight, int value, T Item)
			{
				if (weight <= 0)
				{
					LogWarning($"{SOLVER_LOG_HEADER} item weight must be greater than 0 to be valid. The weight will be interperted as 1. Was this intentional?");
					weight = 1;
				}
				if (value <= 0)
				{
					LogWarning($"{SOLVER_LOG_HEADER} item value must be greater than 0 as the algorithim will never waste weight on an item of 0 or negative value. " +
						$"The item will be ignored. Was this intentional?");
					return;
				}
				if (m_ItemIndex >= MaxNumberOfItems)
				{
					LogError($"{SOLVER_LOG_HEADER} Unable to add item to solver as there is no more room in the solver. You need to create a solver with " +
						$"a higher {nameof(MaxNumberOfItems)} or solve with less items.");
					return; // Could also throw exception. But I prefer to not have my games crash/stop their threads of execution and program resiliantly.
				}
				m_Weights[m_ItemIndex] = weight;
				m_Values[m_ItemIndex] = value;
				m_Items[m_ItemIndex++] = Item;
			}

			public void Clear()
			{
				for (int i = 0; i < m_Items.Length; i++) m_Items[i] = default;
				m_NumItemsInSolution = m_ItemIndex = 0;
				m_ResultingValue = -1;
			}

			/// <summary>
			/// Computes a solution given the provided items and capacity. Use <see cref="Length"/> and <see cref="this[int]"/> to iterate
			/// individual items in the result.
			/// </summary>
			/// <param name="capacity">Clamped between 1 and <see cref="MaxCapacity"/>, inclusive.</param>
			/// <returns>The value of all items added together.</returns>
			public int Solve(int capacity)
			{
#if UNITY
				capacity = Mathf.Clamp(capacity, 1, MaxCapacity);
#else
				capacity = System.Math.Clamp(capacity, 1, MaxCapacity);
#endif
				return m_ResultingValue = Knapsack.Solve(capacity, m_ItemIndex, m_Weights, m_Values, out m_NumItemsInSolution, ref m_Solution, ref m_DpArray);
			}

			public override string ToString()
			{
				if (m_ItemIndex <= 0) return "No Items";
				// For if you're poking around wanting to confirm the contents of the algo //
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				if (!HasResult)
				{
					sb.Append("Unsolved");
					return sb.ToString();
				}
				sb.Append("Result: ");
				sb.Append(m_ResultingValue);
				sb.AppendLine("\nIn Values");
				for (int t = 0; t < m_ItemIndex; t++)
				{
					sb.Append(m_Values[t]);
					sb.Append('|');
				}
				sb.AppendLine("\nIn Weights");
				for (int t = 0; t < m_ItemIndex; t++)
				{
					sb.Append(m_Weights[t]);
					sb.Append('|');
				}
				// If you want to, you may enable this code to print the DP array of the solver which
				// displays the solution space of the problem.
				//sb.AppendLine("\nValue table<mspace=0.5em>");
				//for (int i = 0; i <= m_ItemIndex; i++)
				//{
				//	sb.Append($"{i}:::");
				//	for (int w = 0; w <= MaxCapacity; w++)
				//	{
				//		sb.Append(m_DpArray[i][w]);
				//		sb.Append("|");
				//	}
				//	sb.Append($":::{i}   rval = {(i > 0 ? m_Values[i - 1] : 0)}\n");
				//}
				//sb.Append("</mspace>");
				return sb.ToString();
			}
		}

		const string LOG_HEADER = "[" + nameof(Knapsack) + "]";

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] // Strip in release builds.
#if KNAPSACK_LOGS
		private static void LogError(object o) => UnityEngine.Debug.LogError($"{LOG_HEADER} {(o?.ToString() ?? "null")}");
#else
		private static void LogError(object o) { }
#endif

		[System.Diagnostics.Conditional("UNITY_EDITOR"), System.Diagnostics.Conditional("DEVELOPMENT_BUILD")] // Strip in release builds.
#if KNAPSACK_LOGS
		private static void LogWarning(object o) => UnityEngine.Debug.LogWarning($"{LOG_HEADER} {(o?.ToString() ?? "null")}");
#else
		private static void LogWarning(object o) { }
#endif

		/// <summary>
		/// Given a capacity and a set of weights and values, return the highest value that can be created of the items.
		/// The number of items selected and the indexes of those items are also computed.
		/// </summary>
		/// <param name="capacity">How much weight can we use?</param>
		/// <param name="numItems">The number of items contained within weights/values.</param>
		/// <param name="weights">The weights of each individual item.</param>
		/// <param name="values">The value of each individual item.</param>
		/// <param name="numSelected">The number of valid indexes contained within the <paramref name="selection"/></param>
		/// <param name="selection">An array of indexes that maps to an item whose value is included in the result. If null or of insufficient size, this will be reassigned.</param>
		/// <param name="dp">
		/// A staggered array, for the Dynamic Programming part of the algorithim, of at least the size of the number of (items + 1) by (capacity + 1).
		/// If null or of insufficient size, this will be reassigned.
		/// </param>
		/// <returns>-1 if an error occurs. Otherwise, returns the cumulative value of all items that are contained in the result.</returns>
		public static int Solve(int capacity, int numItems, int[] weights, int[] values, out int numSelected, ref int[] selection, ref int[][] dp)
		{
			// Programmers note...
			// The reason we have the numItems and numSelected and ref to dp is to keep us using fixed size arrays. We don't want to be allocating memory
			// repeatedly as Unity's garbage collecter sucks and will eventually block the render thread while collecting.
			// Lets sanatize/error check here.
			numSelected = 0;
			if (capacity <= 0)
			{
				LogWarning("Capacity is <= 0. Was this intentional? No items will be selected.");
				return 0;
			}
			if (numItems <= 0)
			{
				LogWarning("The number of items is <= 0. Was this intentional? There isn't anything to solve.");
				return 0;
			}
			if (weights == null || weights.Length <= 0)
			{
				LogError("No item weights have been provided.");
				return -1;
			}
			if (values == null || values.Length <= 0)
			{
				LogError("No item values have been provided.");
				return -1;
			}
			if (numItems > weights.Length) LogWarning("The number of items indicated out-sizes the item weights buffer.");
			if (numItems > values.Length) LogWarning("The number of items indicated out-sizes the item values buffer.");
#if UNITY
			numItems = Mathf.Min(numItems, weights.Length, values.Length);
#else
			numItems = System.Math.Min(numItems, System.Math.Min(weights.Length, values.Length));
#endif
			if (dp == null || dp.Length <= numItems) dp = new int[numItems + 1][]; // ensure dp has space for items
			for (int i = 0; i < dp.Length; i++) if (dp[i] == null || dp[i].Length <= capacity) dp[i] = new int[capacity + 1]; // ensure dp has space for capacity
			if (selection == null || selection.Length <= numItems) selection = new int[numItems];
			return InternalSolve(capacity, numItems, weights, values, out numSelected, ref selection, ref dp);
		}

		/// <summary>
		/// Runs the knapsack algorithim using a particularily space efficient form. If you need to see what is selected, 
		/// see <see cref="InternalSolve(int, int, int[], int[], out int, ref int[], ref int[][])"/>
		/// </summary>
		/// <returns>Knapsak value given the capacity.</returns>
		private static int InternalQuickSolve(int capacity, int numItems, int[] weights, int[] values, ref int[] dp)
		{
			if (numItems <= 0) return 0;
			for (int i = 1; i <= numItems; i++)
			{
				for (int j = capacity; j >= weights[i - 1]; j--)
				{
#if UNITY
					dp[j] = Mathf.Max(dp[j], dp[j - weights[i - 1]] + values[i - 1]);
#else
					dp[j] = System.Math.Max(dp[j], dp[j - weights[i - 1]] + values[i - 1]);
#endif
				}
			}
			return dp[capacity];
		}

		/// <summary>
		/// Runs the knapsack algorithim. If you do not need to know which items are selected, please use <see cref="InternalQuickSolve"/>
		/// as it is much faster/space efficient. This version steps back through staggered arrays to compile the selection array.
		/// </summary>
		/// <returns>Knapsak value given the capacity.</returns>
		private static int InternalSolve(int capacity, int numItems, int[] weights, int[] values, out int numSelected, ref int[] selection, ref int[][] dp)
		{
			for (int i = 1; i <= numItems; i++)
			{
				for (int w = 1; w <= capacity; w++)
				{
					if (weights[i - 1] <= w)
					{
#if UNITY
						dp[i][w] = Mathf.Max(values[i - 1] + dp[i - 1][w - weights[i - 1]], dp[i - 1][w]);
#else
						dp[i][w] = System.Math.Max(values[i - 1] + dp[i - 1][w - weights[i - 1]], dp[i - 1][w]);
#endif
					}
					else
					{
						dp[i][w] = dp[i - 1][w];
					}
				}
			}
			numSelected = 0;
			for (int w = capacity, i = numItems; i > 0 && w > 0;)
			{
				if (dp[i - 1][w] == dp[i][w])
				{
					i--;
					continue;
				}
				//Debug.Log($"Picked {i}x{w} >>> {values[i - 1]}"); // What got picked, row X column
				w -= weights[--i];
				selection[numSelected++] = i;
			}
			return dp[numItems][capacity];
		}
	}
}
