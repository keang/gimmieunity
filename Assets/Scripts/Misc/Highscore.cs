using UnityEngine;
using System.Collections;
using Parse;

public class Highscore : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if(GameStateManager.Instance.highScoreObject != null){
			int highscore = GameStateManager.Instance.highScoreObject.Get<int>("score");
			string highscorename =  GameStateManager.Instance.highScoreObject.Get<string>("name");
			guiText.text = "Best: "  + Util.parseScore(highscore) + " by " + highscorename;
			guiText.fontSize = (int)Util.scaleScreen(Util.textSizeNormal);		
		}
	}
}
