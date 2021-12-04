using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_AnimControl : MonoBehaviour
{
    //Resets animator triggers if left for too long
    private Animator AR;
    private Anim_TriggerTimer[] Timers; 

    public void Init(Animator fighterAR)
    {
        AR = fighterAR;
        Timers = new Anim_TriggerTimer[]
        { new Anim_TriggerTimer("A_Pressed", fighterAR), 
            new Anim_TriggerTimer("Jump", fighterAR), 
            new Anim_TriggerTimer("Block", fighterAR), 
            new Anim_TriggerTimer("Unblock", fighterAR),
            new Anim_TriggerTimer("Hurt_Recover", fighterAR)};
    }

    // Update is called once per frame
    void Update()
    {
        //Cycle through each timer, and count them down (by frames) if they are active.
        foreach (Anim_TriggerTimer T in Timers)
        {
            if (T.Get_CountingDown())
                T.UpdateTimer();
        }
    }

    //Activate a specific timer, based on its name
    public void SetTrigger(string trigger)
    {
        foreach(Anim_TriggerTimer T in Timers)
        {
            if(T.Get_TriggerName() == trigger)
            {
                T.SetTrigger();
            }
        }
    }

    //Set the speed of the accompanying animator
    public void SetAnimSpeed(float speed)
    {
        AR.speed = speed;
    }
}
