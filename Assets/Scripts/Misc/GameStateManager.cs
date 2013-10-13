using UnityEngine;
using System.Collections;
using Parse;

public class GameStateManager : MonoBehaviour {

	//properties:
	private static GameStateManager instance;
	public string Username;
	public Texture UserTexture;
	public int lastScore;
	public ParseObject highScoreObject;
	public ParseObject playCountObject;
	public int playCounter;
	public static GameStateManager Instance{
		get{
			if(instance == null){
				instance = new GameObject("GameStateManager").AddComponent<GameStateManager>();
				instance.startCounter();
				Debug.Log("Started state manager!!!");
			}
			return instance;
		}
	}
	
	void startCounter(){
		playCountObject = new ParseObject("PlayCount");
		playCounter = 0;
	}
	
	public void loadHighScore(){
		ParseQuery<ParseObject> query = new ParseQuery<ParseObject>("HighScorer");
		query.GetAsync("7qrQVagc9u").ContinueWith(t =>{
			highScoreObject = t.Result;
		});
		//highScoreObject.FetchAsync<ParseObject>();
		//highScoreObject.FetchAsync();
	}
	
	//set the instance to null when application quits
	public void onApplicationQuit(){
		uploadPlayCount();	
		instance = null;
	}
	
	public void uploadPlayCount(){
		playCountObject.SaveAsync().ContinueWith(t=>{
			playCountObject["replays"] = playCounter;
			//highScoreObject["score"] = GameStateManager.Instance.lastScore;
			playCountObject.SaveAsync();
			Debug.Log("Pushed playcount");
		});
	}
}
