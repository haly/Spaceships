using UnityEngine;
using System.Collections;
//including some .NET for dynamic arrays called List in C#
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
	// NPC Spawn parameters
	public int numberOfNPCs;
	public float spawnRate = 2.0f;
	private float spawnTimer = 0.0f;
	
	//NPC weight parameters
	public float followWt;
	public float wanderWt;
	public float separationWt;
	public float alignWt;
	public float inBoundsWt;
	public float avoidWt;
	public float maxForce;
	public float maxSpeed;
	
	// NPC Steering values
	public float containDist;
	public float avoidDist;
	public float arriveDist;
	public float separationDist;
	public float formationDist;
	public float formationAngle;
	
	// Set in Editor
	public Object npcPrefab;
	public FormationManager formationMan;
	
	// List of NPCs
	private List<GameObject> npcList = new List<GameObject>();
	public List<GameObject> NPCList {get{return npcList;}}

	// 2D C# array for distances
	private float[,] distances;
	
	// List of obstacles
	private List<Obstacle> obstacles = new List<Obstacle>();
	public List<Obstacle> Obstacles {get{return obstacles;}}
	
	// List of spawn points
	private List<GameObject> spawns = new List<GameObject>();

	// Debugging indicators
	public bool DrawGizmos = true;
	
	public void Start()
	{
		// Construct 2D array of distances
		distances = new float[npcList.Count, npcList.Count];
		
		// Populates the list of obstacles
		findObstacles();
		findSpawns();
	}
	
	public void Print(string s)
	{
		print(s);
	}
	
	void findDistances()
	{
		float dist;
		distances = new float[npcList.Count, npcList.Count];
		for(int i = 0 ; i < npcList.Count; i++)
		{
			for(int j = i+1; j < npcList.Count; j++)
			{
				dist = Vector3.Distance(npcList[i].transform.position, npcList[j].transform.position);
				distances[i, j] = dist;
				distances[j, i] = dist;
			}
		}
	}
	
	public float getDistance(int i, int j)
	{
		return distances[i, j];
	}

	// Finds all objects in the scene with the tag "obstacle"
	private void findObstacles()
	{
		GameObject[] obs = GameObject.FindGameObjectsWithTag("obstacle");
		foreach(GameObject go in obs)
		{
			obstacles.Add(go.GetComponent<Obstacle>());
		}
	}
	
	// Spawns an NPC at the target point.
	void spawnNPC(Vector3 spawnPoint)
	{
		NPC newNPC;
		npcList.Add((GameObject) Instantiate(npcPrefab, spawnPoint, Quaternion.identity) ) ;
		newNPC = npcList[npcList.Count - 1].GetComponent<NPC>();
		newNPC.Index = npcList.Count - 1;
		newNPC.setController(gameObject);
		newNPC.name = "NPC " + newNPC.index;
		formationMan.addToFormations(newNPC.Index);
	}
	
	// Finds all spawnpoints in the scene with the tag "spawnpoint"
	void findSpawns()
	{
		GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("spawnpoint");
		foreach(GameObject sp in spawnPoints)
		{
			spawns.Add(sp);
		}
	}
	
	void autoSpawn()
	{
		if (npcList.Count < numberOfNPCs)
		{
			if (spawnTimer <= 0)
			{
				for (int i = 0; i < spawns.Count; i++)
				{
					if (npcList.Count < numberOfNPCs)
						spawnNPC(spawns[i].transform.position);
					else
						break;
				}
				spawnTimer = spawnRate;
			}
			else
			{
				spawnTimer -= Time.deltaTime;
			}
		}
		else if (npcList.Count > numberOfNPCs)
		{
			while (npcList.Count > numberOfNPCs)
			{
				despawnNPC(npcList.Count - 1);
			}
		}
	}
	
	public void despawnNPC(int index)
	{
		if (index <= npcList.Count - 1)
		{
		  	GameObject temp = npcList[index];
			npcList.RemoveAt(index);
			formationMan.removeFromFormations(index);
			Destroy(temp);
			updateNPCIndex();
		}
	}
	
	// Called in order to make sure all the NPCs have the correct index
	// And all their formations have the right listings
	void updateNPCIndex()
	{
		for (int i = 0; i < npcList.Count; i++)
		{
			NPC updateNPC = npcList[i].GetComponent<NPC>();
			int id = updateNPC.MyFormation.Members.IndexOf(updateNPC.Index);
			updateNPC.MyFormation.Members[id] = i;
			updateNPC.Index = i;
			updateNPC.name = "NPC " + i;
		}
	}
	
	public void Update( )
	{
		if (numberOfNPCs < 0)
			numberOfNPCs = 0;

		autoSpawn();
		findDistances();
	}
}