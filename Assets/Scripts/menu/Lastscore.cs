using UnityEngine;
using System.Collections;

public class Lastscore : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int lastScore = GameStateManager.Instance.lastScore;
		if(lastScore>0){
			guiText.text = "Just now: " + Util.parseScore(lastScore);
			guiText.fontSize = (int)Util.scaleScreen(Util.textSizeSmall);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
