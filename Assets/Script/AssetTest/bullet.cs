using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour{

	const float fly_speed = 25.0F;
	const float life_time = 3.0F;
	float timer = 0F;
	public GameObject owner;

	void FixedUpdate(){

		transform.localPosition += transform.forward * Time.deltaTime * fly_speed;
		if( timer >= life_time ){
			Destroy(this.gameObject);
		}

		timer+=Time.deltaTime;

	}

	void OnTriggerEnter(Collider col){

		GameObject col_ob = col.gameObject;

		if ( owner && col_ob != owner ){
			Destroy(this.gameObject);
		}

	}

	public void set_owner(GameObject owner_ob){
		owner = owner_ob;
	}

}
