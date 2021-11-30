using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Active_Hitbox : MonoBehaviour
{
    //Represents an active hitbox. Its data is referenced by hurtboxes it overlaps with. Descriptions updated 10/3

    private SO_Hitbox HB_Data;
    private Fighter_Input OwnerScript;
    public int Owner = 0; //The player that created the hurtbox | 0 = no owner | 1 = player 1 | 2 = player 2
    private bool hit = false; //Set to true after colliding with an opposing hurtbox, usually deactivates the hitbox
    private float current_lifespan = 0; //The current time the hitbox has survived
    private bool facingRight = true;
    private bool permanant = false; //Permanant hitboxes deactivate rather than selfdestruct, to allow use in the animator
    
    //On collision with player hurtbox, check ownership.

    //Called by Fighter_Attack, assigns hitbox information to above variables right after being created
    public void InitializeHitbox(SO_Hitbox Data, Fighter_Input origin, int X, bool fRight)
    {
        HB_Data = Data;
        OwnerScript = origin;
        Owner = X;
        transform.localScale = new Vector3(Data.sizeX, Data.sizeY, 1);
        facingRight = fRight;
    }

    private void Update()
    {
        //Update duration, destroy / deactivate hitbox if it has outlived its lifespan
        current_lifespan += Time.deltaTime;
        if (current_lifespan >= HB_Data.HB_lifespan)
        {
            if(!permanant) Destroy(gameObject); else gameObject.SetActive(false);
        }
    }

    public SO_Hitbox Get_HB_Data()
    {
        return HB_Data;
    }

    public bool Get_Facing()
    {
        return facingRight;
    }

    public bool Get_HitState()
    {
        return hit;
    }

    public Fighter_Input Get_Owner()
    {
        return OwnerScript;
    }

    public void Set_HitState(bool state)
    {
        hit = state;
        OwnerScript.Set_HitStopTime(HB_Data.hitStop);
    }
}
