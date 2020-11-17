using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CEntityStats
{
    // Member variables
    [Header("Stats")]
    [SerializeField] private float m_strength = 10.0f;
    [SerializeField] private float m_intelligence = 10.0f;
    [SerializeField] private float m_agility = 10.0f;
    [SerializeField] private float m_defense = 10.0f;
    [SerializeField] private float m_stamina = 5.0f;
    [SerializeField] private float m_speed = 5.0f;

    private bool m_areCombatChanges = false;

    // Constructor
    public CEntityStats()
    {
    }

    public CEntityStats(bool areTemporaryCombatStats)
    {
        if (areTemporaryCombatStats)
        {
            m_strength = 0.0f;
            m_intelligence = 0.0f;
            m_agility = 0.0f;
            m_defense = 0.0f;
            m_stamina = 0.0f;
            m_speed = 0.0f;

            m_areCombatChanges = true;
        }
    }


    // Methods
    public void ChangeStatBy(float value, StatType stat)
    {
        switch (stat)
        {
            case StatType.Strength:
                m_strength += value;
                break;

            case StatType.Intelligence:
                m_intelligence += value;
                break;

            case StatType.Agility:
                m_agility += value;
                break;

            case StatType.Defense:
                m_defense += value;
                break;
                
            case StatType.Stamina:
                m_stamina += value;
                break;

            case StatType.Speed:
                m_speed += value;
                break;

            case StatType.All:
                m_strength += value;
                m_intelligence += value;
                m_agility += value;
                m_defense += value;
                m_stamina += value;
                m_speed += value;
                break;

            default:
                break;
        }
    }


    // Getter/Setter
    public int GetStatByType(StatType statType)
    {
        float stat;

        switch (statType)
        {
            case StatType.Strength:
                stat = m_strength;
                break;

            case StatType.Intelligence:
                stat = m_intelligence;
                break;

            case StatType.Agility:
                stat = m_agility;
                break;

            case StatType.Defense:
                stat = m_defense;
                break;

            case StatType.Stamina:
                stat = m_stamina;
                break;

            case StatType.Speed:
                stat = m_speed;
                break;

            case StatType.All:
                stat = m_strength + m_intelligence + m_agility + m_defense + m_stamina + m_speed;
                break;

            default:
                Debug.LogError("Invalid StatType.");
                stat = 0.0f;
                break;
        }

        return (int)stat;
    }

    public int GetStrength()
    {
        return (int)m_strength;
    }

    public int GetIntelligence()
    {
        return (int)m_intelligence;
    }

    public int GetAgility()
    {
        return (int)m_agility;
    }

    public int GetDefense()
    {
        return (int)m_defense;
    }

    public int GetStamina()
    {
        return (int)m_stamina;
    }

    public int GetSpeed()
    {
        return (int)m_speed;
    }

    public void SetSpeed(float newSpeed)
    {
        m_speed = newSpeed;
    }

    public float GetStatChange(StatType type)
    {
        float statChangeValue;

        switch (type)
        {
            case StatType.Strength:
                statChangeValue = m_strength;
                break;

            case StatType.Intelligence:
                statChangeValue = m_intelligence;
                break;

            case StatType.Agility:
                statChangeValue = m_agility;
                break;

            case StatType.Defense:
                statChangeValue = m_defense;
                break;

            case StatType.Stamina:
                statChangeValue = m_stamina;
                break;

            case StatType.Speed:
                statChangeValue = m_speed;
                break;

            case StatType.All:
                statChangeValue = m_strength + m_intelligence + m_agility + m_defense + m_stamina + m_speed;
                break;

            default:
                Debug.LogWarning("Stat change type unknown.");
                statChangeValue = 0.0f;
                break;
        }

        return statChangeValue;
    }

    public float GetStatWeight(StatType type)
    {
        float statWeightValue;

        switch (type)
        {
            case StatType.Strength:
                statWeightValue = m_strength;
                break;

            case StatType.Intelligence:
                statWeightValue = m_intelligence;
                break;

            case StatType.Agility:
                statWeightValue = m_agility;
                break;

            case StatType.Defense:
                statWeightValue = m_defense;
                break;

            case StatType.Stamina:
                statWeightValue = m_stamina;
                break;

            case StatType.Speed:
                statWeightValue = m_speed;
                break;

            case StatType.All:
            default:
                Debug.LogWarning("Stat weight type unknown or \"all\".");
                statWeightValue = 0.0f;
                break;
        }

        return statWeightValue;
    }

    public List<float> GetAllCombatChanges()
    {
        if (m_areCombatChanges)
        {
            List<float> allStats = new List<float>();
            allStats.Add(m_strength);
            allStats.Add(m_intelligence);
            allStats.Add(m_agility);
            allStats.Add(m_defense);
            allStats.Add(m_stamina);
            allStats.Add(m_speed);

            return allStats;
        }
        else
        {
            Debug.LogError("EntityStats aren't marked as combat changes.");
            return null;
        }
    }
}