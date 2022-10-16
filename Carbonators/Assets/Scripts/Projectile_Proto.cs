using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Proto : MonoBehaviour
{
    protected SO_Projectile SO_Proj;
    protected SO_Hitbox[] P_hitboxes;
    protected bool facing_left;
    protected Fighter_Input parent_FInput;
    protected Fighter_Attack F_atk;
    protected float remaining_lifetime; //Duration to persist in seconds
    protected Rigidbody2D RB2;
    [SerializeField]
    protected GameObject HitboxPrefab; //Base model hitbox - used to spawn all other hitboxes.

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
        parent_FInput = origin;
        F_atk = parent_FInput.Get_FAtk();
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

    public GameObject CreateHitBox(int moveCode, int boxCode)
    {
        bool found = false;
        GameObject TempHitbox = null;

        foreach (SO_Hitbox H in P_hitboxes)
        {
            if (!found && H.Move_IDNum == moveCode && H.Box_IDNum == boxCode)
            {
                found = true;
                //Retreive the matching hitbox
                SO_Hitbox HB = P_hitboxes[boxCode];
                Active_Hitbox TempHB_Data;

                //Instantiate the hitbox in the expected position if facing right
                if (!facing_left)
                    TempHitbox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
                else
                {
                    //Flip the hitbox position if the fighter is facing right
                    Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                    TempHitbox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
                }

                //Give the hitbox access to the scriptable object storing its data
                TempHB_Data = TempHitbox.GetComponent<Active_Hitbox>();
                TempHB_Data.InitializeHitbox(HB, parent_FInput, F_atk.Get_PortNum(), parent_FInput.Get_FacingRight());
            }
        }
        return TempHitbox;

        /*Identify the referenced hitbox - only attempt if the referenced hitbox exists
        if (P_hitboxes.Length > boxCode)
        {
            //Retreive the matching hitbox
            SO_Hitbox HB = P_hitboxes[boxCode];
            GameObject TempHitbox;
            Active_Hitbox TempHB_Data;

            //Instantiate the hitbox in the expected position if facing right
            if (!facing_left)
                TempHitbox = Instantiate(HitboxPrefab, transform.position + HB.HB_position, Quaternion.identity, transform);
            else
            {
                //Flip the hitbox position if the fighter is facing right
                Vector3 pos_inverse = new Vector3(-HB.HB_position.x, HB.HB_position.y);
                TempHitbox = Instantiate(HitboxPrefab, transform.position + pos_inverse, Quaternion.identity, transform);
            }

            //Give the hitbox access to the scriptable object storing its data
            TempHB_Data = TempHitbox.GetComponent<Active_Hitbox>();
            TempHB_Data.InitializeHitbox(HB, parent_FInput, F_atk.Get_PortNum(), parent_FInput.Get_FacingRight());
            return TempHitbox;
        }
        */
    }
}
