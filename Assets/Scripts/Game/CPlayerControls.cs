using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPlayerControls : MonoBehaviour 
{
	// Member variables
	[Header("General")]
	private bool m_isMoving;

	[Header("Configuration")]
	private Rigidbody m_rigidbody;
	[SerializeField] private GameObject m_camera;
	[SerializeField] private float m_movementSpeed = 5.0f;

	[Tooltip("When to start the movement depending on how much the joystick is moved.")]
	[SerializeField] [Range(0.0f, 1.0f)] private float m_joystickThreshold;
	

	// MonoBehaviour-Methods
	void Start() 
	{
		m_isMoving = false;
		m_rigidbody = GetComponent<Rigidbody>();

		if (transform.parent.GetComponent<CGroup>().GetIsGroupDefeated())
		{
			Debug.Log("Player defeated. CPlayerControls deactivated.");
			DisablePlayerControls();
		}
		else
		{
			SetCamera(); 
		}
	}

	void FixedUpdate()
	{
		float horizontalAxis = Input.GetAxis("Horizontal");
		float verticalAxis = Input.GetAxis("Vertical");
		
		if (horizontalAxis > m_joystickThreshold || horizontalAxis < -m_joystickThreshold || 
			verticalAxis > m_joystickThreshold || verticalAxis < -m_joystickThreshold)
		{
			m_isMoving = true;
			Vector3 cameraForward = m_camera.transform.forward;
			cameraForward.y = 0.0f;

			Vector3 cameraRight = m_camera.transform.right;
			cameraRight.y = 0.0f;


			if (Mathf.Abs(horizontalAxis) > 0.7f || Mathf.Abs(verticalAxis) > 0.7f)
			{
				m_movementSpeed = 10.0f;
			}
			else
			{
				m_movementSpeed = 5.0f;
			}

			Vector3 deltaPosition = (cameraForward.normalized * verticalAxis + cameraRight.normalized * horizontalAxis).normalized;

			deltaPosition *= m_movementSpeed * Time.fixedDeltaTime;
			transform.LookAt(transform.position + deltaPosition);

			m_rigidbody.MovePosition(transform.position + deltaPosition);

			if (m_isMoving)
			{
				transform.parent.GetComponent<CGroup>().AddDeltaPosition(deltaPosition);
				transform.parent.GetComponent<CGroup>().MoveGroupMember();
			}
		}
		else
		{
			if (m_isMoving)
			{
				m_isMoving = false;
			}
		}
	}


	// Methods
	public void DisablePlayerControls()
	{
		enabled = false;
	}


	// Getter/Setter
	public void SetCamera()
	{
		m_camera = CEnvironmentInitializer.GetInstance().GetSceneCamera().gameObject;

		if (m_camera == null)
		{
			if (Camera.main.gameObject != null)
			{
				m_camera = Camera.main.gameObject;
			}
			else
			{
				Debug.LogWarning("No camera found in scene.");
			}
		}
	}
}