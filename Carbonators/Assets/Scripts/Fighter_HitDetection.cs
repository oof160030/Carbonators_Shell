using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_HitDetection : MonoBehaviour
{
    //Registers hit detection from active hitboxes, applies damage and knockback effects back to parent object.
    int owner; //1= p1, 2= p2
    Fighter_Input F_In;

    //Receives information from a hitbox on contact
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("HitBox"))
        {
            //Note- this is also where we should check block height (overhead vs low) and possibly shield strength!
            
            //Get acces to the hitbox script, proceed if owner is different from hurtbox owner and hitbox has not been activated
            Active_Hitbox HB = collision.gameObject.GetComponent<Active_Hitbox>();

            if(HB.Owner != owner && !HB.Get_HitState())
            {
                //Tell the hitbox it has been hit
                HB.Set_HitState(true);

                //Collect all information needed
                SO_Hitbox hit_data = HB.Get_HB_Data();
                bool hBox_facing_right = HB.Get_Facing();

                //Send info to damage manager
                F_In.Damaged(hit_data, hBox_facing_right, HB.Get_Owner());
            }
        }
    }

    public void Init(int port, Fighter_Input Finput)
    {
        owner = port;
        F_In = Finput;
    }
}
