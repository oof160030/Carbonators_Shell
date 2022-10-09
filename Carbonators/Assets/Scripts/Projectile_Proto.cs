using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Proto : MonoBehaviour
{
    protected SO_Projectile SO_Proj;
    protected SO_Hitbox[] P_hitboxes;
    protected bool facing_left;
    protected Fighter_Input owner;
    protected Fighter_Attack F_atk;
    protected float remaining_lifetime; //Duration to persist in seconds
    protected Rigidbody2D RB2;
    [SerializeField]
    protected GameObject prefab_hbox; //Base model hitbox - used to spawn all other hitboxes.

    //Default Initiation
    public void Init()
    {
        //Do nothing for now
    }

    public void Init(SO_Hitbox[] SO_H, bool facing, Fighter_Input origin, SO_Projectile SOP)
    {
        SO_Proj = SOP;
        P_hitboxes = new SO_Hitbox[SO_H.Length];
        System.Array.Copy(SO_H, P_hitboxes, SO_H.Length);
        
        facing_left = facing;
        owner = origin;
        F_atk = owner.Get_FAtk();
        remaining_lifetime = SOP.P_Lifetime;
        RB2 = gameObject.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        if(RB2 == null)
            RB2 = gameObject.GetComponent<Rigidbody2D>(); //Could be redundant, but just in case
    }

    /* Update is called once per frame
    void Update()
    {
        
    }
    */

    private void FixedUpdate()
    {
        //Destroy projectile after time elapses
        projectile_decay();
    }

    //Sets the projectiles motion. Intended for single use
    public void Set_Motion(Vector2 dir, float mag)
    {
        RB2.velocity = dir.normalized * mag;
    }

    public void projectile_decay()
    {
        if (remaining_lifetime > 0)
            remaining_lifetime -= Time.fixedDeltaTime*60;
        if (remaining_lifetime <= 0)
            Destroy(gameObject);
    }
}
