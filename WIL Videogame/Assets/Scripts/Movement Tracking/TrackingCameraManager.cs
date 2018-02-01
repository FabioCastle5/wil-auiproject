using UnityEngine;
using System.Collections;

public class TrackingCameraManager : MonoBehaviour {

	public GameObject player;       //Public variable to store a reference to the player game object

	private Vector3 offset;         //Private variable to store the offset distance between the player and camera

	void Start () 
	{
			offset = new Vector3(0f, 0f, transform.position.z - player.transform.position.z);
	}
		
	void LateUpdate ()
	{
		// Set the position of the camera's transform to be the same as the player's, but offset by the calculated offset distance.
		transform.position = player.transform.position + offset;
	}
}