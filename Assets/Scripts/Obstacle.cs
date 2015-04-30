using UnityEngine;
using System.Collections;

public class Obstacle : MonoBehaviour 
{
	public float radius;
	public Vector3 position;
	
	void Start () 
	{
		tag = "obstacle";
		calcBounds();
	}
	
	void calcBounds()
	{
		Bounds AABB = GetComponent<Renderer>().bounds;
		Vector3 diagonal = AABB.center;
		diagonal.x += AABB.extents.x;
		diagonal.y += AABB.extents.y;
		diagonal.z += AABB.extents.z;
		radius = Vector3.Distance(AABB.center, diagonal);
		position = AABB.center;
	}
	// Update is called once per frame
	void Update () 
	{
		//calcBounds();
	}
}
