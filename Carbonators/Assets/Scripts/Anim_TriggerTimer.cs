using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anim_TriggerTimer
{
    private string triggerName;
    private Animator playerAnimator;
    private float triggerTimer;
    private bool countingDown;

    public Anim_TriggerTimer()
    {
        countingDown = false;
    }
    public Anim_TriggerTimer(string trigger, Animator AR)
    {
        triggerName = trigger;
        playerAnimator = AR;
        countingDown = false;
        triggerTimer = 0;
    }

    public void SetTrigger()
    {
        playerAnimator.SetTrigger(triggerName);
        triggerTimer = 6;
        countingDown = true;
    }
    public void UpdateTimer()
    {
        if(triggerTimer > 0)
        {
            triggerTimer--;
            if (triggerTimer == 0)
            {
                ResetTrigger();
            }   
        } 
    }
    public void ResetTrigger()
    {
        playerAnimator.ResetTrigger(triggerName);
        triggerTimer = 0;
        countingDown = false;
    }

    public bool Get_CountingDown()
    {
        return countingDown;
    }
    public string Get_TriggerName()
    {
        return triggerName;
    }
}
