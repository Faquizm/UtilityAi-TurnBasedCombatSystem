using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CAIContext 
{
	// Member variables
	GameObject m_executor;
	GameObject m_target;
    CAbility m_abilityToExecute;
    int m_entitiesInRadius;
    

    // Constructor
    public CAIContext()
    {
        m_executor = null;
        m_target = null;
        m_abilityToExecute = null;
        m_entitiesInRadius = -1;
    }


    // Getter/Setter
    public void SetExecutor(GameObject executor)
    {
        m_executor = executor;
    }

    public GameObject GetExecutor()
    {
        if (m_executor == null)
        {
            Debug.LogError("Executing entity in context is null.");
        }

        return m_executor;
    }

    public CEntity GetExecutorAsEntity()
    {
        if (m_executor.GetComponent<CEntity>() != null)
        {
            return m_executor.GetComponent<CEntity>();
        }
        else
        {
            Debug.LogError("No entity component found on executor.");
            return null;
        }
    }

    public CCombatParticipant GetExecutorAsParticipant()
    {
        if (m_executor.GetComponent<CEntity>() != null)
        {
            return CCombatSystem.GetInstance().GetParticipantByEntity(m_executor.GetComponent<CEntity>());
        }
        else
        {
            Debug.LogError("No entity component found on executor.");
            return null;
        }
    }

    public void SetTarget(GameObject target)
    {
        m_target = target;
    }

    public GameObject GetTarget()
    {
        if (m_target == null)
        {
            Debug.LogError("Target entity in context is null.");
        }

        return m_target;
    }

    public void SetAbilityToExecute(CAbility ability)
    {
        m_abilityToExecute = ability;
    }

    public CAbility GetAbilityToExecute()
    {
        return m_abilityToExecute;
    }

    public CEntity GetTargetAsEntity()
    {
        if (m_target.GetComponent<CEntity>() != null)
        {
            return m_target.GetComponent<CEntity>();
        }
        else
        {
            Debug.LogError("No entity component found on target.");
            return null;
        }
    }

    public CCombatParticipant GetTargetAsParticipant()
    {
        if (m_target.GetComponent<CEntity>() != null)
        {
            return CCombatSystem.GetInstance().GetParticipantByEntity(m_target.GetComponent<CEntity>());
        }
        else
        {
            Debug.LogError("No entity component found on target.");
            return null;
        }
    }

    public int GetEntitiesInRadius()
    {
        if (m_entitiesInRadius >= 0)
        {
            return m_entitiesInRadius;
        }
        else
        {
            Debug.LogError("Entities in radius is not updated. Update first via utility system.");
            return -1;
        }
    }

    public void SetEntitiesInRadius(int entitiesInRadius)
    {
        m_entitiesInRadius = entitiesInRadius;
    }
}
