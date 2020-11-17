using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCameraControls : MonoBehaviour 
{
	// Constant variables
	private const float MIN_DISTANCE = 5.0f;
	private const float MAX_DISTANCE = 30.0f;

	private const float MIN_ANGLE = 20.0f;
	private const float MAX_ANGLE = 80.0f;

	// Member variables
	private GameObject m_player;


	[Header("Configuration")]
	[SerializeField] private float m_rotationSpeed = 100.0f;
	[SerializeField] private float m_zoomSpeed = 15.0f;
	[SerializeField] [Range(MIN_DISTANCE, MAX_DISTANCE)] private float m_distanceToPlayer = 20.0f;

	[Tooltip("When to start the movement depending on how much the trigger/joystick is pushed.")]
	[SerializeField] [Range(0.0f, 1.0f)] private float m_rotationThreshold = 0.2f;

	[SerializeField] [Range(0.0f, 1.0f)] private float m_joystickThreshold = 0.8f;


	// MonoBehaviour-Methods
	void Start()
	{
		FindPlayer();
		transform.LookAt(m_player.transform);
	}


	void FixedUpdate()
	{
		// Rotating using the buttons LT and RT
		float leftTriggerAxis = Input.GetAxis("LT");
		float rightTriggerAxis = Input.GetAxis("RT");
		
		float rightJoystickHorizontal = Input.GetAxis("HorizontalRightStick");

		float deltaRotation = 0.0f;

		if (leftTriggerAxis > m_rotationThreshold)
		{
			deltaRotation = -leftTriggerAxis * m_rotationSpeed * Time.fixedDeltaTime;
		}
		else if (rightTriggerAxis > m_rotationThreshold)
		{
			deltaRotation = rightTriggerAxis * m_rotationSpeed * Time.fixedDeltaTime;
		}
		else if (rightJoystickHorizontal > m_rotationThreshold || rightJoystickHorizontal < -m_rotationThreshold)
		{
			deltaRotation = rightJoystickHorizontal * m_rotationSpeed * Time.fixedDeltaTime;
		}

		// Zooming in and out using the right joystick
		float verticalAxis = Input.GetAxis("VerticalRightStick");
		if (verticalAxis < -m_joystickThreshold || verticalAxis > m_joystickThreshold)
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

		// DPAD rotation (camera x-Axis)
		float dpadVerticalAxis = Input.GetAxis("DPADVertical");
		if (dpadVerticalAxis < -m_joystickThreshold || dpadVerticalAxis > m_joystickThreshold)
		{
			float cameraRotationX = dpadVerticalAxis * m_rotationSpeed * Time.fixedDeltaTime;


			bool isBelowMinAngle = transform.rotation.eulerAngles.x + cameraRotationX < MIN_ANGLE;
			bool isAboveMaxAngle = transform.rotation.eulerAngles.x + cameraRotationX > MAX_ANGLE;
			if (!(isBelowMinAngle || isAboveMaxAngle))
			{
				transform.RotateAround(m_player.transform.position, transform.right, cameraRotationX);
			}
		}

		// Rotate around player
		transform.RotateAround(m_player.transform.position, Vector3.up, deltaRotation);

		// Distance to player
		Vector3 zoomVector = -transform.forward * m_distanceToPlayer;
		transform.position = m_player.transform.position + zoomVector;
	
		// Follow player
		transform.LookAt(m_player.transform);
	}


	// Methods
	public void FindPlayer()
	{
		if (m_player == null)
		{
			GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

			foreach (GameObject go in players)
			{
				if (go.GetComponent<CPlayerControls>() != null)
				{
					if (go.transform.parent.GetComponent<CGroup>().GetIsGroupDefeated())
					{
						DisableCameraControls();
						Debug.Log("Player defeated. CCameraControls deactivated.");
					}
					else
					{
						m_player = go;
						return;
					}
				}
			}

			Debug.LogWarning("No controlable player found in scene.");
		}
	}

	public void DisableCameraControls()
	{
		enabled = false;
	}

	public void SaveTransformToWorld()
	{
		CWorld.GetInstance().SaveCameraTransform(transform);
	}

	public void LoadTransformFromWorld()
	{
		if (CWorld.GetInstance().GetCameraPosition() != Vector3.zero)
		{
			transform.position = CWorld.GetInstance().GetCameraPosition();
			transform.rotation = Quaternion.Euler(CWorld.GetInstance().GetCameraRotation());
		}
	}
}