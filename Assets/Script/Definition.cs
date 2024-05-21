using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using static Game.Definition;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace Game
{
    public class Definition : MonoBehaviour
    {
        public enum CameraType { FPSCam, TPSCam };

        [Flags]
        public enum ActionConst : short // 일부 불가능한 행동에 대한 열거형 : short -> 0~15 까지 가능, 넘으면 int로 바꿔야함
        {
            None = 0, // 없음
            ChangeCamera = 1 << 0, // 카메라 변경
            CameraZoom = 1 << 1, // 카메라 줌인/줌아웃
            Move = 1 << 2, // 이동
            Jump = 1 << 3, // 점프
            Dodge = 1 << 4,
            Rotate = 1 << 5, // 캐릭터 회전
            Pos = 1 << 6, // Pos
            Parry = 1 << 7, // 패리
            DrawPutMeele = 1 << 8, // 발도/납도
            Attack = 1 << 9, // 공격
            DrawPutGun = 1 << 10, // 총 뽑기/집어넣기
            AimGun = 1 << 11, // 총 조준
            AimShoot = 1 << 12, // 조준상태에서 사격

            All = short.MaxValue
        }

        [Flags]
        public enum IKWeapon : byte
        {
            None = 0,
            PistolFire = 1,

            All = byte.MaxValue
        }

        [Serializable]
        public class Components
        {
            
            public Camera _tpsCam;
            public static Camera tpsCam;

            public Camera _fpsCam;
            public static Camera fpsCam;

            [HideInInspector] public Transform tpHig;
            [HideInInspector] public Transform tpVig;
            [HideInInspector] public Transform fpHig;
            [HideInInspector] public Transform fpVig;

            [HideInInspector] public GameObject tpsCamObj;
            [HideInInspector] public GameObject fpsCamObj;
            [HideInInspector] public GameObject player;

            //[HideInInspector] public CapsuleCollider cap;
            [HideInInspector] public Rigidbody rb;
            //[HideInInspector] public Animator anim;

            [HideInInspector] public CapsuleCollider cap;
            [HideInInspector] public GameObject camControler;

            [HideInInspector] public Animator anim;

            [Range(30f, 240f), Tooltip("게임 프레임")]
            public static int frame = 60;
        }

        [Serializable]
        public class KeyOption
        {
            public KeyCode moveFwd = KeyCode.W;
            public KeyCode moveBwd = KeyCode.S;
            public KeyCode moveLeft = KeyCode.A;
            public KeyCode moveRight = KeyCode.D;
            public KeyCode walk = KeyCode.LeftControl;
            public KeyCode sprint = KeyCode.LeftShift;
            public KeyCode dodge = KeyCode.Mouse3;
            public KeyCode jump = KeyCode.Space;
            public KeyCode switchCam = KeyCode.F5;
            public KeyCode showCursor = KeyCode.Period;
            public KeyCode pos = KeyCode.Tab;
            public KeyCode drawPutMeele = KeyCode.F;
            public KeyCode drawPutGun = KeyCode.G;
            public KeyCode attack = KeyCode.Mouse0;
            public KeyCode aim = KeyCode.Mouse1;
            public KeyCode upperParry = KeyCode.Mouse0;
            public KeyCode midParry = KeyCode.Mouse1;
            public KeyCode underParry = KeyCode.Mouse2;
            

            // Mouse0 => 마우스 왼쪽버튼  Mouse1 => 마우스 오른쪽버튼  Mouse2 => 마우스 휠버튼
            // Mouse3 => 마우스 뒤쪽 보조버튼  Mouse4 => 마우스 앞쪽 보조버튼
        }

        [Serializable]
        public class AnimatorOption
        {
            public string paramMoveX = "Move X";
            public string paramMoveZ = "Move Z";
            public string paramDistY = "Dist Y";
            public string paramisGround = "isGround";
            public string paramDownGravity = "DownGravity";
            public string paramJump = "Jump";
            public string paramDodge = "Dodge";
            public string paramWalk = "Walk";
            public string paramSprint = "Sprint";
            public string paramPos = "Pos";
            public string paramDrawMeele = "DrawMeele";
            public string paramDrawGun = "DrawGun";
            public string paramHParry = "HParry";
            public string paramMParry = "MParry";
            public string paramLParry = "LParry";
            public string paramAttack = "Attack";
            public string paramMeeleAtkType = "MeeleAtkType";
            public string paramParryTrg = "ParryTrigger";
            public string paramAim = "Aim";
            public string paramShoot = "Shoot";
        }

        [Serializable]
        public class CamOption
        {
            [Tooltip("게임시작 카메라")]
            public CameraType initCam;

            [Range(1f, 10f), Tooltip("상하/좌우 회전속도")]
            public float roteSpeed = 2f;

            [Tooltip("카메라 수평 반전여부")]
            public bool revCamX = false;

            [Tooltip("카메라 수직 반전여부")]
            public bool revCamY = false;

            [Range(-90f, 0f), Tooltip("올려다보기 제한각도")]
            public float lookUpDegree = -60f;

            [Range(0f, 75f), Tooltip("내려다보기 제한각도")]
            public float lookDownDegree = 75f;

            [Range(0f, 3.5f), Tooltip("줌 최대거리")]
            public float zoomMax = 3f;

            [Range(0f, 5f), Tooltip("줌 최소거리")]
            public float zoomMin = 3f;

            [Range(1f, 20f), Tooltip("줌 속도")]
            public float zoomSpeed = 10f;

            [Range(0.01f, 0.5f), Tooltip("줌 가속력")]
            public float zoomAccel = 0.1f;

            [Range(0f, 1f), Tooltip("액션 시 줌 속도")]
            public float zoomAction = 0.2f;
        }

        [Serializable]
        public class CheckOption
        {
            [Tooltip("지면으로 체크할 레이어 설정")]
            public LayerMask groundLayerMask = -1;

            [Range(0.01f, 0.5f), Tooltip("전방 감지 거리")]
            public float forwardCheckDist = 0.1f;

            [Range(0.1f, 10f), Tooltip("지면 감지 거리")]
            public float groundCheckDist = 2f;

            [Range(0f, 0.1f), Tooltip("지면 인식 허용 거리")]
            public float groundCheckThreshold = 0.01f;

            [Range(1f, 70f), Tooltip("등반 가능한 경사각")]
            public float maxSlopeAngle = 50f;
        }

        [Serializable]
        public class MoveOption
        {
            [Range(1f, 10f), Tooltip("이동속도")]
            public float speed = 5f;

            [Range(1f, 3f), Tooltip("스프린트 이속 증가 계수")]
            public float sprintCoef = 1.5f;

            [Range(0.1f, 0.5f), Tooltip("걷기 이속 감소 계수")]
            public float walkCoef = 0.5f;

            [Range(1f, 10f), Tooltip("점프 강도")]
            public float jumpForce = 4.2f;

            [Range(0f, 2f), Tooltip("점프 쿨타임")]
            public float jumpCooldown = 0.6f;

            [Range(0, 3), Tooltip("점프 허용 횟수")]
            public int maxJumpCount = 1;

            [Range(0f, 5f), Tooltip("회피 쿨타임")]
            public float dodgeCooldown = 0.1f;

            [Range(0f, 4f), Tooltip("경사로 이동속도 변화율")]
            public float slopeAccel = 1f;

            [Range(-9.81f, 0f), Tooltip("중력 가속도")]
            public float gravityAccel = -9.81f;
        }

        [Serializable]
        public class PublicPlayerState
        {
            [ReadOnly]
            public ActionConst _isActionConst;
            public static ActionConst isActionConst; // 일부 행동이 불가능한 상태인가

            [ReadOnly]
            public IKWeapon _useIK;
            public static IKWeapon useIK; //IK 사용상태

            [Space]

            [ReadOnly]
            public bool _isSprint;
            public static bool isSprint; //달리고있는가

            [ReadOnly]
            public bool _isWalking;
            public static bool isWalking; //살금살금 걷고있는가

            [ReadOnly]
            public bool _isDodge;
            public static bool isDodge; // 회피상태인가

            [ReadOnly]
            public bool _isCurrentFps;
            public static bool isCurrentFps; //시점 fps 상태 여부

            [ReadOnly]
            public bool _isGrounded;
            public static bool isGrounded; //땅에 붙어있는가

            [ReadOnly]
            public bool _followCamToPlayer;
            public static bool followCamToPlayer; //카메라가 플레이어를 따라가는가 (안 따라갈 시 플레이어가 카메라를 따라감)
        }

        [Serializable]
        public class PublicCurrentValue
        {
            [ReadOnly]
            public Vector3 _worldMoveDir;
            public static Vector3 worldMoveDir;

            //[SerializeField, ReadOnly]
            //private Vector3 _groundNormal;
            //public Vector3 groundNormal;

            //[SerializeField, ReadOnly]
            //private Vector3 _groundCross;
            //public Vector3 groundCross;

            //[SerializeField, ReadOnly]
            //private Vector3 _hVelocity;
            //public Vector3 hVelocity;

            [ReadOnly]
            public Vector3 _rootMotionPos;
            public static Vector3 rootMotionPos; // 루트 모션으로 인한 이동 기록
            //public Quaternion rootMotionRot; // 루트 모션으로 인한 회전 기록

            //[Space]
            //[SerializeField, ReadOnly]
            //private float _jumpCooldown;
            //public float jumpCooldown;

            //[SerializeField, ReadOnly]
            //private int _jumpCount;
            //public int jumpCount;

            //[SerializeField, ReadOnly]
            //private float _outOfControlDuration;
            //public float outOfControlDuration;

            [Space]
            [ReadOnly]
            public float _groundDist;
            public static float groundDist; // 바닥과의 거리

            //[SerializeField, ReadOnly]
            //private float _groundSlopeAngle;
            //public float groundSlopeAngle; //현재 바닥의 경사각

            //[SerializeField, ReadOnly]
            //private float _groundVSlopeAngle;
            //public float groundVSlopeAngle; //수직으로 재측정한 경사각

            //[SerializeField, ReadOnly]
            //private float _forwardSlopeAngle;
            //public float forwardSlopeAngle; //캐릭터가 바라보는 방향의 경사각

            //[SerializeField, ReadOnly]
            //private float _slopeAccel;
            //public float slopeAccel; //경사로 가속/감속 비율

            [Space]
            [ReadOnly]
            public float _gravity;
            public static float gravity; //직접 제어하는 중력값
        }
        [SerializeField] private Components _components = new();
        //[SerializeField] private KeyOption _keyOption = new();
        //[SerializeField] private AnimatorOption _animatorOption = new();
        //[SerializeField] private CamOption _camOption = new();
        //[SerializeField] private CheckOption _checkOption = new();
        //[SerializeField] private MoveOption _moveOption = new();
        [SerializeField] private PublicPlayerState _publicPlayerState = new();
        [SerializeField] private PublicCurrentValue _publicCurrentValue = new();

        private Components Com => _components;
        private PublicPlayerState State => _publicPlayerState;
        private PublicCurrentValue Value => _publicCurrentValue;

        private void Awake()
        {
            Components.tpsCam = Com._tpsCam;
            Components.fpsCam = Com._fpsCam;
            /* Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Player"),LayerMask.NameToLayer("Enemy"));
            Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Enemy"),LayerMask.NameToLayer("Enemy")); */
        }

        private void Update()
        {
            if (PublicPlayerState.isActionConst != State._isActionConst) State._isActionConst = PublicPlayerState.isActionConst;
            if (PublicPlayerState.useIK != State._useIK) State._useIK = PublicPlayerState.useIK;
            if (PublicPlayerState.isSprint != State._isSprint) State._isSprint = PublicPlayerState.isSprint;
            if (PublicPlayerState.isWalking != State._isWalking) State._isWalking = PublicPlayerState.isWalking;
            if (PublicPlayerState.isDodge != State._isDodge) State._isDodge = PublicPlayerState.isDodge;
            if (PublicPlayerState.isCurrentFps != State._isCurrentFps) State._isCurrentFps = PublicPlayerState.isCurrentFps;
            if (PublicPlayerState.isGrounded != State._isGrounded) State._isGrounded = PublicPlayerState.isGrounded;
            if (PublicPlayerState.followCamToPlayer != State._followCamToPlayer) State._followCamToPlayer = PublicPlayerState.followCamToPlayer; 
            if (PublicCurrentValue.worldMoveDir != Value._worldMoveDir) Value._worldMoveDir = PublicCurrentValue.worldMoveDir;
            if (PublicCurrentValue.rootMotionPos != Value._rootMotionPos) Value._rootMotionPos = PublicCurrentValue.rootMotionPos;
            if (PublicCurrentValue.groundDist != Value._groundDist) Value._groundDist = PublicCurrentValue.groundDist;
            if (PublicCurrentValue.gravity != Value._gravity) Value._gravity = PublicCurrentValue.gravity;
        }   
    }
}

