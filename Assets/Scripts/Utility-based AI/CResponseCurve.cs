using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CResponseCurve
{
    // Enumerations
    public enum CurveType { Step, Linear, Polynomial, DensityNormalDistribution, Logistic, Logit }


    // Structs
    [System.Serializable]
    public struct Bookends
    {
        public float minValue;
        public float maxValue;
    }


    // Member variables
    [Header("Response Curve", order = 0)][Space(-7, order = 1)]
    [Header("Parameter", order = 2)]
    [SerializeField] private CurveType m_curveType;

    [Tooltip("Slope of the curve.")]
    [SerializeField] private float m_m;

    [Tooltip("Exponent of the curve.")]
    [SerializeField] private float m_k;

    [Tooltip("Vertical shift")]
    [SerializeField] private float m_b;

    [Tooltip("Horizontal shift")]
    [SerializeField] private float m_c;

    [Tooltip("The input value will be clamped between 0 and 1 relative to the minimum and maximum of the bookends.")]
    [SerializeField] private Bookends m_bookends;
    
    float m_stepFunctionOffset = 0.0f;


    // Methods
    public float CalculateResponseCurveValue(float x)
    {
        float curveValue = 0.0f;

        switch (m_curveType)
        {
            case CurveType.Step:
                if ((x - m_c) < (1.0f / (m_k + 1)))
                {
                    curveValue = 0.0f + m_b;
                }
                else if ((x - m_c) > 1.0f - (1.0f / (m_k + 1)))
                {
                    curveValue = 1.0f + m_b;
                }
                else
                {
                    if ((x - m_c) % (1.0f / (m_k + 1)) <= 0.001f)
                    {
                        m_stepFunctionOffset += (1.0f / m_k);
                    }

                    curveValue = m_stepFunctionOffset + m_b;
                }

                // Clamp value to reasonable data points in the graph. Doesn't affect the result.
                curveValue = Mathf.Clamp(curveValue, 0.0f, 1.0f);

                // Reset offset, if the curve starts being calculated
                if (x < 0.0001f)
                {
                    m_stepFunctionOffset = 0.0f;
                }
                break;

            case CurveType.Linear:
                curveValue = m_m * (x - m_c) + m_b;
                break;

            case CurveType.Polynomial:
                curveValue = m_m * Mathf.Pow((x - m_c), m_k) + m_b;
                break;

            case CurveType.DensityNormalDistribution:
                curveValue = m_m * (1.0f / Mathf.Sqrt(2.0f * Mathf.PI * Mathf.Pow(m_k, 2.0f))) * Mathf.Exp(-1.0f  * ((Mathf.Pow((x - m_c), 2.0f)) / (2 * Mathf.Pow(m_k, 2.0f)))) + m_b;
                break;

            case CurveType.Logistic:
                curveValue = (m_k * (1.0f / (1 + Mathf.Exp(-1.0f * (x + m_c) * m_m)))) + m_b;
                break;

            case CurveType.Logit:
                curveValue = m_m * Mathf.Log(((x - m_c) + m_k) / (1.0f - (x - m_c) + m_k)) + m_b;
                
                break;

            default:
                Debug.LogWarning("Unknown response curve type.");
                break;
        }

        return curveValue;
    }

    private float Artanh(float x)
    {
        return 0.5f * Mathf.Log((1 + x) / (1 - x));
    }


    // Getter/Setter
    public Bookends GetBookends()
    {
        return m_bookends;
    }
}