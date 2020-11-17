using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CSubmenuUiItem : CUiItem
{
	// Member variables
	[Header("Properties")]
	[SerializeField] private RectTransform m_nextMenuPanel;
	

	// Methods
	public override void HandleSelectionButton()
	{
		List<RectTransform> entriesOfMenuType = new List<RectTransform>();

		m_nextMenuPanel.gameObject.GetComponent<CanvasGroup>().alpha = 1.0f;

		for (int i = 0; i < m_nextMenuPanel.childCount; i++)
		{
			if (m_nextMenuPanel.GetChild(i).GetComponent<CUiItem>() != null)
			{
				entriesOfMenuType.Add(m_nextMenuPanel.GetChild(i).GetComponent<RectTransform>());
			}
		}

		CCombatSystemUi.GetInstance().GetComponent<CCombatControlUI>().SetEntriesOfMenuType(entriesOfMenuType, m_nextMenuType);
	}


	public override void HandleBackButton()
	{

		m_nextMenuPanel.gameObject.GetComponent<CanvasGroup>().alpha = 0.0f;
	}


	// Getter/Setter
	public RectTransform GetNextMenuPanel()
	{
		return m_nextMenuPanel;
	}
}