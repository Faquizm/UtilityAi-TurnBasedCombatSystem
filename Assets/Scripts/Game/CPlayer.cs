using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CPlayer : CEntity 
{
	// Member variables
	[SerializeField] private int m_lastStorageIndex;


	// MonoBehaviour-Methods
	sealed protected override void Awake()
	{
		base.Awake();
		m_lastStorageIndex = 0;
	}


	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Enemy" && SceneManager.GetActiveScene().buildIndex == 0)
		{
			Debug.Log(gameObject.name + " collided with: " + other.gameObject.name + " of " + other.gameObject.transform.parent.name + "\nFrame: " + Time.frameCount);

			// Prepare the player before starting the combat
			if (!m_isTransformSaved)
			{
				transform.parent.GetComponent<CGroup>().SaveGroupMemberTransform();
			}
			Camera.main.GetComponent<CCameraControls>().SaveTransformToWorld();

			bool isInFieldOfView = IsInFieldOfView(other.transform);
			if (!isInFieldOfView)
			{
				transform.parent.GetComponent<CGroup>().SetIsSurprised(true);
			}
			else
			{
				transform.parent.GetComponent<CGroup>().SetIsSurprised(false);
			}


			// Add player group and enemy group to the scene manager to be moved to the combat scene 
			CSceneManager.GetInstance().AddGameObjectToMove(transform.parent.gameObject);
			CSceneManager.GetInstance().AddGameObjectToMove(other.transform.parent.gameObject);

			CSceneManager.GetInstance().StartCombat();
		}
	}


	// Methods
	public void IncreaseStorageIndex()
	{
		m_lastStorageIndex++;
	}

	public void DecreaseStorageIndex()
	{
		m_lastStorageIndex--;
	}

	public void ResetStorageIndex()
	{
		m_lastStorageIndex = 0;
	}


	// Getter/Setter
	public int GetLastStorageIndex()
	{
		return m_lastStorageIndex;
	}

	public CDefenseAbility GetProtectAbility()
	{
		return m_defenseAbilities[0];
	}

	public CDefenseAbility GetProtectAllyAbility()
	{
		return m_defenseAbilities[1];
	}
}