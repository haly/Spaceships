using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour 
{
	// Character controller for the missile
	CharacterController cc;
	
	// Steering Values
	private float speed= 500.0f;
	private float maxForce = 200.0f;
	private float maxSpeed = 500.0f;
	private float explosionDist = 10.0f;
	private float vision = 45.0f;
	public Vector3 moveDirection;
	public GameObject myTarget = null;
	
	// Manager
	public NPCManager npcMan;
	public NPCManager NPCMan {get{return npcMan;}
								set{npcMan = value;}}
	
	// Timed life
	public float timedLife = 5.0f;
	
	// Explosion prefab (set in editor)
	public Object explosionprefab;
	
	// Use this for initialization
	void Start () 
	{
		cc = gameObject.GetComponent<CharacterController>();
		npcMan = GameObject.Find("Manager").GetComponent<NPCManager>();
		moveDirection = transform.forward;
	}
	
	private void findNearestNPC()
	{
		GameObject target = null;
		float distance = 0.0f;;
		
		foreach (GameObject npc in npcMan.NPCList)
		{
			Vector3 vtoc = npc.transform.position - transform.position;
			float distanceToNPC = Vector3.Distance(transform.position, npc.transform.position);
			
			if (Vector3.Dot(transform.forward, vtoc) < 0)
			{
				continue;
			}
			
			if (distanceToNPC > distance && distance != 0)
			{
				continue;
			}
			
			if (Mathf.Abs(Vector3.Angle(transform.forward, vtoc)) > vision)
			{
				continue;
			}
			
			if (target == null)
			{
				target = npc;
				distance = distanceToNPC;
			}
		}
		
		if (target != null)
		{
		   myTarget = target;
		}
	}
	
	void OnControllerColliderHit(ControllerColliderHit hit)
	{
		Explode();
		Destroy(gameObject);
	}
	
	private void countdown()
	{
		timedLife -= Time.deltaTime;
		if (timedLife < 0)
		{
			Explode();
			Destroy(gameObject);
		}
	}
	
	private Vector3 seek(Vector3 pos)
	{
		Vector3 dv = pos - transform.position;
	 	dv = dv.normalized * maxForce;
	 	dv -= transform.forward * speed;
	 	return dv;
	}
	
	void WithinExplosionDistance()
	{
		if (myTarget != null && 
			Vector3.Distance(transform.position, myTarget.transform.position)< explosionDist)
		{
			int index = myTarget.GetComponent<NPC>().Index;
			npcMan.despawnNPC(index);
			Explode();
			Destroy(gameObject);
		}
	}
	
	void Explode()
	{
		GameObject explosion = (GameObject)Instantiate(explosionprefab, transform.position, transform.rotation);
	}
	
	// Update is called once per frame
	void Update () 
	{
		findNearestNPC();
		WithinExplosionDistance();
		
		Vector3 steeringForce = Vector3.zero;
		if (myTarget != null)
		{
			steeringForce = seek(myTarget.transform.position);
		}
		else
		{
			steeringForce = transform.forward * maxForce;	
		}
		
		moveDirection = transform.forward;
		moveDirection *= speed;
		moveDirection += steeringForce * Time.deltaTime;
		speed = moveDirection.magnitude;
	
		if (speed > maxSpeed) 
		{
			speed = maxSpeed;
			moveDirection = moveDirection.normalized * maxSpeed;
		}

		if  (moveDirection != Vector3.zero) transform.forward = moveDirection;
		cc.Move(moveDirection * Time.deltaTime);
		countdown();
	}
}
