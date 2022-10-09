using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPlayer_Fireball : Projectile_Proto
{
    // Start is called before the first frame update
    
    void Start()
    {
        Vector2 Dir = SO_Proj.P_MoveDirection;
        if(facing_left)
            Set_Motion(Vector3.Reflect(Dir,Vector3.left), SO_Proj.P_MoveSpeed);
        else
            Set_Motion(Dir, SO_Proj.P_MoveSpeed);
    }
}
