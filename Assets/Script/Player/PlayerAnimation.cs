using Game;
// using Newtonsoft.Json.Bson;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.WebApi;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static Game.Definition;
using static UnityEngine.Rendering.DebugUI;

namespace Game
{
    public class PlayerAnimation : MonoBehaviour, IAnimation
    {
        #region 정의

        [Serializable]
        public class PlayerState
        {
            [HideInInspector] public bool isMoving; //움직이고있는가
                                                    //[HideInInspector] public bool isSprint; //달리고있는가
                                                    //[HideInInspector] public bool isWalking; //살금살금 걷고있는가

            public bool isJumpTrg; //점프 입력상태
            public bool isDodgeTrg; //회피 입력상태
            public bool isPos; //Pos 상태인가
            public bool isDrawMeele; //발도 상태인가
            public bool isDrawGun; //총을 뽑고있는 상태인가
            public bool isAim; // 조준 상태인가
            public bool isShoot; //사격상태인가
            public bool isAttack; //공격상태인가
                                  //public IKWeapon useIK; //IK 사용상태
                                  //public ActionConst isActionConst;
            public byte parryType; //패리타입(상단/중단/하단)
            public byte attackType; //공격타입
        }

        //[Serializable]
        //public class CurrentValue
        //{
        //    public Vector3 rootMotionPos; // 루트 모션으로 인한 이동 기록
        //    [HideInInspector] public float groundDist; // 바닥과의 거리
        //    [HideInInspector] public float gravity; // 중력

        //    [Space]
        //    public ActionConst ActionConst;
        //}


        #endregion

        #region 변수/프로퍼티
        private Components _components = new Components();
        //private PublicPlayerState _publicstate = new PublicPlayerState();
        private PublicCurrentValue _currentValue = new PublicCurrentValue();
        [SerializeField] private AnimatorOption _animatorOption = new AnimatorOption();
        [Space]
        [SerializeField] private PlayerState _playerState = new PlayerState();

        private Components Com => _components;
        private AnimatorOption Anim => _animatorOption;
        private PublicCurrentValue Value => _currentValue;
        private PlayerState State => _playerState;
        //private PublicPlayerState PublicState => _publicstate;

        private float _moveX;
        private float _moveZ;

        private IControler Controler => Com.camControler.GetComponent<IControler>();
        private IMoveSet MoveSet => GetComponent<IMoveSet>();
        #endregion

        #region 기본 이벤트
        private void Awake()
        {
            InitComponents();
        }

        private void FixedUpdate()
        {
            //MoveSet.SendAnimationState(PublicPlayerState.followCamToPlayer, PublicCurrentValue.rootMotionPos);

            UpdateAnimeParams();
            AnimatorEvent();
        }
        #endregion

        #region 초기화
        public void InitComponents()
        {
            Com.anim = GetComponent<Animator>();
            Com.camControler = GameObject.FindWithTag("CamControler");
        }
        #endregion

        #region 공개 메서드

        //IKWeapon IAnimation.IsUseIK() => PublicPlayerState.useIK;

        void IAnimation.SendMoveSetState(bool isMoving, bool isDodgeTrg, bool isJumpTrg)
        {
            State.isMoving = isMoving;
            //PublicPlayerState.isSprint = isSprint;
            //PublicPlayerState.isWalking = isWalking;
            //PublicPlayerState.isGrounded = isGrounded;
            State.isJumpTrg = isJumpTrg;
            State.isDodgeTrg = isDodgeTrg;
            //PublicCurrentValue.groundDist = groundDist;
            //PublicCurrentValue.gravity = gravity;
        }

        void IAnimation.SendAnimMoveDir(in Vector3 moveDir)
        {
            //if (Value.ActionConst.HasFlag(PlayerControler.ActionConst.Move)) return; // 이동 불가능 상태일때 이동관련 애니메이션 작동불가
            if (PublicPlayerState.isDodge) return; // 회피 상태일때는 아래껄 사용함

            float x, z;
            if (PublicPlayerState.isCurrentFps)
            {
                x = moveDir.x;
                z = moveDir.z;

                if (PublicPlayerState.isWalking)
                {
                    x *= 0.5f;
                    z *= 0.5f;
                }
                if (PublicPlayerState.isSprint)
                {
                    x *= 2f;
                    z *= 2f;
                }
            }
            else
            {
                x = 0f;
                z = moveDir.sqrMagnitude > 0f ? 1f : 0f;

                if (PublicPlayerState.isWalking)
                    z *= 0.5f;
                if (PublicPlayerState.isSprint)
                    z *= 2f;
            }

            //보간
            const float LerpSpeed = 0.25f;
            _moveX = Mathf.Lerp(_moveX, x, LerpSpeed);
            _moveZ = Mathf.Lerp(_moveZ, z, LerpSpeed);

            if (PublicPlayerState.isActionConst.HasFlag(ActionConst.Move))
            {
                _moveX = 0; _moveZ = 0;
            }
        }

        void IAnimation.SendAnimDodgeDir(in Vector3 dodgeDir)
        {
            float x, z;
            if (PublicPlayerState.isCurrentFps)
            {
                x = dodgeDir.x;
                z = dodgeDir.z;
            } 
            else
            {
                x = 0f;
                z = dodgeDir.sqrMagnitude > 0f ? 1f : 0f;
            }

            _moveX = x;
            _moveZ = z;
        }

        bool IAnimation.SetPos(bool statePos)
        {
            //공중에 떠있는 상태가아니고 Pos 사용불가능 상태일때
            if (!PublicPlayerState.isGrounded || PublicPlayerState.isActionConst.HasFlag(ActionConst.Pos)) return false;

            State.isPos = statePos;

            return true;
        }

        bool IAnimation.SetPosAtk(byte type)
        {
            switch (type)
            {
                case 0:
                    State.parryType = 1;
                    break;

                case 1:
                    State.parryType = 3;
                    break;

                case 2:
                    State.parryType = 2;
                    break;
            }
            return true;
        }

        bool IAnimation.SetMeele(bool stateMeele)
        {
            //떠있지않고 발도/납도 불가능 상태가 아닐때만
            if (!PublicPlayerState.isGrounded || PublicPlayerState.isActionConst.HasFlag(ActionConst.DrawPutMeele)) return false;

            State.isDrawMeele = stateMeele;

            return true;
        }

        bool IAnimation.SetGun(bool stateGun)
        {
            //떠있지않고 총을 뽑거나 집어넣을수 없는 상태가 아닐때만
            if (!PublicPlayerState.isGrounded || PublicPlayerState.isActionConst.HasFlag(ActionConst.DrawPutGun)) return false;

            State.isDrawGun = stateGun;

            return true;
        }

        bool IAnimation.SetAimGun(bool value)
        {
            //떠있지않고 총을 조준할수 없는 상태가 아닐때만
            if (!PublicPlayerState.isGrounded || PublicPlayerState.isActionConst.HasFlag(ActionConst.AimGun)) return false;

            State.isAim = value;

            return true;
        }

        void IAnimation.SetAimShoot()
        {
            State.isShoot = true;
        }

        void IAnimation.MeeleAttack(byte type)
        {
            State.attackType = type;
            State.isAttack = true;
        }
        #endregion

        #region 비공개 메서드
        private void UpdateAnimeParams() //에니메이션
        {
            Com.anim.SetFloat(Anim.paramMoveX, _moveX);
            Com.anim.SetFloat(Anim.paramMoveZ, _moveZ);
            Com.anim.SetFloat(Anim.paramDistY, PublicCurrentValue.groundDist);
            Com.anim.SetBool(Anim.paramDownGravity, PublicCurrentValue.gravity <= 0);
            Com.anim.SetBool(Anim.paramisGround, PublicPlayerState.isGrounded);
            Com.anim.SetBool(Anim.paramPos, State.isPos);
            Com.anim.SetBool(Anim.paramDrawMeele, State.isDrawMeele);
            Com.anim.SetBool(Anim.paramDrawGun, State.isDrawGun);

            if (State.isJumpTrg) Com.anim.SetTrigger(Anim.paramJump);
            if (State.isDodgeTrg) Com.anim.SetTrigger(Anim.paramDodge);

            Com.anim.SetBool(Anim.paramAim, State.isAim);
            Com.anim.SetBool(Anim.paramShoot, State.isShoot);
            Com.anim.SetBool(Anim.paramAttack, State.isAttack);
            Com.anim.SetFloat(Anim.paramMeeleAtkType, State.attackType);

            switch (State.parryType)
            {
                case 1:
                    Com.anim.SetTrigger(Anim.paramHParry);
                    Com.anim.SetBool(Anim.paramParryTrg, true);
                    State.parryType = 0;
                    break;

                case 2:
                    Com.anim.SetTrigger(Anim.paramMParry);
                    Com.anim.SetBool(Anim.paramParryTrg, true);
                    State.parryType = 0;
                    break;

                case 3:
                    Com.anim.SetTrigger(Anim.paramLParry);
                    Com.anim.SetBool(Anim.paramParryTrg, true);
                    State.parryType = 0;
                    break;
            }

            PublicCurrentValue.rootMotionPos = Com.anim.deltaPosition;
        }

        private void AnimatorEvent()
        {
            //PublicPlayerState.isActionConst = Controler.IsActionConst();

            if (Com.anim)
            {
                #region 기본 움직임
                    #region 점프
                    if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartJump")) // 점프가 시작되었을때
                    {
                        PublicPlayerState.isActionConst =
                            ActionConst.All ^
                            ActionConst.ChangeCamera ^
                            ActionConst.CameraZoom ^
                            ActionConst.Move ^
                            ActionConst.Rotate ^
                            ActionConst.Jump; // 카메라 변경, 카메라 줌, 이동, 회전, 점프 이외에 행동 불능
                        if (State.isDrawMeele) PublicPlayerState.isActionConst &= ~ActionConst.Attack; // 발도상태일때 추가적으로 공격 행동 가능
                    }

                    if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndJump")) // 점프가 끝났을때
                    {
                        if (State.isDrawMeele) // 발도 상태인경우
                        {
                            State.isAttack = false;
                            DrawMeele();
                        }
                        else if (State.isDrawGun) // 총을 뽑고있는상태일경우
                            DrawGun();
                        else PublicPlayerState.isActionConst = ActionConst.None; // 그 외는 모든 행동이 다시 가능
                    }
                    #endregion

                    #region 회피
                    if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartRoll"))
                    {
                        PublicPlayerState.isActionConst = ActionConst.All ^ ActionConst.CameraZoom; // 카메라 줌인/줌아웃 이외 행동 불가능
                    }

                    if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndRoll"))
                    {
                        MoveSet.ResetDodge();
                        Com.anim.ResetTrigger(Anim.paramDodge);
                    if (State.isDrawMeele) // 발도 상태인경우
                    {
                        DrawMeele();
                    }
                    else if (State.isDrawGun) // 총을 뽑고있는상태일경우
                        DrawGun();
                    else PublicPlayerState.isActionConst = ActionConst.None; // 그 외는 모든 행동이 다시 가능
                    }
                    #endregion

                #endregion

                #region Parry
                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartParry")) //패리모션이 시작되었을때
                {
                    PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불가능
                    Com.anim.SetBool(Anim.paramParryTrg, false);
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndParry")) // 패리모션이 끝났을때
                {
                    Controler.EndPosCam();
                    State.isPos = false;
                    PublicPlayerState.isActionConst = ActionConst.None; // 모든 행동이 다시 가능
                }
                #endregion

                #region Meele
                #region 발도/납도
                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndMeele")) // 납도/발도 모션이 끝났을때
                {
                    if (State.isDrawMeele)
                        DrawMeele();
                    else PublicPlayerState.isActionConst = ActionConst.None; // 발도 상태가아니면 모든행동 가능
                }

                #endregion

                #region 공격

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartAttack")) // 공격모션이 시작되었을때
                {
                    PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불가능
                    State.isAttack = false;
                    PublicPlayerState.followCamToPlayer = true; // 카메라가 플레이어를 따라감
                }

                if (Com.anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack") &&
                    Com.anim.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.6) // 공격모션이 약 2/3 정도 진행되었을때
                {
                    PublicPlayerState.isActionConst =
                        ActionConst.All ^ ActionConst.Attack; // 공격 이외 행동 불가능
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("ComboAttack")) // 공격모션중 추가공격을 했을때
                {
                    PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불가능
                    State.isAttack = false;
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndAttack")) // 공격모션이 끝났을때
                {
                    PublicPlayerState.followCamToPlayer = false; // 다시 플레이어가 카메라를 따라감
                    DrawMeele();
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartJumpAttack")) // 점프 공격시
                {
                    PublicPlayerState.isActionConst = 
                        ActionConst.All ^ ActionConst.Move; // 이동 이외 행동 불가능
                    State.isAttack = false;
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("JumpAttackLoopDown")) // 점프 공격이 거의 끝났을때
                {
                    PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불가능
                }
                #endregion
                #endregion

                #region Gun
                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndGun")) // 총을 뽑거나 집어넣는 모션이 끝났을때
                {
                    if (State.isDrawGun)
                        DrawGun();
                    else PublicPlayerState.isActionConst = ActionConst.None; // 총을 넣고있는 상태에서는 모든 행동 가능
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartShootAimGun")) // 조준상태에서 사격이 시작되었을때
                {
                    PublicPlayerState.useIK |= IKWeapon.PistolFire;
                    State.isShoot = false;
                    PublicPlayerState.isActionConst =
                        ActionConst.All ^
                        ActionConst.Rotate ^ // 카메라/캐릭터 회전,
                        ActionConst.AimShoot; // 조준상태에서 사격이외에는 모든행동 불능
                }

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("LoopShootAimGun")) // 조준상태에서 사격 중 다시 사격을 했을때
                {
                    State.isShoot = false;
                    PublicPlayerState.isActionConst =
                        ActionConst.All ^
                        ActionConst.Rotate ^ // 카메라/캐릭터 회전,
                        ActionConst.AimShoot; // 조준상태에서 사격이외에는 모든행동 불능
                }


                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("EndShootAimGun")) // 조준상태에서 사격이 끝났을때
                {
                    State.isShoot = false;
                    PublicPlayerState.isActionConst =
                        ActionConst.All ^
                        ActionConst.AimShoot ^ // 조준상태에서 사격,
                        ActionConst.Move ^ // 이동,
                        ActionConst.Rotate ^ // 카메라/캐릭터 회전,
                        ActionConst.AimGun; // 조준행동 이외에 모든행동 불능
                    PublicPlayerState.useIK &= ~IKWeapon.PistolFire;
                }

                #endregion

                #region 공용

                if (Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartMeele") || // 발도/납도 또는
                    Com.anim.GetAnimatorTransitionInfo(0).IsUserName("StartGun")) // 총을 뽑거나 집어넣는 모션이 시작됐을때
                    PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불가능
                #endregion
            }

        }

        private void DrawMeele()
        {
            PublicPlayerState.isActionConst =
                                ActionConst.Pos |
                                ActionConst.Parry |
                                ActionConst.DrawPutGun |
                                ActionConst.AimGun; // Pos, 패리, 총 뽑기/집어넣기, 조준 행동불능
        }

        private void DrawGun()
        {
            PublicPlayerState.isActionConst =
                                ActionConst.Pos |
                                ActionConst.Parry |
                                ActionConst.DrawPutMeele |
                                ActionConst.Attack; // Pos, 패리, 발도/납도, 근접무기 공격 행동불능
        }
        #endregion
    }
}
