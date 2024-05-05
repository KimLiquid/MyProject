using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killed : MonoBehaviour{

	void OnCollisionEnter(Collision col){

		GameObject col_ob = col.gameObject;

		if( col.collider.tag == "enemy" && this.tag == "Player" ){
			
			// print(this.name+" hit by "+col_ob.name );
			get_spawn_killed();

		}

	}

	void OnTriggerEnter(Collider col){

		GameObject col_ob = col.gameObject;
		
		if( col_ob.tag == "bullet" ){

			if( col_ob.GetComponent<bullet>().owner != this.gameObject ){
				print(this.name+" hit by bullet from "+col_ob.GetComponent<bullet>().owner );
				get_spawn_killed();
			}

		}

	}

	void get_spawn_killed(){

		Destroy(this.gameObject);

	}

}
