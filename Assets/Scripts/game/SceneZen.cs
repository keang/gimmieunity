using UnityEngine;
using System.Collections;
using Parse;

public class SceneZen : MonoBehaviour {
	public BoxCollider wall;
	public SphereCollider ball;	
	private int zenTime = 0;
	private bool gameIsOver = false;
	private Vector3 wallStartingPosition;
	// Use this for initialization
	void Start () {
		wallStartingPosition = new Vector3(wall.transform.position.x, 12.5f, 0);	
		InvokeRepeating("updateTime", 1f, 1f);
	}
	void updateTime(){
		zenTime++;
	}
	void OnGUI(){
		if(!gameIsOver){
			if(GUI.Button(new Rect(Util.scaleScreen(60f),Util.scaleScreen(0f),
				Screen.width - Util.scaleScreen(120f),Util.scaleScreen(20f)), "Menu")){
				gameIsOver = true;
				onStopPressed();
			}
		}
	}
	
	void Awake(){
		ball.rigidbody.AddForce(4,4,0, ForceMode.Impulse);
	}	
	
	void Update(){
		//ball drops
		if(ball!=null && ball.transform.position.y <-4) {
			ball.transform.position = Util.resetPosition;
			ball.rigidbody.velocity = new Vector3(0,0,0);
			ball.rigidbody.AddForce(4,4,0, ForceMode.Impulse);		
		}
		//ball stuck/cornered
		if(ball!=null && ball.rigidbody.velocity.magnitude == 0) {
			ball.transform.position =  Util.resetPosition;
			Debug.Log("cornered resolved");
		}
		//animate the walls up upon gameOver
		if(wall!=null && gameIsOver){
			//move wall up
			wall.transform.position = Vector3.Lerp(wall.transform.position, 
				wallStartingPosition, 10*Time.deltaTime);	
		}
	}	
	
	private void onStopPressed(){
		Invoke("goToMenu", 0.45f);
		gameIsOver = true;
	}
	
	private void goToMenu(){
		DontDestroyOnLoad(GameStateManager.Instance);	
		uploadZenTime();
		Application.LoadLevel("menu");
	}
	
	private void uploadZenTime(){
		ParseObject zenTimeObject = new ParseObject("zenTime");
		zenTimeObject["time"] = zenTime;
		zenTimeObject.SaveAsync();
	}
}
