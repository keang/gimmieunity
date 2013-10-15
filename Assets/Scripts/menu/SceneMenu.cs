using UnityEngine;
using System.Collections;

public class SceneMenu : MonoBehaviour {
	public GUISkin mainSkin;
	public GUIText localBestText;
	public BoxCollider wall;
	public float smooth = 0.1f;
		
	private bool playPressed = false;
	private Vector3 startingPosition;
	
	
	// Use this for initialization
	void Start(){
		localBestText.guiText.fontSize = (int)Util.scaleScreen(Util.textSizeSmall);
		localBestText.text = "Your best: " + Util.parseScore(PlayerPrefs.GetInt("localHighScore"));
		startingPosition = new Vector3(wall.transform.position.x, 4.5f, 0f);
		GameStateManager.Instance.loadHighScore();
	}
	void OnGUI () {
		GUI.skin = mainSkin;
		
		//play button	
		if(!playPressed){
			//arcade button
			if(GUI.Button(Util.buttonStandardSize, "Arcade")){
				playPressed = true;
				Invoke ("startGame", 0.55f);
			}
			
			//zen button
			if(GUI.Button(new Rect(Util.scaleScreen(30f),Screen.height/3 + Util.scaleScreen(50f),
			Screen.width - Util.scaleScreen(60f),Util.scaleScreen(40f)), "Zen")){
				playPressed = true;
				Invoke ("startZen", 0.55f);
			}
			
			//+1 button
//			if(GUI.Button(new Rect(Util.scaleScreen(30f),Screen.height/3 - Util.scaleScreen(50f)
//				+ Util.scaleScreen(50f),
//			Screen.width - Util.scaleScreen(60f),Util.scaleScreen(40f)), "+1 point")){
//				GimmieBinding.GimmieTriggerEvent("testevent");
//			}
			
		}
	}
	
	void startGame(){
		DontDestroyOnLoad(GameStateManager.Instance);
		if(!Application.isEditor && GameStateManager.Instance.playCounter != null){
			GameStateManager.Instance.playCounter++;	
			GameStateManager.Instance.uploadPlayCount();
		}
		Application.LoadLevel("mainGame");
	}
	
	void startZen(){
		DontDestroyOnLoad(GameStateManager.Instance);
		Application.LoadLevel("zenMode");
	}
	
	// Update is called once per frame
	void Update () {
	//welcometext
		if(playPressed && wall!=null)		
			wall.transform.position = Vector3.Lerp(wall.transform.position, startingPosition, smooth * Time.deltaTime);	
	}
	
}
