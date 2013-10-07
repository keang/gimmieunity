using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Facebookinit : MonoBehaviour {
	
    private Rect buttonSize;
    private static Dictionary<string, string> profile 	= null;
	private void SetInit(){
		enabled = true; //magical global
	}
	private void onHideUnity(bool isGameShown){
		if(!isGameShown){ //pause the game
			Time.timeScale = 0;
		} else { //start the game back up
			Time.timeScale = 1;
		}
	}
	// Use this for initialization
	void Start () {
		buttonSize = new Rect(scale(30f),Screen.height/2,Screen.width - scale(60f),scale(40f));
		enabled = false;
		//Debug.Log ("start, have id: " + FB.IsLoggedIn);
		DontDestroyOnLoad(GameStateManager.Instance);
		if(!FB.IsLoggedIn)
			FB.Init(SetInit, onHideUnity);
		else 
	        FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", Facebook.HttpMethod.GET, APICallback);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI(){
		string panelText = "Login to facebook";
		//GUI.Label(new Rect(scale(10f),Screen.height/2,Screen.width - scale(20f),scale(40f)), panelText);
		
        if (!FB.IsLoggedIn)
        {
			//GUI.Button(new Rect(scale(10f),Screen.height/2,Screen.width - scale(40f),scale(40f)), panelText);
			Debug.Log ("made button, not logged in");
			
            if (GUI.Button(buttonSize , panelText))
            {	Debug.Log ("clicked login");
                FB.Login("email", LoginCallback);
            }
        } else {
			Debug.Log ("check, have id: " + FB.UserId);
	        FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", Facebook.HttpMethod.GET, APICallback);
		}
	}
	
	void LoginCallback(FBResult result){
		Debug.Log ("logged in!" + FB.UserId);//use this to sync progress etc
		Debug.Log("login result: " + result.Text);
        FB.API("/me?fields=id,first_name,friends.limit(100).fields(first_name,id)", Facebook.HttpMethod.GET, APICallback);
        FB.API(Util.GetPictureURL("me", 128, 128), Facebook.HttpMethod.GET, MyPictureCallback);
	}
	
	void APICallback(FBResult result) // handle user profile info
	{
	    if (result.Error != null)
	    {
	        Debug.LogError(result.Error);
	        return;
	    }
		
	    profile = Util.DeserializeJSONProfile(result.Text);
	    GameStateManager.Instance.Username = profile["first_name"];
	    //friends = Util.DeserializeJSONFriends(result.Text);
	}
	
	void MyPictureCallback(FBResult result) // store user profile pic
	{
    	if (result.Error != null)
    	{
        	Debug.LogError(result.Error);
        	return;
    	}

    	GameStateManager.Instance.UserTexture = result.Texture;
	}
	
	private float scale(float f){
		return f * Screen.height/288;
	}
	
}
