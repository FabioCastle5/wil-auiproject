using UnityEngine;
using System.Collections;

public class Loading : MonoBehaviour {

	public GameObject generator;

	void Start () {
		generator.GetComponent<ObstacleGenerator> ().StartCoroutine ("StartGeneration");
	}

	void OnApplicationQuit() {
		generator.GetComponent<ObstacleGenerator> ().StopGeneration ();
	}
}
