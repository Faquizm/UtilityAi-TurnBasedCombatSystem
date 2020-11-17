using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSelectionContext : MonoBehaviour 
{
	// Member variables
	[Header("Prefabs")]
	[SerializeField] private GameObject m_choiceMenuItemPrefab;

	[Header("UI Elements - Basics")]
	[SerializeField] private RectTransform m_comboAttack;
	[SerializeField] private RectTransform m_criticalAttack;
	[SerializeField] private RectTransform m_move;
	[SerializeField] private RectTransform m_protect;
	[SerializeField] private RectTransform m_protectAlly;
	[SerializeField] private RectTransform m_hide;

	[Header("UI Elements - Skills")]
	[SerializeField] private RectTransform m_choiceMenuSkills;
	[SerializeField] private Image m_currentPlayerIconSkills;
	[SerializeField] private Text m_currentSkillDescription;

	[Header("UI Elements - Magic")]
	[SerializeField] private RectTransform m_choiceMenuMagic;
	[SerializeField] private Image m_currentPlayerIconMagic;
	[SerializeField] private Text m_currentMagicDescription;

	
	// Methods
	public void UpdateSelectionMenu(CCombatParticipant currentParticipant)
	{
		string iconPath = "Entities/" + currentParticipant.GetEntity().GetName() + "/" + currentParticipant.GetEntity().GetIconName();
		m_currentPlayerIconSkills.sprite = Resources.Load<Sprite>(iconPath);
		m_currentPlayerIconMagic.sprite = Resources.Load<Sprite>(iconPath);

		if (currentParticipant.GetEntity().GetType() == typeof(CPlayer))
		{
			CPlayer currentPlayerEntity = (CPlayer)currentParticipant.GetEntity();
			
			m_comboAttack.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetComboAttack());
			m_criticalAttack.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetCriticalAttack());
			m_move.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetMoveAbility());
			m_protect.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetProtectAbility());
			m_protectAlly.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetProtectAllyAbility());
			m_hide.GetComponent<CActionUiItem>().SetAbility(currentPlayerEntity.GetHideAbility());

			int menuCounter = 0;
			for (int i = 0; i < currentPlayerEntity.GetSkillAbilities().Count; i++)
			{
				CAbility currentAbility = currentPlayerEntity.GetSkillAbilities()[i];

				GameObject newSkillItem = Instantiate(m_choiceMenuItemPrefab, m_choiceMenuSkills.transform);
				int currentRows = menuCounter / 2;
				float x = m_choiceMenuItemPrefab.GetComponent<RectTransform>().sizeDelta.x / 2.0f + 23.0f;
				float y = -155.0f - (m_choiceMenuItemPrefab.GetComponent<RectTransform>().sizeDelta.y + 8.0f) * currentRows;

				if (menuCounter % 2 == 0)
				{
					newSkillItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(-x, y);
				}
				else
				{
					newSkillItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
				}
				menuCounter++;

				// Load ability text and icon
				string abilityIconPath = "Ability Icons/" + currentPlayerEntity.GetSkillAbilities()[i].GetAbilityName().Replace(" ", "") + "Icon";
				newSkillItem.GetComponent<CActionUiItem>().SetAbilityIcon(Resources.Load<Sprite>(abilityIconPath));
				newSkillItem.GetComponent<CActionUiItem>().SetAbilityName(currentAbility.GetAbilityName());
				newSkillItem.GetComponent<CActionUiItem>().SetAbility(currentAbility);

				// Check if the player has enough resources. If not, change UI to seem disabled
				bool isUsable = CheckUsability(currentPlayerEntity, currentAbility);

				if (!isUsable)
				{
					DisableItem(newSkillItem.GetComponent<CActionUiItem>());
					newSkillItem.GetComponent<CActionUiItem>().SetIsDisabled(true);
				}
			}

			menuCounter = 0;
			for (int i = 0; i < currentPlayerEntity.GetMagicAbilities().Count; i++)
			{
				CAbility currentAbility = currentPlayerEntity.GetMagicAbilities()[i];

				GameObject newMagicItem = Instantiate(m_choiceMenuItemPrefab, m_choiceMenuMagic.transform);
				int currentRows = menuCounter / 2;
				float x = m_choiceMenuItemPrefab.GetComponent<RectTransform>().sizeDelta.x / 2.0f + 23.0f;
				float y = -155.0f - (m_choiceMenuItemPrefab.GetComponent<RectTransform>().sizeDelta.y + 8.0f) * currentRows;

				if (menuCounter % 2 == 0)
				{
					newMagicItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(-x, y);
				}
				else
				{
					newMagicItem.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
				}
				menuCounter++;

				// Load ability text and icon
				string abilityIconPath = "Ability Icons/" + currentPlayerEntity.GetMagicAbilities()[i].GetAbilityName().Replace(" ", "") + "Icon";

				newMagicItem.GetComponent<CActionUiItem>().SetAbilityIcon(Resources.Load<Sprite>(abilityIconPath));
				newMagicItem.GetComponent<CActionUiItem>().SetAbilityName(currentAbility.GetAbilityName());
				newMagicItem.GetComponent<CActionUiItem>().SetAbility(currentAbility);

				// Check if the player has enough resources. If not, change UI to seem disabled
				bool isUsable = CheckUsability(currentPlayerEntity, currentAbility);

				if (!isUsable)
				{
					DisableItem(newMagicItem.GetComponent<CActionUiItem>());
					newMagicItem.GetComponent<CActionUiItem>().SetIsDisabled(true);
				}
			}
		}
	}


	private bool CheckUsability(CPlayer currentPlayer, CAbility currentAbility)
	{
		bool hasEnoughResources = false;

		bool hasCostsProperty = currentAbility.GetType().GetMethod("GetCosts") != null;
		bool hasResourceTypeProperty = currentAbility.GetType().GetMethod("GetResourceType") != null;
		if (hasCostsProperty && hasResourceTypeProperty)
		{
			ResourceType abilityResource = (ResourceType)currentAbility.GetType().GetMethod("GetResourceType").Invoke(currentAbility, null);
			int abilityCosts = (int)currentAbility.GetType().GetMethod("GetCosts").Invoke(currentAbility, null);


			if (abilityResource == ResourceType.SkillPoints)
			{
				hasEnoughResources = currentPlayer.GetCurrentSkillPoints() >= abilityCosts;
			}
			else if (abilityResource == ResourceType.MagicPoints)
			{
				hasEnoughResources = currentPlayer.GetCurrentMagicPoints() >= abilityCosts;
			}
			else
			{
				Debug.LogError("Unknown resource type when checking usability of an ability.");
			}
		}

		return hasEnoughResources;
	}

	private void DisableItem(CActionUiItem item)
	{
		Color itemColor = item.GetAbilityIconComponent().color;
		item.GetAbilityIconComponent().color = new Color(itemColor.r, itemColor.g, itemColor.b, 0.5f);

		itemColor = item.GetAbilityNameComponent().color;
		item.GetAbilityNameComponent().color = new Color(itemColor.r, itemColor.g, itemColor.b, 0.5f);

		itemColor = item.GetBackgroundImage().color;
		item.GetBackgroundImage().color = new Color(itemColor.r, itemColor.g, itemColor.b, 0.5f);
	}

	
	public void ClearSelectionMenu()
	{
		m_comboAttack.GetComponent<CActionUiItem>().SetAbility(null);
		m_criticalAttack.GetComponent<CActionUiItem>().SetAbility(null);
		m_move.GetComponent<CActionUiItem>().SetAbility(null);
		m_protect.GetComponent<CActionUiItem>().SetAbility(null);
		m_protectAlly.GetComponent<CActionUiItem>().SetAbility(null);
		m_hide.GetComponent<CActionUiItem>().SetAbility(null);

		for (int i = 0; i < m_choiceMenuSkills.childCount; i++)
		{
			if (m_choiceMenuSkills.GetChild(i).GetComponent<CActionUiItem>() != null)
			{
				Destroy(m_choiceMenuSkills.GetChild(i).gameObject);
			}
		}

		for (int i = 0; i < m_choiceMenuMagic.childCount; i++)
		{
			if (m_choiceMenuMagic.GetChild(i).GetComponent<CActionUiItem>() != null)
			{
				Destroy(m_choiceMenuMagic.GetChild(i).gameObject);
			}
		}
	}

	public void UpdateDescriptionText(string description, ResourceType resourceType, int costs)
	{
		if (resourceType == ResourceType.SkillPoints)
		{
			m_currentSkillDescription.text = description;
			m_currentSkillDescription.text += "\n\nCosts: " + costs + " SP";
		}
		else if (resourceType == ResourceType.MagicPoints)
		{
			m_currentMagicDescription.text = description;
			m_currentMagicDescription.text += "\n\nCosts: " + costs + " MP";
		}
		else
		{
			Debug.Log("Unknown Ressource Type when updating scription texts.");
		}
	}
}
