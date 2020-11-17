using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CActionUiItem : CUiItem
{
	// Member variables
	[Header("Action item properties")]
	[SerializeField] private Image m_backgroundImage;
	[SerializeField] private RectTransform m_prevMenuPanel;
	[SerializeField] private CAbility m_ability;
	[SerializeField] private Image m_abilityIcon;
	[SerializeField] private Text m_abilityName;

	[SerializeField] private bool m_isDisabled;
	

	// MonoBehaviour-Methods
	void Awake()
	{
		m_prevMenuPanel = transform.parent.GetComponent<RectTransform>();
	}
	

	// Methods
	public override void HandleSelectionButton()
	{
		// If called, the ability becomes the players decision
		CCombatParticipant currentParticipant = CCombatSystemUi.GetInstance().GetComponent<CCombatControlUI>().GetCurrentParticipant();

		currentParticipant.CreateDecision(m_ability, CCombatSystemUi.GetInstance().GetComponent<CCombatControlUI>().GetChosenTarget());

		// Change the state of the participant
		currentParticipant.SetHasChosenAnAction(true);

		Debug.Log("HandleSelection in CActionUiItem called. Chosen Ability: \"" + m_ability.GetAbilityName() + "\".");		
	}


	public override void HandleBackButton()
	{
	}

	// Getter/Setter
	public RectTransform GetPrevMenuPanel()
	{
		return m_prevMenuPanel;
	}

	public CAbility GetAbility()
	{
		return m_ability;
	}

	public void SetAbility(CAbility ability)
	{
		m_ability = ability;
	}

	public void SetAbilityIcon(Sprite icon)
	{
		m_abilityIcon.sprite = icon;
	}

	public void SetAbilityName(string name)
	{
		m_abilityName.text = name;
	}

	public Image GetBackgroundImage()
	{
		return m_backgroundImage;
	}

	public Image GetAbilityIconComponent()
	{
		return m_abilityIcon;
	}

	public Text GetAbilityNameComponent()
	{
		return m_abilityName;
	}

	public bool GetIsDisabled()
	{
		return m_isDisabled;
	}

	public void SetIsDisabled(bool isDisabled)
	{
		m_isDisabled = isDisabled;
	}
}