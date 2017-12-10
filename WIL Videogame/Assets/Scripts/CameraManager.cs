using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	public GameObject player;       //Public variable to store a reference to the player game object
 

	private Vector3 offset;         //Private variable to store the offset distance between the player and camera

	// Use this for initialization
	void Start () 
	{
		if (player != null)
			offset = transform.position - player.transform.position;
	}

	// LateUpdate is called after Update each frame
	void LateUpdate () 
	{
		if (player != null) {
			// Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
			transform.position = player.transform.position + offset;
		}
	}

	public void SetPlayer(GameObject p) {
		player = p;
		offset = transform.position - player.transform.position;
		Debug.Log ("Set player to be followed: " + player.name);
	}
}

