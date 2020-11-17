using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CStatusUI : MonoBehaviour 
{
	// Member variables
	[SerializeField] private Text m_entityName;
	[SerializeField] private Text m_hp;
	[SerializeField] private Text m_abilityName;
	[SerializeField] private Text m_targetName;
	[SerializeField] private Text m_next;
	[SerializeField] private Text m_strengthText;
	[SerializeField] private Text m_agilityText;
	[SerializeField] private Text m_intelligenceText;
	[SerializeField] private Text m_defenseText;
	[SerializeField] private Text m_speedText;
	
	
	// Methods
	public void UpdateStatusUI(CCombatParticipant participant)
	{
		m_entityName.text = participant.GetEntity().GetName();

		m_hp.text = participant.GetEntity().GetCurrentHealth().ToString();
		float hpPercentage = participant.GetEntity().GetHealthPercentage();
		AdaptTextColor(m_hp, hpPercentage);

		if (participant.GetDecision() == null || participant.GetDecision().GetAbility() == null)
		{
			m_abilityName.text = "---";
			m_targetName.text = "---";
			m_next.text = "Decision";
		}
		else
		{
			m_abilityName.text = participant.GetDecision().GetAbility().GetAbilityName();

			if (participant.GetDecision().GetTarget().GetComponent<CEntity>() == null)
			{
				m_targetName.text = "P" + participant.GetDecision().GetTarget().name.Substring(6);
			}
			else
			{
				m_targetName.text = participant.GetDecision().GetTarget().GetComponent<CEntity>().GetName();
			}
			m_next.text = "Execution";
		}

		int value = (int)(participant.GetEntity().GetStats().GetStrength() * (1.0f + participant.GetCombatStatChanges().GetStatChange(StatType.Strength)));
		m_strengthText.text = value.ToString();
		AdaptTextColor(m_strengthText, participant.GetCombatStatChanges(), StatType.Strength);

		value = (int)(participant.GetEntity().GetStats().GetAgility() * (1.0f + participant.GetCombatStatChanges().GetStatChange(StatType.Agility)));
		m_agilityText.text = value.ToString();
		AdaptTextColor(m_agilityText, participant.GetCombatStatChanges(), StatType.Agility);

		value = (int)(participant.GetEntity().GetStats().GetIntelligence() * (1.0f + participant.GetCombatStatChanges().GetStatChange(StatType.Intelligence)));
		m_intelligenceText.text = value.ToString();
		AdaptTextColor(m_intelligenceText, participant.GetCombatStatChanges(), StatType.Intelligence);

		value = (int)(participant.GetEntity().GetStats().GetDefense() * (1.0f + participant.GetCombatStatChanges().GetStatChange(StatType.Defense)));
		m_defenseText.text = value.ToString();
		AdaptTextColor(m_defenseText, participant.GetCombatStatChanges(), StatType.Defense);

		value = (int)(participant.GetEntity().GetStats().GetSpeed() * (1.0f + participant.GetCombatStatChanges().GetStatChange(StatType.Speed)));
		m_speedText.text = value.ToString();
		AdaptTextColor(m_speedText, participant.GetCombatStatChanges(), StatType.Speed);
	}

	private void AdaptTextColor(Text text, float percentage)
	{
		if (percentage > 0.5f)
		{
			text.color = new Color(0.195f, 0.195f, 0.195f, 1.0f);
		}
		else if (percentage <= 0.5f && percentage > 0.2f)
		{
			text.color = Color.yellow;
		}
		else
		{
			text.color = Color.red;
		}
	}

	private void AdaptTextColor(Text text, CEntityStats combatStats, StatType stat)
	{
		if (combatStats.GetStatChange(stat) < 0.0f)
		{
			text.color = Color.red;
		}
		else if (combatStats.GetStatChange(stat) > 0.001f)
		{
			text.color = Color.green;
		}
		else
		{
			text.color = new Color(0.195f, 0.195f, 0.195f, 1.0f);
		}
	}
}
