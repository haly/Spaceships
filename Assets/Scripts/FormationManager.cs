using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FormationManager : MonoBehaviour 
{
	public NPCManager npcMan;
	
	private List<GameObject> npcList = new List<GameObject>();
	public List<GameObject> NPCList{set{npcList = value;}}
	
	public int formationSize = 5;
	private List<Formation> formationList = new List<Formation>();
	
	// Use this for initialization
	void Start () 
	{
		if (npcMan != null)
		{
			npcList = npcMan.NPCList;
		}
		else
		{
			Debug.LogError("No NPCManager set in editor for FormationManager.");
		}
	}
	
	// Creates a new formation
	void createFormation()
	{
		Formation newFormation =  new Formation();
		newFormation.NPCList = npcList;
		newFormation.NPCMan = npcMan; 
		formationList.Add(newFormation);
	}
	
	// Updates all the formations
	void updateFormations()
	{
		foreach (Formation f in formationList)
		{
			f.Update();
		}
	}
	
	// Clears any empty formations
	void clearEmptyFormations()
	{
		// Check to see if any formations are empty
		List<Formation> toBeRemoved = new List<Formation>();
		foreach (Formation f in formationList)
		{
			if(f.Members.Count == 0)
				toBeRemoved.Add(f);
		}
		foreach (Formation removeF in toBeRemoved)
		{
			formationList.Remove(removeF);
		}
	}
	
	// Find a formation for this NPC using its ID
	public void addToFormations(int index)
	{
		NPC newNPC = npcList[index].GetComponent<NPC>();
		
		// If it's not null and the NPC does not have a formation
		if (newNPC != null && !newNPC.HasFormation)
		{
			// If there are no formations make a new one
			if (formationList.Count == 0)
			{
				createFormation();
			}
			// Look for a formation with room
			foreach(Formation f in formationList)
			{
				if (f.Members.Count < formationSize)
				{
					// Add the NPC's index to the formation
					f.Members.Add(newNPC.index);
					// Set the NPC's hasFormation bool to true
					newNPC.MyFormation = f;
					newNPC.HasFormation = true;
					break;
				}
			}
			// If the new NPC is still looking for a formation
			// after going through all the available ones
			// create a new formation and add the new NPC to it
			if (!newNPC.HasFormation)
			{
				createFormation();
				Formation lastFormation = formationList[formationList.Count - 1];
				lastFormation.Members.Add(newNPC.Index);
				newNPC.MyFormation = lastFormation;
				newNPC.HasFormation = true;
			}
		}
		else if (newNPC == null)
		{
			Debug.LogError("No NPC component in this game object.");
		}
	}
	
	// Removes an NPC from the list of formations using its ID
	public void removeFromFormations(int index)
	{
		foreach (Formation f in formationList)
		{
			f.removeMember(index);
		}
	}
				
	// Update is called once per frame
	void Update () 
	{
		clearEmptyFormations();
		updateFormations();
	}
}
