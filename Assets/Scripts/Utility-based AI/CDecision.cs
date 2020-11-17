using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class CDecision 
{
    // Member variables
    [SerializeField] private CAbility m_chosenAbility;
	[SerializeField] private GameObject m_target;


    public CDecision()
    {
        m_chosenAbility = null;
        m_target = null;
    }

    public CDecision(CDecision decision)
    {
        m_chosenAbility = decision.m_chosenAbility;
        m_target = decision.m_target;
    }

    public CDecision(CAbility ability, GameObject target)
    {
        m_chosenAbility = ability;
        m_target = target;
    }


    // Getter/Setter
    public CAbility GetAbility()
    {
        return m_chosenAbility;
    }

    public void SetAbility(CAbility ability)
    {
        m_chosenAbility = ability;
    }


    public GameObject GetTarget()
    {
        if (m_target == null)
        {
            Debug.LogWarning("Target in decision is null.");
        }

        return m_target;
    }

    public void SetTarget(GameObject target)
    {
        m_target = target;
    }
}