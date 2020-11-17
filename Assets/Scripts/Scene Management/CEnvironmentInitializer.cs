using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnvironmentInitializer : CSceneInitializer
{
	// Member variables
	private static CEnvironmentInitializer m_environmentInitializer;

	[Header("Camera")]
	[SerializeField] private Camera m_sceneCamera;


	// MonoBehaviour-Methods
	sealed protected override void Awake()
	{
		if (m_environmentInitializer == null)
		{
			m_environmentInitializer = GameObject.FindGameObjectWithTag("EnvironmentInitializer").GetComponent<CEnvironmentInitializer>();
		}
	}


	sealed protected override void Start()
	{
		base.Start();
	}
	

	// Methods
	sealed public override void Init()
	{
		GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
		foreach (GameObject go in players)
		{
			if (go.GetComponent<CPlayerControls>() != null)
			{
				go.GetComponent<CPlayerControls>().SetCamera();
				go.GetComponent<CPlayerControls>().enabled = true;
				break;
			}
		}

		// Find the player for the camera controls
		m_sceneCamera.gameObject.GetComponent<CCameraControls>().FindPlayer();

		Debug.Log("CEnvironmentInitializer initialized at " + Time.frameCount + ".");
	}

	sealed public override void Final()
	{
		Debug.Log("CEnvironmentInitializer finalized at " + Time.frameCount + ".");
	}


	// Getter/Setter
	public static CEnvironmentInitializer GetInstance()
	{
		return m_environmentInitializer;
	}

	public Camera GetSceneCamera()
	{
		return m_sceneCamera;
	}
}