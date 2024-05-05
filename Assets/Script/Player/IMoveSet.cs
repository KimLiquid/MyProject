using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public interface IMoveSet
    {
        //void SendAnimationState(bool followCamToPlayer, in Vector3 rootMotionPos);

        void SetMovement(in Vector3 worldMoveDir/*, bool isSprint, bool isWalking*/); //월드 이동벡터 초기화(이동방향 지정, 걷기/스프린트 입력 전달)

        bool SetDodge(in Vector3 worldDodgeDir); //회피 명령, 성공여부 리턴

        void ResetDodge(); //회피 끝 명령
        
        void SetJump(); //점프 명령

        void StopMoving(); //이동 중지

        void KnockBack(in Vector3 force, float time); //밀쳐내기
    }