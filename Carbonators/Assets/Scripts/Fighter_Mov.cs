using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_Mov : MonoBehaviour
{
    // Controls and records fighter's movements (sets gravity, controls jumps and movement)
    private Fighter_Input parent_FInput;
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

    public void Initialize(Fighter_Input Finput)
    {
        parent_FInput = Finput;
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
        if (parent_FInput.GetState() == FighterState.NEUTRAL && grounded && yIn > -1) X = WalkSpeed * xIn;
        else if (grounded) X = 0;
        else X = RB2.velocity.x;

        //Set vertical movement - jump from ground and change grounded state, else accelerate down
        if (jump && grounded)
        {
            grounded = false; GC.overlap = false;
            Y = JumpSpeed;
        }
        else Y = RB2.velocity.y;

        Vector3 traj = new Vector3(X, Y);
        SetMovement(traj);
    }

    public void Gravity_Update()
    {
        float Y = RB2.velocity.y - (gravityAcc * Time.deltaTime);

        Vector3 traj = new Vector3(RB2.velocity.x, Y);
        SetMovement(traj);
    }

    private void SetMovement(Vector3 trajectory)
    {
        //Recieves a vertical and horizontal speed to move, in the form of a vector 2 or 3
        RB2.velocity = trajectory;
    }

    public void Damage_Launch(Vector3 trajectory, bool right)
    {
        if (right)
            RB2.velocity = trajectory;
        else
            RB2.velocity = Vector3.Reflect(trajectory, Vector3.left);
    }

    public void WallMovement(bool onRight)
    {
        if (onRight ^ RB2.velocity.x < 0)
            RB2.velocity = new Vector3(0, RB2.velocity.y);
    }
}
