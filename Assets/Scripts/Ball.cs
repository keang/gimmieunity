using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	public Maingame mainGame;
	private float smooth = 0.2f;
	private Vector3 resetPos;
	// Use this for initialization
	void Start () {
		resetPos = new Vector3(0, 3.4f, 0);
	}
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y <-4 && mainGame != null) {
			mainGame.GameOver();
		}
		if(rigidbody.velocity.magnitude == 0) 
			transform.position =  resetPos;
	}
	
	void Awake(){
		rigidbody.AddForce(4,4,0, ForceMode.Impulse);
		InvokeRepeating("increaseVelocity", 2, 2);
	}
	
	private void increaseVelocity(){
		rigidbody.velocity = rigidbody.velocity*1.02f;
	}
}
