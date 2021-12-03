﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameMGR : MonoBehaviour
{
    //Controls the game's state during each match. May also control game states outside of gameplay, but not sure.
    //For now, there are no characters or menus, and battles (should) reset after play.

    private Fighter_Input fighter1, fighter2; //Access to the two fighter methods.
    public GameObject fighterPrefab;
    public Cam_Control CC; //Access to the camera controller, for keeping players in frame.
    public TextMeshProUGUI P1_HP, P2_HP; //Access to the (placeholder) HP markers (may be moved to a seperate UI controller?)
    void Start()
    {
        //Get access to the two fighters, and sets their inital positions.
        //(Should be replaced by spawning in the 2 fighters itself)
        //GameObject[] fighters = GameObject.FindGameObjectsWithTag("Player");

        fighter1 = Instantiate(fighterPrefab).GetComponent<Fighter_Input>();
        fighter2 = Instantiate(fighterPrefab).GetComponent<Fighter_Input>();

        /*
        if (fighters[0].transform.position.x < fighters[1].transform.position.x)
        {
            fighter1 = fighters[0].GetComponent<Fighter_Input>();
            fighter2 = fighters[1].GetComponent<Fighter_Input>();
            fighter1.Set_OnRight(false); fighter2.Set_OnRight(true);
        }
        else if (fighters[0].transform.position.x > fighters[1].transform.position.x)
        {
            fighter2 = fighters[0].GetComponent<Fighter_Input>();
            fighter1 = fighters[1].GetComponent<Fighter_Input>();
            fighter2.Set_OnRight(false); fighter1.Set_OnRight(true);
        }
        */

        //Gives the fighters access to the manager instance
        fighter1.Init(this, true, fighter2); fighter2.Init(this, false, fighter1);

        CC.fighter1 = fighter1.transform;
        CC.fighter2 = fighter2.transform;

    }

    // Update is called once per frame
    void Update()
    {
        //Tell fighters their relative positions.
        bool firstOnRight = true;
        if(fighter1.transform.position.x < fighter2.transform.position.x)
        {
            fighter1.Set_OnRight(false); fighter2.Set_OnRight(true);
            firstOnRight = false;
        }
        else if (fighter1.transform.position.x > fighter2.transform.position.x)
        {
            fighter2.Set_OnRight(false); fighter1.Set_OnRight(true);
            firstOnRight = true;
        }

        //If both fighters are too far away, don't let them move apart
        if (Mathf.Abs(fighter1.transform.position.x - fighter2.transform.position.x) > 15)
        {
            fighter1.CanBackUp = false;
            fighter2.CanBackUp = false;
        }
        else
        {
            fighter1.CanBackUp = true;
            fighter2.CanBackUp = true;
        }

        //If both fighters are grounded and too close, move them away from each other.
        if (Mathf.Abs(fighter1.transform.position.x - fighter2.transform.position.x) < 2 && fighter1.Get_IsGrounded() && fighter2.Get_IsGrounded())
        {
            //If neither fighter is against the wall, move both fighters slightly
            if(fighter1.CanBackUp && fighter2.CanBackUp)
            {
                //Find midpoint between the fighters
                float midpointX = (fighter1.transform.position.x + fighter2.transform.position.x)/2.0f;
                if (fighter2.Get_OnRight())
                {
                    fighter1.transform.position = new Vector3(midpointX,fighter1.transform.position.y) + new Vector3(-1, 0);
                    fighter2.transform.position = new Vector3(midpointX, fighter1.transform.position.y) + new Vector3(1, 0);
                }
                else
                {
                    fighter1.transform.position = new Vector3(midpointX, fighter1.transform.position.y) + new Vector3(1, 0);
                    fighter2.transform.position = new Vector3(midpointX, fighter1.transform.position.y) + new Vector3(-1, 0);
                }
            }
            //If one fighter is against a wall, check which on is currently closer to the center
            else if(Mathf.Abs(fighter1.transform.position.x) < Mathf.Abs(fighter2.transform.position.x)) //fighter 1 closer
            {
                //move fighter 1 away from fighter 2
                if (fighter2.Get_OnRight())
                    fighter1.transform.position = fighter2.transform.position + new Vector3(-2, 0);
                else
                    fighter1.transform.position = fighter2.transform.position + new Vector3(2, 0);
            }
            else if (Mathf.Abs(fighter1.transform.position.x) > Mathf.Abs(fighter2.transform.position.x)) //fighter 2 closer
            {
                //move fighter 2 away from fighter 1
                if (fighter1.Get_OnRight())
                    fighter2.transform.position = fighter1.transform.position + new Vector3(-2, 0);
                else
                    fighter2.transform.position = fighter1.transform.position + new Vector3(2, 0);
            }
        }
        

    }

    public void updateHealthUI(int port, float health)
    {
        if (port == 1)
            P1_HP.text = "P1: " + health;
        else
            P2_HP.text = "P2: " + health;
    }
}
