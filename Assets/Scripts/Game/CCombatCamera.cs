using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CCombatCamera : MonoBehaviour 
{
	// Constant variables
	private const float MIN_DISTANCE = 40.0f;
	private const float MAX_DISTANCE = 120.0f;

	// Member variables
	[Header("Configuration")]
	[SerializeField] private Transform m_center;
	[SerializeField] private List<Transform> m_cameraTargets;
	[SerializeField] private Transform m_camera;

	[SerializeField] private float m_rotationSpeed = 100.0f;
	[SerializeField, Range(0.0f, 1.0f)] private float m_rotationThreshold = 0.2f;
	
	[SerializeField] private float m_zoomSpeed = 50.0f;
	[SerializeField] [Range(MIN_DISTANCE, MAX_DISTANCE)] private float m_distanceToPlayer = 100.0f;

	private int m_cameraTargetIndex;


	// MonoBehaviour-Methods
	void Awake()
	{
		m_cameraTargets = new List<Transform>();
		m_cameraTargetIndex = 0;
	}
	

	void Start() 
	{
		m_cameraTargets.Add(m_center);
	}
	

	void Update()
	{
		m_camera.LookAt(m_cameraTargets[m_cameraTargetIndex]);
	}


	void FixedUpdate() 
	{
		float leftTriggerAxis = Input.GetAxis("LT");
		float rightTriggerAxis = Input.GetAxis("RT");

		float deltaRotation = 0.0f;

		if (leftTriggerAxis > m_rotationThreshold)
		{
			deltaRotation = leftTriggerAxis * m_rotationSpeed * Time.fixedDeltaTime;
		}
		else if (rightTriggerAxis > m_rotationThreshold)
		{
			deltaRotation = -rightTriggerAxis * m_rotationSpeed * Time.fixedDeltaTime;
		}

		m_camera.RotateAround(m_cameraTargets[m_cameraTargetIndex].position, Vector3.up, deltaRotation);

		// Zooming in and out using the right joystick
		float verticalAxis = Input.GetAxis("VerticalRightStick");
		if (verticalAxis < -m_rotationThreshold || verticalAxis > m_rotationThreshold)
		{
			m_distanceToPlayer -= verticalAxis * Time.fixedDeltaTime * m_zoomSpeed;

			if (m_distanceToPlayer < MIN_DISTANCE)
			{
				m_distanceToPlayer = MIN_DISTANCE;
			}
			else if (m_distanceToPlayer > MAX_DISTANCE)
			{
				m_distanceToPlayer = MAX_DISTANCE;
			}
		}


		if (Input.GetButtonDown("RB"))
		{
			m_distanceToPlayer = (m_cameraTargets[m_cameraTargetIndex].position - m_camera.position).magnitude;
			m_distanceToPlayer = Mathf.Clamp(m_distanceToPlayer, MIN_DISTANCE, MAX_DISTANCE);

			m_cameraTargetIndex++;
			if (m_cameraTargetIndex >= m_cameraTargets.Count)
			{
				m_cameraTargetIndex = 0;
			}
		}
		else if (Input.GetButtonDown("LB"))
		{
			m_distanceToPlayer = (m_cameraTargets[m_cameraTargetIndex].position - m_camera.position).magnitude;
			m_distanceToPlayer = Mathf.Clamp(m_distanceToPlayer, MIN_DISTANCE, MAX_DISTANCE);

			m_cameraTargetIndex--;
			if (m_cameraTargetIndex < 0)
			{
				m_cameraTargetIndex = m_cameraTargets.Count - 1;
			}
		}

		Vector3 zoomVector = -m_camera.transform.forward * m_distanceToPlayer;
		m_camera.transform.position = m_cameraTargets[m_cameraTargetIndex].position + zoomVector;
	}



	// Methods
	public void AddCameraTargets(List<CCombatParticipant> participants)
	{
		foreach (CCombatParticipant participant in participants)
		{
			m_cameraTargets.Add(participant.GetEntity().transform);
		}
	}
}