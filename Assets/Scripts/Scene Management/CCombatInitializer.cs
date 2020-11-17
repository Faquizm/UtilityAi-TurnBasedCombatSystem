using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class CCombatInitializer : CSceneInitializer
{
	// Member variables
	private static CCombatInitializer m_combatInitializer;

	[Header("General")]
	[SerializeField] private List<GameObject> m_participantingGroups;

	[SerializeField] private GameObject m_spawnpointsNeutral;
	[SerializeField] private GameObject m_spawnpointsIsSurprised;
	[SerializeField] private GameObject m_spawnpointsIsSurprising;

	[Header("UI Templates")]
	[SerializeField] private GameObject m_uiPlayerHudElement;


	// MonoBehaviour-Methods
	sealed protected override void Awake()
	{
		if (m_combatInitializer == null)
		{
			m_combatInitializer = GameObject.FindGameObjectWithTag("CombatInitializer").GetComponent<CCombatInitializer>();
			m_participantingGroups = new List<GameObject>();
		}
	}
	

	sealed protected override void Start()
	{
		base.Start();

		CCombatSystemUi.GetInstance().SortIconsOnGaugeBar();
	}

	
	// Methods
	sealed public override void Init()
	{
		// Physics
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Entities"), LayerMask.NameToLayer("Entities"), false);

		// Disable player controls
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject go in players)
		{
			if (go.GetComponent<CPlayerControls>() != null)
			{
				go.GetComponent<CPlayerControls>().enabled = false;
				break;
			}
		}

		List<GameObject> gameObjectsFromOtherScene = CSceneManager.GetInstance().GetGameObjectsToMove();

		// Process all object from the other scene
		bool hasGroupAdvantage = false;         // Did one of the groups got surprised which results in an advantage for the other group?
		foreach (GameObject go in gameObjectsFromOtherScene)
		{
			if (go.GetComponent<CGroup>() != null)
			{
				hasGroupAdvantage |= go.GetComponent<CGroup>().GetIsSurprised();
				m_participantingGroups.Add(go);
			}
		}

		foreach (GameObject participatingGroup in m_participantingGroups)
		{
			foreach (CEntity groupMember in participatingGroup.GetComponent<CGroup>().GetGroupMember())
			{
				if (groupMember.gameObject.layer == LayerMask.NameToLayer("Enemies"))
				{
					groupMember.gameObject.layer = LayerMask.NameToLayer("Entities");
				}
				
				if (groupMember.IsDefeated())
				{
					groupMember.gameObject.SetActive(false);
				}


				// Create a combat participant for each group member
				CCombatParticipant combatParticipant = new CCombatParticipant(groupMember);

				// Add participant to the combat system
				CCombatSystem.GetInstance().AddCombatParticipant(combatParticipant);

				// Give participant a starting position in the combat area and rotation
				int randomSpawnpoint = 0;
				Vector3 randomSpawnpointPosition = Vector3.zero;
				Vector3 lookToCenter = Vector3.zero;
				if (hasGroupAdvantage)
				{
					if (groupMember.transform.parent.GetComponent<CGroup>().GetIsSurprised())
					{
						do
						{
							// Search for an available spawnpoint
							randomSpawnpoint = Random.Range(0, m_spawnpointsIsSurprised.transform.childCount);
						} while (!m_spawnpointsIsSurprised.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().IsAvailable());

						// Reserve the available spawnpoint
						m_spawnpointsIsSurprised.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().Reserve(groupMember.GetComponent<CEntity>());

						randomSpawnpointPosition = m_spawnpointsIsSurprised.transform.GetChild(randomSpawnpoint).position;
						groupMember.transform.localPosition = randomSpawnpointPosition;
						lookToCenter = new Vector3(0.0f, groupMember.transform.position.y, 0.0f);
						groupMember.transform.LookAt(lookToCenter);
						groupMember.transform.Rotate(Vector3.up, 180.0f);
					}
					else
					{
						do
						{
							// Search for an available spawnpoint
							randomSpawnpoint = Random.Range(0, m_spawnpointsIsSurprising.transform.childCount);
						} while (!m_spawnpointsIsSurprising.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().IsAvailable());

						// Reserve the available spawnpoint
						m_spawnpointsIsSurprising.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().Reserve(groupMember.GetComponent<CEntity>());

						randomSpawnpointPosition = m_spawnpointsIsSurprising.transform.GetChild(randomSpawnpoint).position;
						groupMember.transform.localPosition = randomSpawnpointPosition;
						lookToCenter = new Vector3(0.0f, groupMember.transform.position.y, 0.0f);
						groupMember.transform.LookAt(lookToCenter);
					}
				}
				else
				{
					do
					{
						// Search for an available spawnpoint
						randomSpawnpoint = Random.Range(0, m_spawnpointsNeutral.transform.childCount);
					} while (!m_spawnpointsNeutral.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().IsAvailable());

					// Reserve the available spawnpoint
					m_spawnpointsNeutral.transform.GetChild(randomSpawnpoint).GetComponent<CSpawnpoint>().Reserve(groupMember.GetComponent<CEntity>());

					randomSpawnpointPosition = m_spawnpointsNeutral.transform.GetChild(randomSpawnpoint).position;
					groupMember.transform.localPosition = randomSpawnpointPosition;

					Vector3 toCenter = new Vector3(0.0f, groupMember.transform.position.y, 0.0f) + groupMember.transform.position;
					if (toCenter.magnitude > 10.0f)
					{
						lookToCenter = new Vector3(0.0f, groupMember.transform.position.y, 0.0f);
						groupMember.transform.LookAt(lookToCenter);
					}
					else
					{
						lookToCenter = new Vector3(0.0f, groupMember.transform.position.y, 0.0f);
						groupMember.transform.LookAt(lookToCenter);
						groupMember.transform.Rotate(Vector3.up, 180.0f);
					}
				}


				if (!groupMember.gameObject.GetComponent<Collider>().isTrigger)
				{
					groupMember.gameObject.GetComponent<Collider>().isTrigger = true;
				}

				// Init a nav mesh agent to control movements
				if (groupMember.gameObject.GetComponent<NavMeshAgent>() == null)
				{
					groupMember.gameObject.AddComponent<NavMeshAgent>();
					groupMember.gameObject.GetComponent<NavMeshAgent>().baseOffset = 0.0f;
					groupMember.gameObject.GetComponent<NavMeshAgent>().height = groupMember.gameObject.GetComponent<Collider>().bounds.size.y;
				}
			}
		}

		// Init the combat system including initial gauge positioning
		CCombatSystem.GetInstance().Init();

		FindObjectOfType<CCombatCamera>().AddCameraTargets(CCombatSystem.GetInstance().GetBothTeamsAsOneList());

		// Init the combat system UI include icons on combat gauge and HUD
		CCombatSystemUi.GetInstance().Init();
		Debug.Log("CCombatInitializer initialized at " + Time.frameCount + ".");

		// Start combat system
		CCombatSystem.GetInstance().StartCombatSystem();
	}


	sealed public override void Final()
	{
		// Physics
		Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Entities"), LayerMask.NameToLayer("Entities"), true);

		foreach (GameObject go in m_participantingGroups)
		{
			if (go.GetComponent<CGroup>() != null)
			{
				foreach (CEntity entity in go.GetComponent<CGroup>().GetGroupMember())
				{
					if (entity.gameObject.CompareTag("Enemy"))
					{
						entity.gameObject.layer = LayerMask.NameToLayer("Enemies");
					}
					else
					{
						entity.gameObject.GetComponent<CapsuleCollider>().isTrigger = false;
					}

					Destroy(entity.GetComponent<NavMeshAgent>());
				}
			}

			if (go.GetComponent<CGroup>().GetIsGroupDefeated())
			{
				Debug.Log(go.name + " marked as defeated in world.");
				AddDefeatedGroupsToWorld(go.GetComponent<CGroup>());
			}
		}

		m_participantingGroups.Clear();

		Debug.Log("CCombatInitializer finalized at " + Time.frameCount + ".");
	}


	public void AddCombatParticipantsToSceneManager()
	{
		foreach (GameObject combatParticipant in m_participantingGroups)
		{
			CSceneManager.GetInstance().AddGameObjectToMove(combatParticipant);
		}
	}


	public void AddDefeatedGroupsToWorld(CGroup defeatedGroup)
	{
		CWorld.GetInstance().AddDefeatedGroupID(defeatedGroup.GetID());
	}


	public List<Transform> DetermineSpawnpoints(Vector3 entityPosition, float radius)
	{
		List<Transform> availableSpawnpoints = new List<Transform>();
		List<Transform> allSpawnpoints = new List<Transform>();
		float currentRadius = radius;
		float radiusMultiplier = 1.0f;

		for (int i = 0; i < m_spawnpointsNeutral.transform.childCount; i++)
		{
			allSpawnpoints.Add(m_spawnpointsNeutral.transform.GetChild(i));
		}

		for (int i = 0; i < m_spawnpointsIsSurprised.transform.childCount; i++)
		{
			allSpawnpoints.Add(m_spawnpointsIsSurprised.transform.GetChild(i));
		}

		for (int i = 0; i < m_spawnpointsIsSurprising.transform.childCount; i++)
		{
			allSpawnpoints.Add(m_spawnpointsIsSurprising.transform.GetChild(i));
		}


		while (availableSpawnpoints.Count == 0 && radiusMultiplier <= 2.0f)
		{
			foreach (Transform spawnpoint in allSpawnpoints)
			{
				float distanceToEntity = (spawnpoint.position - entityPosition).magnitude;

				bool isCurrentSpawnpointAvailable = spawnpoint.GetComponent<CSpawnpoint>().IsAvailable();
				if (isCurrentSpawnpointAvailable && distanceToEntity < currentRadius)
				{
					availableSpawnpoints.Add(spawnpoint);
				}
			}

			if (availableSpawnpoints.Count == 0)
			{
				Debug.Log("No available spawnpoint found in radius.");
				radiusMultiplier += 0.2f;
				currentRadius = radius * radiusMultiplier;
			}
		}
		

		if (availableSpawnpoints.Count == 0)
		{
			Debug.LogWarning("No spawnpoint found in extended radius.");
		}

		return availableSpawnpoints;
	}


	// Getter/Setter
	public static CCombatInitializer GetInstance()
	{
		return m_combatInitializer;
	}

	public List<Transform> GetNeutralSpawnpoints()
	{
		List<Transform> neutralSpawnpoints = new List<Transform>();
		for(int i = 0; i < m_spawnpointsNeutral.transform.childCount; i++)
		{
			neutralSpawnpoints.Add(m_spawnpointsNeutral.transform.GetChild(i));
		}

		return neutralSpawnpoints;
	}

	public List<Transform> GetAllPoints()
	{
		List<Transform> allPoints = new List<Transform>();

		for (int i = 0; i < m_spawnpointsNeutral.transform.childCount; i++)
		{
			allPoints.Add(m_spawnpointsNeutral.transform.GetChild(i));
		}

		for (int i = 0; i < m_spawnpointsIsSurprised.transform.childCount; i++)
		{
			allPoints.Add(m_spawnpointsIsSurprised.transform.GetChild(i));
		}

		for (int i = 0; i < m_spawnpointsIsSurprising.transform.childCount; i++)
		{
			allPoints.Add(m_spawnpointsIsSurprising.transform.GetChild(i));
		}

		return allPoints;
	}

	public List<Transform> GetAllAvailablePoints()
	{
		List<Transform> allAvailablePoints = new List<Transform>();

		for (int i = 0; i < m_spawnpointsNeutral.transform.childCount; i++)
		{
			if (m_spawnpointsNeutral.transform.GetChild(i).GetComponent<CSpawnpoint>().IsAvailable())
			{
				allAvailablePoints.Add(m_spawnpointsNeutral.transform.GetChild(i));
			}
		}

		for (int i = 0; i < m_spawnpointsIsSurprised.transform.childCount; i++)
		{
			if (m_spawnpointsIsSurprised.transform.GetChild(i).GetComponent<CSpawnpoint>().IsAvailable())
			{
				allAvailablePoints.Add(m_spawnpointsIsSurprised.transform.GetChild(i));
			}
		}

		for (int i = 0; i < m_spawnpointsIsSurprising.transform.childCount; i++)
		{
			if (m_spawnpointsIsSurprising.transform.GetChild(i).GetComponent<CSpawnpoint>().IsAvailable())
			{
				allAvailablePoints.Add(m_spawnpointsIsSurprising.transform.GetChild(i));
			}
		}

		return allAvailablePoints;
	}
}