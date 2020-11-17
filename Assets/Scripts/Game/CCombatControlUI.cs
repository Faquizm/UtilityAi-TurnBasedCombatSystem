using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public enum MenuType { Menu, SubMenu, ChoiceMenu, TargetSelection }

public class CCombatControlUI : MonoBehaviour 
{
	// Classes
	[System.Serializable]
	private class CMenuStack	// LIFO-Stack
	{
		[SerializeField] private List<MenuType> m_combatMenuHistory;

		public CMenuStack()
		{
			m_combatMenuHistory = new List<MenuType>();
			m_combatMenuHistory.Add(MenuType.Menu);
		}

		public void Add(MenuType combatMenu)
		{
			m_combatMenuHistory.Add(combatMenu);
		}

		public void RemoveLast()
		{
			m_combatMenuHistory.RemoveAt(m_combatMenuHistory.Count - 1);
		}
		public void Reset()
		{
			m_combatMenuHistory.Clear();
			m_combatMenuHistory.Add(MenuType.Menu);
		}

		public MenuType GetCurrentMenuType()
		{
			return m_combatMenuHistory[m_combatMenuHistory.Count - 1];
		}

		public MenuType GetPrevMenuType()
		{
			return m_combatMenuHistory[m_combatMenuHistory.Count - 2];
		}
	}


	// Constant variables
	const int MAX_COLUMNS_IN_CHOICEMENUS = 2;


	// Member variables
	[Header("Gameobjects")]
	[SerializeField] private GameObject m_targetSelectionArrow;
	[SerializeField] private int m_selectionIndex = 0;
	[SerializeField] private CCombatParticipant m_currentParticipant;
	[SerializeField] private List<CCombatParticipant> m_playerTeam;
	[SerializeField] private List<CCombatParticipant> m_enemyTeam;

	[Header("UI Elements")]
	[SerializeField] private RectTransform m_contextPanel;
	[SerializeField] private Image m_frameImage;
	[SerializeField] private CStatusUI m_statusUi;
	[SerializeField] private Text m_modeText;

	[SerializeField] private Dictionary<MenuType, List<RectTransform>> m_menus;
	[SerializeField] private Dictionary<MenuType, int> m_lastIndexInMenu;


	[Header("Control Configurations")]
	[SerializeField, Range(0.1f, 0.9f)] private float m_moveThreshold = 0.9f;
	[SerializeField, Range(0.1f, 0.9f)] private float m_resetThreshold = 0.6f;

	[Header("Control States")]
	[SerializeField] private CMenuStack m_menuHistory;
	[SerializeField] private RectTransform m_panelBeforeTargetSelection;
	[SerializeField] private int m_currentFramePositionIndex;
	[SerializeField] private bool m_hasFrameMoved = false;

	[SerializeField] private List<Transform> m_targetTransforms = new List<Transform>();
	[SerializeField] private bool m_isTargetListLoaded = false;

	[SerializeField] private bool m_isObserverModeActivated = false;


	// MonoBehaviour-Methods
	void Awake()
	{
		m_selectionIndex = 0;
		m_playerTeam = new List<CCombatParticipant>();
		m_enemyTeam = new List<CCombatParticipant>();

		m_menus = new Dictionary<MenuType, List<RectTransform>>();
		m_menus.Add(MenuType.Menu, new List<RectTransform>());
		m_menus.Add(MenuType.SubMenu, new List<RectTransform>());
		m_menus.Add(MenuType.ChoiceMenu, new List<RectTransform>());
		m_menus.Add(MenuType.TargetSelection, new List<RectTransform>());

		m_lastIndexInMenu = new Dictionary<MenuType, int>();
		m_lastIndexInMenu.Add(MenuType.Menu, 0);
		m_lastIndexInMenu.Add(MenuType.SubMenu, 0);
		m_lastIndexInMenu.Add(MenuType.ChoiceMenu, 0);
		m_lastIndexInMenu.Add(MenuType.TargetSelection, 0);


		for (int i = 0; i < m_contextPanel.childCount; i++)
		{
			if (m_contextPanel.GetChild(i).GetComponent<CUiItem>())
			{
				m_menus[MenuType.Menu].Add(m_contextPanel.GetChild(i).GetComponent<RectTransform>());
			}
		}

		m_menuHistory = new CMenuStack();
	}
	

	void Update()
	{
		if (m_isObserverModeActivated)
		{
			NavigateObserverMode();
		}
		else
		{
			switch (m_menuHistory.GetCurrentMenuType())
			{
				case MenuType.Menu:
					NavigateCombatMenu();
					break;

				case MenuType.SubMenu:
					NavigateCombatSubmenu();
					break;

				case MenuType.ChoiceMenu:
					NavigateChoiceMenu();
					break;

				case MenuType.TargetSelection:
					NavigateTargetSelection();
					break;

				default:
					break;
			}

			if (Input.GetButtonDown("Select"))
			{
				OnSelectButtonClicked();
			}

			if (Input.GetButtonDown("Back"))
			{
				OnBackButtonClicked();
			}
		}

		if (m_targetSelectionArrow.activeSelf && m_targetTransforms.Count > 0)
		{
			MoveArrowToIndexPosition(m_targetTransforms, m_selectionIndex);

			if (!m_targetTransforms[m_selectionIndex].gameObject.activeSelf)
			{
				int newIndex = 0;
				float clostestDistance = float.MaxValue;

				for (int i = 0; i < m_targetTransforms.Count; i++)
				{
					if (i != m_selectionIndex)
					{
						float distance = (m_targetTransforms[i].position - m_targetTransforms[m_selectionIndex].position).magnitude;

						if (distance < clostestDistance)
						{
							clostestDistance = distance;
							newIndex = i;
						}
					}
				}

				m_selectionIndex = newIndex;
			}
		}
	}	
	

	// Methods
	public void NavigateCombatMenu()
	{
		float horizontalAxis = Input.GetAxis("Horizontal");
		float verticalAxis = Input.GetAxis("Vertical") * -1.0f;

		if (horizontalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex++;
			if (m_currentFramePositionIndex >= m_menus[m_menuHistory.GetCurrentMenuType()].Count)
			{
				m_currentFramePositionIndex = m_menus[m_menuHistory.GetCurrentMenuType()].Count - 1;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;
		}
		else if (horizontalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex--;
			if (m_currentFramePositionIndex < 0)
			{
				m_currentFramePositionIndex = 0;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;
		}

		if (verticalAxis > m_moveThreshold)
		{
			m_hasFrameMoved = true;

			// Behave like menu item was selected
			OnSelectButtonClicked();
		}

		if (m_hasFrameMoved && Mathf.Abs(horizontalAxis) < m_resetThreshold && Mathf.Abs(verticalAxis) < m_resetThreshold)
		{
			m_hasFrameMoved = false;
		}
	}


	public void NavigateCombatSubmenu()
	{
		float verticalAxis = Input.GetAxis("Vertical") * -1.0f;

		if (verticalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex++;
			if (m_currentFramePositionIndex >= m_menus[m_menuHistory.GetCurrentMenuType()].Count)
			{
				m_currentFramePositionIndex = m_menus[m_menuHistory.GetCurrentMenuType()].Count - 1;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;
		}
		else if (verticalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex--;
			if (m_currentFramePositionIndex < 0)
			{
				m_currentFramePositionIndex = 0;
				OnBackButtonClicked();
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;
		}

		if (m_hasFrameMoved && Mathf.Abs(verticalAxis) < m_resetThreshold)
		{
			m_hasFrameMoved = false;
		}
	}


	public void NavigateChoiceMenu()
	{
		float horizontalAxis = Input.GetAxis("Horizontal");
		float verticalAxis = Input.GetAxis("Vertical") * -1.0f;

		// Horizontal control in choice menu
		if (horizontalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex++;
			if (m_currentFramePositionIndex % 2 == 0 || m_currentFramePositionIndex >= m_menus[m_menuHistory.GetCurrentMenuType()].Count)
			{
				m_currentFramePositionIndex--;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;

			UpdateCurrentDescription();
		}
		else if (horizontalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex--;
			if (m_currentFramePositionIndex % 2 == 1 || m_currentFramePositionIndex < 0)
			{
				m_currentFramePositionIndex++;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;

			UpdateCurrentDescription();
		}

		// Vertical control in choice menu
		if (verticalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex += MAX_COLUMNS_IN_CHOICEMENUS;
			if (m_currentFramePositionIndex >= m_menus[m_menuHistory.GetCurrentMenuType()].Count)
			{
				m_currentFramePositionIndex -= MAX_COLUMNS_IN_CHOICEMENUS;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;

			UpdateCurrentDescription();
		}
		else if (verticalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_currentFramePositionIndex -= MAX_COLUMNS_IN_CHOICEMENUS;
			if (m_currentFramePositionIndex < 0)
			{
				m_currentFramePositionIndex += MAX_COLUMNS_IN_CHOICEMENUS;
			}

			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);
			m_hasFrameMoved = true;

			UpdateCurrentDescription();
		}


		if (m_hasFrameMoved && Mathf.Abs(horizontalAxis) < m_resetThreshold && Mathf.Abs(verticalAxis) < m_resetThreshold)
		{
			m_hasFrameMoved = false;
		}
	}


	public void NavigateTargetSelection()
	{
		float horizontalAxis = Input.GetAxis("Horizontal");

		if (!m_isTargetListLoaded)
		{
			m_targetTransforms = new List<Transform>();
			CActionUiItem actionUiItem = m_menus[m_menuHistory.GetPrevMenuType()][m_lastIndexInMenu[m_menuHistory.GetPrevMenuType()]].GetComponent<CActionUiItem>();
			TargetType abilityTargetType = actionUiItem.GetAbility().GetTargetType();
			switch (abilityTargetType)
			{
				case TargetType.Enemy:
					foreach (CCombatParticipant participant in m_enemyTeam)
					{
						if (!participant.GetEntity().IsDefeated())
						{
							m_targetTransforms.Add(participant.GetEntity().transform);
						}
					}
					break;

				case TargetType.Ally:
					bool isProtectAbility = actionUiItem.GetAbility().GetType() == typeof(CDefenseAbility);
					bool isProtectAlly = abilityTargetType == TargetType.Ally;

					foreach (CCombatParticipant participant in m_playerTeam)
					{
						if (isProtectAbility && isProtectAlly)
						{
							if (participant.GetEntity().transform == m_currentParticipant.GetEntity().transform)
							{
								continue;
							}
						}

						if (!participant.GetEntity().IsDefeated())
						{
							m_targetTransforms.Add(participant.GetEntity().transform);
						}
					}
					break;

				case TargetType.Self:
					m_targetTransforms.Add(m_currentParticipant.GetEntity().transform);
					break;

				case TargetType.Environment:
					m_targetTransforms.AddRange(CCombatInitializer.GetInstance().DetermineSpawnpoints(Vector3.zero, float.MaxValue));
					break;

				default:
					break;
			}

			m_isTargetListLoaded = true;
			m_selectionIndex = 0;

			if (abilityTargetType != TargetType.Environment && m_targetTransforms.Count > 0)
			{
				m_statusUi.gameObject.SetActive(true);
				m_statusUi.UpdateStatusUI(CCombatSystemUi.GetInstance().GetParticipantByEntity(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>()));
				CCombatSystemUi.GetInstance().TargetGaugeUnit(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>());
			}
			else
			{
				m_statusUi.gameObject.SetActive(false);
			}
		}


		// Change selection index
		if (horizontalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_selectionIndex++;
			if (m_selectionIndex >= m_targetTransforms.Count)
			{
				m_selectionIndex = 0;
			}

			MoveArrowToIndexPosition(m_targetTransforms, m_selectionIndex);
			m_hasFrameMoved = true;
		}
		else if (horizontalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_selectionIndex--;
			if (m_selectionIndex < 0)
			{
				m_selectionIndex = m_targetTransforms.Count - 1;
			}

			MoveArrowToIndexPosition(m_targetTransforms, m_selectionIndex);
			m_hasFrameMoved = true;
		}

		if (m_statusUi.gameObject.activeSelf)
		{
			m_statusUi.UpdateStatusUI(CCombatSystemUi.GetInstance().GetParticipantByEntity(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>()));
			CCombatSystemUi.GetInstance().TargetGaugeUnit(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>());
		}

		if (m_hasFrameMoved && Mathf.Abs(horizontalAxis) < m_resetThreshold)
		{
			m_hasFrameMoved = false;
		}
	}

	private void NavigateObserverMode()
	{
		float horizontalAxis = Input.GetAxis("Horizontal");

		// Change selection index
		if (horizontalAxis > m_moveThreshold && !m_hasFrameMoved)
		{
			m_selectionIndex++;
			if (m_selectionIndex >= m_targetTransforms.Count)
			{
				m_selectionIndex = 0;
			}

			MoveArrowToIndexPosition(m_targetTransforms, m_selectionIndex);
			m_hasFrameMoved = true;
		}
		else if (horizontalAxis < -m_moveThreshold && !m_hasFrameMoved)
		{
			m_selectionIndex--;
			if (m_selectionIndex < 0)
			{
				m_selectionIndex = m_targetTransforms.Count - 1;
			}

			MoveArrowToIndexPosition(m_targetTransforms, m_selectionIndex);
			m_hasFrameMoved = true;
		}

		if (m_statusUi.gameObject.activeSelf)
		{
			m_statusUi.UpdateStatusUI(CCombatSystemUi.GetInstance().GetParticipantByEntity(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>()));
			CCombatSystemUi.GetInstance().TargetGaugeUnit(m_targetTransforms[m_selectionIndex].GetComponent<CEntity>());
		}

		if (m_hasFrameMoved && Mathf.Abs(horizontalAxis) < m_resetThreshold)
		{
			m_hasFrameMoved = false;
		}
	}


	public void OnSelectButtonClicked()
	{
		MenuType currentMenuType = m_menuHistory.GetCurrentMenuType();
		int currentMenuIndex = m_currentFramePositionIndex;


		// Tell the UI item to handle the selection
		if (m_menuHistory.GetCurrentMenuType() == MenuType.TargetSelection)
		{
			m_menus[m_menuHistory.GetPrevMenuType()][m_lastIndexInMenu[m_menuHistory.GetPrevMenuType()]].GetComponent<CUiItem>().HandleSelectionButton();
			m_targetSelectionArrow.SetActive(false);
			m_statusUi.gameObject.SetActive(false);
			CCombatSystemUi.GetInstance().TargetGaugeUnit(null);
		}
		else
		{
			bool hasSubMenu = m_menus[currentMenuType][currentMenuIndex].GetComponent<CSubmenuUiItem>() != null;
			bool hasAction = m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>() != null;
			if (hasSubMenu)
			{
				HandlePrevMenuAlpha(true);

				// Remember where the frame was positioned to return to that point when using back-button
				m_lastIndexInMenu[currentMenuType] = m_currentFramePositionIndex;

				// Set the current menu type to the type of the upcoming menu
				m_menuHistory.Add(m_menus[currentMenuType][currentMenuIndex].GetComponent<CSubmenuUiItem>().GetNextMenuType());

				// Move frame
				m_currentFramePositionIndex = 0;

				m_menus[currentMenuType][currentMenuIndex].GetComponent<CUiItem>().HandleSelectionButton();
				UpdateCurrentDescription();
			}
			else if (hasAction)
			{
				if (m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetIsDisabled())
				{
					Debug.Log("Not enough resources to use " + m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetAbility().GetAbilityName());
				}
				else
				{		
					// Reset target list
					m_targetTransforms.Clear();
					m_isTargetListLoaded = false;
					UpdateTeams();

					// Handle target selection
					m_targetSelectionArrow.SetActive(true);
					m_enemyTeam.Sort(SortByGaugePosition);
					m_playerTeam.Sort(SortByGaugePosition);

					TargetType actionTargetType = m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetAbility().GetTargetType();
					if (actionTargetType == TargetType.Enemy)
					{
						List<Transform> targets = new List<Transform>();
						targets.Add(m_enemyTeam[0].GetEntity().transform);


						MoveArrowToIndexPosition(targets, 0);
						m_statusUi.gameObject.SetActive(true);
						m_statusUi.UpdateStatusUI(CCombatSystemUi.GetInstance().GetParticipantByEntity(targets[0].GetComponent<CEntity>()));
						CCombatSystemUi.GetInstance().TargetGaugeUnit(targets[0].GetComponent<CEntity>());
					}
					else if (actionTargetType == TargetType.Ally)
					{
						List<Transform> targets = new List<Transform>();
						bool isProtectAbility = m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetAbility().GetType() == typeof(CDefenseAbility);
						bool isProtectAlly = actionTargetType == TargetType.Ally;

						if (isProtectAbility && isProtectAlly && m_playerTeam.Count > 1)
						{
							targets.Add(m_playerTeam[1].GetEntity().transform);
						}
						else
						{
							targets.Add(m_playerTeam[0].GetEntity().transform);
						}


						MoveArrowToIndexPosition(targets, 0);
						m_statusUi.gameObject.SetActive(true);
						m_statusUi.UpdateStatusUI(CCombatSystemUi.GetInstance().GetParticipantByEntity(targets[0].GetComponent<CEntity>()));
						CCombatSystemUi.GetInstance().TargetGaugeUnit(targets[0].GetComponent<CEntity>());
					}
					else if (actionTargetType == TargetType.Environment)
					{
						List<Transform> targets = new List<Transform>(CCombatInitializer.GetInstance().DetermineSpawnpoints(Vector3.zero, float.MaxValue));
						MoveArrowToIndexPosition(targets, 0);
					}
					else
					{
						List<Transform> targets = new List<Transform>();
						targets.Add(m_currentParticipant.GetEntity().transform);
						MoveArrowToIndexPosition(targets, 0);
					}

					// Handle menus
					HandlePrevMenuAlpha(true);

					// Remember where the frame was positioned to return to that point when using back-button
					m_lastIndexInMenu[currentMenuType] = m_currentFramePositionIndex;
					m_panelBeforeTargetSelection = m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetPrevMenuPanel();

					// Set the current menu type to the type of the upcoming menu (LIFO)
					m_menuHistory.Add(m_menus[currentMenuType][currentMenuIndex].GetComponent<CActionUiItem>().GetNextMenuType());
				}
			}
		}

		UpdateFrame();
	}


	public void OnBackButtonClicked()
	{
		if (m_menuHistory.GetCurrentMenuType() != MenuType.Menu && m_menuHistory.GetCurrentMenuType() != MenuType.TargetSelection)
		{
			HandlePrevMenuAlpha(false);
			
			// Deactivate the menu which is about to get left
			m_menus[m_menuHistory.GetCurrentMenuType()][0].transform.parent.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
			m_menus[m_menuHistory.GetCurrentMenuType()].Clear();

			// Go back to previous menu
			m_menuHistory.RemoveLast();

			// Move frame
			m_currentFramePositionIndex = m_lastIndexInMenu[m_menuHistory.GetCurrentMenuType()];				
			UpdateFrame();
		}
		else if (m_menuHistory.GetCurrentMenuType() == MenuType.TargetSelection)
		{
			// Stop blinking
			CCombatSystemUi.GetInstance().TargetGaugeUnit(null);

			// Deactivate status ui element
			m_statusUi.gameObject.SetActive(false);

			// Deactivate Arrow-GameObject
			m_targetSelectionArrow.SetActive(false);

			// Reactivate previous panel
			m_panelBeforeTargetSelection.GetComponent<CanvasGroup>().alpha = 1.0f;

			// Go back to previous menu
			m_menuHistory.RemoveLast();

			// Move frame
			m_currentFramePositionIndex = m_lastIndexInMenu[m_menuHistory.GetCurrentMenuType()];
			UpdateFrame();

			// Reset target list
			m_targetTransforms.Clear();
			m_isTargetListLoaded = false;
		}
	}


	public void UpdateTeams()
	{
		m_playerTeam = new List<CCombatParticipant>(CCombatSystemUi.GetInstance().GetAlivePlayerParticipants());
		m_enemyTeam = new List<CCombatParticipant>(CCombatSystemUi.GetInstance().GetAliveEnemyParticipants());
	}


	public void HandlePrevMenuAlpha(bool isSelectionButtonClicked)
	{
		if (isSelectionButtonClicked)
		{
			if (m_menuHistory.GetCurrentMenuType() == MenuType.SubMenu || m_menuHistory.GetCurrentMenuType() == MenuType.ChoiceMenu)
			{
				m_menus[m_menuHistory.GetCurrentMenuType()][0].parent.GetComponent<CanvasGroup>().alpha = 0.0f;
			}
		}
		else
		{
			if (m_menuHistory.GetPrevMenuType() == MenuType.SubMenu)
			{
				m_menus[m_menuHistory.GetPrevMenuType()][0].parent.GetComponent<CanvasGroup>().alpha = 1.0f;
			}
		}
	}


	private void UpdateFrame()
	{
		if (m_menuHistory.GetCurrentMenuType() == MenuType.TargetSelection)
		{
			m_frameImage.gameObject.SetActive(false);
		}
		else
		{
			m_frameImage.gameObject.SetActive(true);
			MoveFrameToIndexPosition(m_menus[m_menuHistory.GetCurrentMenuType()], m_currentFramePositionIndex);

			if (m_menuHistory.GetCurrentMenuType() == MenuType.Menu)
			{
				m_frameImage.rectTransform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
			}
			else
			{
				m_frameImage.rectTransform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
			}
		}
	}


	private void MoveFrameToIndexPosition(List<RectTransform> menu, int index)
	{
		m_frameImage.GetComponent<RectTransform>().position = menu[index].position;
	}


	private void MoveArrowToIndexPosition(List<Transform> targets, int index)
	{
		float arrowPositionY = 0.0f;
		if (targets[index].GetComponent<Collider>() != null)
		{
			arrowPositionY = targets[index].GetComponent<Collider>().bounds.size.y + 1.0f;
		}
		
		m_targetSelectionArrow.transform.position = new Vector3(targets[index].position.x, arrowPositionY, targets[index].position.z);	
	}


	private void UpdateCurrentDescription()
	{
		bool hasAction = m_menus[m_menuHistory.GetCurrentMenuType()][m_currentFramePositionIndex].GetComponent<CActionUiItem>() != null;
		if (hasAction)
		{
			CActionUiItem currentUiItem = m_menus[m_menuHistory.GetCurrentMenuType()][m_currentFramePositionIndex].GetComponent<CActionUiItem>();
			string description = currentUiItem.GetAbility().GetDescription();

			bool isDamageAbility = currentUiItem.GetAbility().GetType() == typeof(CDamageAbility);
			bool isStatAbility = currentUiItem.GetAbility().GetType() == typeof(CStatAbility);
			bool isHealAbility = currentUiItem.GetAbility().GetType() == typeof(CHealAbility);
			ResourceType resourceType = ResourceType.SkillPoints;
			int costs = 0;
			if (isDamageAbility)
			{
				CDamageAbility dmgAbility = (CDamageAbility)currentUiItem.GetAbility();
				resourceType = dmgAbility.GetResourceType();
				costs = dmgAbility.GetCosts();

				string replacementString;
				int minDamage;
				int maxDamage;
				m_currentParticipant.CalculateDamage(dmgAbility, out minDamage, out maxDamage);
				replacementString = string.Format("{0} - {1}", minDamage, maxDamage);

				description = description.Replace("$AMT", replacementString);
			}
			else if (isStatAbility)
			{
				CStatAbility statAbility = (CStatAbility)currentUiItem.GetAbility();
				resourceType = statAbility.GetResourceType();
				costs = statAbility.GetCosts();

				description = description.Replace("$PER", (Mathf.Abs(statAbility.GetPercentage() * 100.0f)).ToString());

				float duration = statAbility.GetDuration();
				string replacementString;
				if ((int)duration % 60 == 0)
				{
					int minutes = (int)duration / 60;
					if (minutes > 1)
					{
						replacementString = minutes + " minutes";
					}
					else
					{
						replacementString = minutes + " minute";
					}
				}
				else
				{
					int minutes = (int)duration / 60;
					int seconds = (int)duration % 60;

					if (minutes == 0)
					{
						replacementString = seconds + " seconds";
					}
					else if (minutes > 1)
					{
						replacementString = minutes + " minutes and " + seconds + " seconds";
					}
					else
					{
						replacementString = minutes + " minute and " + seconds + " seconds";
					}
				}

				description = description.Replace("$SEC", replacementString);
			}
			else if (isHealAbility)
			{
				CHealAbility healAbility = (CHealAbility)currentUiItem.GetAbility();
				resourceType = healAbility.GetResourceType();
				costs = healAbility.GetCosts();

				description = description.Replace("$AMT", healAbility.GetBaseHeal().ToString());
			}

			CCombatSystemUi.GetInstance().GetSelectionContext().GetComponent<CSelectionContext>().UpdateDescriptionText(description, resourceType, costs);
		}
	}


	public void ActivateObserverMode(List<CCombatParticipant> allParticipants)
	{
		m_isObserverModeActivated = true;
		m_targetTransforms.Clear();

		foreach (CCombatParticipant participant in allParticipants)
		{
			m_targetTransforms.Add(participant.GetEntity().transform);
		}

		m_targetSelectionArrow.SetActive(true);
		MoveArrowToIndexPosition(m_targetTransforms, 0);

		m_modeText.text = "Mode: Observer";

		m_statusUi.gameObject.SetActive(true);
		m_statusUi.UpdateStatusUI(allParticipants[0]);
		CCombatSystemUi.GetInstance().TargetGaugeUnit(allParticipants[0].GetEntity());
	}


	public void DeactivateObserverMode()
	{
		m_isObserverModeActivated = false;
		m_targetTransforms.Clear();

		m_targetSelectionArrow.SetActive(false);

		m_modeText.text = "Mode: Player";

		m_statusUi.gameObject.SetActive(false);
		CCombatSystemUi.GetInstance().TargetGaugeUnit(null);
	}


	public void ResetControl()
	{
		m_menuHistory.Reset();
		m_panelBeforeTargetSelection.GetComponent<CanvasGroup>().alpha = 0.0f;
		m_panelBeforeTargetSelection = null;
		m_currentFramePositionIndex = 0;

		UpdateFrame();
	}


	// Static methods
	public static int SortByGaugePosition(CCombatParticipant participant_01, CCombatParticipant participant_02)
	{
		if (participant_01.GetGaugePosition() > participant_02.GetGaugePosition())
		{
			return -1;
		}
		else if (participant_01.GetGaugePosition() < participant_02.GetGaugePosition())
		{
			return 1;
		}
		else
		{
			return 0;
		}
	}


	// Getter/Setter
	public CCombatParticipant GetCurrentParticipant()
	{
		return m_currentParticipant;
	}

	public void SetCurrentParticipant(CCombatParticipant currentParticipant)
	{
		m_currentParticipant = currentParticipant;
	}

	public GameObject GetChosenTarget()
	{
		return m_targetTransforms[m_selectionIndex].gameObject;
	}

	public void SetEntriesOfMenuType(List<RectTransform> menuEntries, MenuType menuTypeOfEntries)
	{
		m_menus[menuTypeOfEntries].Clear();

		foreach (RectTransform rectTransform in menuEntries)
		{
			m_menus[menuTypeOfEntries].Add(rectTransform);
		}
	}
}