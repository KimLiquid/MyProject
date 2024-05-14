using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Game
{
    public interface IEnemyMoveSet
    {
        // void GetEnemyRoot(in Transform enemyRoot); // enemyRoot 값을 받음

        void SetMovement(in Vector3 moveDir); // 이동방향 설정

        void StopMoving(); // 이동중지 명령을 받음

        void SetMovementSpeed(float speed); // 이동속도 설정

        Vector3 DetectColCenter(); // Com.targetDetectCol.center의 값으로 리턴

        /* (bool cast, RaycastHit hit) CheckObstacle(in Vector3 dir, float maxDistance); */ // 타겟과의 방향, 최대 감지범위를 가져와 감지여부, 감지한오브젝트정보를 리턴

        bool IsUnableMove(); // 이동 불가능 상태 리턴
        (bool isChaseForwardBlocked, Vector3 chaseAvoidDir) IsChaseCheckForward(float chaseForwardCheckDist, float chaseAvoidAngleInterV); // 장애물 감지 상태 리턴

        bool IsAvoidCheckForward(float chaseForwardCheckDist, in Vector3 targetDir); // 장애물 회피
    }
}