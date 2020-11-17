using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGroup : MonoBehaviour
{
	// Member variables
	[Header("General")]
	[SerializeField] private string m_id;
	[SerializeField] private float m_distanceToMember;

	[Header("Member")]
	[SerializeField] private List<CEntity> m_groupMember;

	[Header("State")]
	[SerializeField] private bool m_isGroupDefeated;
	[SerializeField] private bool m_isGroupSurprised;
	private List<Vector3> m_deltaPositionStorage;


	// MonoBehaviour-Methods
	void Awake()
	{
		m_groupMember = new List<CEntity>();
		m_id = gameObject.name.Substring(0, 1) + gameObject.name.Substring(gameObject.name.Length - 2);
		m_isGroupDefeated = false;
		m_isGroupSurprised = false;
		
		DestroyDuplicatedGroupsAtAwake();
	}


	void Start()
	{
		// Toglle all colliders after being sure there aren't complications
		ToggleColliderOfGroupMembers(true);

		// Initialize group
		SortGroupMembers();
		gameObject.tag = gameObject.transform.GetChild(0).gameObject.tag;
		m_deltaPositionStorage = new List<Vector3>();
	}


	// Methods
	public void MoveGroupMember()
	{
		for (int i = 1; i < m_groupMember.Count; i++)
		{
			float totalDeltaMagnitude = 0.0f;
			foreach (Vector3 vector in m_deltaPositionStorage)
			{
				totalDeltaMagnitude += vector.magnitude;
			}

			if (totalDeltaMagnitude > m_distanceToMember * i + 0.01f)
			{
				CPlayer currentGroupMember = (CPlayer)m_groupMember[i];
				Vector3 nextPosition = m_groupMember[i].transform.position + m_deltaPositionStorage[currentGroupMember.GetLastStorageIndex()];
				m_groupMember[i].GetComponent<Rigidbody>().MovePosition(nextPosition);
				m_groupMember[i].transform.LookAt(nextPosition);
				currentGroupMember.IncreaseStorageIndex();

				if (i == m_groupMember.Count - 1)
				{
					m_deltaPositionStorage.RemoveAt(0);

					foreach (CPlayer groupMember in m_groupMember)
					{
						groupMember.DecreaseStorageIndex();
					}
				}
			}
		}
	}


	public void AddGroupMember(CEntity entity)
	{
		m_groupMember.Add(entity);
	}


	public void AddDeltaPosition(Vector3 deltaPosition)
	{
		m_deltaPositionStorage.Add(deltaPosition);
	}


	private void DestroyDuplicatedGroupsAtAwake()
	{
		if (CSceneManager.GetInstance().GetGameObjectsToMoveCount() > 0)
		{
			foreach (GameObject go in CSceneManager.GetInstance().GetGameObjectsToMove())
			{
				if (go.GetComponent<CGroup>() != null)
				{
					if (m_id.Equals(go.GetComponent<CGroup>().m_id))
					{
						Destroy(gameObject);
						return;
					}
				}
			}

			foreach (string id in CWorld.GetInstance().GetDefeatedGroupIDs())
			{
				if (m_id.Equals(id))
				{
					Destroy(gameObject);
					return;
				}
			}
		}
	}


	public void ToggleColliderOfGroupMembers(bool toggleTo)
	{
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			if (gameObject.transform.GetChild(i).GetComponent<CEntity>() == null)
			{
				Destroy(gameObject.transform.GetChild(i).gameObject);
			}
			else
			{
				gameObject.transform.GetChild(i).GetComponent<Collider>().enabled = toggleTo;
			}
		}
	}


	public void SortGroupMembers()
	{
		if (m_groupMember.Count > 1)
		{
			m_groupMember.Sort(CEntity.SortByIndex);

			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).SetSiblingIndex(transform.GetChild(i).GetComponent<CEntity>().GetIndex());
			}
		}
	}


	public void SaveGroupMemberTransform()
	{
		transform.GetChild(0).GetComponent<CEntity>().SaveLastPosition();
		transform.GetChild(0).GetComponent<CEntity>().SaveLastRotation();

		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<CEntity>().SetIsTransformSaved(true);
		}
	}


	public void LoadGroupMemberTransform()
	{
		transform.GetChild(0).GetComponent<CEntity>().LoadLastPosition();
		transform.GetChild(0).GetComponent<CEntity>().LoadLastRotation();

		transform.GetChild(0).GetComponent<CEntity>().SetIsTransformSaved(false);

		for (int i = 1; i < transform.childCount; i++)
		{
			transform.GetChild(i).transform.localPosition = transform.GetChild(0).localPosition;
			transform.GetChild(i).transform.rotation = transform.GetChild(0).localRotation;

			if (transform.GetChild(i).GetComponent<CPlayer>() != null)
			{
				transform.GetChild(i).GetComponent<CPlayer>().ResetStorageIndex();
			}

			transform.GetChild(i).GetComponent<CEntity>().SetIsTransformSaved(false);
		}

		m_deltaPositionStorage.Clear();
	}


	public void ResetGroupMemberVitals()
	{
		for (int i = 0; i < transform.childCount; i++)
		{
			transform.GetChild(i).GetComponent<CEntity>().ResetVitals();
		}
	}



	// Getter/Setter
	public List<CEntity> GetGroupMember()
	{
		return m_groupMember;
	}

	public string GetID()
	{
		return m_id;
	}

	public bool GetIsGroupDefeated()
	{
		return m_isGroupDefeated;
	}

	public void SetIsGroupDefeated(bool value)
	{
		m_isGroupDefeated = value;
	}

	public void SetIsSurprised(bool isSurprised)
	{
		m_isGroupSurprised = isSurprised;
	}

	public bool GetIsSurprised()
	{
		return m_isGroupSurprised;
	}
}