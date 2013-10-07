using UnityEngine;
using System.Collections;

public class introBall : MonoBehaviour {

	// Use this for initialization
	void Start () {
		rigidbody.AddForce(3,0,0, ForceMode.Impulse);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
