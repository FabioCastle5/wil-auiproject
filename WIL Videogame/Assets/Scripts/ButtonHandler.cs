using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ButtonHandler : MonoBehaviour {

	void Start () {
		gameObject.GetComponent<Image> ().color = new Color (0, 0, 0, 0);
	}

	public void DisableClick () {
		gameObject.SetActive (false);
	}

	public void EnableClick () {
		gameObject.SetActive (true);
	}
}
