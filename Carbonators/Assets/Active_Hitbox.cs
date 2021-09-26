using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Active_Hitbox : MonoBehaviour
{
    private SO_Hitbox HB_Data;
    public int Owner = 0; //0 = no owner | 1 = player 1 | 2 = player 2
    private bool hit = false; //Set to true when colliding with hurtbox
    private float current_lifespan = 0;
    
    //On collision with player hurtbox, check ownership.

    public void InitializeHitbox(SO_Hitbox Data, int X)
    {
        HB_Data = Data;
        Owner = X;
        transform.localScale = new Vector3(Data.sizeX, Data.sizeY, 1);
    }

    private void Update()
    {
        //Update duration, destroy if it has outlived lifespan
        current_lifespan += Time.deltaTime;
        if (current_lifespan >= HB_Data.HB_lifespan)
            Destroy(gameObject);
    }
}
