using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSpawnpoint : MonoBehaviour 
{
	// Member variables
	[SerializeField] private bool m_isReserved;
	[SerializeField] private CEntity m_reservedBy;


	// MonoBehaviour-Methods
	void Awake()
	{
		m_reservedBy = null;
		m_isReserved = false;
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.GetComponent<CEntity>() == m_reservedBy)
		{
			m_isReserved = false;
		}
	}
	
	
	// Methods
	public bool IsAvailable()
	{
		return !m_isReserved;
	}

	public void Reserve(CEntity arrivingEntity)
	{
		m_reservedBy = arrivingEntity;
		m_isReserved = true;
	}

	public void Reset()
	{
		m_reservedBy = null;
		m_isReserved = false;
	}
}