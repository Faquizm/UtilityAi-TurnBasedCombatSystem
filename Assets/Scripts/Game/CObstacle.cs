using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CObstacle : MonoBehaviour 
{
	// Methods
	public List<Transform> DetermineHideSpots(GameObject hideFrom)
	{
		List<Transform> possibleHideSpots = new List<Transform>();

		foreach (Transform hideSpot in CCombatInitializer.GetInstance().GetAllPoints())
		{
			Ray linearDistance = new Ray(hideSpot.position, hideFrom.transform.position - hideSpot.position);
			RaycastHit hit;
			LayerMask layerMask = LayerMask.GetMask("Entities");
			Physics.Raycast(linearDistance, out hit, layerMask);

			if (hit.transform == transform)
			{
				possibleHideSpots.Add(hideSpot);
			}
		}

		return possibleHideSpots;
	}
}