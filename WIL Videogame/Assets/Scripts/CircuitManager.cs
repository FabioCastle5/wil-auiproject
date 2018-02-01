using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class CircuitManager : MonoBehaviour {

	public int simultaneousTiles;
	public GameObject vCheckpoint;
	public GameObject hCheckpoint;
	public Image advanceBar;
	private float deltaXBar;
	private float initialWidth;
	private GameObject flagToBeDrawn;

	private List<TileData> tiles;
	private int nextTile;
	private List<GameObject> activeTiles;

	private const float offsetUp45 = 3.47f;
	private const int verticalOffset = 12;
	private const float horizontalOffset = 19.19f;

	private int initialDirection;

	private int checkpointIndex;


	public IEnumerator NextCheckpoint() {
		Debug.Log ("CircuitManager updating");
		checkpointIndex++;

		// advance checkpointbar
		if (checkpointIndex < tiles.Count) {
			Debug.Log ("Checkpoint index now: " + checkpointIndex);
			float newX = tiles [checkpointIndex].x;
			float newY = tiles [checkpointIndex].y;

			bool horizontal = false;
			if (tiles [checkpointIndex].tileObject.name.Contains ("TileEW")) {
				// must be rotated 90°
				horizontal = true;
			} else if (tiles [checkpointIndex].tileObject.name.Contains ("CurveNE") || tiles [checkpointIndex].tileObject.name.Contains ("CurveWN")) {
				horizontal = true;
				newX += 8.5f;
			} else if (tiles [checkpointIndex].tileObject.name.Contains ("Curve")) {
				horizontal = true;
				newX -= 8.5f;
			}
			Vector3 pos = new Vector3 (newX, newY, 0f);
			if (horizontal) {
				Instantiate (hCheckpoint, pos, hCheckpoint.transform.rotation);
			} else {
				Instantiate (vCheckpoint, pos, vCheckpoint.transform.rotation);
			}
		}

		yield return null;

		// draw next tile
		if (checkpointIndex > 2)
			DrawNextTile ();

		yield return null;

		// fulfil advance bar
		float dim = deltaXBar * (checkpointIndex - 1);
		Debug.Log("Initial width: " + initialWidth + " Index: " + checkpointIndex + " Actual: " + dim);
		float proposition = dim / initialWidth;
		Debug.Log ("Proposition: " + proposition);
		advanceBar.transform.localScale = new Vector3(proposition, 1f, 1f);
	}
	
	public void AddTile (float x, float y, GameObject tile) {
		if (tiles == null)
			tiles = new List<TileData> ();
		TileData newTile = new TileData();
		newTile.x = x;
		newTile.y = y;
		newTile.tileObject = tile;
		tiles.Add (newTile);
	}

	public void Reset () {
		tiles = new List<TileData>();
		activeTiles = new List<GameObject> (simultaneousTiles);
		initialDirection = 0;
		nextTile = 0;
	}

	public void SetInitialDirection(int direction) {
		initialDirection = direction;
	}

	public int GetInitialDirection () {
		return initialDirection;
	}

	public float GetInitialX () {
		return tiles [0].x;
	}

	public float GetInitialY () {
		return tiles [0].y;
	}

	public void DrawFinalFlag (GameObject flag) {
		flagToBeDrawn = flag;
	}

	public void DrawNextTile () {
		if (nextTile < tiles.Count) {
			Debug.Log ("Drawing a new tile");
			GameObject oldTile = activeTiles [0];
			activeTiles.RemoveAt (0);
			Destroy (oldTile);
			// draw next tile only if it doesn't overlap any of the previous ones
			TileData newTileData = tiles [nextTile];
			float oldx, oldy;
			bool overlap = false;
			for (int i = 0; i < activeTiles.Count && !overlap; i++) {
				oldx = activeTiles [i].transform.position.x;
				oldy = activeTiles [i].transform.position.y;
				if (newTileData.x == oldx && newTileData.y == oldy)
					overlap = true;
			}
			if (!overlap) {
				GameObject newTile = Instantiate (newTileData.tileObject, new Vector3 (newTileData.x, newTileData.y, 0f), Quaternion.identity) as GameObject;
				activeTiles.Add (newTile);
				if (nextTile == tiles.Count - 1) {
					// draw final flag
					GameObject instance = Instantiate (flagToBeDrawn, new Vector3(newTileData.x,newTileData.y,0f), flagToBeDrawn.transform.rotation) as GameObject;
					instance.GetComponent<BoxCollider2D> ().enabled = true;
				}
				nextTile++;
			} else {
				Debug.Log ("Overlap found: posticipating tile drawing");
			}
		}
	}

	public void StartDrawing () {
		for (; nextTile < simultaneousTiles && nextTile < tiles.Count;) {
			// draw next tile only if it doesn't overlap any of the previous ones
			TileData newTileData = tiles [nextTile];
			float oldx, oldy;
			bool overlap = false;
			for (int i = 0; i < activeTiles.Count && !overlap; i++) {
				oldx = activeTiles [i].transform.position.x;
				oldy = activeTiles [i].transform.position.y;
				if (newTileData.x == oldx && newTileData.y == oldy)
					overlap = true;
			}
			Debug.Log ("Overlap: " + overlap);
			if (!overlap) {
				Debug.Log ("Drawing " + newTileData.tileObject.name + " at " + newTileData.x + " " + newTileData.y);
				GameObject newTile = Instantiate (newTileData.tileObject, new Vector3 (newTileData.x, newTileData.y, 0f), Quaternion.identity) as GameObject;
				activeTiles.Add (newTile);
				nextTile++;
			} else {
				Debug.Log ("Overlap found: posticipating tile drawing");
			}
		}
		SetCheckpoint ();
	}

	void SetCheckpoint () {
		// evaluate the deltax for the bar at every checkpoint and draw it with 0 width
		RectTransform rt = (RectTransform)advanceBar.gameObject.transform;
		this.initialWidth = rt.rect.width;
		this.deltaXBar = initialWidth / (tiles.Count);
		float proposition = deltaXBar / initialWidth / 10;
		Debug.Log ("Initial width: " + initialWidth);
		Debug.Log ("Delta x bar: " + deltaXBar);
		Debug.Log ("Number of tiles: " + tiles.Count);
		advanceBar.transform.localScale = new Vector3(proposition, 1f, 1f);
		if (tiles.Count > 1) {
			checkpointIndex = 1;
			Debug.Log ("Tile: " + tiles [1].tileObject.name);
			float newX = tiles [checkpointIndex].x;
			float newY = tiles [checkpointIndex].y;
			bool horizontal = false;
			if (tiles [checkpointIndex].tileObject.name.Contains ("TileEW")) {
				// must be rotated 90°
				horizontal = true;
			} else if (tiles [checkpointIndex].tileObject.name.Contains ("CurveNE") || tiles [checkpointIndex].tileObject.name.Contains ("CurveWN")) {
				horizontal = true;
				newX += 9f;
			} else if (tiles [checkpointIndex].tileObject.name.Contains ("Curve")) {
				horizontal = true;
				newX -= 9f;
			}
			Vector3 pos = new Vector3 (newX, newY, 0f);
			if (horizontal) {
				Instantiate (hCheckpoint, pos, hCheckpoint.transform.rotation);
			} else {
				Instantiate (vCheckpoint, pos, vCheckpoint.transform.rotation);
			}
		}
	}
}

class TileData {
	public float x, y;
	public GameObject tileObject;
}