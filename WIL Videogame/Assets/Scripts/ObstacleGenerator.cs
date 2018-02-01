using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObstacleGenerator : MonoBehaviour {

	public int updatePeriod;
	public int deletePeriod;
	public int oneProbability;
	public int twoProbability;

	public GameObject player;
	public GameObject obstacle;

	private List<GameObject> obstacles;
	private int randomNumber;
	private bool stop;
	private const int hoffset = 12;
	private const int voffset = 7;
	private int align;
	private int count;


	void Start () {
		obstacles = new List<GameObject> ();
		stop = false;
		align = 1;
		count = 0;
	}

	public IEnumerator StartGeneration() {
		while (!stop) {
			count++;
			if (count >= deletePeriod) {
				DeleteObstacles ();
				count = 0;
			}
			// 99 is inclusive
			randomNumber = Random.Range (0, 99);
			if (randomNumber < oneProbability) {
				GenerateOne ();
			} else if (randomNumber < (twoProbability + oneProbability)) {
				GenerateTwo();
			}
			yield return new WaitForSeconds(updatePeriod);
		}
	}
	
	void GenerateOne () {
		int dir = player.GetComponent<PlayerController> ().GetRouteDirection ();
		float x, y;
		if (dir == 0) {
			x = player.transform.position.x + hoffset;
			y = player.transform.position.y + align * voffset / 2;
		} else if (dir == 90) {
			x = player.transform.position.x + align * voffset / 2;
			y = player.transform.position.y + voffset;
		} else if (dir == 180) {
			x = player.transform.position.x - hoffset;
			y = player.transform.position.y + align * voffset / 2;
		} else {
			x = player.transform.position.x + align * voffset / 2;
			y = player.transform.position.y - voffset;
		}
		GameObject ob = Instantiate(obstacle, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;
		ob.GetComponent<ObstacleManager> ().SetDirection (dir);
		obstacles.Add (ob);
		align = -align;
	}
	void GenerateTwo () {
	}

	void DeleteObstacles () {
		for (int i = 0; i < 2 && i < obstacles.Count; i++) {
			GameObject ob = obstacles [0];
			obstacles.RemoveAt (0);
			Destroy (ob);
		}
	}

	public void StopGeneration () {
		stop = true;
	}
}
