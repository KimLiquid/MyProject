/*
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeeleActive : MonoBehaviour
{
    private IControler controler;
    private IMoveSet moveSet;

    private void Awake()
    {
        controler = GetComponentInParent<IControler>();
        moveSet = GetComponentInParent<IMoveSet>();
    }

    public void ActionDelayStart()
    {
        controler.ActionDelay(true);
        //moveSet.ActionDelay(true);
    }

    public void ActionDelayEnd()
    {
        controler.ActionDelay(false);
        moveSet.MeeleAttack(0);
        //moveSet.ActionDelay(false);
    }

    public void Hit()
    {

    }
}
*/