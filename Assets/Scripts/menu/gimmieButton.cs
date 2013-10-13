using UnityEngine;
using System.Collections;

public class gimmieButton : MonoBehaviour {
	// Use this for initialization
	void OnMouseDown () {
		Debug.Log("Mouse Down");
	}
	void Update(){
		if(Input.GetMouseButtonDown(0)){
			Ray ray = new Ray();
			RaycastHit hits = new RaycastHit();
			ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			if(Physics.Raycast(ray, out hits)){
				if(hits.transform.name == "gimmieButton"){
					GimmieBinding.OnMouseDown();	
				}
			}
		}	
	}
}
