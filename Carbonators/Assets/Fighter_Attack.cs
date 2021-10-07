using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Attack : MonoBehaviour
{
    // Recieves input from Fighter Input, and executes attacks based on the inputs provided. Descriptions updated 10/3

    public SO_HitboxList HBox_List; //The hitboxes the given character can create
    public GameObject HitboxPrefab; //The hitbox prefab object that is used to instantiate the hitboxes
    private int owner; //The player associated with the fighter | 1 = player 1 | 2 = player 2
    private Fighter_Input parent_FInput; //The fighter input object on the same object
    private Animator Atk_AR; //The animator tied to the fighter, controls fighting logic to a degree
    
    //References the player's inputs once a button is pressed or released, and activates a move based on the results
    public void CheckMoveList(bool APress, bool groundDown)
    {
        //
        if (APress && groundDown)
            Atk_AR.SetTrigger("A_Down_Pressed");
        else if(APress)
            Atk_AR.SetTrigger("A_Pressed");
    }


    //Creates a hitbox, based on a provided hitbox code.
    public void CreateHitbox(int boxCode)
    {
        //Identify the referenced hitbox - only attempt if the referenced hitbox exists
        if(HBox_List.hitbox.Length > boxCode)
        {
            //Retreive the matching hitbox
            SO_Hitbox HB = HBox_List.hitbox[boxCode];

            //Instantiate the hitbox, and store a reference to it 
            GameObject newHitBox;

            //Instantiate the hitbox in the expected position if facing right
            if (parent_FInput.IsFacingRight())
                newHitBox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
            else
            {
                //Flip the hitbox position if the fighter is facing right
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                newHitBox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
            }

            //Give the hitbox access to the scriptable object storing its data
            Active_Hitbox HB_Script = newHitBox.GetComponent<Active_Hitbox>();
            HB_Script.InitializeHitbox(HB, owner, parent_FInput.IsFacingRight());
        }
    }

    //When starting a match, set up the attack's owner, Fighter_Input script, and animator
    public void Initialize(int port, Fighter_Input parent, Animator X)
    {
        owner = port;
        parent_FInput = parent;
        Atk_AR = X;
    }
}
