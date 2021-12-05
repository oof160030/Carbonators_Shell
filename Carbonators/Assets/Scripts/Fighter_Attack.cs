using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Attack : MonoBehaviour
{
    // Recieves input from Fighter Input, and executes attacks based on the inputs provided. Descriptions updated 10/3

    public SO_HitboxList HBox_List; //The hitboxes the given character can create
    public GameObject HitboxPrefab; //The hitbox prefab object that is used to instantiate new hitboxes

    private GameObject MyHitbox; //Gameobject access to the dedicated player hitbox
    [SerializeField]
    private Active_Hitbox MyHB_Data; //The hitbox script of the dedicated player hitbox
    private GameObject TempHitbox; //Gameobject access to the temporary hitbox spawned
    private Active_Hitbox TempHB_Data;

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
        MyHitbox = MyHB_Data.gameObject;

        MyHB_Data.Init(port, true, parent, this);
        MyHitbox.SetActive(false);

        F_AC = AC;
    }

    //References the player's inputs once a button is pressed or released, and activates a move based on the results
    public void CheckMoveList(bool AIsPressed, float ATime, bool BIsPressed, float BTime)
    {
        if (AIsPressed && ATime == 0)
        {
            F_AC.SetTrigger("A_Pressed");
        }
        if (BIsPressed && BTime == 0)
            F_AC.SetTrigger("B_Pressed");
    }

    /// <summary>
    /// Transfer attack request from animator to FighterInput, (to change to attack or neutral state)
    /// </summary>
    /// <param name="state"></param>
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

            //Instantiate the hitbox in the expected position if facing right
            if (parent_FInput.Get_FacingRight())
                TempHitbox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
            else
            {
                //Flip the hitbox position if the fighter is facing right
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                TempHitbox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
            }

            //Give the hitbox access to the scriptable object storing its data
            TempHB_Data = TempHitbox.GetComponent<Active_Hitbox>();
            TempHB_Data.InitializeHitbox(HB, parent_FInput, owner, parent_FInput.Get_FacingRight());
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

            //NOTE: Position will be determined by the animator in this case

            //Give the hitbox access to the scriptable object storing its data
            MyHitbox.SetActive(true);
            MyHB_Data.InitializeHitbox(HB, parent_FInput, owner, parent_FInput.Get_FacingRight());

            //TEMP: Instantiate the hitbox in the expected position if facing right
            if (parent_FInput.Get_FacingRight())
                MyHitbox.transform.position = transform.position + HB.HB_position;
            else
            {
                //Flip the hitbox position if the fighter is facing right
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                MyHitbox.transform.position = transform.position + pos_inverse;
            }
        }
    }

    public void DisableHitbox()
    {
        if (MyHitbox.activeInHierarchy)
            MyHB_Data.DisableHitbox();
        if (TempHitbox)
        {
            TempHB_Data.DisableHitbox();
            TempHB_Data = null;
            TempHitbox = null;
        }    
    }
}
