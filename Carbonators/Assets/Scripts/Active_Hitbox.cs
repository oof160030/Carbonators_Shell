using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Active_Hitbox : MonoBehaviour
{
    //Represents an active hitbox. Its data is referenced by hurtboxes it overlaps with. Descriptions updated 10/3

    private SO_Hitbox HB_Data;
    private Fighter_Input Parent_FIn;
    private Fighter_Attack Parent_FAtk;
    public int Owner = 0; //The player that created the hurtbox | 0 = no owner | 1 = player 1 | 2 = player 2
    private bool hit = false; //Set to true after colliding with an opposing hurtbox, usually deactivates the hitbox
    private float current_lifespan = 0; //The current time the hitbox has survived
    private bool facingRight = true;
    private bool permanant = false; //Permanant hitboxes deactivate rather than selfdestruct, to allow use in the animator
    
    //Initialization for the permanant Hitbox Object
    public void Init(int ownerNum, bool perm, Fighter_Input FIN, Fighter_Attack FATK)
    {
        Owner = ownerNum;
        permanant = perm;
        Parent_FIn = FIN;
        Parent_FAtk = FATK;
        Debug.Log("Initialized Fighter" + ownerNum + " Hitbox");
    }
    
    //Called by Fighter_Attack, assigns hitbox information to above variables right after being created
    public void InitializeHitbox(SO_Hitbox Data, Fighter_Input origin, int X, bool fRight)
    {
        HB_Data = Data;
        Parent_FIn = origin;
        if (!Parent_FAtk)
            Parent_FAtk = origin.Get_FAtk();
        Owner = X;
        transform.localScale = new Vector3(Data.sizeX, Data.sizeY, 1);
        facingRight = fRight;

        current_lifespan = 0;
        hit = false;
    }

    private void FixedUpdate()
    {
        current_lifespan += Time.fixedDeltaTime*60;
        if (current_lifespan >= HB_Data.HB_lifespan)
        {
            Parent_FAtk.DisableHitbox();
        }
    }

    public void DisableHitbox()
    {
        if (!permanant)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
            
    }

    public SO_Hitbox Get_HB_Data()
    {
        return HB_Data;
    }

    public bool Get_FacingRight()
    {
        return facingRight;
    }

    public bool Get_HitStatus()
    {
        return hit;
    }

    public Fighter_Input Get_OwnerFInput()
    {
        return Parent_FIn;
    }

    public void Set_HitStatus(bool state)
    {
        hit = state;
        if(hit)
            Parent_FIn.Set_HitStopTime(HB_Data.hitStop);
    }
}
