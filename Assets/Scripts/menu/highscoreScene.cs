using UnityEngine;
using System.Collections;
using Parse;

public class highscoreScene : MonoBehaviour {
	public GUISkin highscoreSceneSkin;
	public GUIText highScoreGUIText;
	private string name = "";
		
	// Use this for initialization
	void Start () {
		highScoreGUIText.text = "High Score!! " + Util.parseScore(GameStateManager.Instance.lastScore); 
	}
	
	void OnGUI(){
		GUI.skin = highscoreSceneSkin;
		GUI.Label(new Rect(Util.scaleScreen(30f),Screen.height/3 + Util.scaleScreen(50f),
			Util.scaleScreen(60f),Util.scaleScreen(40f)), "Name: ");
		name = GUI.TextField(new Rect(Util.scaleScreen(120f),Screen.height/3 + Util.scaleScreen(50f),
			Screen.width - Util.scaleScreen(150f),Util.scaleScreen(40f)), name, 40);
		
		//submit button	
		if(GUI.Button(Util.buttonStandardSize, "Submit")){
			//upload data to parse
			uploadParse();	
			//upload to gimmie;
			GimmieBinding.GimmieTriggerEvent("breakhighscore");
			//return to menuScene	
			DontDestroyOnLoad(GameStateManager.Instance);
			Application.LoadLevel("menu");
		}
	}
	
	void uploadParse(){
		if(name.Equals("", System.StringComparison.Ordinal)){
			name = "Anonymous";
		}
		GameStateManager.Instance.highScoreObject.SaveAsync().ContinueWith(t=>{
			GameStateManager.Instance.highScoreObject["name"] = name;
			GameStateManager.Instance.highScoreObject["score"] = GameStateManager.Instance.lastScore;
			//highScoreObject["score"] = GameStateManager.Instance.lastScore;
			GameStateManager.Instance.highScoreObject.SaveAsync();
		});
	}
	// Update is called once per frame
	void Update () {
	
	}
}
