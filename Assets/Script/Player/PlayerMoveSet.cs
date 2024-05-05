// using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.Rendering.HighDefinition.Attributes;
using UnityEngine.UIElements;
using static Game.Definition;

namespace Game
{

    public class PlayerMoveSet : MonoBehaviour, IMoveSet
    {
        /*
         정의 
        */
        #region .
        [Serializable]
        public class PlayerState
        {
            public bool isMoving; //움직이고있는가
            //public bool isSprint; //달리고있는가
            //public bool isWalking; //살금살금 걷고있는가
            //public bool isGrounded; //땅에 붙어있는가
            public bool isOnSteepSlope; //등반 불가능한 경사로인가
            public bool isForwardBlocked; //전방에 장애물 존재
            public bool isOutOfControl; //제어 불가 상태
            public bool isDodgeTrg; // 회피 입력상태
            //public bool isDodge; // 회피 상태
            public bool isJumpTrg; //점프 입력상태
            public bool isJump; //점프 상태
            //public bool followCamToPlayer; //카메라가 플레이어를 따라가는가 (안 따라갈 시 플레이어가 카메라를 따라감)
        }

        [Serializable]
        public class CurrentValue
        {
            //    public Vector3 worldMoveDir;
            public Vector3 worldDodgeDir;
            public Vector3 groundNormal;
            public Vector3 groundCross;
            public Vector3 hVelocity;

            //    public Vector3 rootMotionPos; // 루트 모션으로 인한 이동 기록
            //    //public Quaternion rootMotionRot; // 루트 모션으로 인한 회전 기록

            [Space]
            public float jumpCooldown; // 남은 점프 쿨타임
            public int jumpCount; // 현재 점프횟수
            public float dodgeCooldown; // 남은 회피 쿨타임
            public float outOfControlDuration; // 행동불가 시간

            //    [Space]
            //    public float groundDist;
            public float groundSlopeAngle; //현재 바닥의 경사각
            public float groundVSlopeAngle; //수직으로 재측정한 경사각
            public float forwardSlopeAngle; //캐릭터가 바라보는 방향의 경사각
            public float slopeAccel; //경사로 가속/감속 비율

            //    [Space]
            //    public float gravity; //직접 제어하는 중력값
        }
        #endregion

        /*
         변수,프로퍼티
        */
        #region .
        private Components _components = new Components();
        //private PublicPlayerState _publicstate = new PublicPlayerState();
        [SerializeField] private CheckOption _checkOption = new CheckOption();
        [Space]
        [SerializeField] private MoveOption _moveOption = new MoveOption();
        [Space]
        [SerializeField] private PlayerState _playerState = new PlayerState();
        [Space]
        [SerializeField] private CurrentValue _currentValue = new CurrentValue();
        //[SerializeField] private AnimatorOption _animatorOption = new AnimatorOption();

        private Components Com => _components;
        private CheckOption Check => _checkOption;
        private MoveOption Move => _moveOption;
        private PlayerState State => _playerState;
        //private PublicPlayerState PublicState => _publicstate;
        private CurrentValue Value => _currentValue;
        //private AnimatorOption Anim => _animatorOption;

        private float _capRadiusDiff;
        private float _fixedDeltaTime;
        private float _castRadius; // 원기둥, 캡슐 레이캐스트 반지름

        private IAnimation Animation => GetComponent<IAnimation>();

        private Vector3 CapsuleTopCenterPoint
           => new Vector3(Com.camControler.transform.position.x, Com.camControler.transform.position.y + Com.cap.height - Com.cap.radius, Com.camControler.transform.position.z);
        private Vector3 CapsuleBottomCenterPoint
            => new Vector3(Com.camControler.transform.position.x, Com.camControler.transform.position.y + Com.cap.radius, Com.camControler.transform.position.z);
        
        private float[] _debugVar = new float[10];
        #endregion

        /*
         유니티 이벤트 
        */
        #region
        private void Awake()
        {
            InitComponents();
            InitRigidbody();
            InitCapsuleCollider();
        }

        private void FixedUpdate()
        {
            UpdateValues();

            CheckGround();
            CheckForward();

            UpdateGravity();

            CalculateMove();
            ApplyMovementToRigidbody();

            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void LateUpdate()
        {
            FollowObject();
        }
        #endregion

        /*
         초기화 
        */
        #region
        private void InitComponents()
        {
            //Com.anim = GetComponent<Animator>();
            Com.camControler = GameObject.FindWithTag("CamControler");
        }

        private void InitRigidbody()
        {
            Com.camControler.TryGetComponent(out Com.rb);
            if (Com.rb == null) Com.rb = Com.camControler.AddComponent<Rigidbody>();

            //회전은 자식 트랜스폼을 통해 직접 제어할 것이기 때문에 리자드바디 회전은 제한
            Com.rb.constraints = RigidbodyConstraints.FreezeRotation;
            Com.rb.interpolation = RigidbodyInterpolation.Interpolate;
            Com.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Com.rb.useGravity = false; //중력은 직접 제어
        }

        private void InitCapsuleCollider()
        {
            Com.camControler.TryGetComponent(out Com.cap);
            if (Com.cap == null)
            {
                Com.cap = Com.camControler.gameObject.AddComponent<CapsuleCollider>();

                //렌더러를 모두 탐색하여 높이 결정
                float maxHeight = -1f;

                //SkinnedMeshRenderer 확인
                var smrArr = GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrArr.Length > 0)
                {
                    foreach (var smr in smrArr)
                    {
                        var matrix = Matrix4x4.TRS(smr.gameObject.transform.localPosition, smr.gameObject.transform.localRotation, smr.gameObject.transform.localScale);
                        foreach (var vertex in smr.sharedMesh.vertices)
                        {
                            var realVertex = matrix.MultiplyPoint(vertex);
                            if (maxHeight < realVertex.y)
                                maxHeight = realVertex.y;
                        }
                    }
                }
                else //MeshFilter 확인
                {
                    var mfArr = GetComponentsInChildren<MeshFilter>();
                    if (mfArr.Length > 0)
                    {
                        foreach (var mf in mfArr)
                        {
                            var matrix = Matrix4x4.TRS(mf.gameObject.transform.localPosition, mf.gameObject.transform.localRotation, mf.gameObject.transform.localScale);
                            foreach (var vertex in mf.mesh.vertices)
                            {
                                var realVertex = matrix.MultiplyPoint(vertex);
                                if (maxHeight < realVertex.y)
                                    maxHeight = realVertex.y;
                            }
                        }
                    }
                }

                //캡슐 콜라이더 값 설정
                if (maxHeight <= 0) maxHeight = 1f;

                float center = maxHeight * 0.5f;

                Com.cap.height = maxHeight;
                Com.cap.center = Vector3.up * center;
                Com.cap.radius = 0.2f;
            }

            _castRadius = Com.cap.radius * 0.9f;
            _capRadiusDiff = Com.cap.radius - _castRadius + 0.05f;
        }
        #endregion

        /*
         그 외 다른 메서드 
        */
        #region 공개 메서드

        //void IMoveSet.SendAnimationState(bool followCamToPlayer, in Vector3 rootMotionPos)
        //{
        //    PublicPlayerState.followCamToPlayer = followCamToPlayer;
        //    PublicCurrentValue.rootMotionPos = rootMotionPos;
        //}

        void IMoveSet.SetMovement(in Vector3 worldMoveDir)//, bool isSprint, bool isWalking)
        {
            PublicCurrentValue.worldMoveDir = worldMoveDir;
            State.isMoving = worldMoveDir.sqrMagnitude > 0.01f;
            //State.isSprint = isSprint;
            //State.isWalking = isWalking;
        }

        bool IMoveSet.SetDodge(in Vector3 worldDodgeDir)
        {
            if (!PublicPlayerState.isGrounded) return false; // 공중에 떠있을 경우 회피행동 불가

            Value.worldDodgeDir = worldDodgeDir;
            State.isDodgeTrg = true;

            return true;
        }

        void IMoveSet.SetJump()
        {
            if (!PublicPlayerState.isGrounded && Value.jumpCount == 0) return; //첫 점프는 지면 위에서만

            //점프 쿨타임/횟수 획인
            if (Value.jumpCooldown > 0f || Value.jumpCount >= Move.maxJumpCount) return;

            if (State.isOnSteepSlope) return; //접근 불가능 경사로에서 점프 불가능

            State.isJumpTrg = true;
        }

        void IMoveSet.StopMoving()
        {
            PublicCurrentValue.worldMoveDir = Vector3.zero;
            State.isMoving = false;
            PublicPlayerState.isSprint = false;
            PublicPlayerState.isWalking = false;
        }

        void IMoveSet.KnockBack(in Vector3 force, float time)
        {
            SetOutOfControl(time);
            Com.rb.AddForce(force, ForceMode.Impulse);
        }

        void IMoveSet.ResetDodge()
        {
            PublicPlayerState.isDodge = false;
            State.isDodgeTrg = false;
            Value.dodgeCooldown = Move.dodgeCooldown;
        }

        public void SetOutOfControl(float time)
        {
            Value.outOfControlDuration = time;
            ResetJump();
        }
        #endregion

        #region 비공개 메서드

        private void ResetJump()
        {
            Value.jumpCooldown = 0f;
            Value.jumpCount = 0;
            State.isJump = false;
            State.isJumpTrg = false;
        }

        private void CheckGround() //지면 검사
        {
            PublicCurrentValue.groundDist = float.MaxValue;
            Value.groundNormal = Vector3.up;
            Value.groundSlopeAngle = 0f;
            Value.forwardSlopeAngle = 0f;

            bool cast = Physics.SphereCast(CapsuleBottomCenterPoint, _castRadius, Vector3.down,
                out var hit, Check.groundCheckDist, Check.groundLayerMask, QueryTriggerInteraction.Ignore);
            Debug.DrawRay(CapsuleBottomCenterPoint, Vector3.down, Color.green);

            PublicPlayerState.isGrounded = false;

            if (cast)
            {
                //지면 노멀벡터 초기화
                Value.groundNormal = hit.normal;

                //현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Value.groundSlopeAngle = Vector3.Angle(Value.groundNormal, Vector3.up);
                Value.forwardSlopeAngle = Vector3.Angle(Value.groundNormal, PublicCurrentValue.worldMoveDir) - 90f;

                State.isOnSteepSlope = Value.groundSlopeAngle >= Check.maxSlopeAngle;

                PublicCurrentValue.groundDist = Mathf.Max(hit.distance - _capRadiusDiff - Check.groundCheckThreshold, 0f);

                PublicPlayerState.isGrounded = (PublicCurrentValue.groundDist <= 0.0001f) && !State.isOnSteepSlope;
            }

            //월드 이동벡터 회전축
            Value.groundCross = Vector3.Cross(Value.groundNormal, Vector3.up);
        }

        private void CheckForward() //장애물 검사
        {
            bool cast = Physics.CapsuleCast(CapsuleBottomCenterPoint, CapsuleTopCenterPoint, _castRadius, PublicCurrentValue.worldMoveDir + Vector3.down * 0.1f,
                out var hit, Check.forwardCheckDist, -1, QueryTriggerInteraction.Ignore);
            Debug.DrawRay(CapsuleTopCenterPoint, PublicCurrentValue.worldMoveDir + Vector3.down * 0.1f,Color.blue);

            State.isForwardBlocked = false;
            if (cast)
            {
                float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
                State.isForwardBlocked = forwardObstacleAngle >= Check.maxSlopeAngle;
            }
        }

        private void UpdateGravity()
        {
            if (PublicPlayerState.isGrounded)
            {
                PublicCurrentValue.gravity = 0;
                Value.jumpCount = 0;
                State.isJump = false;
            }
            else
            {
                PublicCurrentValue.gravity += _fixedDeltaTime * Move.gravityAccel;
            }
        }

        private void UpdateValues()
        {
            // PlayerAnimation 에 주기적으로 상태 전송
            Animation.SendMoveSetState(State.isMoving, State.isDodgeTrg ,State.isJumpTrg);

            //점프 쿨타임 계산
            if (Value.jumpCooldown > 0f) Value.jumpCooldown -= _fixedDeltaTime;

            //회피 쿨타임 계산
            if (Value.dodgeCooldown > 0f) Value.dodgeCooldown -= _fixedDeltaTime;

            //OutOfControl 갱신
            State.isOutOfControl = Value.outOfControlDuration > 0f;

            if (State.isOutOfControl)
            {
                Value.outOfControlDuration -= _fixedDeltaTime;
                PublicCurrentValue.worldMoveDir = Vector3.zero;
            }
        }

        private void CalculateMove()
        {
            if (State.isOutOfControl)
            {
                Value.hVelocity = Vector3.zero;
                return;
            }

            //점프
            if (State.isJumpTrg && Value.jumpCooldown <= 0f)
            {
                PublicCurrentValue.gravity = Move.jumpForce;

                Value.jumpCooldown = Move.jumpCooldown;
                State.isJumpTrg = false;
                State.isJump = true;

                Value.jumpCount++;
            }

            //회피
            if (State.isDodgeTrg && Value.dodgeCooldown <= 0f)
            {
                State.isDodgeTrg = false;
                PublicPlayerState.isDodge = true;
            }

            //x,z 이동속도 계산
            //공중에서 전방이 막힌 경우 제한 (지상에서는 벽에 붙어서 이동할수있도록 허용)
            if (State.isForwardBlocked && !PublicPlayerState.isGrounded || State.isJump && PublicPlayerState.isGrounded)
            {
                Value.hVelocity = Vector3.zero;
            }
            else if (PublicPlayerState.isDodge) { } // 회피상태일경우 이동불가
            else // 이동가능한 경우 (지상이거나 전방이 막히지 않은경우)
            {
                float speed = !State.isMoving ? 0f :
                                PublicPlayerState.isWalking ? Move.speed * Move.walkCoef :
                                PublicPlayerState.isSprint ? Move.speed * Move.sprintCoef : Move.speed;

                Value.hVelocity = PublicCurrentValue.worldMoveDir * speed;
            }


            //x,z 벡터 회전
            //지상이거나 지면에 가까운 높이
            if (PublicPlayerState.isGrounded || PublicCurrentValue.groundDist < Check.groundCheckDist && !State.isJump)
            {
                if (State.isMoving && !State.isForwardBlocked)
                {
                    //경사로 인한 가속또는 감속
                    if (Move.slopeAccel > 0f)
                    {
                        bool isPlus = Value.forwardSlopeAngle >= 0f;
                        float absFsAngle = isPlus ? Value.forwardSlopeAngle : -Value.forwardSlopeAngle;
                        float accel = Move.slopeAccel * absFsAngle * 0.01111f + 1f;
                        Value.slopeAccel = !isPlus ? accel : 1f / accel;

                        Value.hVelocity *= Value.slopeAccel;
                    }

                    //벡터 회전(경사로)
                    Value.hVelocity =
                        Quaternion.AngleAxis(-Value.groundSlopeAngle, Value.groundCross) * Value.hVelocity;
                }
            }
        }

        private void ApplyMovementToRigidbody() //리자드바디 최종 속도 적용
        {
            if (State.isOutOfControl)
            {
                Com.rb.velocity = new Vector3(Com.rb.velocity.x, PublicCurrentValue.gravity, Com.rb.velocity.z);
                return;
            }

            if (PublicPlayerState.isDodge)
            {
                Com.rb.velocity = (Value.worldDodgeDir * Move.speed) + (Vector3.up * PublicCurrentValue.gravity) /*+ PublicCurrentValue.rootMotionPos*/;
                return;
            }

            Com.rb.velocity = Value.hVelocity + (Vector3.up * PublicCurrentValue.gravity) + PublicCurrentValue.rootMotionPos;
            
        }

        private void FollowObject()
        {
            if(PublicPlayerState.followCamToPlayer)
                Com.camControler.transform.position = transform.position;
            else
                transform.position = Com.camControler.transform.position;
        }
        #endregion
    }
}


