using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter_AnimControl : MonoBehaviour
{
    //Resets animator triggers if left for too long
    public int Block_AnimTimer, Unblock_AnimTimer, Light_AnimTimer, Jump_AnimTimer;
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
        /*
        if(Light_AnimTimer > 0)
        {
            Light_AnimTimer--;
            if (Light_AnimTimer == 0)
                AR.ResetTrigger("A_Pressed");
        }
        if (Jump_AnimTimer > 0)
        {
            Jump_AnimTimer--;
            if (Jump_AnimTimer == 0)
                AR.ResetTrigger("Jump");
        }
        if (Block_AnimTimer > 0)
        {
            Block_AnimTimer--;
            if (Block_AnimTimer == 0)
                AR.ResetTrigger("Block");
        }
        if (Unblock_AnimTimer > 0)
        {
            Unblock_AnimTimer--;
            if (Unblock_AnimTimer == 0)
                AR.ResetTrigger("Unblock");
        }
        */

        foreach (Anim_TriggerTimer T in Timers)
        {
            if (T.Get_CountingDown())
                T.UpdateTimer();
        }
    }

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

    public void SetAnimSpeed(float speed)
    {
        AR.speed = speed;
    }
}
