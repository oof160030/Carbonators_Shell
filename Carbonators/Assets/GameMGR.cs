using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMGR : MonoBehaviour
{
    //Controls the game's state during each match.
    //For now, there are no characters or menus, and battles reset after play.

    Fighter_Input fighter1, fighter2;
    public Cam_Control CC;
    void Start()
    {
        //Get access to the two fighters, set their inital positions.
        GameObject[] fighters = GameObject.FindGameObjectsWithTag("Player");
        if (fighters[0].transform.position.x < fighters[1].transform.position.x)
        {
            fighter1 = fighters[0].GetComponent<Fighter_Input>();
            fighter2 = fighters[1].GetComponent<Fighter_Input>();
            fighter1.SetRightBool(false); fighter2.SetRightBool(true);
        }
        else if (fighters[0].transform.position.x > fighters[1].transform.position.x)
        {
            fighter2 = fighters[0].GetComponent<Fighter_Input>();
            fighter1 = fighters[1].GetComponent<Fighter_Input>();
            fighter2.SetRightBool(false); fighter1.SetRightBool(true);
        }
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
            fighter1.SetRightBool(false); fighter2.SetRightBool(true);
            firstOnRight = false;
        }
        else if (fighter1.transform.position.x > fighter2.transform.position.x)
        {
            fighter2.SetRightBool(false); fighter1.SetRightBool(true);
            firstOnRight = true;
        }

        //If both fighters are grounded and too close, move them away from each other.
        if(Mathf.Abs(fighter1.transform.position.x - fighter2.transform.position.x) < 2 && fighter1.IsGrounded() && fighter2.IsGrounded())
        {
            //Check which on is currently closer to the center
            if(Mathf.Abs(fighter1.transform.position.x) < Mathf.Abs(fighter2.transform.position.x)) //fighter 1 closer
            {
                //move fighter 1 away from fighter 2
                if (fighter2.IsOnRight())
                    fighter1.transform.position = fighter2.transform.position + new Vector3(-2, 0);
                else
                    fighter1.transform.position = fighter2.transform.position + new Vector3(2, 0);
            }
            else if (Mathf.Abs(fighter1.transform.position.x) > Mathf.Abs(fighter2.transform.position.x)) //fighter 2 closer
            {
                //move fighter 2 away from fighter 1
                if (fighter1.IsOnRight())
                    fighter2.transform.position = fighter1.transform.position + new Vector3(-2, 0);
                else
                    fighter2.transform.position = fighter1.transform.position + new Vector3(2, 0);
            }
        }
        //If both fighters are too far away, don't let them move apart
        if(Mathf.Abs(fighter1.transform.position.x - fighter2.transform.position.x) > 15)
        {
            fighter1.CanBackUp = false;
            fighter2.CanBackUp = false;
        }
        else
        {
            fighter1.CanBackUp = true;
            fighter2.CanBackUp = true;
        }

    }
}
