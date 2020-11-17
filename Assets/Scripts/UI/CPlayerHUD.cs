using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CPlayerHUD : MonoBehaviour 
{
	// Member variables
	[SerializeField] private CCombatParticipant m_playerParticipant;
	[SerializeField] private Image m_icon;
	[SerializeField] private Text m_name;
	[SerializeField] private Text m_hp;
	[SerializeField] private Text m_sp;
	[SerializeField] private Text m_mp;
	
	
	// Methods
	public void InitializeHUD(CCombatParticipant playerParticipant)
	{
		m_playerParticipant = playerParticipant;
		m_name.text = playerParticipant.GetEntity().GetName().ToUpper();
		string iconPath = "Entities/" + playerParticipant.GetEntity().GetName() + "/" + playerParticipant.GetEntity().GetIconName();
		m_icon.sprite = Resources.Load<Sprite>(iconPath);
		UpdateAll();
	}

	public void UpdateAll()
	{
		UpdateHP();
		UpdateSP();
		UpdateMP();
	}

	public void UpdateHP()
	{
		float currentHealthPercentage = m_playerParticipant.GetEntity().GetHealthPercentage();

		m_hp.text = "HP: " + m_playerParticipant.GetEntity().GetCurrentHealth().ToString();
		
		if (currentHealthPercentage > 0.5f)
		{
			m_hp.color = Color.black;
		}
		else if (currentHealthPercentage <= 0.5f && currentHealthPercentage > 0.2f)
		{
			m_hp.color = Color.yellow;
		}
		else
		{
			m_hp.color = Color.red;
		}
	}

	public void UpdateSP()
	{
		float currentSkillPointPercentage = m_playerParticipant.GetEntity().GetSkillPointsPercentage();

		m_sp.text = "SP: " + m_playerParticipant.GetEntity().GetCurrentSkillPoints().ToString();

		if (currentSkillPointPercentage > 0.5f)
		{
			m_sp.color = Color.black;
		}
		else if (currentSkillPointPercentage <= 0.5f && currentSkillPointPercentage > 0.2f)
		{
			m_sp.color = Color.yellow;
		}
		else
		{
			m_sp.color = Color.red;
		}
	}

	public void UpdateMP()
	{
		float currentMagicPointPercentage = m_playerParticipant.GetEntity().GetMagicPointsPercentage();

		m_mp.text = "MP: " + m_playerParticipant.GetEntity().GetCurrentMagicPoints().ToString();

		if (currentMagicPointPercentage > 0.5f)
		{
			m_mp.color = Color.black;
		}
		else if (currentMagicPointPercentage <= 0.5f && currentMagicPointPercentage > 0.2f)
		{
			m_mp.color = Color.yellow;
		}
		else
		{
			m_mp.color = Color.red;
		}
	}
}
