using UnityEngine;
using System.Collections;

public class Maingame : MonoBehaviour {
	public GUIText scoreGuiText;
	public GUIText highScoreGuiText;
	public BoxCollider wall;
	
	private bool highScoreLoaded = false;
	private int highScore;
	private bool gameIsOver = false;
	private Vector3 wallStartingPosition;
	private int score =0;
	// Use this for initialization
	void Start () {
		scoreGuiText.fontSize = (int)Util.scaleScreen(Util.textSizeNormal);
		wallStartingPosition = new Vector3(wall.transform.position.x, 12.5f, 0);	
		InvokeRepeating("updateScore", 0.04f, 0.04f);	
	}
	void Awake(){
		
	}
	void updateScore(){
		score+=4;
		if(scoreGuiText!=null)
			scoreGuiText.text = Util.parseScore(score);
		if(score>highScore)
		{
			if(highScoreGuiText!=null){
				highScoreGuiText.text = Util.parseScore(score);	
				highScoreGuiText.fontSize = (int)Util.scaleScreen(Util.textSizeLarge);
			}
			scoreGuiText.fontSize = (int)Util.scaleScreen(Util.textSizeLarge);
		}
		if(score>PlayerPrefs.GetInt("localHighScore"))
			PlayerPrefs.SetInt("localHighScore", score);
	}
	void Update(){
		if(!highScoreLoaded)
			if(GameStateManager.Instance.highScoreObject!=null){
				highScore = GameStateManager.Instance.highScoreObject.Get<int>("score");
				highScoreLoaded=true;
			}
		
		if(gameIsOver && wall!=null){
			//move wall up
			wall.transform.position = Vector3.Lerp(wall.transform.position, 
				wallStartingPosition, 10*Time.deltaTime);	
		}
	}	
	public void GameOver()
	{
		gameIsOver = true;
		GameStateManager.Instance.lastScore = score;
		if(score>highScore)
		{
			
			//PlayerPrefs.SetString ("highscorer", GameStateManager.Instance.Username);
			//PlayerPrefs.SetInt("highScore", score);
			Invoke ("goToScoreSubmission", 0.45f);
		} else {
			Invoke("goToMenu", 0.45f);
		}
		
	}
	
	private void goToScoreSubmission(){
		DontDestroyOnLoad(GameStateManager.Instance);
		Application.LoadLevel("highscore");
	}
	private void goToMenu(){
		DontDestroyOnLoad(GameStateManager.Instance);
		Application.LoadLevel("menu");
	}
	
}
