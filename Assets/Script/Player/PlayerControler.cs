using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Resources;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEditor;
//using Unity.VisualScripting;
//using UnityEditor.Rendering;
using UnityEngine;
using static Game.Definition;
using static UnityEngine.Rendering.DebugUI;
using CameraType = Game.Definition.CameraType;
//using UnityEngine.InputSystem;

namespace Game
{
    public class PlayerControler : MonoBehaviour, IControler
    {
        /*
        정의
       */
        #region .
        [Serializable]
        public class PlayerState
        {
            public bool isCursorActive; //마우스커서 활성화 상태여부
            //public bool isCurrentFps; //시점 fps 상태 여부
            public bool isMoving; //움직이고있는가
            //public bool isSprint; //달리고있는가
            //public bool isWalking; //살금살금 걷고있는가
            public bool isPos; // Pos상태인가
            public bool isParryMotion; //패리 모션 상태인가
            public bool isMotionActive; // 모션 상태인가
            public bool isDrawMeele; // 발도 상태인가
            public bool isDrawGun; // 총을 뽑은 상태인가
            public bool isAimGunTrg; // 총을 조준하는 키를 누른상태인가
            public bool isAimGun; // 총을 조준하고있는 상태인가
            //public ActionConst isActionConst; // 일부 행동이 불가능한 상태인가
            #region 이전 코드
            //public bool isGrounded; //땅에 붙어있는가
            //public bool isPos; //자세 상태인가
            //public bool isOnSteepSlope; //등반 불가능한 경사로인가
            //public bool isForwardBlocked; //전방에 장애물 존재
            //public bool isOutOfControl; //제어 불가 상태
            //public bool isJumptTrg; //점프 입력상태
            //public bool isJump; //점프 상태
            //public bool isAttackDelay;
            //public bool isPosTrg; // Pos를 누른 상태인가
            //public bool isPosCam; // Pos를 눌러 카메라 위치가 바뀐 상태인가
            #endregion
        }
        #endregion

        /*
          필드, 프로퍼티
        */
        #region .
        [SerializeField] private Components _components = new Components();
        [Space]
        #region 이전 코드
        //        [Space]
        //        [SerializeField] private AnimatorOption _animatorOption = new AnimatorOption();
        #endregion
        //private PublicPlayerState _publicstate = new PublicPlayerState();
        [SerializeField] private KeyOption _keyOption = new KeyOption();
        [Space]
        [SerializeField] private CamOption _camOption = new CamOption();
        [Space]
        [SerializeField] private PlayerState _state = new PlayerState();

        private Components Com => _components;
        private KeyOption Key => _keyOption;
        private CamOption Cam => _camOption;
        //        public AnimatorOption Anim => _animatorOption;
        private PlayerState State => _state;

        //private PublicPlayerState PublicState => _publicstate;

        public bool CurrentFps
        {
            get { return PublicPlayerState.isCurrentFps; }
            set
            {
                PublicPlayerState.isCurrentFps = value;
                CameraView();
            }
        }

        private Vector3 _moveDir; // 이동 방향 (로컬)
        private Vector3 _worldMove; // 이동 방향 (글로벌)
        private Vector3 _prevCamPosition; //특정 액션을 사용하기 직전 카메라의 위치
        private Vector2 _rotation;
        #region 이전 코드
        //private float _groundCheckRadius;
        //private float _distFromGround;
        #endregion

        private readonly WaitForEndOfFrame _waitFrame = new WaitForEndOfFrame();

        private bool _prevCurrentFps; //특전 액션을 사용하기 직전의 카메라 설정
        private float _tpsCamZoomInitDistance; //tps카메라, rig 사이의 초기 거리
        private float _tpsCamWheelInput = 0; //tps카메라 휠 입력값
        private float _currentWheel;
        private float _zoomActionDelay; //특정 액션 시 줌인/줌아웃 동안의 딜레이
        private float _deltaTime;
        #endregion

        /*
         이벤트 
        */
        #region .
        private void Start()
        {
            InitComponents();
            InitSettings();
        }
        private void Update()
        {
            SetValuesByKeyInput();
            //            UpdateAnimeParams();

            ShowCursorToggle();
            CameraViewToggle();
            TPSCamZoom();

            PlayerMove();
            Rotate();

            SendInterface();
            UpdateCurrentValues();

            _deltaTime = Time.deltaTime;
            Application.targetFrameRate = Components.frame;
        }

        private void UpdateCurrentValues()
        {
            if (_zoomActionDelay > 0f) //&& !State.isPosCam)
                _zoomActionDelay -= _deltaTime;
        }
        #endregion

        /*
         초기화
        */
        #region .
        private void InitComponents()
        {
            LogNotInitComponentError(Components.tpsCam, "TPS Camera");
            LogNotInitComponentError(Components.fpsCam, "FPS Camera");
            TryGetComponent(out Com.rb);
            //Com.anim = GetComponentInChildren<Animator>();

            Com.tpsCamObj = Components.tpsCam.gameObject;
            Com.tpVig = Components.tpsCam.transform.parent;
            Com.tpHig = Com.tpVig.parent;
            Com.fpsCamObj = Components.fpsCam.gameObject;
            Com.fpVig = Components.fpsCam.transform.parent;
            Com.fpHig = Com.fpVig.parent;
            Com.player = GameObject.FindWithTag("Player");
        }

        private void InitSettings()
        {
            //Rigidbody
            if (Com.rb)
            {
                // 회전은 트랜스폼을 통해 직접 제어할 것이기 때문에 리지드바디 회전은 제한
                Com.rb.constraints = RigidbodyConstraints.FreezeRotation;
            }

            //Camera
            var allCams = FindObjectsOfType<Camera>();
            foreach (var cam in allCams)
            {
                cam.gameObject.SetActive(false);
            }
            //설정한 카메라 하나만 활성화
            PublicPlayerState.isCurrentFps = Cam.initCam == CameraType.FPSCam;
            Com.fpsCamObj.SetActive(PublicPlayerState.isCurrentFps);
            Com.tpsCamObj.SetActive(!PublicPlayerState.isCurrentFps);

            #region 이전 코드
            //TryGetComponent(out CapsuleCollider cCol);
            //_groundCheckRadius = cCol ? cCol.radius : 0.1f;
            #endregion

            _tpsCamZoomInitDistance = Vector3.Distance(Com.tpHig.position, Components.tpsCam.transform.position);
        }
        #endregion

        /*
         입력, 1/3인칭 이동,회전
        */
        #region .
        //키보드 입력을 통해 필드 초기화
        private void SetValuesByKeyInput()
        {
            float h = 0f, v = 0f;

            if (Input.GetKey(Key.moveFwd)) v += 1.0f;
            if (Input.GetKey(Key.moveBwd)) v -= 1.0f;
            if (Input.GetKey(Key.moveLeft)) h -= 1.0f;
            if (Input.GetKey(Key.moveRight)) h += 1.0f;

            _moveDir = /*!State.isActionConst.HasFlag(ActionConst.Move) ?*/ new Vector3(h, 0f, v).normalized; //: Vector3.zero; // 캐릭터 이동 방향, 이동 불능 상태일 시 없음
            _rotation = new Vector2(Input.GetAxisRaw("Mouse X"), -Input.GetAxisRaw("Mouse Y")); //마우스 이동 방향

            _rotation.x = Cam.revCamX ? -_rotation.x : _rotation.x; // 카메라 수평회전 반전 활성화 시 반전
            _rotation.y = Cam.revCamY ? -_rotation.y : _rotation.y; // 카메라 수직회전 반전 활성화 시 반전

            State.isMoving = _moveDir.sqrMagnitude > 0.01f;
            PublicPlayerState.isSprint = Input.GetKey(Key.sprint);
            PublicPlayerState.isWalking = Input.GetKey(Key.walk);
            _tpsCamWheelInput = Input.GetAxisRaw("Mouse ScrollWheel");
            _currentWheel = Mathf.Lerp(_currentWheel, _tpsCamWheelInput, Cam.zoomAccel);


        }

        //캐릭터 이동
        private void PlayerMove()
        {
            //실제 이동 벡터 계산
            //1인칭(fps)
            if (PublicPlayerState.isCurrentFps)
                _worldMove = Com.fpHig.TransformDirection(_moveDir);
            //3인칭(tps)
            else
                _worldMove = Com.tpHig.TransformDirection(_moveDir);
        }

        private void Rotate()
        {
            Transform verRig, horRig;
            if (PublicPlayerState.isCurrentFps)
            { //fps회전
                if (PublicPlayerState.isActionConst.HasFlag(ActionConst.Rotate)) return; // 회전 행동불능일때 fps일 경우엔 카메라 회전을 막음
                verRig = Com.fpVig;
                horRig = Com.fpHig;
                Com.player.transform.eulerAngles = Com.fpHig.transform.eulerAngles;
            }
            else
            { //tps회전
                verRig = Com.tpVig;
                horRig = Com.tpHig;
                RotateFPSRoot();
            }

            if (State.isCursorActive) return;

            float deltaCoef = _deltaTime * 50f;

            //상하 : 수직 회전
            float xRotPrev = verRig.localEulerAngles.x; // 이동 이전 카메라의 상하(x좌표) 각도
            float xRotNext = xRotPrev + _rotation.y * Cam.roteSpeed * deltaCoef; // fps 카메라의 회전 이후 상하(x좌표) 각도

            if (xRotNext > 180f) xRotNext -= 360f;

            //좌우 : 수평 회전
            float yRotPrev = horRig.localEulerAngles.y; // 이동 이전 fps 카메라의 좌우(y좌표) 각도
            float yRotNext = yRotPrev + _rotation.x * Cam.roteSpeed * deltaCoef; // fps 카메라의 회전 이후 좌우(y좌표) 각도

            //상하 회전 가능여부
            bool xRotatable = Cam.lookUpDegree < xRotNext &&
                               Cam.lookDownDegree > xRotNext;

            //상하 회전 적용
            verRig.localEulerAngles = Vector3.right * (xRotatable ? xRotNext : xRotPrev);

            //좌우 회전 적용
            horRig.localEulerAngles = Vector3.up * yRotNext;
        }

        //3인칭에서 움직일때 캐릭터 회전 적용
        private void RotateFPSRoot()
        {
            //움직이지 않거나 회전 불가능상태인데 Pos상태도 아닐때 회전안함
            if (!State.isMoving || PublicPlayerState.isActionConst.HasFlag(ActionConst.Rotate)) return;
            
            //Vector3 dir = Com.tpVig.TransformDirection(_moveDir);
            float currentY = Com.fpHig.localEulerAngles.y;
            float nextY = Quaternion.LookRotation(_worldMove).eulerAngles.y;

            if (nextY - currentY > 180f) nextY -= 360f;
            else if (currentY - nextY > 180f) nextY += 360;

            /*Com.fpHig.eulerAngles*/Com.fpHig.localEulerAngles = Vector3.up * Mathf.Lerp(currentY, nextY, /*1f*/0.3f);
            Com.player.transform.eulerAngles = Com.fpHig.localEulerAngles/*Com.fpHig.eulerAngles*/; // 캐릭터에 회전값 반영
        }

        //ActionConst IControler.IsActionConst() => PublicPlayerState.isActionConst;

        //void IControler.SetActionConst(ActionConst ActionConst)
        //{
        //    PublicPlayerState.isActionConst = ActionConst;
        //}

        //void IControler.AddActionConst(ActionConst ActionConst)
        //{
        //    PublicPlayerState.isActionConst |= ActionConst;
        //}

        //void IControler.SubActionConst(ActionConst ActionConst)
        //{
        //    PublicPlayerState.isActionConst &= ActionConst;
        //}

        //void IControler.AllActionConst(bool action)
        //{
        //    if(action) PublicPlayerState.isActionConst = ActionConst.All;
        //    else PublicPlayerState.isActionConst = ActionConst.None;
        //}
        #endregion

        /*
         커서 토글 
        */
        #region .
        private void ShowCursorToggle()
        {
            if (Input.GetKeyDown(Key.showCursor))
                State.isCursorActive = !State.isCursorActive;

            ShowCursor(State.isCursorActive);
        }

        private void ShowCursor(bool value)
        {
            Cursor.visible = value;
            Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked;
        }
        #endregion

        /*
         카메라
        */
        #region .
        private void CameraViewToggle()
        {
            if (Input.GetKeyDown(Key.switchCam) && !PublicPlayerState.isActionConst.HasFlag(ActionConst.ChangeCamera))
                CurrentFps = !PublicPlayerState.isCurrentFps;

        }

        private void CameraView()
        {
            Com.fpsCamObj.SetActive(PublicPlayerState.isCurrentFps);
            Com.tpsCamObj.SetActive(!PublicPlayerState.isCurrentFps);

            Transform verRigPrev, horRigPrev, verRigNext, horRigNext;

            //TPS -> FPS
            if (PublicPlayerState.isCurrentFps)
            {
                verRigPrev = Com.tpVig;
                verRigNext = Com.fpVig;
                horRigPrev = Com.tpHig;
                horRigNext = Com.fpHig;
            }
            //FPS -> TPS
            else
            {
                verRigPrev = Com.fpVig;
                verRigNext = Com.tpVig;
                horRigPrev = Com.fpHig;
                horRigNext = Com.tpHig;
            }
            Vector3 tpsAngleV = verRigPrev.localEulerAngles;
            Vector3 tpsAngleH = horRigPrev.localEulerAngles;
            verRigNext.localEulerAngles = Vector3.right * tpsAngleV.x;
            horRigNext.localEulerAngles = Vector3.up * tpsAngleH.y;
        }

        private void TPSCamZoom()
        {
            if (PublicPlayerState.isActionConst.HasFlag(ActionConst.CameraZoom)) return; // 카메라 줌인/줌아웃 기능이 막혀있을때 사용불가
            if (PublicPlayerState.isCurrentFps) return; // fps 카메라일경우 반환 (tps 카메라에서만 가능)
            if (Mathf.Abs(_currentWheel) < 0.01f) return; //휠 입력이 있을때만 가능

            Transform tpsCamTr = Components.tpsCam.transform;
            Transform tpsCamRig = Com.tpVig;

            float zoom = _deltaTime * Cam.zoomSpeed;
            float currentCamToRigDist = Vector3.Distance(tpsCamTr.position, tpsCamRig.position);
            Vector3 move = Vector3.forward * zoom * _currentWheel * 10f;

            //줌 인
            if (_currentWheel > 0.01f)
            {
                if (_tpsCamZoomInitDistance - currentCamToRigDist < Cam.zoomMax)
                    tpsCamTr.Translate(move, Space.Self);
            }
            else if (_currentWheel < -0.01f)
            {
                if (currentCamToRigDist - _tpsCamZoomInitDistance < Cam.zoomMin)
                    tpsCamTr.Translate(move, Space.Self);
            }
            //줌 아웃
        }

        void IControler.EndPosCam()
        {
            Transform tpsCamTr = Components.tpsCam.transform;

            //액션을 취한 후 tps 카메라를 다시 원래위치로 돌려놓음
            StartCoroutine(CamCoroutine(tpsCamTr,new Vector3(0f, 0f, -1.5f), _prevCamPosition, Cam.zoomAction));
            State.isPos = false;
            PublicPlayerState.isActionConst = ActionConst.None;
        }

        IEnumerator CamCoroutine(Transform target, Vector3 start, Vector3 end, float duration, byte special = 0)
        {
            //_zoomActionDelay = duration;
            float time = 0;

            _zoomActionDelay = duration * 1.001f ;
            
            while(time < duration) // target가 start에서 end로 duration초에 걸쳐서 이동함
            {
                time += _deltaTime;
                target.localPosition = Vector3.Lerp(start, end, time / duration);
                yield return _waitFrame;
            }

            target.localPosition = end;

            switch (special)
            {
                case 0:
                    break;
                case 1:
                    CurrentFps = _prevCurrentFps; // 조준해제에 따른 카메라 전환상태 복구
                    target.localPosition = _prevCamPosition;
                    break;
                default:
                    break;
            }
        }

        #endregion

        /*
         인터페이스
        */
        #region

        private void SendInterface()
        {
            IMoveSet moveSet = Com.player.GetComponent<IMoveSet>();
            IAnimation animation = Com.player.GetComponent<IAnimation>();

            //이동조작 전송
            if (State.isMoving && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Move)) // 이동조작 전송, 이동 불가능 상태일시 전송안됨
            {
                moveSet.SetMovement(_worldMove/*, State.isSprint, State.isWalking*/);
            }
            else moveSet.StopMoving(); // 이동하지않거나, 이동불가능 상태일시 멈춤

            animation.SendAnimMoveDir(_moveDir);

            // 회피조작 전송, 이동상태가아니거나 회피불가능 상태일시 전송안됨
            if (Input.GetKeyDown(Key.dodge) && State.isMoving && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Dodge))
            {
                bool success = moveSet.SetDodge(_worldMove);
                if (success) animation.SendAnimDodgeDir(_moveDir);
            }

            // 점프조작 전송, 점프 불가능 상태일시 전송안됨
            if (Input.GetKey(Key.jump) && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Jump)) moveSet.SetJump();

            // Pos조작 전송, 딜레이상태거나, Pos 불가능 상태일시 전송안됨
            if (Input.GetKeyDown(Key.pos) && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Pos) && _zoomActionDelay <= 0)
            {
                PublicPlayerState.isActionConst |= ActionConst.Pos & ActionConst.CameraZoom;
                if (!State.isPos) PlayAction(Action.pos, true);
                else if (State.isPos) PlayAction(Action.pos, false);
                #region 이전 코드
                //if (!State.isPosTrg && _zoomActionDelay <= 0 && !State.isActionConst.HasFlag(ActionConst.Pos)) PlayAction(Action.pos,true);
                
                //State.isPosTrg = true;
                //State.isPos = moveSet.SetPos(true);
                //if (!State.isPos) State.isPosTrg = false;
                //if (State.isPos && State.isPosTrg)
                //{
                //    State.isMotionActive = true;
                //    _prevCamPosition = tpsCamTr.localPosition; // tps 카메라의 기존위치를 다른함수에 기록
                //    State.isPosCam = true;
                //    _zoomActionDelay = Cam.zoomaction;

                //Pos 상태에 들어갔을때 tps카메라의 위치가 바뀜
                //    StartCoroutine(CamCoroutine(tpsCamTr, _prevCamPosition, new Vector3(0f, 0f, -1.5f), Cam.zoomaction));
                //}
                
                //else if (State.isPosTrg && _zoomActionDelay <= 0) PlayAction(Action.pos,false);
                
                //State.isPosTrg = false;
                //State.isPos = moveSet.SetPos(false);
                //if (!State.isPos) State.isPosTrg = true;
                //if (State.isPos && !State.isPosTrg)
                //{
                //    State.isMotionActive = false;
                //    State.isPosCam = false;
                //    _zoomActionDelay = Cam.zoomaction;

                //Pos 상태를 해제했기때문에 tps 카메라를 다시 원래위치로 돌려놓음
                //    StartCoroutine(CamCoroutine(tpsCamTr, new Vector3(0f, 0f, -1.5f), _prevCamPosition, Cam.zoomaction));
                //}
                #endregion
            }
            // 발도/납도 조작 전송, 발도/납도 불가능 상태일시 전송안됨
            else if (Input.GetKeyDown(Key.drawPutMeele) && !PublicPlayerState.isActionConst.HasFlag(ActionConst.DrawPutMeele))
            {
                if (!State.isDrawMeele) PlayAction(Action.meele, true);
                else if (State.isDrawMeele) PlayAction(Action.meele, false);
                #region 이전 코드
                    //if (!State.isDrawMeele)
                    //{
                    //    State.isDrawMeele = true;
                    //    bool success = moveSet.SetMeele(true);
                    //    if (!success) State.isDrawMeele = false;
                    //    else State.isMotionActive = true;
                    //}
                    //else if(State.isDrawMeele)
                    //{
                    //    
                    //    State.isDrawMeele = false;
                    //    bool success = moveSet.SetMeele(false);
                    //    if (!success) State.isDrawMeele = true;
                    //    else State.isMotionActive = true;
                    //}
                    //moveSet.StopMoving();
                    #endregion
            }
            // 총 뽑기/집어넣기 조작 전송, 총 뽑기/집어넣기 불가능 상태일시 전송안됨
            else if (Input.GetKeyDown(Key.drawPutGun) && !PublicPlayerState.isActionConst.HasFlag(ActionConst.DrawPutGun))
            {
                if (!State.isDrawGun) PlayAction(Action.gun, true);
                else if (State.isDrawGun) PlayAction(Action.gun, false);
            }


            // 근접무기 공격 조작 전송, 발도상태가 아니거나 공격불가능 상태일시 전송안됨
            if (Input.GetKeyDown(Key.attack) && State.isDrawMeele && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Attack))
            {
                byte atkType = (byte)UnityEngine.Random.Range(1,10);
                animation.MeeleAttack(atkType);
                PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불능
                //moveSet.ActionDelay(true);
            }

            // 총 조준 조작 전송, 총을 뽑고있지않거나 조준불가능 상태일시 전송안됨
            if (Input.GetKeyDown(Key.aim) && State.isDrawGun && !PublicPlayerState.isActionConst.HasFlag(ActionConst.AimGun) && _zoomActionDelay <= 0)
            {
                PublicPlayerState.isActionConst |= ActionConst.AimGun & ActionConst.CameraZoom;
                if (!State.isAimGun) PlayAction(Action.aimgun, true);
                else if (State.isAimGun) PlayAction(Action.aimgun, false);
            }

            // 조준사격 조작 전송, 조준상태가아니거나 조준사격 불가능 상태일시 전송안됨
            if (Input.GetKeyDown(Key.attack) && State.isAimGun && !PublicPlayerState.isActionConst.HasFlag(ActionConst.AimShoot))
            {
                PublicPlayerState.isActionConst = ActionConst.All;
                animation.SetAimShoot();
            }

            //패리 조작 전송
            if (State.isPos && !PublicPlayerState.isActionConst.HasFlag(ActionConst.Parry)) //Pos상태가 아니거나 패리 불가능 상태일 시 전송안됨
                if (/*State.isPos && State.isPosTrg &&*/ Input.GetKeyDown(Key.upperParry)) animation.SetPosAtk(0);
                else if (Input.GetKeyDown(Key.midParry)) animation.SetPosAtk(1);
                else if (Input.GetKeyDown(Key.underParry)) animation.SetPosAtk(2);

            //if (isParryAction) State.isPosTrg = false;
        }

        private enum Action: byte
        {
            pos,
            meele,
            gun,
            aimgun,
            shoot
        }

        private void PlayAction(Action type, bool value)
        {
            //IMoveSet moveSet = Com.player.GetComponent<IMoveSet>();
            IAnimation animation = Com.player.GetComponent<IAnimation>();

            switch(type)
            {
                #region Action.pos
                case Action.pos: // Pos상태
                    Transform tpsCamTr = Components.tpsCam.transform;
                    // Pos상태를 킬때 value = true , 끌때 value = false
                    State.isPos = value;
                    bool success = animation.SetPos(value);

                    if (!success) 
                    {
                        State.isPos = !value;
                        PublicPlayerState.isActionConst &= ~ActionConst.Pos;
                        break;
                    }

                    PublicPlayerState.isActionConst = ActionConst.All;

                    if (value)
                    {
                        _prevCamPosition = tpsCamTr.localPosition; // 카메라 이동 전 tps카메라의 위치를 저장

                        //Pos 상태에 들어갔을때 tps카메라의 위치가 바뀜
                        StartCoroutine(CamCoroutine(tpsCamTr, _prevCamPosition, new Vector3(0f, 0f, -1.5f), Cam.zoomAction));
                        PublicPlayerState.isActionConst &= ~ActionConst.Pos & ~ActionConst.Rotate & ~ActionConst.Parry;
                    }
                    else
                    {
                        //Pos 상태를 해제했기때문에 tps 카메라를 다시 원래위치로 돌려놓음
                        StartCoroutine(CamCoroutine(tpsCamTr, new Vector3(0f, 0f, -1.5f), _prevCamPosition, Cam.zoomAction));
                        PublicPlayerState.isActionConst = ActionConst.None;
                    }
                    break;
                #endregion

                #region Action.meele
                case Action.meele: // 근접무기 발도/납도
                    // 발도시 value = true , 납도시 value = false
                    State.isDrawMeele = value;
                    success = animation.SetMeele(value);

                    if (!success) State.isDrawMeele = !value;
                    else PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불능
                    break;
                #endregion

                #region Action.gun
                case Action.gun: // 총 뽑기/집어넣기
                    // 뽑을시 value = true , 집어넣을시 value = false
                    State.isDrawGun = value;
                    success = animation.SetGun(value);

                    if (!success) State.isDrawGun = !value;
                    else PublicPlayerState.isActionConst = ActionConst.All; // 모든 행동 불능
                    break;
                #endregion

                #region Action.aimgun
                case Action.aimgun:
                    Transform fpsCamTr = Components.fpsCam.transform;
                    State.isAimGun = value;
                    success = animation.SetAimGun(value);

                    if (!success)
                    {
                        State.isAimGun = !value;
                        PublicPlayerState.isActionConst &= ~ActionConst.AimGun;
                        break;
                    }

                    Vector3 cameraPosition;
                    PublicPlayerState.isActionConst = ActionConst.All;

                    if (value)
                    {
                        if (PublicPlayerState.isCurrentFps) cameraPosition = Components.fpsCam.transform.localPosition;
                        else cameraPosition = Components.tpsCam.transform.localPosition;

                        _prevCamPosition = fpsCamTr.localPosition;// 카메라 이동 전 fps카메라의 위치를 저장
                        _prevCurrentFps = PublicPlayerState.isCurrentFps; // 카메라를 고정 시키기 전 카메라의 전환상태 저장
                        CurrentFps = true; // 카메라의 전환 상태를 강제로 fps로 설정

                        StartCoroutine(CamCoroutine(fpsCamTr, cameraPosition, new Vector3(-0.4f, -0.22f, -0.66f), Cam.zoomAction));
                        PublicPlayerState.isActionConst &= ~ActionConst.Move & ~ActionConst.Rotate & ~ActionConst.AimGun & ~ActionConst.AimShoot;
                    }
                    else
                    {
                        if (_prevCurrentFps) cameraPosition = _prevCamPosition;
                        else cameraPosition = Components.tpsCam.transform.localPosition;

                        StartCoroutine(CamCoroutine(fpsCamTr, new Vector3(-0.4f, -0.22f, -0.66f), cameraPosition, Cam.zoomAction, 1));

                        PublicPlayerState.isActionConst = ActionConst.Pos | ActionConst.Parry | ActionConst.DrawPutMeele | ActionConst.Attack;
                    }

                    break;
                #endregion

                //case Action.shoot:
                    
                //    break;
            }
        }
        #endregion


        /*
         오류 감지
        */
        #region .
        private void LogNotInitComponentError<T>(T component, string componentName) where T : Component
        {
            if (component == null)
                Debug.LogError($"{componentName} 컴포넌트를 인스펙터에 넣어주세요!");
        }
        #endregion
    }

}