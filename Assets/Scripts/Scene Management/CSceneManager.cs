using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CSceneManager : MonoBehaviour
{
	// Member variables
	private static CSceneManager m_sceneManager;
	bool m_isLoadingScene;

	[Header("Scene Objects")]
	[SerializeField] private CSceneInitializer m_currentInitializer;
	[SerializeField] private List<GameObject> m_gameObjectsToMove;

	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_sceneManager == null)
		{
			m_sceneManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<CSceneManager>();
			m_isLoadingScene = false;
			m_gameObjectsToMove = new List<GameObject>();

			m_currentInitializer = FindObjectOfType<CSceneInitializer>();

			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	
	// Methods
	public void StartCombat()
	{
		if (!m_isLoadingScene)
		{
			StartCoroutine(LoadAsyncCombatScene());
			m_isLoadingScene = true;
		}
	}


	public void LeaveCombatScene()
	{
		StartCoroutine(LoadAsyncEnvironmentScene());
	}


	public void AddGameObjectToMove(GameObject go)
	{
		if (!m_isLoadingScene)
		{
			m_gameObjectsToMove.Add(go);
		}
	}


	public void ClearGameObjectsToMove()
	{
		m_gameObjectsToMove.Clear();
	}


	// Coroutine-Methods
	IEnumerator LoadAsyncCombatScene()
	{
		// Load the combat scene
		Scene currentScene = SceneManager.GetActiveScene();

		string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(1));
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
	
		while (!asyncLoad.isDone)
		{
			yield return null;
		}
		
		// Move scene objects to the loaded scene
		foreach (GameObject go in m_gameObjectsToMove)
		{
			SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(sceneName));
		}

		// Unload environment scene
		SceneManager.UnloadSceneAsync(currentScene);
		m_isLoadingScene = false;
		m_gameObjectsToMove.Clear();
	}


	IEnumerator LoadAsyncEnvironmentScene()
	{
		// Deactivate collider to prevent collider to trigger unintentionally
		foreach (GameObject go in m_gameObjectsToMove)
		{
			if (go.GetComponent<CGroup>() != null)
			{
				go.GetComponent<CGroup>().ToggleColliderOfGroupMembers(false);
			}
		}

		// Load the environment scene
		Scene currentScene = SceneManager.GetActiveScene();

		string sceneName = System.IO.Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(0));
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

		while (!asyncLoad.isDone)
		{
			yield return null;
			Debug.Log("Loading done at: " + Time.frameCount);
		}

		// Move all undefeated groups back to the other scene to their original position and reset vitals if necessary
		foreach (GameObject go in m_gameObjectsToMove)
		{
			if (go.GetComponent<CGroup>() != null)
			{
				if (!go.GetComponent<CGroup>().GetIsGroupDefeated())
				{
					SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(sceneName));
					go.GetComponent<CGroup>().LoadGroupMemberTransform();
					Debug.Log("Group Resetted at: " + Time.frameCount);
					CEnvironmentInitializer.GetInstance().GetSceneCamera().gameObject.GetComponent<CCameraControls>().LoadTransformFromWorld();
					go.GetComponent<CGroup>().ToggleColliderOfGroupMembers(true);
					
					if (go.tag == "Enemy")
					{
						go.GetComponent<CGroup>().ResetGroupMemberVitals();
					}
				}
				else
				{
					Destroy(go);
				}
			}
		}

		// Unload scene
		SceneManager.UnloadSceneAsync(currentScene);
		m_isLoadingScene = false;
		m_gameObjectsToMove.Clear();
	}


	// Getter/Setter
	public static CSceneManager GetInstance()
	{
		return m_sceneManager;
	}

	public CSceneInitializer GetCurrentSceneInitializer()
	{
		return m_currentInitializer;
	}

	public void SetCurrentSceneInitializer(CSceneInitializer sceneInitializer)
	{
		m_currentInitializer = sceneInitializer;
	}
	
	public List<GameObject> GetGameObjectsToMove()
	{
		return m_gameObjectsToMove;
	}

	public int GetGameObjectsToMoveCount()
	{
		return m_gameObjectsToMove.Count;
	}
}