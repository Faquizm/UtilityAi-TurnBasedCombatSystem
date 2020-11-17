using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEffect : MonoBehaviour 
{
	// Enum
	public enum EffectType { Normal, MeleeAttack, RangeAttack }
	private enum RotationAxis { X, Y, Z }

	// Member variables
	[Header("Effect")]
	[SerializeField] private GameObject m_effectTemplate;
	private List<GameObject> m_instantiatedEffects;
	private CEntity m_belongingEntity;

	[Header("Settings - General")]
	[SerializeField] private EffectType m_effectType;
	[SerializeField, Range(2.0f, 10.0f)] private float m_effectDuration = 6.0f;
	[SerializeField, Range(0.01f, 5.0f)] private float m_effectOffset = 1.0f;
	[SerializeField] private bool m_isAoeWithOneEffect = false;
	[SerializeField] private bool m_isLookingToCamera = true;
	[SerializeField] private bool m_spawnsBeneath = false;
	private bool m_isDurationAdjusted;
	private float m_timer;

	[Header("Settings - Rotation")]
	[SerializeField] private bool m_isRotating = false;
	[SerializeField] private float m_rotatingSpeed = 5.0f;
	[SerializeField] private RotationAxis m_rotationAxis = RotationAxis.Y;

	[Header("Settings - Lavitation")]
	[SerializeField] private bool m_isLevitating = false;
	[SerializeField] private float m_levitateSpeed = 0.015f;
	[SerializeField] private float m_levitateRange = 0.5f;
	private List<float> m_levitateStartPositions = new List<float>();
	private bool m_moveUpwards = true;

	[Header("Settings - Horizontal Movement")]
	[SerializeField] private bool m_isMovingForward = false;
	[SerializeField] private float m_moveSpeed = 0.05f;
	private Vector3 m_moveStartPosition = Vector3.zero;
	private GameObject m_moveEmpty;
	private float m_moveDistance = 0.0f;			
	private float m_passedDistance = 0.0f;
	private bool m_reset = true;

	[Header("Settings - Hitting")]
	[SerializeField] private bool m_IsHitting = false;
	[SerializeField] private float m_hitSpeed = 2.0f;
	private bool m_hitReset = true;
	private Vector3 m_deltaPos = Vector3.zero;
	private float m_passedRotation = 0.0f;

	[Header("Settings - Explosion")]
	[SerializeField] private bool m_isExplosion = false;
	[SerializeField] private float m_explosionDistance = 5.0f;
	[SerializeField] private float m_explosionSpeed = 1.0f;
	private float m_explosionProgress = 0.0f;

	
	// MonoBehaviour-Methods
	void Awake()
	{
		m_instantiatedEffects = new List<GameObject>();
		m_timer = 0.0f;
		m_isDurationAdjusted = false;
	}

	
	void Update() 
	{
		for (int i = 0; i < m_instantiatedEffects.Count; i++)
		{
			if (m_isLookingToCamera)
			{
				m_instantiatedEffects[i].transform.LookAt(Camera.main.transform);
			}

			if (m_isRotating)
			{
				Rotate(m_instantiatedEffects[i]);
			}

			if (m_isLevitating)
			{
				Levitate(m_instantiatedEffects[i], m_levitateStartPositions[i]);
			}

			if (m_isMovingForward)
			{
				MoveForward(m_instantiatedEffects[i]);
			}

			if (m_IsHitting)
			{
				Hit(m_instantiatedEffects[i]);
			}

			if (m_isExplosion)
			{
				Explode(m_instantiatedEffects[i]);
			}
		}

		if (!CCombatSystem.GetInstance().GetIsRunning())
		{
			DestroyImmediate(gameObject);
		}

		if (!CCombatSystem.GetInstance().GetIsPaused() || !m_isDurationAdjusted)
		{
			m_timer += Time.deltaTime;
		}

		if (m_timer >= m_effectDuration || !m_belongingEntity.gameObject.activeSelf)
		{
			Destroy(gameObject);
		}
	}


	// Methods
	public void Init(CCombatParticipant executor, bool hasAreaOfEffect)
	{
		m_belongingEntity = executor.GetDecision().GetTarget().GetComponent<CEntity>();

		if (executor.GetDecision().GetAbility().GetType() == typeof(CDefenseAbility))
		{
			if (executor.GetDecision().GetAbility().GetTargetType() == TargetType.Ally || executor.GetDecision().GetAbility().GetTargetType() == TargetType.Self)
			{
				Debug.Log("Effect duration adjusted to defense ability duration.");
				m_isDurationAdjusted = true;
				m_effectDuration = (float)executor.GetDecision().GetAbility().GetType().GetMethod("GetDuration").Invoke(executor.GetDecision().GetAbility(), null);
			}
		}

		if (hasAreaOfEffect)
		{
			if (executor.GetDecision().GetAbility().GetType().GetMethod("GetRadius") != null)
			{
				float abilityRadius = (float)executor.GetDecision().GetAbility().GetType().GetMethod("GetRadius").Invoke(executor.GetDecision().GetAbility(), null);

				List<CCombatParticipant> participantsInRadius = CCombatSystem.GetInstance().DetermineParticipantsInRadius(abilityRadius, executor.GetDecision().GetTarget());
				foreach (CCombatParticipant participant in participantsInRadius)
				{
					if (m_isAoeWithOneEffect)
					{
						if (executor.GetDecision().GetTarget().GetComponent<CEntity>() == participant.GetEntity())
						{
							GameObject instantiatedEffect = Instantiate(m_effectTemplate, transform);
							instantiatedEffect.transform.localPosition = participant.GetEntity().transform.localPosition;

							if (m_spawnsBeneath)
							{
								instantiatedEffect.transform.localPosition = m_belongingEntity.transform.localPosition;
							}
							else
							{
								instantiatedEffect.transform.Translate(0.0f, participant.GetEntity().GetComponent<Collider>().bounds.size.y + m_effectOffset, 0.0f);
							}

							m_instantiatedEffects.Add(instantiatedEffect);
							m_levitateStartPositions.Add(instantiatedEffect.transform.localPosition.y);
							break;
						}
					}
					else
					{
						GameObject instantiatedEffect = Instantiate(m_effectTemplate, transform);
						instantiatedEffect.transform.localPosition = participant.GetEntity().transform.localPosition;

						if (m_spawnsBeneath)
						{
							instantiatedEffect.transform.localPosition = m_belongingEntity.transform.localPosition;
						}
						else
						{
							instantiatedEffect.transform.Translate(0.0f, participant.GetEntity().GetComponent<Collider>().bounds.size.y + m_effectOffset, 0.0f);
						}

						m_instantiatedEffects.Add(instantiatedEffect);
						m_levitateStartPositions.Add(instantiatedEffect.transform.localPosition.y);
					}
				}
			}
			else
			{
				Debug.LogError("No radius found in ability despite being marked as area of effect ability.");
			}

		}
		else
		{
			GameObject instantiatedEffect = Instantiate(m_effectTemplate, transform);
			instantiatedEffect.transform.localPosition = Vector3.zero;

			if (m_effectType == EffectType.Normal)
			{
				instantiatedEffect.transform.localPosition = executor.GetDecision().GetTarget().transform.localPosition;
				if (m_spawnsBeneath)
				{
					instantiatedEffect.transform.localPosition = m_belongingEntity.transform.localPosition;
				}
				else
				{
					instantiatedEffect.transform.Translate(0.0f, executor.GetDecision().GetTarget().GetComponent<Collider>().bounds.size.y + m_effectOffset, 0.0f);
				}
				

			}
			else if (m_effectType == EffectType.MeleeAttack || m_effectType == EffectType.RangeAttack)
			{
				Vector3 executorPos = executor.GetEntity().transform.localPosition;
				Vector3 targetPos = executor.GetDecision().GetTarget().transform.localPosition;
				Vector3 deltaPos = targetPos - executorPos;
				Vector3 effectPos;

				if (m_IsHitting)
				{
					m_deltaPos = deltaPos;
					effectPos = executorPos + deltaPos * 0.25f;
					effectPos.y = m_effectOffset;
				}
				else
				{
					effectPos = executorPos + deltaPos * 0.5f;
					effectPos.y = executor.GetEntity().GetComponent<Collider>().bounds.size.y;
				}
			
				instantiatedEffect.transform.localPosition = effectPos;
				instantiatedEffect.transform.localRotation = executor.GetEntity().transform.localRotation;
			
				if (m_isMovingForward)
				{
					m_moveDistance = deltaPos.magnitude * 0.5f;
					m_moveEmpty = new GameObject();
					m_moveEmpty.transform.position = effectPos;
					m_moveEmpty.transform.rotation = executor.GetEntity().transform.localRotation;
					m_moveStartPosition = executorPos + deltaPos * 0.25f;
					m_moveStartPosition.y = m_effectOffset;
				}
			}

			m_levitateStartPositions.Add(instantiatedEffect.transform.localPosition.y);
			m_instantiatedEffects.Add(instantiatedEffect);
					   			 
		}
	}


	public void Rotate(GameObject effect)
	{
		if (!m_isMovingForward)
		{
			effect.transform.localPosition = new Vector3(m_belongingEntity.transform.position.x, effect.transform.position.y, m_belongingEntity.transform.position.z);
		}

		if (m_rotationAxis == RotationAxis.Y)
		{
			effect.transform.Rotate(Vector3.up, m_rotatingSpeed, Space.World);
		}
		else if (m_rotationAxis == RotationAxis.Z)
		{
			effect.transform.Rotate(Vector3.forward, m_rotatingSpeed, Space.World);
		}
		else
		{
			effect.transform.Rotate(Vector3.right, m_rotatingSpeed, Space.World);
		}
	}


	public void Levitate(GameObject effect, float levitateStartPosition)
	{
		if (m_moveUpwards)
		{
			effect.transform.Translate(0.0f, m_levitateSpeed, 0.0f);

			if (effect.transform.localPosition.y > levitateStartPosition + m_levitateRange / 2.0f)
			{
				m_moveUpwards = false;
			}
		}
		else
		{
			effect.transform.Translate(0.0f, -m_levitateSpeed, 0.0f);

			if (effect.transform.localPosition.y < levitateStartPosition - m_levitateRange / 2.0f)
			{
				m_moveUpwards = true;
			}
		}
	}


	public void MoveForward(GameObject effect)
	{
		if (effect.GetComponent<BoxCollider>() != null)
		{
			if (m_reset)
			{
				effect.transform.position = m_moveStartPosition;
				m_passedDistance = 0.0f;
				m_reset = false;
			}
			else
			{
				// Move towards to target until reset
				effect.transform.position += m_moveEmpty.transform.forward * m_moveSpeed;
				m_passedDistance += m_moveSpeed;

				if (m_passedDistance > m_moveDistance)
				{
					m_reset = true;
				}
			}			
		}
		else
		{
			Debug.LogError("Effect is missing a box collider to be moved forward.");
		}

	}

	public void Hit(GameObject effect)
	{
		if (m_hitReset)
		{
			effect.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			m_passedRotation = 0.0f;
			m_hitReset = false;
		}
		else
		{
			effect.transform.Rotate(Vector3.Cross(m_deltaPos, Vector3.up), -m_hitSpeed);
			m_passedRotation += m_hitSpeed;
			
			if (m_passedRotation >= 75.0f)
			{
				m_hitReset = true;
			}
		}
	}

	private void Explode(GameObject effect)
	{
		if (m_explosionProgress < m_explosionDistance)
		{
			m_explosionProgress += m_explosionSpeed * Time.deltaTime;
			effect.GetComponent<CExplosionEffect>().SetRadius(m_explosionProgress);
		}
		else
		{
			m_explosionProgress = 0.0f;
		}
	}
}