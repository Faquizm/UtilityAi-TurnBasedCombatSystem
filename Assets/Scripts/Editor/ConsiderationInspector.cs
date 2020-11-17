using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CConsideration), true), CanEditMultipleObjects]
public class ConsiderationInspector : Editor 
{
    struct Coordinates
    {
        public float x;
        public float y;
    }

    // Member variables for drawing the response curve
    SerializedProperty m_responseCurve;
    SerializedProperty m_curveType;
    SerializedProperty m_m;
    SerializedProperty m_k;
    SerializedProperty m_b;
    SerializedProperty m_c;
    SerializedProperty m_bookends;

    Material m_material;

    List<Coordinates> m_curveValues;
    float m_calculationSteps;
    CConsideration m_consideration = null;
    static int m_considerationCounter = 0;

    float m_rectWidth;
    float m_rectHeight;

    float m_gridWidth;
    float m_gridHeight;

    float m_heightOffset;

    int m_cellAmount;
    float m_cellSize;

    List<GUILayoutOption> m_singleVerticalLabelLayoutOptions;

    
    // Methods
    private void OnEnable()
    {
        m_responseCurve = serializedObject.FindProperty("m_responseCurve");
        m_curveType = m_responseCurve.FindPropertyRelative("m_curveType");
        m_m = m_responseCurve.FindPropertyRelative("m_m");
        m_k = m_responseCurve.FindPropertyRelative("m_k");
        m_b = m_responseCurve.FindPropertyRelative("m_b");
        m_c = m_responseCurve.FindPropertyRelative("m_c");
        m_bookends = m_responseCurve.FindPropertyRelative("m_bookends");

        m_singleVerticalLabelLayoutOptions = new List<GUILayoutOption>();
        m_singleVerticalLabelLayoutOptions.Add(GUILayout.Width(23));
        m_singleVerticalLabelLayoutOptions.Add(GUILayout.Height(18));

        // Prepare shader and materials
        var shader = Shader.Find("Hidden/Internal-Colored");
        m_material = new Material(shader);


        // Prepare consideration data
        m_curveValues = new List<Coordinates>();
        m_calculationSteps = 0.001f;


        // Set the current consideration
        m_consideration = ((CConsideration)target).GetComponents<CConsideration>()[m_considerationCounter];
        m_considerationCounter++;


        // Calculate curve values
        RecalculateCurve();


        // Prepare grid 
        m_rectWidth = 200.0f;
        m_rectHeight = 220.0f;

        m_gridWidth = 200.0f;
        m_gridHeight = 200.0f;

        m_heightOffset = 20.0f;

        m_cellAmount = 10;
        m_cellSize = m_gridWidth / m_cellAmount;
    }


    private void OnDisable()
    {
        m_considerationCounter = 0;

        DestroyImmediate(m_material);
    }

    public override void OnInspectorGUI()
    {
        // Display variables
        ShowNecessaryVariables();

        // Draw graphs
        DrawResponseCurve();
    }

    private void ShowNecessaryVariables()
    {
        serializedObject.Update();

        CResponseCurve.CurveType curveType = (CResponseCurve.CurveType)m_curveType.enumValueIndex;
        switch (curveType)
        {
            case CResponseCurve.CurveType.Step:
                EditorGUILayout.PropertyField(m_curveType, new GUIContent("Curve Type"));
                EditorGUILayout.PropertyField(m_k, new GUIContent("k"));
                EditorGUILayout.PropertyField(m_bookends, new GUIContent("Bookends"), true);
                break;

            case CResponseCurve.CurveType.Linear:

                EditorGUILayout.PropertyField(m_curveType, new GUIContent("Curve Type"));
                EditorGUILayout.PropertyField(m_m, new GUIContent("m"));
                EditorGUILayout.PropertyField(m_b, new GUIContent("b"));
                EditorGUILayout.PropertyField(m_c, new GUIContent("c"));
                EditorGUILayout.PropertyField(m_bookends, new GUIContent("Bookends"), true);
                break;

            case CResponseCurve.CurveType.Polynomial:
            case CResponseCurve.CurveType.DensityNormalDistribution:
            case CResponseCurve.CurveType.Logistic:
            case CResponseCurve.CurveType.Logit:
                EditorGUILayout.PropertyField(m_curveType, new GUIContent("Curve Type"));
                EditorGUILayout.PropertyField(m_m, new GUIContent("m"));
                EditorGUILayout.PropertyField(m_k, new GUIContent("k"));
                EditorGUILayout.PropertyField(m_b, new GUIContent("b"));
                EditorGUILayout.PropertyField(m_c, new GUIContent("c"));
                EditorGUILayout.PropertyField(m_bookends, new GUIContent("Bookends"), true);
                break;

            default:
                break;

        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawResponseCurve()
    {
        // Arrange the vertical axis beside the graph
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();
        GUILayout.Space(10);

        for (int i = 10; i > 0; i--)
        {
            float axisLabel = i / 10.0f;
            EditorGUILayout.LabelField(axisLabel.ToString("0.0"), EditorStyles.centeredGreyMiniLabel, m_singleVerticalLabelLayoutOptions.ToArray());
        }

        EditorGUILayout.EndVertical();


        // Prepare rectangles
        Rect rectGrid = GUILayoutUtility.GetRect(m_rectWidth, m_rectWidth, m_rectHeight, m_rectHeight);

        // Draw grid rectangle
        if (Event.current.type == EventType.Repaint)
        {
            // Setup rectangle
            rectGrid.width = m_gridWidth;
            rectGrid.height = m_gridHeight;
            rectGrid.center += new Vector2((m_rectWidth - m_gridWidth) / 2.0f, m_heightOffset);


            // Draw the grid
            GUI.BeginClip(rectGrid);
            GL.Clear(true, false, Color.black);
            m_material.SetPass(0);

            // White background
            GL.Begin(GL.QUADS);
            GL.Color(Color.white);
            GL.Vertex3(0.0f, 0.0f, 0.0f);
            GL.Vertex3(rectGrid.width, 0.0f, 0.0f);
            GL.Vertex3(rectGrid.width, rectGrid.height, 0.0f);
            GL.Vertex3(0.0f, rectGrid.height, 0.0f);
            GL.End();

            // Grey grid lines
            GL.Begin(GL.LINES);
            GL.Color(Color.grey);

            for (int i = 0; i <= m_cellAmount; i++)
            {
                float lineStart = i * m_cellSize;

                // Vertical lines
                if (lineStart >= 0 && lineStart <= m_gridWidth)
                {
                    GL.Vertex3(lineStart, 0, 0);
                    GL.Vertex3(lineStart, m_gridHeight, 0);
                }

                // Horizontal lines
                if (lineStart <= m_gridHeight)
                {
                    GL.Vertex3(0, lineStart, 0);
                    GL.Vertex3(m_gridWidth, lineStart, 0);
                }
            }
            GL.End();


            // Draw the curve
            // Recalculate curve value before it is painted
            RecalculateCurve();

            GL.Begin(GL.LINE_STRIP);
            GL.Color(Color.black);

            for (int i = 0; i < m_curveValues.Count; i++)
            {
                float x = m_curveValues[i].x * m_gridWidth;
                float y = m_gridHeight - m_curveValues[i].y * m_gridHeight;

                if (y >= 0.0f && y <= m_gridHeight)
                {
                    GL.Vertex3(x, y, 0);
                }
            }
            GL.End();
            GUI.EndClip();
        }

        GUILayout.Space(30);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("0.0       0.1 0.2 0.3 0.4 0.5 0.6 0.7 0.8 0.9 1.0      ", EditorStyles.centeredGreyMiniLabel);
    }


    private void RecalculateCurve()
    {
        m_curveValues.Clear();

        float f = 0.0f;
        while (f <= 1.0f)
        {
            Coordinates coords;
            coords.x = f;
            coords.y = m_consideration.GetResponseCurve().CalculateResponseCurveValue(f);


            m_curveValues.Add(coords);

            f += m_calculationSteps;
        }
    }
}
