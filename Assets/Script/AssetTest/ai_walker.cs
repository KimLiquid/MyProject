// ****************************************************************
// Unity Game Engine AI-Walker V1
// Everything in this file by MrLarodos: http://www.youtube.com/user/MrLarodos
//
// Released under the Creative Commons Attribution 3.0 Unported License:
// http://creativecommons.org/licenses/by/3.0/de/
// http://creativecommons.org/licenses/by/3.0/
//
// If you use this file or parts of it, you have to include this information header.
//
// DEUTSCH:
// Wenn Du diese Datei oder Teile davon benutzt, musst Du diesen Infoteil beilegen.
// ****************************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ai_walker : MonoBehaviour {

	public bool debug_print;

	public bool avoid_holes;
	public float base_length;
	public float look_height_offset;
	public string target_tag;

	public bool can_attack;
	public GameObject bullet_ob;

	public string waypoint_tag;
	public string waypoint_prefix;

	public bool animations_active;
	public bool sounds_active;
	public AudioClip idle_sound;
	public AudioClip notice_sound;
	public AudioClip shot_sound;
	public AudioClip leg_1_sound;
	public AudioClip leg_2_sound;
	public AudioClip step_1_sound;
	public AudioClip step_2_sound;
	public AudioClip idle_robot_sound;

	bool debug_gizmos;
	
	Animator anim;
	string anim_idle_state_name;
	string anim_walk_state_name;
	string anim_run_state_name;

	AudioSource audio_source;

	string last_waypoint_name;
	float waypoint_touch_range;
	bool waypoint_mode;
	GameObject waypoint_ob;

	Vector3 look_height_offset_v3;
	
	GameObject bullet_spawn;
	float attack_range;
	float attack_reload_sec;
	float next_attack_time;
	float attack_delay_sec_min;
	float attack_delay_sec_max;
	float attack_delay_standard;
	float attack_delay_end;

	bool attack_possible;
	bool direct_way_possible;
	GameObject target_ob;
	
	RaycastHit see_right;
	RaycastHit see_right_20;
	RaycastHit see_right_45;
	RaycastHit see_front;
	RaycastHit see_front_far;
	RaycastHit sphere_front_far;
	RaycastHit see_attack;
	RaycastHit see_left_45;
	RaycastHit see_left_20;
	RaycastHit see_left;

	Vector3 right;
	Vector3 right_20;
	Vector3 right_45;
	Vector3 front;
	Vector3 left_45;
	Vector3 left_20;
	Vector3 left;

	bool right_hit;
	bool right_20_hit;
	bool right_45_hit;
	bool front_hit;
	bool line_way_see_target;
	bool attack_see_target;
	bool sphere_way_see_target;
	bool left_hit;
	bool left_20_hit;
	bool left_45_hit;
	bool any_relevant_hit;

	string right_ob;
	string right_20_ob;
	string right_45_ob;
	string front_ob;
	string left_ob;
	string left_20_ob;
	string left_45_ob;

	float left_right_length;
	float left_right_20_length;
	float left_right_45_length;
	float front_length;
	float far_front_length;
	float sphere_front_far_size;
	float down_length;

	float turn_speed;
	float turn_speed_2_target;
	float walk_speed;
	float alert_speed;
	float hunt_speed;

	float timer;
	float timer_look_2_target;
	float sec_look_2_target;

	float timer_next_decision;
	float sec_next_decision_after;

	float idle_walk_time;
	int idle_walk_time_min;
	int idle_walk_time_max;

	int idle_walk_random_val;
	int idle_walk_random_min;
	int idle_walk_random_max;

	float idle_walk_time_stop;

	float alert_range;
	float alert_search_range_add_val;
	float alert_search_range;
	bool alert_by_sight_only;

	bool target_noticed;
	
	bool alerted;
	float alert_time_end;
	int alert_time_min_sec;
	int alert_time_max_sec;
	bool alert_others_by_tag;
	float alert_others_by_tag_range;
	
	bool move_forward;
	bool turn_left;
	bool turn_right;

	string last_turn_decision;

	int degrees;
	Quaternion target_rotation;

	string my_status_cluster;
	string my_status_anim;

	bool ai_active;
	float ai_range_add_val;
	float ai_border_add_val;
	float ai_deactivation_range;
	float ai_activation_range;

	Rigidbody npc_rb;

	void Awake(){

		debug_gizmos = true; //Debug-Linien anzeigen

		turn_speed = 10.0F; //Geschwindigkeit der eigenen Rotation beim Ausweichen
		degrees = 20; //Drehwinkel in Grad pro Schritt, um Wänden oder Objekten auszuweichen

		turn_speed_2_target = 4.0F; //Geschwindigkeit der eigenen Rotation beim Anvisieren des Ziels
		sec_look_2_target = 0.75F; //Wartezeit nach Ausweichen (any_relevant_hit==true), bevor das Ziel wieder anvisiert wird (sofern es ein Ziel gibt)

		walk_speed = 2.5F; //Wenn Ziel nicht sichtbar, oder kein Ziel vorhanden, dann diese Geschwindigkeit annehmen
		alert_speed = 3.5F; //Wenn Ziel nicht sichtbar, aber alerted, dann diese Geschwindigkeit annehmen
		hunt_speed = 5.0F; //Wenn Ziel sichtbar, dann diese Geschwindigkeit annehmen

		sec_next_decision_after = 1.0F; //Alle x Sekunden wird per Zufall entschieden, in welche Richtung bei frontalem Hinderniss gedreht wird

		idle_walk_random_min = 25; //Je kleiner die Zahl, umso größer die Chance (sofern gezogen), dass im Idle-Mode (kein Ziel vorhanden) gelaufen wird
		idle_walk_random_max = 100; //Je größer die Zahl, umso kleiner die Chance (sofern gezogen), dass im Idle-Mode (kein Ziel vorhanden) gelaufen wird

		idle_walk_time_min = 1; //Minimale Zeit zum Laufen am Stück im Idle
		idle_walk_time_max = 10; //Maximale Zeit zum Laufen am Stück im Idle

		alert_by_sight_only = true; //Nur alarmiert werden, wenn Ziel sichtbar
		alert_range = 20.0F; //Nur alarmiert werden, wenn Ziel in dieser Entfernung
		alert_search_range_add_val = 5.0F; //Wenn alarmiert, das Ziel nur innerhalb alert_range + alert_search_range_add_val verfolgen
		ai_range_add_val = 25.0F; //Wenn Ziele-Tag vorhanden und Ziel kommt in die Range (alert_range + alert_search_range_add_val + ai_range_add_val), dann AI on
		ai_border_add_val = 5.0F; //Wenn Ziele-Tag vorhanden, aber kein Ziel innerhalb der Range (alert_range + alert_search_range_add_val + ai_range_add_val + ai_border_add_val), dann AI off

		alert_others_by_tag = true; //Andere mit gleichem Tag alarmieren
		alert_others_by_tag_range = 10.0F; //Andere mit gleichem Tag innerhalb dieser Entfernung alarmieren
		
		alert_time_min_sec = 5; //Minimale Zeit, alarmiert zu bleiben
		alert_time_max_sec = 15; //Maximale Zeit, alarmiert zu bleiben

		attack_range = alert_range * 0.75F; //Ab dieser Entfernung ist eine Attacke (Projektil) möglich
		attack_reload_sec = 0.25F; //Dauer in Sekunden zum Nachladen der Waffe
		attack_delay_sec_min = 0.10F; //Minimale Verzögerung vor Attacke (einmalige Selektion der individuellen Verzögerung zum Start zwischen Min / Max)
		attack_delay_sec_max = 0.25F; //Maximale Verzögerung vor Attacke (einmalige Selektion der individuellen Verzögerung zum Start zwischen Min / Max)

		//START Animations-Parameter-Block############################################
		anim_idle_state_name = "idling"; //Name des Leerlauf- / Herumstehen-Animationsparameters (Bool)
		anim_walk_state_name = "walking"; //Name des Laufen-Animationsparameters (Bool)
		anim_run_state_name = "running"; //Name des Rennen-Animationsparameters (Bool)
		//ENDE Animations-Parameter-Block############################################

		//#######################
		if(base_length==0)base_length = 1; //Multiplikator zur relativen Anpassung der folgenden Sichtweiten etc.

		left_right_length = base_length * 2.0F; //Rechnerische Sichtweite nach links und rechts
		left_right_20_length = base_length * 2.2F; //Rechnerische Sichtweite nach 20 Grad links und rechts
		left_right_45_length = base_length * 2.0F; //Rechnerische Sichtweite nach 45 Grad links und rechts
		front_length = base_length * 2.25F; //Rechnerische Sichtweite nach vorne
		sphere_front_far_size = base_length * 1.25F; //Rechnerische Seitenlänge der Sichtsphere
		far_front_length = base_length * alert_range; //Rechnerische Sichtweite vorne in alert_range
		down_length = base_length * 3.5F;
		waypoint_touch_range = base_length * 1.5F; //Rechnerische Nähe zum Waypoint für die Auswahl eines neuen Waypoint

		look_height_offset_v3 = new Vector3(0F,look_height_offset,0F); //Höhe der Pins bzw. Augen des NPC von Objektzentrum aus höher (+) oder tiefer (-) einstellen
		//#######################

	}

	void Start(){

		timer = 0F;
		timer_next_decision = timer;
		idle_walk_time_stop = timer;
		alert_time_end = timer;
		next_attack_time = timer;
		idle_walk_time = 0F;
		move_forward = false;
		target_noticed = false;
		alerted = false;
		attack_possible = false;
		ai_active = true;
		last_turn_decision = "left";
		last_waypoint_name = "";

		if(can_attack){
			try{
				bullet_spawn = transform.Find("bullet_spawn").gameObject;
			}catch{
				print("Kein 'bullet_spawn'-Objekt im NPC " + this.name + " gefunden! NPC wird entfernt.");
				Destroy(this);
				return;
			}
		}

		idle_walk_random_val = Random.Range( idle_walk_random_min , idle_walk_random_max + 1 );
		attack_delay_standard = Random.Range( attack_delay_sec_min , attack_delay_sec_max );
		alert_search_range = alert_range + alert_search_range_add_val;
		ai_deactivation_range = alert_range + alert_search_range_add_val + ai_range_add_val + ai_border_add_val;
		ai_activation_range = alert_range + alert_search_range_add_val + ai_range_add_val;

		npc_rb = this.GetComponent<Rigidbody>();
		if(!npc_rb){
			print("Kein Rigidbody im NPC " + this.name + " gefunden! NPC wird entfernt.");
			Destroy(this);
			return;
		}
		audio_source = GetComponent<AudioSource>();
		if(sounds_active && !audio_source){
			print("Keine Audio Source im NPC " + this.name + " gefunden! NPC wird entfernt.");
			Destroy(this.gameObject);
			return;
		}
		anim = this.GetComponent<Animator>();

	}
	
	void FixedUpdate(){

		if( target_tag != "" ){ //Vorgabe für Ziele-Tag existiert
			
			waypoint_mode = false;
			target_ob = near_target(target_tag); //Falls es ein Ziel-Tag gibt, das näheste suchen und Entfernung checken

			if(ai_active==false){
				if(debug_print)print(this.name+": AI ist aus");
				set_status("idle","stand");
				return;
			}

		}

		//##START VARIABLEN RESET################################
		float act_walk_speed = walk_speed;
		float walk_speed_cap = 1.0F;

		left_hit = false;
		left_ob = "";

		left_20_hit = false;
		left_20_ob = "";

		left_45_hit = false;
		left_45_ob = "";

		front_hit = false;
		front_ob = "";
		
		sphere_way_see_target = false;
		line_way_see_target = false;
		attack_see_target = false;

		right_45_hit = false;
		right_45_ob = "";
		
		right_20_hit = false;
		right_20_ob = "";

		right_hit = false;
		right_ob = "";

		any_relevant_hit = false;

		direct_way_possible = false;

		right = transform.right;
		right_20 = Quaternion.Euler(0,20,0) * transform.forward;
		right_45 = Quaternion.Euler(0,45,0) * transform.forward;
		front = transform.forward;
		left_45 = Quaternion.Euler(0,-45,0) * transform.forward;
		left_20 = Quaternion.Euler(0,-20,0) * transform.forward;
		left = -transform.right;
		//##ENDE VARIABLEN RESET################################

		//##START INFORMATIONEN EINHOLEN################################
		if( target_ob && Physics.Raycast((transform.position + look_height_offset_v3), transform.forward, out see_attack, attack_range) ){ //Ziel direkt anvisiert
			if( see_attack.transform.name == target_ob.name )attack_see_target = true;
		}

		if( target_ob && Physics.Raycast((transform.position + look_height_offset_v3), ( target_ob.transform.position - (transform.position + look_height_offset_v3) ), out see_front_far, far_front_length) ){ //Sicht in Luftlinie zum Ziel vorhanden
			if( see_front_far.transform.name == target_ob.name )line_way_see_target = true;
		}

		if( target_ob && Physics.SphereCast((transform.position + look_height_offset_v3), sphere_front_far_size, ( target_ob.transform.position - (transform.position + look_height_offset_v3) ), out sphere_front_far, far_front_length) ){ //Direkter, breiter Weg zum Ziel vorhanden
			if( sphere_front_far.transform.name == target_ob.name )sphere_way_see_target = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), right, out see_right, left_right_length) ){
			right_hit = true;
			right_ob = see_right.transform.tag;
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (right * left_right_length)) , (transform.up*-1), out see_right, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			right_hit = true;
			right_ob = "hole";
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), right_45, out see_right_45, left_right_45_length) ){
			right_45_hit = true;
			right_45_ob = see_right_45.transform.tag;
			any_relevant_hit = true;
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (right_45 * left_right_45_length)) , (transform.up*-1), out see_right_45, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			right_45_hit = true;
			right_45_ob = "hole";
			any_relevant_hit = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), right_20, out see_right_20, left_right_20_length) ){
			right_20_ob = see_right_20.transform.tag;
			if(target_ob && target_ob.name == see_right_20.transform.name){
				right_20_hit = false; //Kein Treffer, wenn es sich um das Zielobjekt handelt
			}else{
				right_20_hit = true;
				any_relevant_hit = true;
			}
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (right_20 * left_right_20_length)) , (transform.up*-1), out see_right_20, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			right_20_hit = true;
			right_20_ob = "hole";
			any_relevant_hit = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), front, out see_front, front_length) ){
			front_ob = see_front.transform.tag;
			if(target_ob && target_ob.name == see_front.transform.name){
				front_hit = false; //Kein Treffer, wenn es sich um das Zielobjekt handelt
			}else{
				front_hit = true;
				any_relevant_hit = true;
			}
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (front * front_length)) , (transform.up*-1), out see_front, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			front_hit = true;
			front_ob = "hole";
			any_relevant_hit = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), left_20, out see_left_20, left_right_20_length) ){
			left_20_ob = see_left_20.transform.tag;
			if(target_ob && target_ob.name == see_left_20.transform.name){
				left_20_hit = false; //Kein Treffer, wenn es sich um das Zielobjekt handelt
			}else{
				left_20_hit = true;
				any_relevant_hit = true;
			}
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (left_20 * left_right_20_length)) , (transform.up*-1), out see_left_20, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			left_20_hit = true;
			left_20_ob = "hole";
			any_relevant_hit = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), left_45, out see_left_45, left_right_45_length) ){
			left_45_hit = true;
			left_45_ob = see_left_45.transform.tag;
			any_relevant_hit = true;
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (left_45 * left_right_45_length)) , (transform.up*-1), out see_left_45, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			left_45_hit = true;
			left_45_ob = "hole";
			any_relevant_hit = true;
		}

		if( Physics.Raycast((transform.position + look_height_offset_v3), left, out see_left, left_right_length) ){
			left_hit = true;
			left_ob = see_left.transform.tag;
		}else if( avoid_holes && !Physics.Raycast( ((transform.position + look_height_offset_v3) + (left * left_right_length)) , (transform.up*-1), out see_left, down_length) ){ //Treffer auf einen Abgrund (Falltiefe >= down_length)
			left_hit = true;
			left_ob = "hole";
		}

		if( right_20_hit || left_20_hit ){ //Bei Treffer auf den 20-Grad-Pins ebenfalls Front-Treffer = true
			front_hit = true;
		}

		//##MOTIVATIONS-INFORMATIONEN EINHOLEN################################
		if( target_ob && Vector3.Distance(target_ob.transform.position, transform.position) <= alert_range){ //Target innerhalb alert_range

			if(!target_noticed && !alert_by_sight_only){ //Auch alarmiert sein, wenn Ziel nicht sichtbar
				target_noticed = true;
			}else if( !target_noticed && alert_by_sight_only && line_way_see_target ){ //Nur dann alarmiert sein, wenn Ziel sichtbar
				target_noticed = true;
			}

		}else if(target_noticed){ //Target NICHT innerhalb alert_range ODER kein Target vorhanden
			target_noticed = false;
		}
		//##ENDE INFORMATIONEN EINHOLEN################################

		//##START REAKTION FESTLEGEN################################
		if( (!target_ob || !target_noticed) && waypoint_tag != "" ){ //Vorgabe für Waypoint-Parent existiert und es wurde kein Ziel gewählt

			if( waypoint_ob && Vector3.Distance(waypoint_ob.transform.position, transform.position) <= waypoint_touch_range ){
				if(debug_print)print(this.name+": Wegpunkt erreicht >>> "+waypoint_ob.name);
				waypoint_ob = null; //Wegpunkt erreicht
			}else if(!waypoint_ob){
				waypoint_ob = next_waypoint(waypoint_tag, waypoint_prefix, last_waypoint_name);
				if(debug_print)print(this.name+": Neues waypoint_ob >>> "+waypoint_ob.name);
			}

			if(waypoint_ob)waypoint_mode = true;

		}

		if( !left_45_hit && !right_45_hit && !front_hit ){ //Nichts im Weg
		
			if(debug_print)print(this.name+": dec_1 | " + (timer - timer_look_2_target));
			turn_left = false;
			turn_right = false;

			if(sphere_way_see_target){
				if(debug_print)print(this.name+": dec_1.1 | direct_way_possible!");
				direct_way_possible = true; //Spieler in Sicht mit breitem Weg!
			}

		}else if( sphere_way_see_target && !front_hit ){ //Spieler in Sicht mit breitem Weg!

			if(debug_print)print(this.name+": dec_1.2 | direct_way_possible!");
			direct_way_possible = true;

		}else if( front_hit && left_hit && !right_45_hit ){

			if(debug_print)print(this.name+": dec_2");

			walk_speed_cap=0.25F;

			turn_right = true;
			turn_left = false;

		}else if( front_hit && right_hit && !left_45_hit ){

			if(debug_print)print(this.name+": dec_3");

			walk_speed_cap=0.25F;

			turn_left = true;
			turn_right = false;

		// }else if( left_45_hit && !front_hit && !right_45_hit && !right_hit && !left_hit ){
		}else if( left_45_hit && !front_hit && !right_45_hit && !right_hit ){

			if(debug_print)print(this.name+": dec_4");

			walk_speed_cap=0.25F;

			turn_left = false;
			turn_right = true;

		// }else if( right_45_hit && !front_hit && !left_45_hit && !right_hit && !left_hit ){
		}else if( right_45_hit && !front_hit && !left_45_hit && !left_hit ){

			if(debug_print)print(this.name+": dec_5");

			walk_speed_cap=0.25F;

			turn_right = false;
			turn_left = true;

		}else if( front_hit && !left_hit && !right_hit || left_45_hit && right_45_hit || front_hit && left_45_hit || front_hit && right_45_hit ){

			walk_speed_cap=0.15F;

			if(last_turn_decision=="right"){
				turn_right = true;
				turn_left = false;
			}else{
				turn_left = true;
				turn_right = false;
			}

			if(debug_print)if(turn_right)print(this.name+": dec_6_frontal - "+front_ob+" - turn: right");
			if(debug_print)if(turn_left)print(this.name+": dec_6_frontal - "+front_ob+" - turn: left");

		}else{

			if(debug_print)print(this.name+": no_dec");
		}

		if( !turn_right && !turn_left && timer >= timer_next_decision ){
			set_turn_decision();
		}

		if( any_relevant_hit ){
			timer_look_2_target = timer + sec_look_2_target;
			if(debug_print)print(this.name+": timer_look_2_target set to "+timer_look_2_target);
		}

		if( target_noticed ){ //Target wurde bemerkt = Search

			move_forward = true;
			act_walk_speed = hunt_speed;
			alerted = true;
			alert_time_end = timer + Random.Range(alert_time_min_sec,alert_time_max_sec+1);

			if(debug_print)print("act_walk_speed = hunt_speed = " + act_walk_speed);

			if(alert_others_by_tag){
				int alerted_others_count = alert_others_by_tag_count(); //Andere mit gleichem Tag alarmieren
				if(debug_print)print( this.name + " - alert_others_by_tag_count: " + alerted_others_count );
			}

		}else if(alerted){ //Immer noch vom Target alarmiert

			move_forward = true;
			act_walk_speed = alert_speed;

			if(debug_print)print("act_walk_speed = alert_speed = " + act_walk_speed);

			if(timer >= alert_time_end)alerted=false;

		}else{ //Target wurde nicht bemerkt oder kein Target vorhanden = Idle

			if( timer >= idle_walk_time_stop ){

				move_forward = false;

				if( (int)Random.Range(1,idle_walk_random_val) == 1 ){
					idle_walk_time = (int)Random.Range( idle_walk_time_min , idle_walk_time_max );
					idle_walk_time_stop = timer + idle_walk_time;
					move_forward = true;
				}

			}

		}

		if( target_ob && attack_see_target && !attack_possible ){ //Ziel vorhanden und Ziel direkt voraus
			attack_possible = true;
			attack_delay_end = timer + attack_delay_standard;
			if(debug_print)print(this.name+" könnte angreifen. attack_delay_end:"+attack_delay_end+" / timer: "+timer);
		}
		//##ENDE REAKTION FESTLEGEN################################

		//##START AKTION AUSLÖSEN###################################################
		if( can_attack && attack_possible && timer >= attack_delay_end ){
			move_forward = false;
			attack();
			attack_possible = false;
		}

		if( ((target_ob && (target_noticed || alerted)) && !turn_left && !turn_right && timer >= timer_look_2_target) || (target_ob && (target_noticed || alerted)) && direct_way_possible ){ //Ziel anvisieren V3

			float act_turn_speed_2_target = turn_speed_2_target;
			if(direct_way_possible)act_turn_speed_2_target = turn_speed_2_target * 2; //Falls breiter Korridor zum Gegner vorhanden, dann schneller anvisieren

			Vector3 target_site = new Vector3 (target_ob.transform.position.x , transform.position.y , target_ob.transform.position.z);
			target_rotation = Quaternion.LookRotation( target_site - transform.position );
			transform.rotation = Quaternion.Slerp(transform.rotation, target_rotation, act_turn_speed_2_target * Time.deltaTime);

		}else if( (waypoint_ob && timer >= timer_look_2_target && waypoint_mode) && (!turn_left && !turn_right) ){ //Waypoint anvisieren

			Vector3 waypoint_site = new Vector3 (waypoint_ob.transform.position.x , transform.position.y , waypoint_ob.transform.position.z);
			Quaternion waypoint_rotation = Quaternion.LookRotation( waypoint_site - transform.position );
			transform.rotation = Quaternion.Slerp(transform.rotation, waypoint_rotation, turn_speed_2_target * Time.deltaTime);

		}else if( turn_right && !turn_left ){ //Rechts abbiegen

		 	if(debug_print)print(this.name+": turn_right");

			last_turn_decision = "right";
			int target_look = (int)transform.eulerAngles.y + degrees;
			target_rotation = Quaternion.Euler(0, target_look, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target_rotation, turn_speed * Time.deltaTime);

		}else if( !turn_right && turn_left ){ //Links abbiegen

			if(debug_print)print(this.name+": turn_left");
			last_turn_decision = "left";
			int target_look = (int)transform.eulerAngles.y - degrees;
			target_rotation = Quaternion.Euler(0, target_look, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target_rotation, turn_speed * Time.deltaTime);

		}

		if(move_forward){ //Vorwärts bewegen
			// transform.localPosition += transform.forward * Time.deltaTime * (act_walk_speed * walk_speed_cap); //Bewegung ohne einen Rigidbody zu benutzen
			var moveAmount = (act_walk_speed * walk_speed_cap) * Time.deltaTime;
			npc_rb.MovePosition( transform.position + (transform.forward * moveAmount) );
		}

		//#######################
		if( move_forward && alerted && !target_noticed ){

			set_status("notice","walk_fast");
			if(debug_print)print(this.name + ": move forward fast alerted and target not noticed. (act_walk_speed * walk_speed_cap) * Time.deltaTime = " + (act_walk_speed * walk_speed_cap) * Time.deltaTime);

		}else if( move_forward && alerted && target_noticed ){

			set_status("notice","run");

		}else if( move_forward && !alerted && !target_noticed ){

			set_status("idle","walk");
			if(debug_print)print(this.name + ": move forward not alerted and target not noticed. (act_walk_speed * walk_speed_cap) * Time.deltaTime = " + (act_walk_speed * walk_speed_cap) * Time.deltaTime);

		}else if( !move_forward && !alerted && !target_noticed ){

			set_status("idle","stand");

		}else if( move_forward && waypoint_mode ){

			set_status("idle","walk");
			if(debug_print)print(this.name + ": move forward in waypoint mode");
		
		}else if( !move_forward && waypoint_mode ){

			set_status("idle","stand");
		
		}else if( !move_forward ){

			set_status(my_status_cluster,"stand");

			if(debug_print){
				print(this.name + ": #################################");
				print(this.name + ": Laufe nicht?!");
				print(this.name + ": move_forward:"+move_forward);
				print(this.name + ": alerted:"+alerted);
				print(this.name + ": waypoint_mode:"+waypoint_mode);
				print(this.name + ": target_noticed:"+target_noticed);
				print(this.name + ": #################################");
			}

		}else{

			set_status(my_status_cluster,"stand");

			if(debug_print){
				print(this.name + ": #################################");
				print(this.name + ": unbekannter Zustand!");
				print(this.name + ": move_forward:"+move_forward);
				print(this.name + ": alerted:"+alerted);
				print(this.name + ": waypoint_mode:"+waypoint_mode);
				print(this.name + ": target_noticed:"+target_noticed);
				print(this.name + ": #################################");
			}

		}
		//##ENDE AKTION AUSLÖSEN###################################################

		timer+= Time.deltaTime;

	}

	void set_status(string new_status_cluster, string new_status_anim){

		if(my_status_cluster==null){
			my_status_cluster = new_status_cluster;
			if(debug_print)print(this.name + " | " + System.Math.Round(timer,1) + ": Erststatus Cluster " + my_status_anim + " gesetzt.");
		}else if( my_status_cluster != new_status_cluster ){
			
			my_status_cluster = new_status_cluster;

			if( new_status_cluster == "notice"){

				if(sounds_active)play_sound(notice_sound,1.0F);

			}else if( new_status_cluster == "idle"){

				if(sounds_active)play_sound(idle_sound,1.0F);

			}
			if(debug_print)print(this.name + " | " + System.Math.Round(timer,1) + ": Statuswechsel von Cluster " + my_status_anim + " auf Cluster " + new_status_cluster + " gesetzt.");

		}

		if( my_status_anim==null || my_status_anim != new_status_anim ){

			if(my_status_anim==null){
				if(debug_print)print(this.name + " | " + System.Math.Round(timer,1) + ": Erststatus Anim " + new_status_anim + " gesetzt.");
			}else{
				if(debug_print)print(this.name + " | " + System.Math.Round(timer,1) + ": Statuswechsel von Anim " + my_status_anim + " auf Anim " + new_status_anim + " bei gesetzt.");
			}

			my_status_anim = new_status_anim;

			if(debug_print)print(this.name + ": #################################");
			if(debug_print)print(this.name + ": Changed Animimation State");
			if(debug_print)print(this.name + ": new_status_anim = " + new_status_anim);
			if(debug_print)print(this.name + ": new_status_cluster = " + new_status_cluster);
			if(debug_print)print(this.name + ": #################################");

			if(animations_active){
				if( new_status_anim == "stand"){
					anim.SetBool(anim_idle_state_name,true);
					anim.SetBool(anim_walk_state_name,false);
					anim.SetBool(anim_run_state_name,false);
				}else if( new_status_anim == "walk" || new_status_anim == "walk_fast"){
					anim.SetBool(anim_idle_state_name,false);
					anim.SetBool(anim_walk_state_name,true);
					anim.SetBool(anim_run_state_name,false);
				}else if( new_status_anim == "run"){
					anim.SetBool(anim_idle_state_name,false);
					anim.SetBool(anim_walk_state_name,false);
					anim.SetBool(anim_run_state_name,true);
				}else{
					if(debug_print)print(this.name + ": #################################");
					if(debug_print)print(this.name + ": Missing Animimation State");
					if(debug_print)print(this.name + ": new_status_anim = " + new_status_anim);
					if(debug_print)print(this.name + ": new_status_cluster = " + new_status_cluster);
					if(debug_print)print(this.name + ": #################################");
				}
			}

		}

	}

	void anim_sound(string soundname){
		// print("soundname:"+soundname);
		if(soundname=="leg"){
			int zufall_sound = Random.Range(1,3);
			if(zufall_sound==1 && sounds_active)play_sound(leg_1_sound,0.25F);
			if(zufall_sound==2 && sounds_active)play_sound(leg_2_sound,0.25F);
		}else if(soundname=="step"){
			int zufall_sound = Random.Range(1,3);
			if(zufall_sound==1 && sounds_active)play_sound(step_1_sound,1.0F);
			if(zufall_sound==2 && sounds_active)play_sound(step_2_sound,1.0F);
		}else if(soundname=="idle"){
			if(sounds_active)play_sound(idle_robot_sound,1.0F);
		}
	}

	void play_sound(AudioClip sound_file, float vol){

		// audio_source.clip = sound_file;
		// audio_source.volume = vol;
		audio_source.loop = false;
		audio_source.pitch = Random.Range(0.75F,1.25F);
		// audio_source.Play();
		audio_source.PlayOneShot(sound_file, vol);

	}

	public void set_alerted(string alert_partner){
		
		if(!alerted){
			alerted = true;
			alert_time_end = timer + Random.Range(alert_time_min_sec,alert_time_max_sec+1);
			if(debug_print)print(this.name+": Ich wurde von " + alert_partner + " bis " + alert_time_end + " (" + timer + ") alarmiert!");
		}

	}

	int alert_others_by_tag_count(){

		GameObject[] others = GameObject.FindGameObjectsWithTag( transform.tag );

		Vector3 position = transform.position;
		int alerted_count = 0;

		foreach (GameObject other in others){

			float dist_2_other = Vector3.Distance(other.transform.position, transform.position);

			if ( other != this.gameObject && dist_2_other <= alert_others_by_tag_range){
				other.GetComponent<ai_walker>().set_alerted(this.name);
				alerted_count++;
				if(debug_print)print(this.name+": Ich habe "+other.name+" alarmiert!");
			}

		}

		return alerted_count;

	}

	string set_turn_decision(){

		if( (int)Random.Range(1, 3) == 1 ){
			if( !right_hit ){
				last_turn_decision = "right";
			}else{
				last_turn_decision = "left";
			}
		}else{
			if( !left_hit ){
				last_turn_decision = "left";
			}else{
				last_turn_decision = "right";
			}
		}

		if(debug_print)print(this.name+": decision_made");
		timer_next_decision = timer + sec_next_decision_after;
		return last_turn_decision;

	}

	GameObject near_target(string target_tag_text){

		GameObject[] targets = GameObject.FindGameObjectsWithTag(target_tag_text);
		float last_dist=-1;
		
		GameObject neareast_target=null;

		foreach(GameObject target_cur in targets){

			if( Vector3.Distance(target_cur.transform.position, transform.position) < last_dist || last_dist==-1 ){
				last_dist = Vector3.Distance(target_cur.transform.position, transform.position);
				neareast_target = target_cur;
			}

		}

		if(neareast_target && last_dist <= alert_search_range){ //Ziel innerhalb Alarmierungs-Range, also AI on und Zielvorgabe
			
			ai_active = true;
			return neareast_target;

		}else if(neareast_target && last_dist >= ai_deactivation_range){ //Ziel außerhalb AI-Range, also AI off
			
			ai_active = false;
			return null;

		}else if(neareast_target && last_dist <= ai_activation_range){ //Ziel außerhalb Alarmierungs-Range, aber in AI-Range, also AI on und keine Zielvorgabe
			
			ai_active = true;
			return null;

		}else if(!neareast_target){

			ai_active = true;
			return null;

		}else{ //Ziel innerhalb Grenzregion von ai_activation_range & ai_deactivation_range
			return null;
		}
		
	}

	GameObject next_waypoint(string waypoint_tag_text, string waypoint_prefix_text, string last_waypoint_name_text){
	
		GameObject cur_waypoint = null;

		if(last_waypoint_name==""){
			cur_waypoint = get_nearest_wp(waypoint_tag_text);
		}else{
			cur_waypoint = get_next_wp(last_waypoint_name,waypoint_prefix_text);
		}

		if(cur_waypoint)last_waypoint_name = cur_waypoint.name;
		return cur_waypoint;
		
	}

	GameObject get_nearest_wp(string waypoint_tag_text){

		GameObject[] waypoints = GameObject.FindGameObjectsWithTag(waypoint_tag);
		float last_dist=-1;
		
		GameObject neareast_waypoint=null;

		foreach(GameObject cur_waypoint in waypoints){

			if( Vector3.Distance(cur_waypoint.transform.position, transform.position) < last_dist || last_dist==-1 ){
				last_dist = Vector3.Distance(cur_waypoint.transform.position, transform.position);
				neareast_waypoint = cur_waypoint;
			}

		}

		return neareast_waypoint;

	}

	GameObject get_next_wp(string last_waypoint_name_text, string waypoint_prefix_text){

		int next_waypoint_number = int.Parse(last_waypoint_name_text.Replace(waypoint_prefix_text,""));

		GameObject next_waypoint = GameObject.Find(waypoint_prefix_text + (next_waypoint_number + 1) );

		if(next_waypoint == null)next_waypoint = GameObject.Find(waypoint_prefix_text + "1" );

		return next_waypoint;

	}

	void attack(){
		
		if( timer >= next_attack_time ){

			GameObject bullet;
			bullet = Instantiate( bullet_ob , bullet_spawn.transform.position , bullet_spawn.transform.rotation ) as GameObject;
			bullet.GetComponent<bullet>().set_owner(this.gameObject);
			if(sounds_active)play_sound(shot_sound,1.0F);
			next_attack_time = timer + attack_reload_sec + attack_delay_standard;

		}

	}


	void OnDrawGizmos(){

		if(debug_gizmos){
		
			Gizmos.color = Color.yellow;
			if(right_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), right * left_right_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (right * left_right_length)) , ((transform.up*-1) * down_length) );
			//################################################################
			if(right_20_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), right_20 * left_right_20_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (right_20 * left_right_20_length)) , ((transform.up*-1) * down_length) );
			//################################################################
			if(right_45_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), right_45 * left_right_45_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (right_45 * left_right_45_length )) , ((transform.up*-1) * down_length) );
			//################################################################
			if(front_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), transform.forward * front_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (transform.forward * front_length )) , ((transform.up*-1) * down_length) );
			//################################################################
			if(left_45_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), left_45 * left_right_45_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (left_45 * left_right_45_length)) , ((transform.up*-1) * down_length) );
			//################################################################
			if(left_20_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), left_20 * left_right_20_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (left_20 * left_right_20_length)) , ((transform.up*-1) * down_length) );
			//################################################################
			if(left_hit){
				Gizmos.color = Color.red;
			}else{
				Gizmos.color = Color.yellow;
			}
			Gizmos.DrawRay((transform.position + look_height_offset_v3), left * left_right_length);
			if(avoid_holes)Gizmos.DrawRay( ((transform.position + look_height_offset_v3) + (left * left_right_length)) , ((transform.up*-1) * down_length) );
			//################################################################

			Gizmos.color = Color.green;

			Gizmos.DrawWireSphere( sphere_front_far.point, sphere_front_far_size);
			if(target_ob && sphere_way_see_target)Gizmos.DrawRay((transform.position + look_height_offset_v3), ( target_ob.transform.position - (transform.position + look_height_offset_v3) ).normalized * (Vector3.Distance(target_ob.transform.position, transform.position)) );

		}

	}

}