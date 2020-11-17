using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CEnemy : CEntity
{
	// MonoBehaviour-Methods
	sealed protected override void Awake()
	{
		base.Awake();
	}


	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.tag == "Player" && SceneManager.GetActiveScene().buildIndex == 0)
		{
			// Prepare the enemy before starting the combat
			transform.parent.GetComponent<CGroup>().SaveGroupMemberTransform();

			bool isInFieldOfView = IsInFieldOfView(other.transform);
			if (!isInFieldOfView)
			{
				transform.parent.GetComponent<CGroup>().SetIsSurprised(true);
			}
			else
			{
				transform.parent.GetComponent<CGroup>().SetIsSurprised(false);
			}
		}
	}
}