using System.Collections;
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
        //Instnatiate and save access to the two fighters.
        fighter1 = Instantiate(fighterPrefab).GetComponent<Fighter_Input>();
        fighter2 = Instantiate(fighterPrefab).GetComponent<Fighter_Input>();

        //Initialize the two fighters, give them acces to this manager and each other
        fighter1.Init(this, true, fighter2); fighter2.Init(this, false, fighter1);

        //Give camera access to the fighters as well
        CC.Init(fighter1.transform, fighter2.transform);
    }

    // Update is called once per frame
    void Update()
    {
        //Have fighters update their relative positions.
        fighter1.UpdateRelativePositionValues(); fighter2.UpdateRelativePositionValues();

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

    public void PH_updateHealthUI(int port, float health)
    {
        if (port == 1)
            P1_HP.text = "P1: " + health;
        else
            P2_HP.text = "P2: " + health;
    }
}
