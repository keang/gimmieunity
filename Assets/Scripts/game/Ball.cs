using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	public SceneMainGame mainGame;
	private float smooth = 0.2f;
	
	// Update is called once per frame
	void Update () {
		if(transform.position.y <-4 && mainGame != null) {
			mainGame.GameOver();
		}
		if(rigidbody.velocity.magnitude == 0) 
			transform.position =  Util.resetPosition;
	}
	
	void Awake(){
		rigidbody.AddForce(4,4,0, ForceMode.Impulse);
		InvokeRepeating("increaseVelocity", 2, 2);
	}
	
	private void increaseVelocity(){
		rigidbody.velocity = rigidbody.velocity*1.02f;
	}
}
