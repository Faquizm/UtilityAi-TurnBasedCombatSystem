using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CCombatSystemUi : MonoBehaviour
{
	// Member variables
	public static CCombatSystemUi m_combatSystemUi;

	[Header("Camera")]
	[SerializeField] private Camera m_combatCamera;

	[Header("Data")]
	[SerializeField] private List<CCombatParticipant> m_enemyTeamReference;
	[SerializeField] private List<CCombatParticipant> m_playerTeamReference;
	[SerializeField] private List<CCombatParticipant> m_allParticipantsReference;

	[Header("HUD")]
	[SerializeField] private RectTransform m_playerHudContext;
	[SerializeField, Range(5, 20)] private int m_spaceBetweenElements = 20;

	[Header("UI Elements")]
	[SerializeField] private RectTransform m_selectionContextPanel;
	[SerializeField] private RectTransform m_playerHudContentPanel;
	[SerializeField] private RectTransform m_hudFrame;
	[SerializeField] private Text m_pausedText;

	[Header("UI Templates")]
	[SerializeField] private GameObject m_uiPlayerHudElement;

	[Header("UI - Gauge")]
	[SerializeField] private RectTransform m_gaugeBar;
	[SerializeField] private GameObject m_playerUiTemplate;
	[SerializeField] private GameObject m_enemyUiTemplate;

	[Space(7)]
	[SerializeField] private List<CGaugeUnit> m_gaugeUnits;


	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_combatSystemUi == null)
		{
			m_combatSystemUi = GameObject.FindGameObjectWithTag("CombatSystemUi").GetComponent<CCombatSystemUi>();
		}
	}


	// Methods
	public void Init()
	{
		if (CCombatSystem.GetInstance().GetEnemyTeam().Count > 0 && CCombatSystem.GetInstance().GetPlayerTeam().Count > 0)
		{
			// Get the data from the CombatSystem
			m_enemyTeamReference = CCombatSystem.GetInstance().GetEnemyTeam();
			m_playerTeamReference = CCombatSystem.GetInstance().GetPlayerTeam();
			m_allParticipantsReference = CCombatSystem.GetInstance().GetBothTeamsAsOneList();

			m_gaugeUnits = new List<CGaugeUnit>();
			// Create all gauge entities
			foreach (CCombatParticipant participant in m_allParticipantsReference)
			{
				m_gaugeUnits.Add(CreateGaugeUnit(participant));
			}
		}
		else
		{
			Debug.LogError("Couldn't initialize CombatSystemUi. No teams created.");
		}

		InitializePlayerHUD();
		GetComponent<CCombatControlUI>().UpdateTeams();
	}


	private void InitializePlayerHUD()
	{
		List<CCombatParticipant> playerTeam = m_playerTeamReference;
		int playerCount = playerTeam.Count;
		int hudElementWidth = (int)m_uiPlayerHudElement.GetComponent<RectTransform>().sizeDelta.x;
		int space = m_spaceBetweenElements * (playerCount + 1);

		m_playerHudContext.GetComponent<RectTransform>().sizeDelta = new Vector2(hudElementWidth * playerCount + space, 96.0f);

		for (int i = 0; i < playerCount; i++)
		{
			GameObject go = Instantiate(m_uiPlayerHudElement, m_playerHudContext.GetChild(0).transform);

			go.GetComponent<CPlayerHUD>().InitializeHUD(playerTeam[i]);

			go.name = "PlayerHUD_" + (i + 1);
			go.GetComponent<RectTransform>().anchoredPosition = new Vector3(hudElementWidth / 2 + i * hudElementWidth + m_spaceBetweenElements * (i + 1), 0.0f, 0.0f);
		}
	}


	public IEnumerator RunCombatUi()
	{
		Debug.Log("Combat system UI running...");

		while (CCombatSystem.GetInstance().GetIsRunning())
		{
			// When the combat system continues, update the player HUD
			for (int i = 0; i < m_playerHudContentPanel.childCount; i++)
			{
				m_playerHudContentPanel.GetChild(i).GetComponent<CPlayerHUD>().UpdateAll();
			}

			// Update the icon position for each gauge unit
			foreach (CGaugeUnit gaugeUnit in m_gaugeUnits)
			{
				if (gaugeUnit.GetCombatParticipant().GetHasDecisionChanged())
				{
					if (gaugeUnit.GetCombatParticipant().GetEntity().CompareTag("Enemy"))
					{
						gaugeUnit.GetCombatParticipant().GetEntity().GetComponentInChildren<Canvas>().gameObject.AddComponent<CDecisionVisualization>();
						gaugeUnit.GetCombatParticipant().GetEntity().GetComponentInChildren<Canvas>().gameObject.GetComponent<CDecisionVisualization>().ActivateTimer(2.0f);

						CCombatSystem.GetInstance().ResetParticipantHasDecisionChanged(gaugeUnit.GetCombatParticipant());
					}
				}

				gaugeUnit.UpdateIconPosition();
			}

			if (CCombatSystem.GetInstance().GetIsPaused())
			{
				bool isAnyoneExecuting = false;
				foreach (CCombatParticipant participant in m_allParticipantsReference)
				{
					isAnyoneExecuting |= participant.GetIsExecutingAction();
				}

				if (isAnyoneExecuting)
				{
					m_pausedText.text = "--- Executing ---";
				}
				else
				{
					m_pausedText.text = "--- Paused ---";
				}

			}
			else
			{
				m_pausedText.text = "--- Running ---";
			}


			yield return new WaitForEndOfFrame();
		}

		Debug.Log("Combat system UI stopped.");
	}


	public CGaugeUnit CreateGaugeUnit(CCombatParticipant participant)
	{
		GameObject entityUi;

		string iconPath = string.Format("Entities/{0}/{1}", participant.GetEntity().GetName(), participant.GetEntity().GetIconName());
		if (participant.GetEntity().CompareTag("Player"))
		{
			entityUi = Instantiate(m_playerUiTemplate, m_gaugeBar.transform);
			entityUi.GetComponent<CPlayerUi>().Init(iconPath);
		}
		else if (participant.GetEntity().CompareTag("Enemy"))
		{
			int participantIndex = participant.GetEntity().GetIndex();
			entityUi = Instantiate(m_enemyUiTemplate, m_gaugeBar.transform);
			entityUi.GetComponent<CEnemyUi>().Init(iconPath, participantIndex + 1);     // Being Index with 1
		}
		else
		{
			entityUi = new GameObject();
			Debug.LogError(string.Format("Combat participant ({0}) has invalid tag.", participant.GetEntity().GetName()));
		}

		entityUi.tag = participant.GetEntity().tag;

		return new CGaugeUnit(participant, entityUi.GetComponent<RectTransform>());
	}


	public void ToggleParticipantUI(CCombatParticipant participant, bool turnOn)
	{
		int participantIndex = participant.GetEntity().GetIndex();

		if (turnOn)
		{
			// Selection panel
			GetComponent<CCombatControlUI>().SetCurrentParticipant(participant);
			GetComponent<CCombatControlUI>().enabled = true;
			m_selectionContextPanel.GetComponent<CanvasGroup>().alpha = 1.0f;

			// Player HUD
			m_playerHudContentPanel.GetChild(participantIndex).localScale = new Vector3(1.05f, 1.05f, 1.05f);
			m_hudFrame.position = m_playerHudContentPanel.GetChild(participantIndex).position;
			m_hudFrame.gameObject.SetActive(true);

			m_selectionContextPanel.GetComponent<CSelectionContext>().UpdateSelectionMenu(participant);

			// Activate decision UI
			participant.GetEntity().GetComponentInChildren<Canvas>().gameObject.AddComponent<CDecisionVisualization>();
		}
		else
		{
			// Selection panel
			GetComponent<CCombatControlUI>().enabled = false;
			m_selectionContextPanel.GetComponent<CanvasGroup>().alpha = 0.0f;

			// Player HUD
			m_playerHudContentPanel.GetChild(participantIndex).localScale = new Vector3(1.0f, 1.0f, 1.0f);
			m_hudFrame.gameObject.SetActive(false);

			GetComponent<CCombatControlUI>().ResetControl();

			m_selectionContextPanel.GetComponent<CSelectionContext>().ClearSelectionMenu();

			// Destroy decision UI
			Destroy(participant.GetEntity().GetComponentInChildren<Canvas>().gameObject.GetComponent<CDecisionVisualization>());
		}
	}

	public void ToggleObserverUI(bool turnOn)
	{
		if (turnOn)
		{
			GetComponent<CCombatControlUI>().enabled = true;
			GetComponent<CCombatControlUI>().ActivateObserverMode(m_allParticipantsReference);
		}
		else
		{
			GetComponent<CCombatControlUI>().enabled = false;
			GetComponent<CCombatControlUI>().DeactivateObserverMode();
		}
	}


	public void TargetGaugeUnit(CEntity entity)
	{
		foreach (CGaugeUnit gaugeUnit in m_gaugeUnits)
		{
			if (gaugeUnit.GetCombatParticipant().GetEntity() == entity)
			{
				gaugeUnit.GetParticipantUi().GetComponent<CEntityUi>().StartBlinking();
			}
			else
			{
				gaugeUnit.GetParticipantUi().GetComponent<CEntityUi>().StopBlinking();
			}
		}
	}


	public void SortIconsOnGaugeBar()
	{
		List<GameObject> entitiesOnGaugeBar = new List<GameObject>();

		// Get all children from the gauge bar
		for (int i = 0; i < m_gaugeBar.childCount; i++)
		{
			entitiesOnGaugeBar.Add(m_gaugeBar.GetChild(i).gameObject);
		}

		// Sort them
		entitiesOnGaugeBar.Sort(SortByTag);

		// Reorder them
		for (int i = 0; i < entitiesOnGaugeBar.Count; i++)
		{
			entitiesOnGaugeBar[i].gameObject.transform.SetAsLastSibling();
		}
	}


	public static int SortByTag(GameObject entity_01, GameObject entity_02)
	{
		if (entity_01.tag.Equals("Player") && entity_02.tag.Equals("Player")
			|| entity_01.tag.Equals("Enemy") && entity_02.tag.Equals("Enemy"))
		{
			return 0;
		}
		else if (entity_01.tag.Equals("Player") && entity_02.tag.Equals("Enemy"))
		{
			return 1;
		}
		else if (entity_01.tag.Equals("Enemy") && entity_02.tag.Equals("Player"))
		{
			return -1;
		}
		else
		{
			Debug.LogError("Unknown entity tags.");
			return 0;
		}
	}


	// Getter/Setter
	public static CCombatSystemUi GetInstance()
	{
		return m_combatSystemUi;
	}

	public Camera GetCombatCamera()
	{
		return m_combatCamera;
	}

	public CCombatParticipant GetParticipantByEntity(CEntity entity)
	{
		return CCombatSystem.GetInstance().GetParticipantByEntity(entity);
	}

	public List<CCombatParticipant> GetAlivePlayerParticipants()
	{
		return CCombatSystem.GetInstance().GetAlivePlayerParticipants();
	}

	public List<CCombatParticipant> GetAliveEnemyParticipants()
	{
		return CCombatSystem.GetInstance().GetAliveEnemyParticipants();
	}

	public RectTransform GetSelectionContext()
	{
		return m_selectionContextPanel;
	}	
}