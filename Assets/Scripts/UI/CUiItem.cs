using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CUiItem : MonoBehaviour 
{
	// Member variables
	[Header("Item properties")]
	[SerializeField] protected MenuType m_nextMenuType;


	// Methods
	public abstract void HandleSelectionButton();
	public abstract void HandleBackButton();


	// Getter/Setter
	public MenuType GetNextMenuType()
	{
		return m_nextMenuType;
	}
}
