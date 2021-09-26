using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Mov : MonoBehaviour
{
    // Controls and records fighter's movements (sets gravity, controls jumps and movement)
    private Rigidbody2D RB2;
    private GroundCheck GC;
    public float WalkSpeed, JumpSpeed;
    public float gravityAcc;
    public bool grounded;
    
    void Start()
    {
        RB2 = GetComponent<Rigidbody2D>();
        GC = GetComponentInChildren<GroundCheck>();
    }

    private void Update()
    {
        grounded = GC.overlap;
    }

    public void Movement_Update(int xIn, int yIn, bool jump)
    {
        //In cases where the fighter is moving themselves, updates their movements
        float X, Y;
        //Set horizontal movement on ground, maintain velocity in air
        if (grounded) X = WalkSpeed * xIn;
        else X = RB2.velocity.x;

        //Set vertical movement - jump from ground and change grounded state, else accelerate down
        if (jump && grounded)
        {
            grounded = false; GC.overlap = false;
            Y = JumpSpeed;
        }
        else Y = RB2.velocity.y - (gravityAcc*Time.deltaTime);

        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    private void SetMovement(Vector3 trajectory)
    {
        //Recieves a vertical and horizontal speed to move, in the form of a vector 2 or 3
        RB2.velocity = trajectory;
    }

    public void WallMovement(bool onRight)
    {
        if (onRight ^ RB2.velocity.x < 0)
            RB2.velocity = new Vector3(0, RB2.velocity.y);
    }
}
