using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;

public class NPC : MonoBehaviour 
{
	// Our index
	public int index = -1;
	public int Index{ get{ return index; } set{index = value;}}
	
	// Manager and Character Controller
	public NPCManager npcMan = null;
	public CharacterController cc = null;
	
	// Movement variables
	private float speed = 10.0f;
	private Vector3 moveDirection;
	
	// Steering variables
	private Vector3 steeringForce;
	
	// Wander variables
	public float wanderRad = 5.0f;
	public float wanderAngX = 0.0f;
	public float wanderAngY = 0.0f;
	public float wanderDist = 200.0f;
	public float wanderMax = 30.0f;
	private Vector3 redDot;
	
	// Probe values
	private Vector3 frontProbe;
	private Vector3 leftProbe;
	private Vector3 rightProbe;
	private Vector3 topProbe;
	private Vector3 bottomProbe;
	private Vector3 randomProbe;
	private float probeDist = 300.0f;
	private float sideProbeDist = 100.0f;
	private float probeAng = 30.0f;
	
	// Formation variables
	private Formation myFormation;
	public Formation MyFormation{	get{return myFormation;}
									set{myFormation = value;}}
	public bool hasFormation = false;
	public bool HasFormation{		get{return hasFormation;}
									set{hasFormation = value;}}
	public Vector3 myPosition = Vector3.zero;
	public Vector3 MyPosition {		get{return myPosition;}
									set{myPosition = value;}}
	public Transform myLeader;
	public Transform MyLeader {		get{return myLeader;}
									set{myLeader = value;}}
	
	// Other values
	private float radius;
	private bool initialized = false;
	
	//list of nearby NPCs
	private List<GameObject> nearNPCS = new List<GameObject>();
	private List<float> nearNPCDistances = new List<float>();
	
	public void setController(GameObject inControllerGO)
	{
		npcMan = inControllerGO.GetComponent<NPCManager>();
	}
	
	public void Start()
	{
		cc = gameObject.GetComponent<CharacterController>();
	 
		moveDirection = transform.forward;
		
		Bounds AABB = GetComponent<Renderer>().bounds;
		radius = Mathf.Sqrt(AABB.extents.x * AABB.extents.x + 
							AABB.extents.z * AABB.extents.z);
		myLeader = null;
		initialized = true;
	}
	
	private Vector3 Separation ( )
	{
		Vector3 dv = Vector3.zero;
		Vector3 sum = Vector3.zero;
		
		for (int i = 0; i < npcMan.NPCList.Count; i++)
		{
			if (index > -1 &&
				index != i &&
				npcMan.getDistance(index, i) <= npcMan.separationDist)
			{
				Vector3 runAway = transform.position - npcMan.NPCList[i].transform.position;
				runAway.Normalize();
				sum += runAway;
			}
		}
		
		dv = sum.normalized * npcMan.maxForce;
	 	dv -= transform.forward * speed;
	 	return dv;
	}
	
	private Vector3 Align(Transform trans)
	{
		Vector3 dv = trans.forward; 
		dv *= npcMan.maxForce;
		dv -= transform.forward * speed;
 		return dv;
	}
	
	private Vector3 wander()
	{	
		wanderAngX += ((Random.value * wanderMax * 2) - wanderMax);
		wanderAngY += ((Random.value * wanderMax * 2) - wanderMax);
		redDot = transform.position + transform.forward * wanderDist;
		Vector3 offset = transform.forward * wanderRad;
		offset = Quaternion.AngleAxis(wanderAngX, Vector3.right) * offset;
		offset = Quaternion.AngleAxis(wanderAngY, Vector3.up)  * offset;
		redDot += offset;
		
		return seek(redDot);
	}
	
	private Vector3 seek(Vector3 pos)
	{
		Vector3 dv = pos - transform.position;
	 	dv = dv.normalized * npcMan.maxForce;
	 	dv -= transform.forward * speed;
	 	return dv;
	}
	
	private Vector3 seek(Transform trans) 
	{
		return seek(trans.position);
	}
		
	private Vector3 flee(GameObject go) 
	{
		Vector3 dv = transform.position - go.transform.position;
	 	dv = dv.normalized * npcMan.maxForce;
	 	dv -= transform.forward * speed;
	 	return dv;
	}

	private Vector3 arrive(Vector3 pos)
	{
		Vector3 dv = Vector3.zero;
		float distance = Vector3.Distance(transform.position, pos);
		
		// Check if the target is within arriving distance
		if (distance < npcMan.arriveDist && distance > 0)
		{
			// if the target is in frontProbe of this npc
			if (Vector3.Dot(transform.forward, pos) > 0)
			{
				Vector3 brake = transform.forward * speed * -1.0f;
				// Create a weight that's inversely proportional
				// to the target's distance from this npc
				// Formula: weight = 1.0 - distance/arriveDist;
				float distWeight = 1.0f - distance/(npcMan.arriveDist + 0.01f);
				dv = brake * distWeight - transform.forward * speed;
			}
		}
		
		dv += seek(pos);
		return dv;
	}
	
	private Vector3 avoid()
	{
		Vector3 dv = Vector3.zero;
		Obstacle threat = null;
		Vector3 threatVTOC = Vector3.zero;
		
		foreach (Obstacle ob in npcMan.Obstacles)
		{
			Vector3 vtoc = ob.position - transform.position;
			
			if (vtoc.magnitude - ob.radius > npcMan.avoidDist)
			{
				continue;
			}
			
			if (Vector3.Dot(transform.forward, vtoc) < 0)
			{
				continue;
			}
			
			float rightDot = Vector3.Dot(transform.right, vtoc);
			if (radius + ob.radius < rightDot)
			{
				continue;
			}
			
			if (threat == null ||
				Vector3.Dot(transform.forward, ob.position) <
				Vector3.Dot(transform.forward, threat.position))
			{
				threat = ob;
				threatVTOC = vtoc;
			}
		}
		
		if (threat != null)
		{
		    RaycastHit hit = new RaycastHit();
			int obstacleLayer = 1 << 8;
			Physics.Raycast(transform.position, threatVTOC, out hit, obstacleLayer);
			
			Vector3 normal = hit.normal * npcMan.avoidDist;
			Vector3 seekPoint = normal + hit.point;
			
			dv = seek(seekPoint);
		}
		
		return dv;
	}
	
	// Use raycasting to test 3 directions in front of, and to either side of the flocker
	// Checks against all colliders in the "walls" layer
	// Finds a point orthogonal at the point on the surface where the ray collids against
	// Desired velocity is a seek toward the point
	private Vector3 fiveProbeContainment( )
	{
		Vector3 dv = Vector3.zero;
		int wallLayer = 1<<9;
		RaycastHit hit = new RaycastHit();
		
    	if (Physics.Raycast(transform.position, leftProbe, out hit, sideProbeDist,  wallLayer) ||
			Physics.Raycast(transform.position, rightProbe, out hit, sideProbeDist, wallLayer) ||
			Physics.Raycast(transform.position, topProbe, out hit, sideProbeDist,  wallLayer) ||
			Physics.Raycast(transform.position, bottomProbe, out hit, sideProbeDist, wallLayer) ||
			Physics.Raycast(transform.position, frontProbe, out hit, probeDist, wallLayer))
		{
			Vector3 normal = hit.normal * npcMan.containDist;
			Vector3 seekPoint = normal + hit.point;
			
			dv = seek(seekPoint);
		}
		
		return dv;
	}
	
	private Vector3 randomProbeContainment( )
	{
		Vector3 dv = Vector3.zero;
		int wallLayer = 1<<9;
		RaycastHit hit = new RaycastHit();
		
    	if (Physics.Raycast(transform.position, randomProbe, out hit, sideProbeDist,  wallLayer) ||
			Physics.Raycast(transform.position, frontProbe, out hit, probeDist, wallLayer))
		{
			Vector3 normal = hit.normal * npcMan.containDist;
			Vector3 seekPoint = normal + hit.point;
			
			dv = seek(seekPoint);
		}
		
		return dv;
	}
	
	// Update is called once per frame
	public void FixedUpdate ()
	{	
		checkLeader();
		//CalcFiveProbes();
		CalcRandomProbes();
		CalcSteeringForce();
		ClampSteering();
		
		moveDirection = transform.forward;
		moveDirection *= speed;
		moveDirection += steeringForce * Time.deltaTime;
		speed = moveDirection.magnitude;

		if (speed > npcMan.maxSpeed) 
		{
			speed = npcMan.maxSpeed;
			moveDirection = moveDirection.normalized * npcMan.maxSpeed;
		}
		
		if  (moveDirection != Vector3.zero) transform.forward = moveDirection;
		cc.Move(moveDirection * Time.deltaTime);
	}
	
	private void CalcSteeringForce()
	{
		steeringForce = Vector3.zero;
		if (myPosition != Vector3.zero)
			steeringForce += npcMan.followWt * arrive(myPosition);
		if (myLeader != null)
			steeringForce += npcMan.alignWt * Align(myLeader);
		steeringForce += npcMan.wanderWt * wander( );
	    steeringForce += npcMan.inBoundsWt * randomProbeContainment( );		
		steeringForce += npcMan.separationWt * Separation();
		steeringForce += npcMan.avoidWt * avoid();
	}
	
	private void ClampSteering()
	{
		if (steeringForce.magnitude > npcMan.maxForce)
		{
			steeringForce.Normalize( );
			steeringForce *= npcMan.maxForce;
		}		
	}
	
	// Calculates five directions (forward and one for left/right/top/bottom)
	// Probes to be used for raycasting
	private void CalcFiveProbes()
	{
		frontProbe = transform.forward * probeDist;
		
		leftProbe = transform.forward * sideProbeDist;
		leftProbe = Quaternion.AngleAxis(probeAng, transform.up) * leftProbe;
		
		rightProbe = transform.forward * sideProbeDist;
		rightProbe = Quaternion.AngleAxis(-probeAng, transform.up) * rightProbe;

		topProbe = transform.forward * sideProbeDist;
		topProbe = Quaternion.AngleAxis(probeAng, transform.right) * topProbe;
		
		bottomProbe = transform.forward * sideProbeDist;
		bottomProbe = Quaternion.AngleAxis(-probeAng, transform.right) * bottomProbe;
	}
	
	// Calculates two directions (forward and one randomly distributed in a circle in front
	private void CalcRandomProbes()
	{
		frontProbe = transform.forward * probeDist;
		randomProbe = transform.forward * sideProbeDist;
		randomProbe = Quaternion.AngleAxis(probeAng, transform.up) * randomProbe;
		randomProbe = Quaternion.AngleAxis(Random.value * 360.0f, transform.forward) * randomProbe;
	}
	
	// Checks to see if this formation has a leader
	void checkLeader()
	{
		if (myFormation.Leader != -1)
		{
			myLeader = npcMan.NPCList[myFormation.Leader].GetComponent<NPC>().transform;
		}
	}
	
	void OnDrawGizmos()
	{
		if (npcMan.DrawGizmos && initialized)
		{
			Gizmos.DrawRay(transform.position, frontProbe);
			Gizmos.DrawRay(transform.position, rightProbe);
			Gizmos.DrawRay(transform.position, leftProbe);
			Gizmos.DrawRay(transform.position, topProbe);
			Gizmos.DrawRay(transform.position, bottomProbe);
			Gizmos.DrawRay(transform.position, randomProbe);
			Gizmos.color = Color.red;
			Gizmos.DrawRay(transform.position, redDot - transform.position);
			Gizmos.color = Color.magenta;
			Gizmos.DrawRay(transform.position, steeringForce);
		}
	}
}

