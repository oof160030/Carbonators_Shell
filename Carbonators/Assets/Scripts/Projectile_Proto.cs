using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile_Proto : MonoBehaviour
{
    private SO_Hitbox P_hitbox;
    private bool facing_left;
    private Fighter_Input owner;
    private float remaining_lifetime; //Duration to persist in seconds
    private Rigidbody2D RB2;

    //Default Initiation
    public void Init()
    {
        //Do nothing for now
    }

    public void Init(SO_Hitbox SO_H, bool facing, Fighter_Input origin, float time)
    {
        P_hitbox = SO_H;
        facing_left = facing;
        owner = origin;
        remaining_lifetime = time;
        RB2 = gameObject.GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Destroy projectile after time elapses
        if (remaining_lifetime > 0)
            remaining_lifetime -= Time.deltaTime;
        if (remaining_lifetime <= 0)
            Destroy(gameObject);
    }
}
