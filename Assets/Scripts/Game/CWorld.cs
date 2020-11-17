using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CWorld : MonoBehaviour 
{
	// Member variables
	private static CWorld m_world;

	private bool m_isWorldStarted = false;

	// Remember all defeated groups to prevent them from spawning
	[Header("World data")]
	[SerializeField] private List<string> m_defeatedGroupIDs;

	// Remember camera position and rotation
	private Vector3 m_cameraPosition = Vector3.zero;
	private Vector3 m_cameraRotation = Vector3.zero;


	// MonoBehaviour-Methods
	void Awake()
	{
		if (m_world == null)
		{
			m_world = GameObject.FindGameObjectWithTag("World").GetComponent<CWorld>();
			m_defeatedGroupIDs = new List<string>();

			// World should exist in all scenes
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	void Start()
	{
		m_isWorldStarted = true;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}

		if (Input.GetKeyDown(KeyCode.E))
		{
			CDecisionLogger.ExportAllLogs();
		}
	}


	// Methods
	public void AddDefeatedGroupID(string defeatedGroupID)
	{
		m_defeatedGroupIDs.Add(defeatedGroupID);
	}

	public void SaveCameraTransform(Transform cameraTransform)
	{
		m_cameraPosition = cameraTransform.position;
		m_cameraRotation = cameraTransform.rotation.eulerAngles;
	}


	// Getter/Setter
	public static CWorld GetInstance()
	{
		return m_world;
	}

	public List<string> GetDefeatedGroupIDs()
	{
		return m_defeatedGroupIDs;
	}

	public Vector3 GetCameraPosition()
	{
		return m_cameraPosition;
	}

	public Vector3 GetCameraRotation()
	{
		return m_cameraRotation;
	}

	public bool GetIsStarted()
	{
		return m_isWorldStarted;
	}
}