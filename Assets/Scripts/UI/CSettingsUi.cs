using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CSettingsUi : MonoBehaviour 
{
	// Member variables
	[SerializeField] private Dropdown m_decisionHeuristicDropdown;
	[SerializeField] private Text m_rangeText;
	[SerializeField] private InputField m_rangeInputField;

	[SerializeField] private Dropdown m_actionScoreCompensationDropdown;
	[SerializeField] private Text m_epsilonText;
	[SerializeField] private InputField m_epsilonInputField;

	[SerializeField] private Slider m_manipulationRangeSlider;
	[SerializeField] private Text m_manipulationRangeText;
	[SerializeField] private InputField m_manipulationRangeInputField;

	private CUtilityAiSystem.DecisionHeuristic m_heuristic;
	private int m_rangeInput;

	private CUtilityAiSystem.ActionScoreCompensation m_compensation;
	private float m_epsilonInput;

	private float m_manipulationRange;

	void Start()
	{
		m_heuristic = CSettings.GetInstance().GetDecisionHeuristic();
		m_rangeInput = CSettings.GetInstance().GetHeuristicRange();

		m_compensation = CSettings.GetInstance().GetActionScoreCompensation();
		m_epsilonInput = CSettings.GetInstance().GetEpsilon();

		ToggleRange();
		ToggleEpsilon();
	}


	// Methods
	public void UpdateHeuristic()
	{
		m_heuristic = (CUtilityAiSystem.DecisionHeuristic)m_decisionHeuristicDropdown.value;

		UpdateRangeInput();
		UpdateManipulationRangeInput();
		UpdateEpsilonInput();
		ToggleRange();
	}

	public void ToggleRange()
	{
		if (m_heuristic == CUtilityAiSystem.DecisionHeuristic.RandomFromBest || m_heuristic == CUtilityAiSystem.DecisionHeuristic.RandomAroundMedian)
		{
			m_rangeText.gameObject.SetActive(true);
			m_rangeInputField.gameObject.SetActive(true);

			m_manipulationRangeText.gameObject.SetActive(false);
			m_manipulationRangeSlider.gameObject.SetActive(false);
			m_manipulationRangeInputField.gameObject.SetActive(false);
		}
		else if (m_heuristic == CUtilityAiSystem.DecisionHeuristic.ActionScoreManipulation)
		{
			m_rangeText.gameObject.SetActive(false);
			m_rangeInputField.gameObject.SetActive(false);

			m_manipulationRangeText.gameObject.SetActive(true);
			m_manipulationRangeSlider.gameObject.SetActive(true);
			m_manipulationRangeInputField.gameObject.SetActive(true);
		}
		else
		{
			m_rangeText.gameObject.SetActive(false);
			m_rangeInputField.gameObject.SetActive(false);

			m_manipulationRangeText.gameObject.SetActive(false);
			m_manipulationRangeSlider.gameObject.SetActive(false);
			m_manipulationRangeInputField.gameObject.SetActive(false);
		}

		UpdateSettings();
	}

	public void UpdateRangeInput()
	{
		m_rangeInput = int.Parse(m_rangeInputField.text);
		m_rangeInput = Mathf.Clamp(m_rangeInput, 0, 20);
		m_rangeInputField.text = m_rangeInput.ToString();

		UpdateSettings();
	}

	public void UpdateManipulationRangeInput()
	{
		m_manipulationRange = m_manipulationRangeSlider.value;
		m_manipulationRangeInputField.text = m_manipulationRange.ToString();


		UpdateSettings();
	}

	public void UpdateCompensation()
	{
		m_compensation = (CUtilityAiSystem.ActionScoreCompensation)m_actionScoreCompensationDropdown.value;

		ToggleEpsilon();
	}

	public void ToggleEpsilon()
	{
		if (m_compensation == CUtilityAiSystem.ActionScoreCompensation.ActionScoreDamping)
		{
			m_epsilonText.enabled = true;
			m_epsilonInputField.gameObject.SetActive(true);
		}
		else
		{
			m_epsilonText.enabled = false;
			m_epsilonInputField.gameObject.SetActive(false);
		}

		UpdateSettings();
	}

	public void UpdateEpsilonInput()
	{
		m_epsilonInput = float.Parse(m_epsilonInputField.text);
		m_epsilonInput = Mathf.Clamp(m_epsilonInput, 0.01f, 2.0f);
		m_epsilonInputField.text = m_epsilonInput.ToString();

		UpdateSettings();
	}


	public void UpdateSettings()
	{
		CSettings.GetInstance().SetDecisionHeuristic(m_heuristic);
		CSettings.GetInstance().SetHeuristicRange(m_rangeInput);

		CSettings.GetInstance().SetActionScoreCompensation(m_compensation);
		CSettings.GetInstance().SetEpsilon(m_epsilonInput);

		CSettings.GetInstance().SetManipulationRange(m_manipulationRange);
	}
}
