using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Formation
{
	// The NPC manager
	private NPCManager npcMan;
	public NPCManager NPCMan{set{npcMan = value;}}
	
	// The list of NPCs used in the NPCManager
	private List<GameObject> npcList;
	public List<GameObject> NPCList{set{npcList = value;}}
	
	// A list of members in this formation 
	private List<int> members = new List<int>();
	public List<int> Members {get{return members;}}
	
	// The index of the leader NPC
	// An index of -1 means there is no leader
	private int leader = -1;
	public int Leader {get{return leader;}}
	
	// Use this for initialization
	void Start () 
	{
		if (npcMan != null)
		{
			npcList = npcMan.NPCList;
		}
		else
		{
			Debug.LogError("No NPCManager set in for Formation.");
		}
	}
	
	// Sets a formation member as a leader if there are none
	void promoteLeader()
	{
		if (members.Count > 0)
		{
			leader = members[0];
		}
	}
	
	// Determines the positions of all the formation members
	void determinePositions()
	{
		if (leader != -1)
		{
			NPC currentLeader = npcList[leader].GetComponent<NPC>();
			Transform leaderTrans = currentLeader.transform;
			currentLeader.MyPosition = Vector3.zero;
			currentLeader.MyLeader = null;
			
			for (int i = 1; i < members.Count; i++)
			{
				// Create a vector extending backwards from 
				// the leader
				Vector3 targetPosition = leaderTrans.forward;
				targetPosition *= -1.0f * npcMan.formationDist * i;  
				// Odds go to the right, evens to the left
				if (i%2 > 0)
				 	targetPosition = Quaternion.AngleAxis(npcMan.formationAngle, leaderTrans.up) * targetPosition;
				else
					targetPosition = Quaternion.AngleAxis(-npcMan.formationAngle, leaderTrans.up) * targetPosition;
				
				targetPosition += leaderTrans.position;
				npcList[members[i]].GetComponent<NPC>().MyPosition = targetPosition;
				npcList[members[i]].GetComponent<NPC>().MyLeader = leaderTrans;
			}
		}
	}
	
	// Removes a member from this formation 
	// Uses their numerical index in the list of NPCs
	public void removeMember(int id)
	{
		members.Remove(id);
		if (leader == id)
		{
			leader = -1;
		}
	}
	
	// Update is called once per frame
	public void Update () 
	{
		promoteLeader();
		determinePositions();
	}
}
