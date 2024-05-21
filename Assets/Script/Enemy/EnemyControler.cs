using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using static Game.Definition;
using UnityEngine;
using System.Net.WebSockets;
using Unity.VisualScripting;
using UnityEngine.Animations;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEditor.EditorTools;
using UnityEditor;
using System.ComponentModel;

namespace Game
{
    [RequireComponent(typeof(EnemyMoveSet))]
    public class EnemyControler : MonoBehaviour, IEnemy
    {
        public enum AggroType : byte
        {
            [Tooltip("공격 안함")]
            friendly = 0,

            [Tooltip("비선공")]
            neutrality = 1,

            [Tooltip("선공")]
            hostility = 2
        }

        [Serializable]
        public class Components
        {
            public Transform enemyRoot;

            /*[HideInInspector] *//*public Rigidbody rb;*/
            /*[HideInInspector] *//* public MeshCollider col; */
            public Transform targetDetectEye; // 타겟을 찾는 눈에 대한 트랜스폼
            /*[HideInInspector] */public SphereCollider targetDetectCol;
            /*[HideInInspector] */public Transform target;
            /*[HideInInspector] */public CapsuleCollider targetCol;
            /*[HideInInspector] *//*public Transform meshCenter;*/

            /* [HideInInspector] public WaitForSeconds waitPatrol;
            [HideInInspector] public WaitForSeconds waitGroggy;

            [HideInInspector] public WaitForSeconds dontMoveWaitPatrol; */
        }

        [Serializable]
        public class EnemyStats
        {
            [Tooltip("최대 체력")]
            public float maxHealth;

            [Tooltip("최대 스테미나")]
            public float maxStamina;

            [Tooltip("공격력")]
            public float attackDamage;

            [Tooltip("방어력")]
            public float defense;

            [Tooltip("속도")]
            public float speed;

            [Tooltip("그로기 지속시간")]
            public float groggyTime;

            [Tooltip("기척 감지 범위")]
            public float senseRange = 10f;
        }

        [Serializable]
        public class EnemyState
        {
            public float health; // 체력
            public float stamina; // 스테미나
            public bool isDetectSense; // 기척감지 상태
            public bool isAggro; // 어그로 상태
            public bool isGroggy; // 그로기 상태
            public bool isMoving; // 이동중인가

            //public bool isForwardBlocked; // 앞이 막혀있는가
            //public bool isGrounded; // 지면에 닿아있는가

            //public bool isOnSteepSlope;
        }

        [Serializable]
        public class EnemyOption
        {
            [Range(0f, 180f), Tooltip("기척 안 어그로 감지 각도")]
            public float aggroAngle = 75f;

            [Tooltip("다음 순찰 선회 초기 쿨타임")]
            public float defaultPatrolCooldown = 10f;

            [Range(0.001f, 1f), Tooltip("다음 순찰 선회쿨타임 증감 계수 (최소 난수)")]
            public float patrolCooldownARandomNumberMin = 1f;

            [Min(1f), Tooltip("다음 순찰 선회쿨타임 증감 계수 (최대 난수)")]
            public float patrolCooldownARandomNumberMax = 1f;

            [Tooltip("순찰 선회 시간")]
            public float patrolRotateTime = 1f;

            [Range(0, 100), Tooltip("순찰 이동 없이 돌아보기만 할 확률 (백분율)")]
            public float patrolDontMovePercent = 50f;

            [Range(0, 5), Tooltip("돌아보기만 했을경우 선회 쿨타임 증감 계수")]
            public float ifPatrolDontMoveCooldownCoef = 0.5f;

            [Min(0.001f), Tooltip("순찰 지속 시간 (최솟값)")]
            public float patrolMoveTimeMin = 5f;

            [Min(0.001f), Tooltip("순찰 지속 시간 (최댓값)")]
            public float patrolMoveTimeMax = 10f;

            [Range(1f, 3f), Tooltip("추격 시 늘어나는 감지 범위 계수")]
            public float chaseRangeCoef = 1.5f;

            [Min(0f), Tooltip("추격 시 이동속도 배율")]
            public float chaseMovementMultiply = 1.25f;

            [Min(0f), Tooltip("추격 시 장애물 감지 거리")]
            public float chaseForwardCheckDist = 10f;

            [Range(0f, 180f), Tooltip("앞에 장애물이 있을 시 우회 검사 각도 간격")]
            public float chaseAvoidAngleInterV = 15f;

            [Min(0.001f), Tooltip("타겟을 놓쳤을 때 멈춰있는 시간 (최솟값)")]
            public float targetMissTimeMin = 0.5f;

            [Min(0.001f), Tooltip("타겟을 놓쳤을 때 멈춰있는 시간 (최댓값)")]
            public float targetMissTimeMax = 1f;

            [Tooltip("어그로 타입 (공격안함, 비선공, 선공")]
            public AggroType aggroType;
        }

        [Serializable]
        public class EnemyValue
        {
            public Vector3 moveDir; // 로컬 이동방향
            public Vector3 worldMoveDir; // 글로벌 이동방향

            public Vector3 targetDir; // 타겟과의 거리벡터(거리/방향)

            public bool checkForwardCast;
            public RaycastHit checkForwardHit;

            public float patrolCooldown; // 현재 순찰 쿨타임

            public float patrolDuration; // 순찰 지속시간

            public float targetDot; // 타겟과의 내적 각도

            #region .
        //    public Vector3 moveDir; // ??? ????

        //    public Vector3 meshCenter; // ??? ????????? ???
        //    public Vector3 meshExtend; // ??? ????????? ??????

        //    public Vector3 worldMeshCenter; // ????????????? ??? ????????? ???

        //    public Vector3 groundNormal;
        //    public float groundSlopeAngle;
        //    public float forwardSlopeAngle;
        //    public float groundDist;
            #endregion
        }

        [Serializable]
        public class EnemyDebug
        {
            [Tooltip("순찰/추적기능 제거(움직이지않음)")]
            public bool isDontControl = false;
            [Space]

            [Tooltip("디버그 기즈모 표기 여부")]
            public bool isDrawGizmos = true;
            [Space]
            
            [Tooltip("기척 감지 이전 색상")]
            public Color senseDebugColor = new Color(0, 0, 0, 0.5f);

            [Tooltip("기척 감지 색상")]
            public Color detectSenseDebugColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

            [Tooltip("어그로 감지 색상")]
            public Color detectAggroDebugColor = new Color(1, 1, 1, 0.5f);
        }

        [SerializeField] private Components _components = new();
        //[SerializeField] private CheckOption _checkOption = new();
        [SerializeField] private EnemyStats _enemyStats = new();
        [SerializeField] private EnemyState _enemyState = new();
        [SerializeField] private EnemyOption _enemyOption = new();
        [SerializeField] private EnemyValue _enemyValue = new();
        [SerializeField] private EnemyDebug _enemyDebug = new();

        private Components Com => _components;
        //private CheckOption Check => _checkOption;
        private EnemyStats Stats => _enemyStats;
        private EnemyState State => _enemyState;
        private EnemyOption Option => _enemyOption;
        private EnemyValue Value => _enemyValue;
        private EnemyDebug EDebug => _enemyDebug;

        //private Coroutine _moveCoroutine;
        private readonly WaitForFixedUpdate _waitFixedUpdate = new();
        private readonly WaitForEndOfFrame _waitFrame = new();

        private bool _initReady = false; // 초기화 완료 여부
        private float _aggroAngleRad; // 어그로 감지 각도 (라디안)
        private float _targerDotRad; // 타겟과의 내적 각도 (라디안)

        private float _deltaTime;

        private Coroutine _runningCoroutine = null;

        private readonly Vector3 _notReadyCenterValue = new Vector3(13579.13579f, 13579.13579f, 13579.13579f);

        private Transform _debugVar;

        private Vector3 _chaseAvoidDir;

        private bool _isChaseForwardBlocked;

        // public Vector3 MoveDir
        // {
        //     get { return Vector3.zero; }
        //     set
        //     {
        //         MoveSet.SetMovement(value);
        //         transform.rotation = Quaternion.LookRotation(value);
        //     }
        // }

        //private Vector3 _meshCenter; // ??? ????????? ???
        //private Vector3 _meshExtend; // ??? ????????? ??????
        //private Vector3 _meshScaledExtend; // ??? ????????? ???? ????? ??????
        //private float _meshFwdExtend; // ??? ????????? ???? ????????? ????
        //private float _meshDownExtend; // ??? ????????? ???? ????????? ????

        //private Color _debugRed = new Color(1, 0, 0, 0.5f);
        //private Color _debugGreen = new Color(0, 1, 0, 0.5f);
        //private Color _debugBlue = new Color(0, 0, 1, 0.5f);

        //private RaycastHit _hit;

        private IEnemyMoveSet MoveSet => GetComponent<IEnemyMoveSet>();

        public void Start()
        {
            _runningCoroutine = StartCoroutine(InitComponents());
            //InitMeshPoint();
            
        }

        public void Update()
        {
            if(!_initReady) return;

            DetectSense();
            EnemyMove();
            _deltaTime = Time.deltaTime;
        }

        private IEnumerator InitComponents()
        {
            //TryGetComponent(out Com.rb);
            //if(Com.rb == null) Com.rb = gameObject.AddComponent<Rigidbody>();
            /* Com.targetDetectEye = transform.Find("TargetDetectEye"); // 해당 오브젝트 자식에 "TargetDetectEye"를 찾아봄 */
            foreach(Transform child in transform)
            {
                if(child.CompareTag("Root"))
                {
                    Com.enemyRoot = child;
                    break;
                } 
            }

            Com.targetDetectEye = Com.enemyRoot.Find("TargetDetectEye");
            if(Com.targetDetectEye == null) // Com.enemyRoot에 TargetDetectEye가 없는경우
            {
                Com.targetDetectEye = new GameObject("TargetDetectEye").transform; // 생성 후
                Com.targetDetectEye.SetParent(Com.enemyRoot); // 자식으로 설정 후
                while(MoveSet.DetectColCenter() == _notReadyCenterValue) // EnemyMoveSet.cs에 capCol.center값이 생길때까지 대기
                {
                    yield return null;
                }
                Com.targetDetectEye.position = Com.enemyRoot.TransformPoint(MoveSet.DetectColCenter());
                Com.targetDetectEye = Com.enemyRoot.Find("TargetDetectEye"); // 그 이후 다시 "TargetDetecter"를 찾음
            }
            TryGetComponent(out Com.targetDetectCol);
            if(Com.targetDetectCol == null) Com.targetDetectCol = Com.targetDetectEye.AddComponent<SphereCollider>();
            /* TryGetComponent(out Com.col);
            if(Com.col == null) Com.col = gameObject.AddComponent<MeshCollider>(); */

            //Com.meshCenter = transform.GetChildByName("MeshCenter");
            // MoveSet.GetEnemyRoot(Com.enemyRoot);
            Com.target = GameObject.FindGameObjectWithTag("CamControler").transform;
            /* Com.waitPatrol = new(Stats.patrolCooldown);
            Com.waitGroggy = new(Stats.groggyTime);
            Com.dontMoveWaitPatrol = new(Stats.patrolCooldown * Stats.ifPatrolDontMoveCooldownCoef); */

            //Com.rb.constraints = RigidbodyConstraints.FreezeRotation;
            //Com.rb.interpolation = RigidbodyInterpolation.Interpolate;
            //Com.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            //Com.rb.useGravity = false;
            InitState();
            InitColider();
            _initReady = true;
            ExecuteCoroutine(PatrolMove()); // 순찰
        }

        private void InitState()
        {
            State.health = Stats.maxHealth;
            State.stamina = Stats.maxStamina;
            MoveSet.SetMovementSpeed(Stats.speed);
        }

        private void InitColider()
        {
            /* var matrix = Matrix4x4.TRS(Com.targetDetectEye.localPosition, Com.targetDetectEye.localRotation, Com.targetDetectEye.localScale); */
            Com.target.TryGetComponent(out Com.targetCol);
            /* if(_customTargetDetectEye) */ // 커스텀 여부에 따라 Com.targetDetectEye의 위치를 가져오거나
            Com.targetDetectCol.center = Com.targetDetectEye.InverseTransformPoint(Com.targetDetectEye.position)/* matrix.MultiplyPoint(Com.targetDetectEye.localPosition) *//* Vector3.Scale(transform.InverseTransformPoint(Com.targetDetectEye.position), transform.lossyScale) */;
            Com.targetDetectCol.isTrigger = true;
            /* else // EnemyMoveSet에서 메시의 가운데를 가져옴
                Invoke(nameof(DelayMethod), 0.01f); */

            Com.targetDetectCol.radius = Stats.senseRange/*  * Mathf.Max(Mathf.Abs(Com.targetDetectEye.lossyScale.x), Mathf.Abs(Com.targetDetectEye.lossyScale.y), Mathf.Abs(Com.targetDetectEye.lossyScale.z)) */;
        }

        /* private void DelayMethod()
        {
            Com.targetDetectCol.center = MoveSet.DetectColCenter();
        }
 */
        private void ExecuteCoroutine(IEnumerator coroutine)
        {
            if(_runningCoroutine != null) StopCoroutine(_runningCoroutine); // _runningCoroutine에 이미 실행중인 코루틴이있을경우 그 코루틴을 끄고
            _runningCoroutine = StartCoroutine(coroutine); // 다른 코루틴을 실행하면서 _runningCoroutine에 새로 집어넣음
        }

        private IEnumerator PatrolMove()
        {
            while(true)
            {
                if(!EDebug.isDontControl)
                {
                    Value.moveDir = FindNewPoint(); // 이동 방향 지정
                    CalculateMoveDir(); // 이동 방향을 월드벡터로 바꾼 후
                    yield return StartCoroutine(PatrolRotateCoroutine()); // 이동 방향으로 선회가 끝날때까지 대기 후 이동
                    for (float time = 0; time < Value.patrolDuration; time += _deltaTime)
                    { // 이후  순찰 지속시간이 끝나거나
                        if(MoveSet.IsUnableMove()) // 이동이 불가능할때까지 이동방향으로 직진
                            break;
                        yield return _waitFixedUpdate;
                    }
                    MoveSet.StopMoving(); // 순찰 지속시간이 끝나거나 이동이 불가능하면 정지 후
                    State.isMoving = false;
                }
                yield return YieldDefinition.WaitForSeconds(Value.patrolCooldown); // 다음 순찰시간까지 대기
            }
        }

        private IEnumerator PatrolRotateCoroutine()
        {        
            float prevY = Com.enemyRoot.localEulerAngles.y;  //현재 방향
            float nextY = Quaternion.LookRotation(Value.worldMoveDir).eulerAngles.y; //지정된 방향

            if (nextY - prevY > 180f) nextY -= 360;
            else if (prevY - nextY > 180f) nextY += 360;

            // float roteProcess = 0;
            for(float roteProcess = 0; roteProcess < Option.patrolRotateTime; roteProcess += _deltaTime)
            // while(roteProcess < Stats.patrolRotateTime)
            {
                // roteProcess += _deltaTime;
                Com.enemyRoot.localEulerAngles = Vector3.up * Mathf.Lerp(prevY, nextY, roteProcess / Option.patrolRotateTime);
                yield return _waitFrame;
            } // Stats.patrolRotateTime 에 걸쳐 현재 방향에서 지정된 방향으로 선회시킴
            Com.enemyRoot.localEulerAngles = Vector3.up * nextY; 

            // 방향지정이 끝나면 순찰 지속시간을 정하고
            Value.patrolDuration = CalculatePatrolDontMovePercent();
                
            State.isMoving = true; // 이동을 시작함
        }
        
        private Vector3 FindNewPoint()
        {
            float nextAngle = UnityEngine.Random.Range(0, 360) * Mathf.Rad2Deg;
            return new Vector3(Mathf.Sin(nextAngle), 0f, Mathf.Cos(nextAngle)).normalized;
        }

        private float CalculatePatrolDontMovePercent()
        {
            Value.patrolCooldown = Option.defaultPatrolCooldown * 
                UnityEngine.Random.Range(Option.patrolCooldownARandomNumberMin, Option.patrolCooldownARandomNumberMax);

            if(UnityEngine.Random.Range(0f, 100f) <= Option.patrolDontMovePercent
                && Option.patrolDontMovePercent != 0) // 추가적으로 움직이지 않을 확률이 존재하여 움직이지않을경우
            {
                Value.patrolCooldown *= Option.ifPatrolDontMoveCooldownCoef;
                return 0; // Stats.ifPatrolDontMoveCooldownCoef 만큼 순찰 쿨타임을 곱한뒤 선회만 하고 움직이지않음
            }
            else
                return UnityEngine.Random.Range(Option.patrolMoveTimeMin, Option.patrolMoveTimeMax);
                
            /* float randomPercent = UnityEngine.Random.Range(0f, 100f);
                randomPercent <= Stats.patrolDontMovePercent
                && Stats.patrolDontMovePercent != 0 ?   // 추가적으로 움직이지 않을 확률이 존재함
                0 : UnityEngine.Random.Range(Stats.patrolMoveTimeMin, Stats.patrolMoveTimeMax); */
        }

        /*IEnumerator EnemyMove()
        {
            yield return _waitFixedUpdate;
        }*/

        Transform IEnemy.GetEnemyRoot() => Com.enemyRoot;

        void IEnemy.UpdateHealth(float damage)
        {
            State.health -= damage;
            if (State.health <= 0)
            {

            }
        }

        void IEnemy.UpdateStamina(float damage)
        {
            State.stamina -= damage;
            if (State.stamina <= 0)
            {
                StartCoroutine(StaminaZero());
            }
        }

        IEnumerator StaminaZero()
        {
            State.isGroggy = true;
            yield return YieldDefinition.WaitForSeconds(Stats.groggyTime);
            State.isGroggy = false;
        }

        private void OnDrawGizmos()
        {
            if(!EDebug.isDrawGizmos || !_initReady) return; //디버그 기즈모 표기를 꺼놨거나 초기화 준비가 덜 된 경우 표시안함
            
            Handles.color = !State.isDetectSense ? 
                EDebug.senseDebugColor : !State.isAggro ?
                    EDebug.detectSenseDebugColor : EDebug.detectAggroDebugColor;
            Handles.DrawSolidArc(Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center), Vector3.up, Com.enemyRoot.forward, Option.aggroAngle * 0.5f, Com.targetDetectCol.radius);
            Handles.DrawSolidArc(Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center), Vector3.up, Com.enemyRoot.forward, Option.aggroAngle * -0.5f, Com.targetDetectCol.radius);

            Gizmos.color = !State.isDetectSense ? 
                EDebug.senseDebugColor : !State.isAggro ?
                    EDebug.detectSenseDebugColor : EDebug.detectAggroDebugColor;

            if(Value.checkForwardCast)
            {
                Gizmos.DrawRay(Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center), (Value.targetDir.normalized * Value.checkForwardHit.distance) + Vector3.down * 0.1f);
            }
            else
                Gizmos.DrawRay(Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center), (Value.targetDir.normalized * Com.targetDetectCol.radius) + Vector3.down * 0.1f);
        }

        #region 이전 코드
        //private void UpdateValue()
        //{
        //    Value.worldMeshCenter = transform.TransformPoint(Value.meshCenter);
        //}

        //private void CheckGround()
        //{
        //    PublicCurrentValue.groundDist = float.MaxValue;
        //    Value.groundNormal = Vector3.up;
        //    Value.groundSlopeAngle = 0f;
        //    Value.forwardSlopeAngle = 0f;

        //    bool cast = Physics.BoxCast(Value.worldMeshCenter, _meshScaledExtend, Vector3.down, out var hit,
        //        transform.rotation, /*_meshDownExtend + */Check.groundCheckDist, -1, QueryTriggerInteraction.Ignore);

        //    State.isGrounded = false;
        //    if (cast)
        //    {
        //        EDebug.groundHit = hit;
        //        EDebug.hasDetectGround = true;

        //        //???? ?????? ????
        //        Value.groundNormal = hit.normal;

        //        //???? ????? ?????? ??? ?????(?? ??????? ????)
        //        Value.groundSlopeAngle = Vector3.Angle(Value.groundNormal, Vector3.up);
        //        Value.forwardSlopeAngle = Vector3.Angle(Value.groundNormal, Value.moveDir) - 90f;

        //        State.isOnSteepSlope = Value.groundSlopeAngle >= Check.maxSlopeAngle;

        //        Value.groundDist = Mathf.Max(hit.distance /*- _capRadiusDiff*/ - Check.groundCheckThreshold, 0f);

        //        State.isGrounded = (Value.groundDist <= 0.0001f) && !State.isOnSteepSlope;
        //    } else EDebug.hasDetectGround = false;
        //}

        //private void CheckForward()
        //{
        //    bool cast = Physics.BoxCast(Value.worldMeshCenter, _meshScaledExtend, Value.moveDir, out var hit,
        //        transform.rotation, _meshFwdExtend + Check.forwardCheckDist, -1, QueryTriggerInteraction.Ignore);

        //    State.isForwardBlocked = false;
        //    if (cast)
        //    {
        //        EDebug.forwardHit = hit;
        //        EDebug.hasDetectForward = true;

        //        float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
        //        State.isForwardBlocked = forwardObstacleAngle >= Check.maxSlopeAngle;
        //    } else EDebug.hasDetectForward = false;

        //    //UnityEngine.Debug.DrawRay(_meshCenter, Vector3.forward/*Value.moveDir*/ * hit.distance);
        //    //UnityEngine.Debug.Dra(_meshCenter + Vector3.forward/*Value.moveDir*/ * hit.distance, _meshExtend);
        //}

        /*void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(_meshCenter, Vector3.forward * (_meshFwdExtend + Check.forwardCheckDist));
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_meshCenter + Vector3.forward * _hit.distance, _meshScaledExtend * 2f);
        }*/

        /* private void OnTriggerEnter(Collider other)
        {
            if (other.transform == Com.target)
            {
                State.isDetectSense = true;
                if(Stats.aggroType.HasFlag(AggroType.hostility))
                    DetectAggroRange();
            }
        } */

        /* private void OnTriggerExit(Collider other)
        {
            if (other.transform == Com.target && State.isDetectSense)
            {
                State.isDetectSense = false;
                State.isAggro = false;
            }
        } */
        #endregion

        private void CalculateMoveDir()
        {
            Value.worldMoveDir = transform.TransformDirection(Value.moveDir);
        }

        private void DetectSense()
        {
            if(EDebug.isDontControl || !_initReady || State.isAggro) return; //컨트롤 불가능설정을 했거나 준비가 덜됐거나 이미 어그로가 끌린상태에서는 작동안함
            
            /* TransformPoint(Com.targetCol.center) */;
            /* _debugVar[1] = Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center); */

            Value.targetDir = Com.target.TransformPoint(Com.targetCol.center) - Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center); // 타겟과 이어지는 선을 만듬
            if(Value.targetDir.magnitude <= Com.targetDetectCol.radius) // 타겟과 이어진 길이가 기척감지범위보다 좁을 경우
            {
                State.isDetectSense = true; // 기척 감지 여부가 켜지고
                DetectAggroRange(); // 어그로가 끌리는상황인지 감지함 
            }
            else if(Value.targetDir.magnitude > Com.targetDetectCol.radius) // 타겟과 이어진 길이가 기척감지보다 멀 경우
                State.isDetectSense = false; // 기척 감지 여부를 끔
        }   

        private void DetectAggroRange()
        {
            _aggroAngleRad = Mathf.Cos(Option.aggroAngle * Mathf.Deg2Rad * 0.5f);
            _targerDotRad = Vector3.Dot(Com.enemyRoot.forward, Value.targetDir.normalized);
            Value.targetDot = _targerDotRad * Mathf.Rad2Deg * 2f;
            if(_targerDotRad > _aggroAngleRad)
            {
                Value.checkForwardCast =
                    #region CheckForwardCast
                    Physics.Raycast(
                            origin: Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center)/* Com.targetDetectCol.center + (Value.targetDir.normalized * Com.targetDetectCol.radius) */,
                            direction: Value.targetDir + Vector3.down * 0.1f,
                            hitInfo: out Value.checkForwardHit,
                            maxDistance: Com.targetDetectCol.radius,
                            layerMask: -1,
                            QueryTriggerInteraction.Ignore);
                    #endregion

                _debugVar = Value.checkForwardHit.transform;
                /* (Value.checkForwardCast, Value.checkForwardHit) = MoveSet.CheckObstacle(Value.targetDir, Com.targetDetectCol.radius); */
                if(Value.checkForwardCast && Value.checkForwardHit.transform == Com.target)
                {
                    State.isAggro = true;
                    StopAllCoroutines(); // 적을 발견시 모든 코루틴 중지 후
                    _runningCoroutine = null;
                    State.isMoving = false; 
                    MoveSet.StopMoving(); // 모든 움직임을 중지
                    ExecuteCoroutine(ChaseMove()); // 이후 추격 시작
                    /* _runningCoroutine = StartCoroutine(ChaseMove());  */
                }
                
            }
        }

        private IEnumerator ChaseMove()
        {
            Com.targetDetectCol.radius = Stats.senseRange * Option.chaseRangeCoef; // 추격 시작 전 기척 감지범위를 지정한 배율만큼 늘림 (*= Stats.chaseRangeCoef)
            MoveSet.SetMovementSpeed(Stats.speed * Option.chaseMovementMultiply); // 그리고 이동속도를 지정한 배율만큼 변경함
            /* Value.targetDir = Com.target.TransformPoint(Com.targetCol.center) - Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center); // 타겟과 이어지는 벡터를 갱신
            Value.worldMoveDir = Value.targetDir.normalized; // 이동방향을 타겟쪽으로 설정 */
            while(Value.targetDir.magnitude <= Com.targetDetectCol.radius) // 타겟과 이어진 길이가 늘어난 기척감지범위보다 길어질때까지 반복
            {
                if(!EDebug.isDontControl)
                {
                    Value.targetDir = Com.target.TransformPoint(Com.targetCol.center) - Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center); // 타겟과 이어지는 벡터를 갱신
                    Value.worldMoveDir = Value.targetDir.normalized; // 이동방향을 타겟쪽으로 설정
                    (_isChaseForwardBlocked, _chaseAvoidDir) = MoveSet.IsChaseCheckForward(Option.chaseForwardCheckDist, Option.chaseAvoidAngleInterV); // 전방 장애물 검사
                    while(_isChaseForwardBlocked) // 전방에 장애물이 없을때 까지 반복
                    {
                        Value.targetDir = Com.target.TransformPoint(Com.targetCol.center) - Com.targetDetectEye.TransformPoint(Com.targetDetectCol.center); // 타겟과 이어지는 벡터를 지속적으로 갱신하고
                        Value.worldMoveDir = _chaseAvoidDir; // 이동방향을 우회로로 설정
                        _isChaseForwardBlocked = MoveSet.IsAvoidCheckForward(Option.chaseForwardCheckDist, Value.targetDir); // 타겟 방향의 장애물 검사
                        yield return _waitFixedUpdate;
                    }
                    State.isMoving = true;
                    yield return _waitFixedUpdate;
                }
                
            }
            MoveSet.StopMoving(); // 타겟과 이어진 길이가 늘어난 기척감지범위보다 길어지면 정지 후
            State.isMoving = false;

            Com.targetDetectCol.radius = Stats.senseRange; // 다시 기척 감지범위를 원상태로 돌려놓음
            MoveSet.SetMovementSpeed(Stats.speed); // 그리고 원래 이동속도로 변경함
            State.isDetectSense = false;
            State.isAggro = false; // 기척감지, 어그로감지 상태를 끈 후
            yield return YieldDefinition.WaitForSeconds(
                UnityEngine.Random.Range(Option.targetMissTimeMin, Option.targetMissTimeMax)); // 지정한 만큼 가만히 있다가(감지는 할 수 있음)
            ExecuteCoroutine(PatrolMove()); // 다시 순찰 상태로 돌아감
        }

        private void EnemyMove()
        {
            if(!State.isMoving || EDebug.isDontControl || !_initReady) return;
            // CalculateMoveDir();
            ApplyRotate();
            MoveSet.SetMovement(Value.worldMoveDir);
            
        }

        private void ApplyRotate()
        {
            float currentY = Com.enemyRoot.localEulerAngles.y;
            float nextY = Quaternion.LookRotation(Value.worldMoveDir).eulerAngles.y;

            if (nextY - currentY > 180f) nextY -= 360;
            else if (currentY - nextY > 180f) nextY += 360;

            Com.enemyRoot.localEulerAngles = Vector3.up * Mathf.Lerp(currentY, nextY, 0.3f);
        }
    }
}