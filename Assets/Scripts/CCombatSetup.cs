using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CCombatSetup : MonoBehaviour 
{
	// Member variables
	private bool m_isEvaluationCombat;

	private List<Transform> m_allSpawnPoints;
	private List<CCombatParticipant> m_enemyTeam;
	private List<CCombatParticipant> m_playerTeam;

	[SerializeField] private RectTransform m_scenarioDecision;
	[SerializeField] private RectTransform m_scenarioStart;
	
	// MonoBehaviour-Methods
	void Awake()
	{
		m_isEvaluationCombat = false;
	}
	
	void Start() 
	{
		
	}
	
	void Update() 
	{
		if (CCombatSystem.GetInstance().GetIsRunning() && m_isEvaluationCombat == false)
		{
			if (CCombatSystem.GetInstance().GetEnemyTeam()[0].GetEntity().transform.parent.name.Contains("Optional"))
			{
				ResetSpawnpoints();
				SetupVariables();
				SetupCombatSystem();
				m_isEvaluationCombat = true;

				m_scenarioDecision.gameObject.SetActive(true);
				m_scenarioStart.gameObject.SetActive(false);
			}
		}

		if (m_isEvaluationCombat)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario01();
				SetupCombatSystem();
			}

			if (!Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario02_SingleTargetHeal();
				SetupCombatSystem();
			}

			if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Alpha2))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario02_AreaOfEffectHeal();
				SetupCombatSystem();
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario03();
				SetupCombatSystem();
			}

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario04();
				SetupCombatSystem();
			}

			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario05();
				SetupCombatSystem();
			}

			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				m_scenarioDecision.gameObject.SetActive(false);
				m_scenarioStart.gameObject.SetActive(true);
				ResetSpawnpoints();
				SetupScenario06();
				SetupCombatSystem();
			}

			if (Input.GetKeyDown(KeyCode.C))
			{
				if (m_scenarioStart.gameObject.activeSelf)
				{
					m_scenarioStart.gameObject.SetActive(false);
				}

				CCombatSystem.GetInstance().SetIsPaused(!CCombatSystem.GetInstance().GetIsPaused());
			}
		}
	}
	
	
	// Methods
	public void ResetLogEntries()
	{
		foreach (CCombatParticipant participant in m_enemyTeam)
		{
			CDecisionLogger.ResetEntityFile(participant);
		}

		foreach (CCombatParticipant participant in m_playerTeam)
		{
			CDecisionLogger.ResetEntityFile(participant);
		}
	}

	public void SetupVariables()
	{
		foreach (Transform transform in m_allSpawnPoints)
		{
			transform.GetComponent<CSpawnpoint>().Reset();
		}

		m_enemyTeam = new List<CCombatParticipant>();
		m_enemyTeam.AddRange(CCombatSystem.GetInstance().GetEnemyTeam());

		m_playerTeam = new List<CCombatParticipant>();
		m_playerTeam.AddRange(CCombatSystem.GetInstance().GetPlayerTeam());

		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = false;
		}
	}

	public void ResetSpawnpoints()
	{
		m_allSpawnPoints = new List<Transform>();
		m_allSpawnPoints.AddRange(CCombatInitializer.GetInstance().GetAllPoints());
	}

	public void SetupCombatSystem()
	{
		CCombatSystem.GetInstance().SetIsPaused(true);
		CCombatSystemUi.GetInstance().ToggleObserverUI(true);
	}

	public void SetupScenario01()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[0].GetEntity().SetIsControlledByAI(true);


		// Position
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[16].position;
		m_allSpawnPoints[16].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[40].position;
		m_allSpawnPoints[40].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[2].position;
		m_allSpawnPoints[2].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[29].position;
		m_allSpawnPoints[29].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[32].position;
		m_allSpawnPoints[32].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[19].position;
		m_allSpawnPoints[19].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(590.0f);
		m_playerTeam[1].SetGaugePosition(75.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(0.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);


		// Damage
		m_enemyTeam[1].GetEntity().TakeDamage(14.0f);
		m_enemyTeam[2].GetEntity().TakeDamage(60.0f);


		// Freezing
		m_playerTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);


		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}
	
	public void SetupScenario02_SingleTargetHeal()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[1].GetEntity().SetIsControlledByAI(true);


		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[34].position;
		m_allSpawnPoints[34].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[31].position;
		m_allSpawnPoints[31].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[38].position;
		m_allSpawnPoints[38].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[35].position;
		m_allSpawnPoints[35].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[0].position;
		m_allSpawnPoints[0].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[14].position;
		m_allSpawnPoints[14].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(0.0f);
		m_playerTeam[1].SetGaugePosition(590.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(0.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);


		// Damage
		m_playerTeam[0].GetEntity().TakeDamage(64.0f);
		m_playerTeam[1].GetEntity().TakeDamage(30.0f);

		m_enemyTeam[0].GetEntity().TakeDamage(20.0f);
		m_enemyTeam[1].GetEntity().TakeDamage(5.0f);
		m_enemyTeam[2].GetEntity().TakeDamage(14.0f);
		m_enemyTeam[3].GetEntity().TakeDamage(17.0f);


		// Freezing
		m_playerTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}
	
	public void SetupScenario02_AreaOfEffectHeal()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[1].GetEntity().SetIsControlledByAI(true);


		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[34].position;
		m_allSpawnPoints[34].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[6].position;
		m_allSpawnPoints[6].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[38].position;
		m_allSpawnPoints[38].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[12].position;
		m_allSpawnPoints[12].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[13].position;
		m_allSpawnPoints[13].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[14].position;
		m_allSpawnPoints[14].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(0.0f);
		m_playerTeam[1].SetGaugePosition(590.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(0.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);


		// Damage
		m_playerTeam[0].GetEntity().TakeDamage(56.0f);
		m_playerTeam[1].GetEntity().TakeDamage(45.0f);
		m_playerTeam[2].GetEntity().TakeDamage(70.0f);
		m_playerTeam[3].GetEntity().TakeDamage(40.0f);

		m_enemyTeam[0].GetEntity().TakeDamage(20.0f);
		m_enemyTeam[1].GetEntity().TakeDamage(32.0f);
		m_enemyTeam[2].GetEntity().TakeDamage(14.0f);
		m_enemyTeam[3].GetEntity().TakeDamage(17.0f);

		// Freezing
		m_playerTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);


		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}

	public void SetupScenario03()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[3].GetEntity().SetIsControlledByAI(true);

		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[40].position;
		m_allSpawnPoints[40].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[30].position;
		m_allSpawnPoints[30].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[29].position;
		m_allSpawnPoints[29].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[24].position;
		m_allSpawnPoints[24].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[5].position;
		m_allSpawnPoints[5].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[38].position;
		m_allSpawnPoints[38].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[8].position;
		m_allSpawnPoints[8].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[1].position;
		m_allSpawnPoints[1].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(0.0f);
		m_playerTeam[1].SetGaugePosition(0.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(590.0f);

		m_enemyTeam[0].SetGaugePosition(0.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);

		// Freezing
		m_playerTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);


		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}

	public void SetupScenario04()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[0].GetEntity().SetIsControlledByAI(true);

		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[31].position;
		m_allSpawnPoints[31].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[6].position;
		m_allSpawnPoints[6].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[32].position;
		m_allSpawnPoints[32].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[12].position;
		m_allSpawnPoints[12].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[24].position;
		m_allSpawnPoints[24].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[25].position;
		m_allSpawnPoints[25].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(590.0f);
		m_playerTeam[1].SetGaugePosition(0.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(750.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);

		// Freezing
		m_playerTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);


		// Decision
		m_enemyTeam[0].MakeDecision();
		


		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}
	
	public void SetupScenario05()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[0].GetEntity().SetIsControlledByAI(true);


		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[31].position;
		m_allSpawnPoints[31].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[6].position;
		m_allSpawnPoints[6].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[32].position;
		m_allSpawnPoints[32].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[2].position;
		m_allSpawnPoints[2].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[27].position;
		m_allSpawnPoints[27].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[28].position;
		m_allSpawnPoints[28].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(590.0f);
		m_playerTeam[1].SetGaugePosition(0.0f);
		m_playerTeam[2].SetGaugePosition(125.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(385.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);


		// Damage
		m_playerTeam[0].GetEntity().TakeDamage(64.0f);


		// Decision
		m_enemyTeam[0].MakeDecision();


		// Freezing
		m_playerTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);


		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}

	public void SetupScenario06()
	{
		ResetLogEntries();

		// AI Control
		m_playerTeam[2].GetEntity().SetIsControlledByAI(true);


		// Positions
		m_playerTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[31].position;
		m_allSpawnPoints[31].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[0].GetEntity());

		m_playerTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[6].position;
		m_allSpawnPoints[6].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[1].GetEntity());

		m_playerTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[9].position;
		m_allSpawnPoints[9].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[2].GetEntity());

		m_playerTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[15].position;
		m_allSpawnPoints[15].GetComponent<CSpawnpoint>().Reserve(m_playerTeam[3].GetEntity());

		m_enemyTeam[0].GetEntity().transform.localPosition = m_allSpawnPoints[32].position;
		m_allSpawnPoints[32].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[0].GetEntity());

		m_enemyTeam[1].GetEntity().transform.localPosition = m_allSpawnPoints[2].position;
		m_allSpawnPoints[2].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[1].GetEntity());

		m_enemyTeam[2].GetEntity().transform.localPosition = m_allSpawnPoints[27].position;
		m_allSpawnPoints[27].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[2].GetEntity());

		m_enemyTeam[3].GetEntity().transform.localPosition = m_allSpawnPoints[28].position;
		m_allSpawnPoints[28].GetComponent<CSpawnpoint>().Reserve(m_enemyTeam[3].GetEntity());


		// Gauge Position
		m_playerTeam[0].SetGaugePosition(25.0f);
		m_playerTeam[1].SetGaugePosition(0.0f);
		m_playerTeam[2].SetGaugePosition(590.0f);
		m_playerTeam[3].SetGaugePosition(0.0f);

		m_enemyTeam[0].SetGaugePosition(75.0f);
		m_enemyTeam[1].SetGaugePosition(0.0f);
		m_enemyTeam[2].SetGaugePosition(0.0f);
		m_enemyTeam[3].SetGaugePosition(50.0f);


		// Damage
		m_playerTeam[0].GetEntity().TakeDamage(64.0f);
		m_playerTeam[2].GetEntity().SpendResource(ResourceType.SkillPoints, 32);
		m_playerTeam[2].GetEntity().SpendResource(ResourceType.MagicPoints, 12);


		// Freezing
		m_playerTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_playerTeam[3].GetEntity().GetStats().SetSpeed(0.0f);

		m_enemyTeam[0].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[1].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[2].GetEntity().GetStats().SetSpeed(0.0f);
		m_enemyTeam[3].GetEntity().GetStats().SetSpeed(0.0f);



		foreach (CCombatParticipant p in m_playerTeam)
		{
			p.GetEntity().GetComponent<NavMeshAgent>().enabled = true;
		}
	}

	// Getter/Setter


}
