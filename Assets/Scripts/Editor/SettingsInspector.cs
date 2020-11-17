using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSettings), true), CanEditMultipleObjects]
public class SettingsInspector : Editor 
{
	// Member variables
	CSettings m_settings;

	SerializedProperty m_decisionHeuristic;
	SerializedProperty m_heuristicRange;
	SerializedProperty m_manipulationRange;

	SerializedProperty m_actionScoreCompensation;
	SerializedProperty m_actionScoreDampingEpsilon;

	// Methods
	private void OnEnable()
	{
		m_settings = ((CSettings)target).GetComponent<CSettings>();

		m_decisionHeuristic = serializedObject.FindProperty("m_decisionHeuristic");
		m_heuristicRange = serializedObject.FindProperty("m_heuristicRange");
		m_manipulationRange = serializedObject.FindProperty("m_manipulationRange");

		m_actionScoreCompensation = serializedObject.FindProperty("m_actionScoreCompensation");
		m_actionScoreDampingEpsilon = serializedObject.FindProperty("m_actionScoreDampingEpsilon");
	}


	public override void OnInspectorGUI()
	{
		serializedObject.Update();

		EditorGUILayout.PropertyField(m_decisionHeuristic, new GUIContent("Decision Heuristic"));
		CUtilityAiSystem.DecisionHeuristic heuristic = (CUtilityAiSystem.DecisionHeuristic)m_decisionHeuristic.enumValueIndex;
		switch (heuristic)
		{
			case CUtilityAiSystem.DecisionHeuristic.Best:
			case CUtilityAiSystem.DecisionHeuristic.Median:
			case CUtilityAiSystem.DecisionHeuristic.Arbitrary:
				break;
			case CUtilityAiSystem.DecisionHeuristic.RandomFromBest:
			case CUtilityAiSystem.DecisionHeuristic.RandomAroundMedian:
				EditorGUILayout.PropertyField(m_heuristicRange, new GUIContent("Range"));
				break;
			case CUtilityAiSystem.DecisionHeuristic.ActionScoreManipulation:
				EditorGUILayout.PropertyField(m_manipulationRange, new GUIContent("Range"));
				break;
			default:
				break;
		}


		EditorGUILayout.PropertyField(m_actionScoreCompensation, new GUIContent("Action Score Compensation"));
		CUtilityAiSystem.ActionScoreCompensation compensation = (CUtilityAiSystem.ActionScoreCompensation)m_actionScoreCompensation.enumValueIndex;
		switch (compensation)
		{
			case CUtilityAiSystem.ActionScoreCompensation.None:
			case CUtilityAiSystem.ActionScoreCompensation.MarkAndSizer:
			case CUtilityAiSystem.ActionScoreCompensation.Average:
				break;

			case CUtilityAiSystem.ActionScoreCompensation.ActionScoreDamping:
				EditorGUILayout.PropertyField(m_actionScoreDampingEpsilon, new GUIContent("Epsilon"));
				break;

			default:
				break;
		}

		serializedObject.ApplyModifiedProperties();
	}
}
