using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class PlayerCharacter : MonoBehaviour 
{
	// Components
	CharacterController cc;
	Camera mc;
	
	// Movement 
	private Vector3 moveDirection;
	public float velocity = 0.0f;
	public float acceleration = 0.0f;
	public float targetVel = 0.0f;
	
	// Rotation
	private Quaternion initialOrientation;
	private Quaternion currentRotation;
	private Vector3 cummulativeRotation;
	public float rotationFactor = 200.0f;
	
	// Constants
	private const float MAX_SPEED = 150.0f;
	private const float ROTATION_DAMP = 3.0f;
	
	// Combat
	public Vector3 missileLauncher = new Vector3(0, -1.8f, 0);
	public Object missileprefab;
	public float missileCooldown = 0.0f;
	public float missileRate = 2.0f;
	private bool lastMouseDown = false;
	private bool currentMouseDown = false;
	
	// Initialization
	void Start () 
	{
		cc = gameObject.GetComponent<CharacterController>();
		mc = gameObject.GetComponent<Camera>();
		
		moveDirection = Vector3.zero;
		initialOrientation = transform.rotation;
		cummulativeRotation = Vector3.zero;
	}
	
	void LaunchMissile()
	{
		if (missileCooldown <= 0)
		{
			GameObject newMissile = (GameObject)Instantiate(missileprefab, transform.position + missileLauncher, transform.rotation);
			missileCooldown = missileRate;
		}
	}
	
	void UserInput()
	{	
		// Changes the target velocity depending on keyboard input
		targetVel += Input.GetAxis("Vertical") * acceleration * Time.deltaTime;
		
		if (Input.GetMouseButton(0))
		{
			LaunchMissile();
		}
		
		missileCooldown -= missileRate * Time.deltaTime;
		if (missileCooldown < 0)
			missileCooldown = 0;
		
		// Change cummulative rotation based on mouse input
		cummulativeRotation.x -= Input.GetAxis("Mouse Y") * Time.deltaTime * rotationFactor;
		cummulativeRotation.y += Input.GetAxis("Mouse X") * Time.deltaTime * rotationFactor;
		
		// Clamp the rotation around the x axis
		if (Mathf.Abs(cummulativeRotation.x) > 90)
			cummulativeRotation.x -= cummulativeRotation.x%90;

		// Create the target rotation from the cummulative rotatons
		Quaternion targetRotation = Quaternion.Euler(cummulativeRotation.x, cummulativeRotation.y, 0.0f);
		currentRotation = Quaternion.Slerp(currentRotation, targetRotation, ROTATION_DAMP * Time.deltaTime);
	}
	
	void AccelToTarget ()
	{
		if (targetVel > 0)
		{
			if (velocity < targetVel)
				velocity += acceleration * Time.deltaTime;
			else if (velocity > targetVel)
				velocity -= acceleration * Time.deltaTime;
	
			if (velocity > targetVel)
				velocity = targetVel;
		}
		else
			velocity = 0;
	}
	
	void ClampToTarget()
	{
		if (targetVel > MAX_SPEED)
			targetVel = MAX_SPEED;
		else if (targetVel < 0)
			targetVel = 0;
	}
	
	// Update is called once per frame
	void Update () 
	{
		UserInput();
		AccelToTarget();
		ClampToTarget();
		
		transform.rotation = currentRotation;
		moveDirection = transform.forward.normalized;
		moveDirection *= velocity;
		cc.Move(moveDirection * Time.deltaTime);
	}
}
