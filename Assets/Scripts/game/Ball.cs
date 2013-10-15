using UnityEngine;
using System.Collections;

public class Ball : MonoBehaviour {
	public SceneMainGame mainGame;
	private float smooth = 0.2f;
	private bool gameIsOver = false;
	// Update is called once per frame
	void Update () {
		if(transform.position.y <-4 && mainGame != null && !gameIsOver) {
			mainGame.GameOver();
			gameIsOver=true;
		}
		//cornered
		if(rigidbody.velocity.magnitude == 0) 
			transform.position =  Util.resetPosition;
		
		//above the roof
		if(transform.position.y>4.5f){
			transform.position = new Vector3(transform.position.x, 4.4f, 0f);
		}
	}
	
	void Awake(){
		rigidbody.AddForce(4,4,0, ForceMode.Impulse);
		InvokeRepeating("increaseVelocity", 2, 2);
	}
	
	private void increaseVelocity(){
		rigidbody.velocity = rigidbody.velocity*1.02f;
	}
}
