using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CTrap : MonoBehaviour 
{
	// Member variables
	[SerializeField] private List<GameObject> m_trapEnemies;
	private GameObject m_target = null;
	

	// MonoBehaviour-Methods
	void Update()
	{
		foreach (GameObject go in m_trapEnemies)
		{
			if (m_target != null && go != null)
			{
				Vector3 newPos = Vector3.MoveTowards(go.transform.position, m_target.transform.position, 5.0f * Time.deltaTime);
				go.transform.position = newPos;
				go.transform.LookAt(m_target.transform);
			}
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.CompareTag("Player"))
		{
			m_target = other.gameObject;
		}
	}
	
	
	// Methods
	
	
	// Getter/Setter
	
	
}
