using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CExplosionEffect : MonoBehaviour
{
	// Member variables
	[SerializeField] private float m_radius;

	// MonoBehaviour-Methods
	void Update()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).localPosition = new Vector3(m_radius, 0.0f, 0.0f);
		}

		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).RotateAround(transform.position, Vector3.up, (360.0f / transform.childCount) * i);
		}
	}


	// Getter/Setter
	public float GetRadius()
	{
		return m_radius;
	}


	public void SetRadius(float radius)
	{
		m_radius = radius;
	}
}