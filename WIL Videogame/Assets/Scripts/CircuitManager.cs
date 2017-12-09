using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CircuitManager : MonoBehaviour {

	private List<TileData> tiles;

	private const float offsetUp45 = 3.47f;
	private const int verticalOffset = 12;
	private const float horizontalOffset = 19.19f;

	private int initialDirection;
	
	public void AddTile (float x, float y, GameObject tile) {
		if (tiles == null)
			tiles = new List<TileData> ();
		TileData newTile = new TileData();
		newTile.x = x;
		newTile.y = y;
		newTile.tileObject = tile;
		newTile.tileObject.transform.SetParent (gameObject.transform);
	}

	public void Reset () {
		tiles = new List<TileData>();
		initialDirection = 0;
	}

	public void SetInitialDirection(int direction) {
		initialDirection = direction;
	}

	public int GetInitialDirection () {
		return initialDirection;
	}

	public void DrawFinalFlag (GameObject flag) {
		int i = 0;
		while (i < tiles.Count - 1)
			i++;
		float x = tiles[i].x;
		float y = tiles[i].y;
		GameObject instance = Instantiate (flag, new Vector3(x,y,0f), flag.transform.rotation) as GameObject;
		instance.GetComponent<BoxCollider2D> ().enabled = true;
	}
}

class TileData {
	public float x, y;
	public GameObject tileObject;
}