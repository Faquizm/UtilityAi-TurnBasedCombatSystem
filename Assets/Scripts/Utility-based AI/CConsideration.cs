using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class CConsideration : MonoBehaviour
{
	// Member variables
	[Header("General")]
	protected string m_name;
	protected float m_input;
	protected float m_score;

	[Header("Response Curve")]
	[SerializeField] protected CResponseCurve m_responseCurve = new CResponseCurve();


	// MonoBehaviour-Methods
	protected virtual void Start()
	{
		foreach (CAction action in GetComponents<CAction>())
		{
			if (m_responseCurve.GetBookends().minValue >= m_responseCurve.GetBookends().maxValue)
			{
				Debug.LogWarning("Bookend minimum is greater or equal it's maximum in \"" + gameObject.name + "\" of \"" + transform.parent.parent.name + "\". This may result in unintended behaviour.");
			}

			action.AddConsideration(this);
		}

		m_name = GetType().ToString().Substring(1);
	}


	// Methods
	public abstract float CalculateConsiderationScore();

	public float MapToBookends(float value)
	{
		return Mathf.InverseLerp(m_responseCurve.GetBookends().minValue, m_responseCurve.GetBookends().maxValue, value);
	}

	// Getter/Setter
	public string GetName()
	{
		return m_name;
	}

	public CResponseCurve GetResponseCurve()
	{
		return m_responseCurve;
	}
}
