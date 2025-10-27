using UnityEngine;

namespace WeightedSpawning
{
	/// <summary>
	/// Cubie is stupid. Cubie just moves towards 0 and deletes/pools itself upon getting close enough.
	/// </summary>
	public class Cubie : MonoBehaviour
	{
		/// <summary>How quickly this Cubie should move towards the world origin.</summary>
		public float Speed = 8;
		/// <summary>When cubie would move into this range of the world center, it will either destory itself or diable and return to an object pool.</summary>
		public float DestroyRange = 3;
		/// <summary>when within the <see cref="DestroyRange"/> + <see cref="ScaleRange"/>, Cubie will begin to shrink to 0. If <= 0, Cubie will never scale.</summary>
		public float ScaleRange = 1;
		// Pool is assumed to to main thread safe only and should not be mutated by any other threads.
		private System.Collections.Generic.Stack<GameObject> m_ObjectPool;

		// When Cubie is pooled this is what resets the object to it's initial working state.
		private void OnEnable()
		{
			transform.localScale = Vector3.one;
		}

		void Update()
		{
			Vector3 p = Vector3.MoveTowards(transform.position, Vector3.zero, Time.deltaTime * Speed);
			float len = p.magnitude;
			if (len < DestroyRange)
			{
				if (m_ObjectPool != null)
				{
					gameObject.SetActive(false);
					m_ObjectPool.Push(gameObject);
					return;
				}
				Destroy(gameObject);
				return;
			}
			if (ScaleRange > 0)
			{
				transform.localScale = Vector3.one * Mathf.Clamp(((len - DestroyRange) / ScaleRange), 0, 1);
			}
			transform.position = p;
		}

		public void SetObjectPool(System.Collections.Generic.Stack<GameObject> pool)
		{
			m_ObjectPool = pool;
		}
	}
}
