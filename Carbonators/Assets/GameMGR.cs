using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMGR : MonoBehaviour
{
    //Controls the game's state during each match.
    //For now, there are no characters or menus, and battles reset after play.

    Fighter_Input fighter1, fighter2;
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
        else
        {
            fighter2 = fighters[0].GetComponent<Fighter_Input>();
            fighter1 = fighters[1].GetComponent<Fighter_Input>();
            fighter2.SetRightBool(false); fighter1.SetRightBool(true);
        }
            
    }

    // Update is called once per frame
    void Update()
    {
        //Tell fighters their relative positions.
        if(fighter1.transform.position.x < fighter2.transform.position.x)
        {
            fighter1.SetRightBool(false); fighter2.SetRightBool(true);
        }
        else
        {
            fighter2.SetRightBool(false); fighter1.SetRightBool(true);
        }
    }
}
