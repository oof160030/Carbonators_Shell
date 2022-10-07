using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_HitDetection : MonoBehaviour
{
    //Registers hit detection from active hitboxes, applies damage and knockback effects back to parent object.
    int owner; //1= p1, 2= p2
    Fighter_Input F_In;

    //Variables used to store data from incoming hitboxes
    SO_Hitbox hit_data;
    bool hBox_facing_right;
    Fighter_Input hBox_Owner_FInput;
    int hBox_Priority;

    public void Init(int port, Fighter_Input Finput)
    {
        owner = port;
        F_In = Finput;
        hBox_Priority = -1;
    }

    private void Update()
    {
        //Only send data if a hitbox has been stored
        if(hBox_Priority > -1)
        {
            //Send info to damage manager
            F_In.Damaged(hit_data, hBox_facing_right, hBox_Owner_FInput);
            //Reset hitbox variables
            ClearHitbox();
        }
    }

    //Receives information from a hitbox on contact
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("HitBox"))
        {
            //Note- this is also where we should check block height (overhead vs low) and possibly shield strength!
            
            //Get access to the hitbox script, proceed if owner is different from hurtbox owner and hitbox has not been activated
            Active_Hitbox HB = collision.gameObject.GetComponent<Active_Hitbox>();

            if(HB.Owner != owner && !HB.Get_HitStatus())
            {
                //Tell the hitbox it has been hit
                HB.Set_HitStatus(true);

                //Check if a higher priority move has occured - if yes, collect needed data
                SO_Hitbox TEMP_hit_data = HB.Get_HB_Data();
                if (TEMP_hit_data.HB_priority > hBox_Priority)
                {
                    //Collect all information needed
                    SaveHitbox(HB);
                }
            }
        }
    }

    //Stores the data of the last hitbox collided with. To be passed to Fighter_Input during update
    private void SaveHitbox(Active_Hitbox HB)
    {
        hit_data = HB.Get_HB_Data();
        hBox_facing_right = HB.Get_FacingRight();
        hBox_Owner_FInput = HB.Get_OwnerFInput();
        hBox_Priority = hit_data.HB_priority;
    }

    //Clears the data of the last hitbox collided with. Called right after passing data to Fighter_Input
    private void ClearHitbox()
    {
        hit_data = null;
        hBox_facing_right = true;
        hBox_Owner_FInput = null;
        hBox_Priority = -1;
    }
}
