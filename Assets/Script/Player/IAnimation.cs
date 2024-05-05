using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Definition;

public interface IAnimation
{
    void SendAnimMoveDir(in Vector3 moveDir/*, bool isCurrentFps*/); // 이동방향을 받아옴

    void SendAnimDodgeDir(in Vector3 dodgeDir); // 회피방향을 받아옴

    //Game.PlayerMoveSet에 일부 값을 받아옴
    void SendMoveSetState(bool isMoving, bool isDodgeTrg /*bool isSprint, bool isWalking, bool isGrounded, */,bool isJumpTrg/*, float groundDist, float gravity*/);

    //IKWeapon IsUseIK(); //state.isUseIK 값 리턴

    bool SetPos(bool statePos); //Pos 명령 , 입력 성공 여부 리턴
    
    bool SetPosAtk(byte type); //Pos 상태 이후 입력 명령 , 입력 성공 여부 리턴

    bool SetMeele(bool stateMeele); //발도/납도 명령 , 입력 성공 여부 리턴

    bool SetGun(bool stateGun); //총 뽑기/집어넣기 명령 , 입력 성공 여부 리턴

    bool SetAimGun(bool value); // 총 조준/조준해제 명령 , 입력 성공 여부 리턴

    void SetAimShoot();//bool value);

    void MeeleAttack(byte type); // 근접무기 공격 명령

}
