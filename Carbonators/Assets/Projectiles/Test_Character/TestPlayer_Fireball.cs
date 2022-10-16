using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer_Fireball : Projectile_Proto
{
    // Start is called before the first frame update
    Active_Hitbox PJ_HBox;

    void Start()
    {
        Vector2 Dir = SO_Proj.P_MoveDirection;
        if(facing_left)
            Set_Motion(Vector3.Reflect(Dir,Vector3.left), SO_Proj.P_MoveSpeed);
        else
            Set_Motion(Dir, SO_Proj.P_MoveSpeed);

        PJ_HBox = CreateHitBox(0,0).GetComponent<Active_Hitbox>();
    }

    private void Update()
    {
        if (PJ_HBox.Get_HitStatus())
            Destroy(gameObject);
    }
}
