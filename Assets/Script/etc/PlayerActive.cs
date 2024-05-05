using Game;
using JetBrains.Annotations;
// using Newtonsoft.Json.Bson;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class PlayerActive : MonoBehaviour
{
    private IControler controler;
    private IMoveSet moveSet;

    void Awake()
    {
        controler = GetComponentInParent<IControler>();
        moveSet = GetComponentInParent<IMoveSet>();
    }
    
    /*
    public void JumpStart()
    {
        controler.ActionDelay(
            PlayerControler.CannotAction.Pos |
            PlayerControler.CannotAction.Parry |
            PlayerControler.CannotAction.DrawPutMeele |
            PlayerControler.CannotAction.Attack); // 점프 시 Pos, 패리, 발도/납도, 공격 행동불능
    }

    public void JumpEnd() //1
    {
        controler.ActionDelay(false); 
    }

    public void DrawParry() //2
    {
        controler.ActionDelay(true);
    }

    public void PutParry() //3
    {
        controler.ActionDelay(false);
        moveSet.EndPos();
        controler.EndPosCam();
    }

    public void MeeleStart() //2
    {
        controler.ActionDelay(true);
        //moveSet.ActionDelay(true);
    }

    public void MeeleEnd() //1
    {
        controler.ActionDelay(false);
        moveSet.MeeleAttack(0);
        //moveSet.ActionDelay(false);
    }

    public void Hit()
    {

    }
    */
}
