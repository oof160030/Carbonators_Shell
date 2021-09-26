using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Attack : MonoBehaviour
{
    // Recieves input from Fighter Input, and executes attacks based on the inputs
    public SO_HitboxList HBox_List; //The hitboxes associated with the character
    public GameObject HitboxPrefab;
    private int owner;
    private Fighter_Input parent_FInput;
    private Animator Atk_AR;
    
    //Check player inputs against movelist, and start any valid moves
    public void CheckMoveList(bool APress)
    {
        //
        if (APress)
            Atk_AR.SetTrigger("A_Pressed");

    }


    //Create hitbox
    public void CreateHitbox(int boxCode)
    {
        //Identify the referenced hitbox - only attempt if the referenced hitbox exists
        if(HBox_List.hitbox.Length > boxCode)
        {
            SO_Hitbox HB = HBox_List.hitbox[boxCode];

            //Instantiate said hitbox
            GameObject newHitBox;
            if (parent_FInput.IsFacingRight())
                newHitBox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
            else
            {
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                newHitBox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
            }

            //Give hitbox its data
            Active_Hitbox HB_Script = newHitBox.GetComponent<Active_Hitbox>();
            HB_Script.InitializeHitbox(HB, owner);
        }
    }

    public void Initialize(int port, Fighter_Input parent, Animator X)
    {
        owner = port;
        parent_FInput = parent;
        Atk_AR = X;
    }
}
