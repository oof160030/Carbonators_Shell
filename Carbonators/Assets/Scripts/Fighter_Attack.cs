using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Attack : MonoBehaviour
{
    // Recieves input from Fighter Input, and executes attacks based on the inputs provided. Descriptions updated 10/3

    public SO_HitboxList HBox_List; //The hitboxes the given character can create
    public GameObject HitboxPrefab; //The hitbox prefab object that is used to instantiate new hitboxes
    private GameObject MyHitbox; //Gameobject access to the dedicated player hitbox
    private Active_Hitbox MyHB_Data; //The hitbox script of the dedicated player hitbox
    private int owner; //The player associated with the fighter | 1 = player 1 | 2 = player 2
    private Fighter_Input parent_FInput; //The fighter input object on the same object
    //private Animator Atk_AR; //The animator tied to the fighter, controls fighting logic to a degree
    private Fighter_AnimControl F_AC; //Access to the animator controller, which contributes to state logic

    //When starting a match, set up the attack's owner, Fighter_Input script, and animator
    public void Init(int port, Fighter_Input parent, Fighter_AnimControl AC)
    {
        owner = port;
        parent_FInput = parent;

        MyHB_Data = GetComponentInChildren<Active_Hitbox>();
        MyHitbox = MyHB_Data.gameObject; MyHB_Data.Owner = port;
        MyHitbox.SetActive(false);

        F_AC = AC;
    }

    //References the player's inputs once a button is pressed or released, and activates a move based on the results
    public void CheckMoveList(bool APress, bool groundDown)
    {
        if (APress)
        {
            F_AC.SetTrigger("A_Pressed");
        }
    }

    //Initate Attack called by animator at start of attack animations, to lock the player into the attack
    public void InitiateAttack(int state)
    {
        //Request Fighter Input change to attack state
        parent_FInput.SetAttackState(state == 1);
    }

    /// <summary> Creates a hitbox, based on a provided hitbox code. </summary>
    /// <param name="boxCode"></param>
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
            if (parent_FInput.Get_FacingRight())
                newHitBox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
            else
            {
                //Flip the hitbox position if the fighter is facing right
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                newHitBox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
            }

            //Give the hitbox access to the scriptable object storing its data
            Active_Hitbox HB_Script = newHitBox.GetComponent<Active_Hitbox>();
            HB_Script.InitializeHitbox(HB, parent_FInput, owner, parent_FInput.Get_FacingRight());
        }
    }

    /// <summary> Activates the player's dedicated hitbox, and gives it parameters based on a provided hitbox code. </summary>
    /// <param name="boxCode"></param>
    public void ActivateHitbox(int boxCode)
    {
        //Identify the referenced hitbox - only attempt if the referenced hitbox exists
        if (HBox_List.hitbox.Length > boxCode)
        {
            //Retreive the matching hitbox
            SO_Hitbox HB = HBox_List.hitbox[boxCode];

            //NOTE: Position is determined by the animator in this case

            //Give the hitbox access to the scriptable object storing its data
            MyHitbox.SetActive(true);
            MyHB_Data.InitializeHitbox(HB, parent_FInput, owner, parent_FInput.Get_FacingRight());
        }
    }
}
