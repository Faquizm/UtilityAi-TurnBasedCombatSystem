using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEntity : MonoBehaviour
{
	// Member variables
	[Header("General")]
	[SerializeField] protected int m_index; 
	[SerializeField, HideInInspector] protected string m_name;

	[Header("Data")]
	[SerializeField, HideInInspector] protected TextAsset m_jsonFile;
	[SerializeField, HideInInspector] protected string m_entityFolderName;
	[SerializeField, HideInInspector] protected List<string> m_abilityTypes;
	[SerializeField, HideInInspector] protected List<string> m_jsonFileNames;
	[SerializeField, HideInInspector] protected string m_iconName;

	[Header("Vitals")]
	[SerializeField] protected int m_baseHealth = 30;
	[SerializeField, DisabledVariable] protected int m_maxHealth;
	protected int m_currentHealth;
	[SerializeField] protected bool m_isDefeated;

	[Space(7)]
	[SerializeField] protected int m_baseSkillPoints = 20;
	[SerializeField, DisabledVariable] protected int m_maxSkillPoints;
	protected int m_currentSkillPoints;

	[Space(7)]
	[SerializeField] protected int m_baseMagicPoints = 5;
	[SerializeField, DisabledVariable] protected int m_maxMagicPoints;
	protected int m_currentMagicPoints;

	[Header("Character stats")]
	[SerializeField] protected CEntityStats m_stats;
	[SerializeField, HideInInspector] protected CEntityStats m_statWeights;

	[Header("Abilities")]
	protected CDamageAbility m_comboAttack;
	protected CDamageAbility m_criticalAttack;
	protected List<CAbility> m_skillAbilities;
	protected List<CAbility> m_magicAbilities;
	protected List<CDefenseAbility> m_defenseAbilities;
	protected CDefenseAbility m_move;
	protected CDefenseAbility m_hide;

	[Header("Configuration")]
	[SerializeField, Range(30, 360)] protected float m_fieldOfView = 120.0f;
	[SerializeField] protected bool m_isControlledByAI = true;

	[Header("Actions")]
    [SerializeField] protected List<CAction> m_actions;

	[Header("Worldspecific Variables")]
	[SerializeField] protected bool m_isTransformSaved;
	[SerializeField] protected Vector3 m_lastPosition;
	[SerializeField] protected Vector3 m_lastRotation;


	// MonoBehaviour-Methods
	protected virtual void Awake()
	{
		// Initialize lists
		m_abilityTypes = new List<string>();
		m_defenseAbilities = new List<CDefenseAbility>();
		m_skillAbilities = new List<CAbility>();
		m_magicAbilities = new List<CAbility>();

		// Set vitals to max
		m_currentHealth = m_maxHealth;
		m_currentSkillPoints = m_maxSkillPoints;
		m_currentMagicPoints = m_maxMagicPoints;

		if (m_jsonFile != null)
		{
			JsonUtility.FromJsonOverwrite(m_jsonFile.text, this);

			// Create abilities
			CreateAbilititesFromJSON();
		}
		
		GetComponentInParent<CGroup>().AddGroupMember(this);

		GetComponent<Collider>().enabled = false;

		m_isTransformSaved = false;
	}


	void OnValidate()
	{
		if (m_jsonFile != null)
		{
			JsonUtility.FromJsonOverwrite(m_jsonFile.text, this);
		}

		UpdateMaxVitals();
	}


	// Methods
	public static int SortByIndex(CEntity entity_01, CEntity entity_02)
	{
		return entity_01.m_index.CompareTo(entity_02.m_index);
	}
	

	public void UpdateMaxVitals()
	{
		if (m_stats != null)
		{
			// Calculate new health maximum
			m_maxHealth = m_baseHealth + (int)(m_stats.GetStamina() * m_statWeights.GetStatWeight(StatType.Stamina) * 2);

			// Calculate new skill points maximum
			m_maxSkillPoints = m_baseSkillPoints + (int)((m_stats.GetStrength() * m_statWeights.GetStatWeight(StatType.Strength) + m_stats.GetAgility() * m_statWeights.GetStatWeight(StatType.Agility)) * 0.25f);

			// Calculate new magic points maximum
			m_maxMagicPoints = m_baseMagicPoints + (int)(m_stats.GetIntelligence() * m_statWeights.GetStatWeight(StatType.Intelligence) * 0.5f);
		}
	}

	public void CreateAbilititesFromJSON()
	{
		for (int i = 0; i < m_abilityTypes.Count; i++)
		{
			// Create Ability-Object
			object[] constructorParameters = new object[2];
			constructorParameters[0] = m_entityFolderName;
			constructorParameters[1] = m_jsonFileNames[i];

			System.Runtime.Remoting.ObjectHandle objectHandle = System.Activator.CreateInstance(null, m_abilityTypes[i], true, 0, null, constructorParameters, null, null, null);
			object currentAbility = objectHandle.Unwrap();

			// Assign basic/skill/magic//defense abilities
			if (currentAbility.GetType() == typeof(CDamageAbility))
			{
				CDamageAbility currentDamageAbility = (CDamageAbility)currentAbility;

				if (currentDamageAbility.GetAbilityName().Equals("Combo Attack"))
				{
					m_comboAttack = currentDamageAbility;
					continue;
				}

				if (currentDamageAbility.GetAbilityName().Equals("Critical Attack"))
				{
					m_criticalAttack = currentDamageAbility;
					continue;
				}


				if (currentDamageAbility.GetResourceType() == ResourceType.SkillPoints)
				{
					m_skillAbilities.Add(currentDamageAbility);
				}
				else if(currentDamageAbility.GetResourceType() == ResourceType.MagicPoints)
				{
					m_magicAbilities.Add(currentDamageAbility);
				}
				else
				{
					Debug.LogWarning("Couldn't add " + currentDamageAbility.GetAbilityName() + " to an ability list.");
				}
			}
			else if (currentAbility.GetType() == typeof(CStatAbility))
			{
				CStatAbility currentStatAbility = (CStatAbility)currentAbility;

				if (currentStatAbility.GetResourceType() == ResourceType.SkillPoints)
				{
					m_skillAbilities.Add(currentStatAbility);
				}
				else if (currentStatAbility.GetResourceType() == ResourceType.MagicPoints)
				{
					m_magicAbilities.Add(currentStatAbility);
				}
				else
				{
					Debug.LogWarning("Couldn't add " + currentStatAbility.GetAbilityName() + " to an ability list.");
				}
			}
			else if (currentAbility.GetType() == typeof(CHealAbility))
			{
				CHealAbility currentHealAbility = (CHealAbility)currentAbility;

				if (currentHealAbility.GetResourceType() == ResourceType.SkillPoints)
				{
					m_skillAbilities.Add(currentHealAbility);
				}
				else if (currentHealAbility.GetResourceType() == ResourceType.MagicPoints)
				{
					m_magicAbilities.Add(currentHealAbility);
				}
				else
				{
					Debug.LogWarning("Couldn't add " + currentHealAbility.GetAbilityName() + " to an ability list.");
				}
			}
			else if (currentAbility.GetType() == typeof(CDefenseAbility))
			{
				CDefenseAbility currentDefenseAbility = (CDefenseAbility)currentAbility;

				if (currentDefenseAbility.GetAbilityName().Equals("Move"))
				{
					m_move = currentDefenseAbility;
					continue;
				}

				if (currentDefenseAbility.GetAbilityName().Equals("Hide"))
				{
					m_hide = currentDefenseAbility;
					continue;
				}

				m_defenseAbilities.Add(currentDefenseAbility);
			}
		}

		Debug.Log("Finished loading abilities of " + m_name + "\nSkills: " + m_skillAbilities.Count + "; Spells: " + m_magicAbilities.Count);
	}


	public void UpdateIsDefeated()
	{
		if (m_currentHealth <= 0)
		{
			gameObject.SetActive(false);
			m_currentHealth = 0;
			m_isDefeated = true;
			StopAllCoroutines();
		}
	}


	public void TakeDamage(float amount)
	{
		m_currentHealth -= (int)amount;

		UpdateIsDefeated();
	}

	public void TakeHeal(float amount)
	{
		m_currentHealth += (int)amount;

		if (m_currentHealth > m_maxHealth)
		{
			m_currentHealth = m_maxHealth;
		}
	}

	public void SpendResource(ResourceType resourceType, int costs)
	{
		if (resourceType == ResourceType.SkillPoints)
		{
			m_currentSkillPoints -= costs;
		}
		else if (resourceType == ResourceType.MagicPoints)
		{
			m_currentMagicPoints -= costs;
		}
		else
		{
			Debug.LogError("Unknown Resource type detected when spending resource.");
		}
	}


	public void AddActionToEntity(CAction action)
	{
		m_actions.Add(action);
	}


	public void RemoveActionFromEntity(CAction action)
	{
		m_actions.Remove(action);
	}


	public void SaveLastPosition()
	{
		Debug.Log(string.Format("{0} saved position: {1} (local: {2})", m_name, transform.position, transform.localPosition));
		m_lastPosition = transform.localPosition;
	}


	public void SaveLastRotation()
	{
		m_lastRotation = transform.rotation.eulerAngles;
	}


	public void LoadLastPosition()
	{
		transform.localPosition = m_lastPosition;
	}


	public void LoadLastRotation()
	{
		transform.rotation = Quaternion.Euler(m_lastRotation);
	}


	public bool IsDefeated()
	{
		if (m_currentHealth <= 0)
		{
			return true;
		}
		else
		{
			return false;
		}
	}


	public void ResetVitals()
	{
		m_currentHealth = m_maxHealth;
		m_currentSkillPoints = m_maxSkillPoints;
		m_currentMagicPoints = m_maxMagicPoints;
	}


	public bool IsInFieldOfView(Transform other)
	{
		Vector3 directionToOther = (other.localPosition - transform.localPosition).normalized;
		float angleToOther = Vector3.Angle(directionToOther, transform.forward);

		if (angleToOther < m_fieldOfView / 2.0f)
		{
			return true;
		}
		else
		{
			return false;
		}
	}



	// Getter/Setter
	public int GetIndex()
	{
		return m_index;
	}

	public string GetName()
	{
		return m_name;
	}

	public string GetFolderName()
	{
		return m_entityFolderName;
	}

	public string GetIconName()
	{
		return m_iconName;
	}

	public int GetCurrentHealth()
	{
		return m_currentHealth;
	}

	public float GetHealthPercentage()
	{
		return (float)m_currentHealth / m_maxHealth;
	}

	public int GetCurrentSkillPoints()
	{
		return m_currentSkillPoints;
	}

	public float GetSkillPointsPercentage()
	{
		return (float)m_currentSkillPoints / m_maxSkillPoints;
	}
	
	public int GetCurrentMagicPoints()
	{
		return m_currentMagicPoints;
	}

	public float GetMagicPointsPercentage()
	{
		return (float)m_currentMagicPoints / m_maxMagicPoints;
	}

	public CEntityStats GetStats()
	{
		return m_stats;
	}

	public CEntityStats GetStatWeights()
	{
		return m_statWeights;
	}

	public CDamageAbility GetComboAttack()
	{
		return m_comboAttack;
	}

	public CDamageAbility GetCriticalAttack()
	{
		return m_criticalAttack;
	}

	public List<CAbility> GetSkillAbilities()
	{
		return m_skillAbilities;
	}

	public List<CAbility> GetMagicAbilities()
	{
		return m_magicAbilities;
	}

	public List<CDefenseAbility> GetDefenseAbilities()
	{
		return m_defenseAbilities;
	}

	public CDefenseAbility GetMoveAbility()
	{
		return m_move;
	}
	
	public CDefenseAbility GetHideAbility()
	{
		return m_hide;
	}

	public void SetIsControlledByAI(bool isControlledByAI)
	{
		m_isControlledByAI = isControlledByAI;
	}

	public bool GetIsControlledByAI()
	{
		return m_isControlledByAI;
	}
	
	public List<CAction> GetEntityActions()
	{
		return m_actions;
	}

	public void SetIsTransformSaved(bool isSaved)
	{
		m_isTransformSaved = isSaved;
	}
}
