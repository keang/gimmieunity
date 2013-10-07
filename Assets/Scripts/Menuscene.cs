using UnityEngine;
using System.Collections;

public class Menuscene : MonoBehaviour {
	public GUISkin mainSkin;
	public GUIText welcomeText;
	public GUIText localBestText;
	public BoxCollider wall;
	public float smooth = 0.1f;
		
	private bool playArcadePressed = false;
	private Vector3 startingPosition;
	
	
	// Use this for initialization
	void Start(){
		localBestText.text = "Your best: " + Util.parseScore(PlayerPrefs.GetInt("localHighScore"));
		startingPosition = new Vector3(wall.transform.position.x, 4.5f, 0f);
		GameStateManager.Instance.loadHighScore();
	}
	void OnGUI () {
		GUI.skin = mainSkin;
		
		//setTextsizes
		if(welcomeText!=null)
		welcomeText.fontSize = (int)Util.scaleScreen(Util.textSizeNormal);
		
		//play button	
		if(!playArcadePressed){
			if(GUI.Button(Util.buttonStandardSize, "Arcade")){
				playArcadePressed = true;
				Invoke ("startGame", 0.55f);
			}
			if(GUI.Button(new Rect(Util.scaleScreen(30f),Screen.height/3 + Util.scaleScreen(50f),
			Util.scaleScreen(60f),Util.scaleScreen(40f)), "Zen")){
				playArcadePressed = true;
				Invoke ("startZen", 0.55f);
			}
		}
	}
	
	void startGame(){
		DontDestroyOnLoad(GameStateManager.Instance);
		if(GameStateManager.Instance.playCounter != null){
			GameStateManager.Instance.playCounter++;	
			//GameStateManager.Instance.uploadPlayCount();
		}
		Application.LoadLevel("mainGame");
	}
	void startZen(){
		DontDestroyOnLoad(GameStateManager.Instance);
		//Application.LoadLevel("zenMode");
	}
	// Update is called once per frame
	void Update () {
	//welcometext
		if(playArcadePressed && wall!=null)		
			wall.transform.position = Vector3.Lerp(wall.transform.position, startingPosition, smooth * Time.deltaTime);	
		if(GameStateManager.Instance.Username != null){
			welcomeText.text = "Hello, " + GameStateManager.Instance.Username;
		}	
	}
	
}
