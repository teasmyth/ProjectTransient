using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitNode : ActionNode
{
    public float waitTime = 1.0f;
    private float _startTime;
    protected override void OnStart()
    {
        _startTime = Time.time;
    }

    protected override void OnStop()
    {
        
    }

    protected override State OnUpdate()
    {
       if (Time.time - _startTime > waitTime)
       {
           return State.Success;
       }
       
       return State.Running;
    }
}
