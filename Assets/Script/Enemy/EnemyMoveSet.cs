using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.ShaderGraph;
using UnityEditorInternal;
using UnityEngine;
using static Game.Definition;
using static UnityEngine.Rendering.DebugUI;

namespace Game
{
    // [RequireComponent(typeof(Rigidbody))]
    public class EnemyMoveSet : MonoBehaviour, IEnemyMoveSet
    {

        [Serializable]
        public class Components
        {
            public Transform enemyRoot;
            public Rigidbody rb;
            public CapsuleCollider capCol; // CollideType가 캡슐일때만 사용
        }

        [Serializable]
        public class EnemyOption
        {
            [Tooltip("콜라이더 직접 설정 여부")]
            public bool setColliderSelf = false;

            [Range(0f, 4f), Tooltip("경사로 이동속도 변화율")]
            public float slopeAccel = 1f;

            [Range(-9.81f, 0f), Tooltip("중력 가속도")]
            public float gravityAccel = -9.81f;
        }

        [Serializable]
        public class EnemyValue
        {
            public Vector3 worldMoveDir; // 이동 방향
            public float moveSpeed; // 이동 속도

            public float castRadius; // 레이케스트 시 캡슐의 반지름
            public float capRadiusDiff; // 캡슐의 반지름과 레이케스트 시 캡슐의 반지름의 차이

            public RaycastHit forwardHit; // 정면 레이케스트
            public RaycastHit groundHit; // 지면 레이케스트
            public RaycastHit chaseForwardHit; // 추격시 정면 레이케스트

            public Vector3 groundNormal;
            public Vector3 groundCross;
            public float groundSlopeAngle;
            public float forwardSlopeAngle;
            public float slopeAccel;
            public float groundDist;
            public Vector3 chaseAvoidDir;

            public float gravity;
            public Vector3 hVelocity;
        }

        [Serializable]
        public class EnemyState
        {
            public bool isMoving; // 이동중인가

            public bool isForwardBlocked; // 앞 길이 막혀있는가
            public bool isGrounded; // 공중에 떠있는가

            public bool isChaseForwardBlocked; // 추격하는 경로에 막고있는 물체가 있는가

            public bool isOnSteepSlope; // 등반 불가능한 경사로인가
        }

        [Serializable]
        public class EnemyDebug
        {

            [Tooltip("이동기능 제거")]
            public bool isDontMove = false;
            [Space]

            [Tooltip("디버그 기즈모 표기 여부")]
            public bool isDrawGizmos = true;
            [Space]

            [Tooltip("정면 감지 이전 선 색상")]
            public Color forwardDebugColor/* { get; }*/ = new Color(0, 0, 1, 0.5f);
            [Tooltip("지면 감지 이전 선 색상")]
            public Color groundDebugColor/* { get; }*/ = new Color(0, 1, 0, 0.5f);
            
            [Space]

            [Tooltip("정면 감지 선 색상")]
            public Color detectForwardDebugColor/* { get; }*/ = new Color(1, 0, 1, 0.5f);
            [Tooltip("지면 감지 선 색상")]
            public Color detectGroundDebugColor/* { get; }*/ = new Color(1, 1, 0, 0.5f);

            [Space]
            public bool hasDetectForward; //정면 감지 여부
            public bool hasDetectGround; //지면 감지 여부

            [Space]
            
            [Tooltip("디버그로 점을 찍을 때 색상")]
            public Color debugDotColor = new Color(1, 0, 0, 0.5f);
            
            [Tooltip("디버그로 점을 찍을 때 색상(보조)")]
            public Color debugSubDotColor = new Color(0, 1, 1, 0.5f);

            public bool useDot; // 점 사용 여부

            public List<Vector3> dotPositions = new(); // 점 위치

            public bool useSubDot; // 보조 점 사용 여부

            public List<Vector3> subDotPositions = new(); // 보조 점 위치

            public float dotSize = 0.05f; // 점의 크기
        }

        [SerializeField] private Components _components = new();
        [SerializeField] private CheckOption _checkOption = new();
        [SerializeField] private EnemyOption _enemyOption = new();
        [SerializeField] private EnemyState _enemyState = new();
        [SerializeField] private EnemyValue _enemyValue = new();
        [SerializeField] private EnemyDebug _enemyDebug = new();

        private Components Com => _components;
        private CheckOption Check => _checkOption;
        private EnemyOption Option => _enemyOption;
        private EnemyState State => _enemyState;
        private EnemyValue Value => _enemyValue;
        private EnemyDebug EDebug => _enemyDebug;

        /* private Transform[] _debugVar = new Transform[10];
        private bool[] _debugVarBool = new bool[10]; */

        private float _checkAngle;
        private readonly Vector3 _notReadyCenterValue = new Vector3(13579.13579f, 13579.13579f, 13579.13579f);
        private float _fixedDeltaTime;
        private bool _initColiderReady = false;
        /* private float _checkAngleInterVRad; */

        private IEnemy Control => GetComponent<IEnemy>();

        private Vector3 CapTopCenterPoint
            => new Vector3(transform.position.x, transform.position.y + (Com.capCol != null ? Com.capCol.height - Com.capCol.radius : 0), transform.position.z);
        private Vector3 CapBottomCenterPoint
            => new Vector3(transform.position.x, transform.position.y + (Com.capCol != null ? Com.capCol.radius : 0), transform.position.z);
       
        private void Start()
        {
            StartCoroutine(InitComponents());
        }

        private void FixedUpdate()
        {
            if(!_initColiderReady) return;
            
            UpdateValues();
            UpdateGravity();

            CheckGround();
            CheckForward();

            CalculateMove();
            ApplyMovementToRigidbody();

            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        private IEnumerator InitComponents()
        {
            while(Control.GetEnemyRoot() == null)  // EnemyControler.cs에 enemyRoot값이 생길때까지 대기
                yield return null; 
            Com.enemyRoot = Control.GetEnemyRoot(); // EnemyControler.cs에 있는 enemyRoot를 가져옴

            gameObject.TryGetComponent(out Com.rb);
            if (Com.rb == null) Com.rb = gameObject.AddComponent<Rigidbody>();

            Com.enemyRoot.TryGetComponent(out Com.capCol);
            if (Com.capCol == null) Com.capCol = Com.enemyRoot.gameObject.AddComponent<CapsuleCollider>();
   
            Com.rb.constraints = RigidbodyConstraints.FreezeRotation;
            Com.rb.interpolation = RigidbodyInterpolation.Interpolate;
            Com.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Com.rb.useGravity = false;

            InitCollidePoint();
        }

        private void InitCollidePoint()
        {
            /*렌더러를 모두 탐색하여 각각 x,y,z의 최고값을 구함
            float maxPosX = 0;
            float maxPosY = 0;
            float maxPosZ = 0;
            Vector3 boundCenter = new();

            float maxPosX = 0;
            float maxPosY = 0;
            float maxPosZ = 0;

            float minPosX = 0;
            float minPosY = 0;
            float minPosZ = 0;

            Vector3 minMeshPoint = new();
            Vector3 maxMeshPoint = new();*/
            #region .
            ////kinnedMeshRenderer 확인
            //var smrArr = GetComponentsInChildren<SkinnedMeshRenderer>();
            //if (smrArr.Length > 0)
            //{
            //    bool oneArr = true;
            //    foreach (var smr in smrArr)
            //    {
            //        if (oneArr)
            //        {
            //            _meshCenter = transform.InverseTransformPoint(smr.bounds.center);
            //            //_meshCenter = smr.sharedMesh.bounds.center;
            //            _meshExtend = smr.sharedMesh.bounds.extents;
            //            oneArr = false;
            //        }

            //        //_meshCenter = (_meshCenter + smr.sharedMesh.bounds.center) * 0.5f;
            //        _meshExtend = smr.sharedMesh.bounds.extents;
            //        /*foreach (var vertex in smr.sharedMesh.vertices)
            //        {
            //            if (maxPosX < vertex.x)
            //                maxPosX = vertex.x;

            //            if (minPosX > vertex.x)
            //                minPosX = vertex.x;

            //            if (maxPosY < vertex.y)
            //                maxPosY = vertex.y;

            //            if (minPosY > vertex.y)
            //                minPosY = vertex.y;

            //            if (maxPosZ < vertex.z)
            //                maxPosZ = vertex.z;

            //            if (minPosZ > vertex.z)
            //                minPosZ = vertex.z;
            //        }*/
            //    }
            //}
            //else //MeshFilter 확인
            //{
            //    var mfArr = GetComponentsInChildren<MeshFilter>();
            //    if (mfArr.Length > 0)
            //    {
            //        bool oneArr = true;
            //        foreach (var mf in mfArr)
            //        {
            //            if (oneArr)
            //            {
            //                _meshCenter = mf.mesh.bounds.center;
            //                _meshExtend = mf.sharedMesh.bounds.extents;
            //                oneArr = false;
            //            }

            //            _meshCenter = (_meshCenter + mf.sharedMesh.bounds.center) * 0.5f;
            //            _meshExtend = mf.sharedMesh.bounds.extents;
            //            /*foreach (var vertex in mf.mesh.vertices)
            //            {
            //                if (maxPosX < vertex.x)
            //                    maxPosX = vertex.x;

            //                if (minPosX > vertex.x)
            //                    minPosX = vertex.x;

            //                if (maxPosY < vertex.y)
            //                    maxPosY = vertex.y;

            //                if (minPosY > vertex.y)
            //                    minPosY = vertex.y;

            //                if (maxPosZ < vertex.z)
            //                    maxPosZ = vertex.z;

            //                if (minPosZ > vertex.z)
            //                    minPosZ = vertex.z;
            //            }*/
            //        }
            //    }
            //}
            #endregion
            /*_meshCenter = ( (Vector3.right * maxPosX) + (Vector3.up * maxPosY) + (Vector3.forward * maxPosZ) ) * 0.5f;
            Com.meshCenter.localPosition = _meshCenter;
            _meshExtend = new Vector3(maxPosX, maxPosY, maxPosZ);
            Value.topCenterPoint = transform.TransformDirection(transform.position.x, maxTop, transform.position.z);
            Value.forwardCenterPoint = transform.TransformDirection(transform.position.x, transform.position.y, maxForward);
            _meshCenter = new Vector3((maxPosX + minPosX) * 0.5f, (maxPosY + minPosY) * 0.5f, (maxPosZ + minPosZ) * 0.5f);
            _meshExtend = new Vector3(1, 1, 1); */
            
            if (!Option.setColliderSelf) // 직접설정을 하지 않을 시 자동으로 지정 
            {
                /*Vector3 maxVertex = new(float.MinValue, float.MinValue, float.MinValue);
                Vector3 minVertex = new(float.MaxValue, float.MaxValue, float.MaxValue);*/
                float maxHeight = -1f;
                /* float minHeight = -1f; */

                //SkinnedMeshRenderer 확인
                var smrArr = GetComponentsInChildren<SkinnedMeshRenderer>();
                if (smrArr.Length > 0)
                {
                    foreach (var smr in smrArr) //모든 SkinnedMeshRenderer 에서
                    {
                        var matrix = Matrix4x4.TRS(smr.gameObject.transform.localPosition, smr.gameObject.transform.localRotation, smr.gameObject.transform.localScale);
                        foreach (var vertex in smr.sharedMesh.vertices) //모든 메쉬의 꼭짓점 확인
                        {
                            var realVertex = matrix.MultiplyPoint(vertex)/* smr.gameObject.transform.localPosition + vertex */;

                            EDebug.dotPositions.Add(transform.TransformPoint(realVertex));
                            
                            if (maxHeight < realVertex.y)
                                maxHeight = realVertex.y;

                            /* if (minHeight > vertex.y)
                                minHeight = vertex.y; */
                        }
                    }
                }
                else //MeshFilter 확인
                {
                    var mfArr = GetComponentsInChildren<MeshFilter>();
                    if (mfArr.Length > 0)
                    {
                        foreach (var mf in mfArr) //모든 MeshFilter 에서
                        {
                            var matrix = Matrix4x4.TRS(mf.gameObject.transform.localPosition, mf.gameObject.transform.localRotation, mf.gameObject.transform.localScale);
                            foreach (var vertex in mf.mesh.vertices) //모든 메쉬의 꼭짓점 확인
                            {
                                var realVertex = matrix.MultiplyPoint(vertex)/* mf.gameObject.transform.localPosition + vertex */;
                                EDebug.subDotPositions.Add(transform.TransformPoint(realVertex));

                                if (maxHeight < realVertex.y)
                                    maxHeight = realVertex.y;

                                /* if (minHeight > vertex.y)
                                    minHeight = vertex.y; */
                            }
                        }
                    }
                }

                //캡슐 콜라이더 값 설정
                // if (maxHeight <= 0) maxHeight = 1f;
                /* float height =
                    (TransformPointFloat(maxHeight, 'y') - TransformPointFloat(minHeight, 'y')) / transform.lossyScale.y;
                float center = 
                    InverseTransformPointFloat(
                        (TransformPointFloat(maxHeight, 'y') + TransformPointFloat(minHeight, 'y')) * 0.5f , 'y'
                    ); */
                float center = maxHeight * 0.5f;

                Com.capCol.height = maxHeight;
                Com.capCol.center = Vector3.up * center;
                Com.capCol.radius = Math.Clamp(Com.capCol.height * 0.5f, 0f, 0.2f);
            }

            Value.castRadius = Com.capCol.radius * 0.9f;
            Value.capRadiusDiff = Com.capCol.radius - Value.castRadius + 0.05f;

            _initColiderReady = true;
        }

        /// <summary>
        /// 특정한 방향만 로컬 벡터에서 월드 벡터로 바꿉니다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction">
        /// 방향에 대한 인수
        /// <para>x, y, z 중 하나 입력 ( 이 외의 값이 들어갈 경우 0 반환 )</para>
        /// </param>
        /// <returns></returns>
        float TransformPointFloat(float value, char direction)
        {
            return direction switch
            {
                'x' => transform.TransformPoint(value, 0, 0).x,
                'y' => transform.TransformPoint(0, value, 0).y,
                'z' => transform.TransformPoint(0, 0, value).z,
                _ => 0,
            };
        }

        /// <summary>
        /// 특정한 방향만 월드 벡터에서 로컬 벡터로 바꿉니다.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="direction">
        /// 방향에 대한 인수
        /// <para>x, y, z 중 하나 입력 ( 이 외의 값이 들어갈 경우 0 반환 )</para>
        /// </param>
        /// <returns></returns>
        float InverseTransformPointFloat(float value, char direction)
        {
            return direction switch
            {
                'x' => transform.InverseTransformPoint(value, 0, 0).x,
                'y' => transform.InverseTransformPoint(0, value, 0).y,
                'z' => transform.InverseTransformPoint(0, 0, value).z,
                _ => 0,
            };
        }

        // void IEnemyMoveSet.GetEnemyRoot(in Transform enemyRoot)
        // {
        //     Com.enemyRoot = enemyRoot;
        // }

        void IEnemyMoveSet.SetMovement(in Vector3 worldMoveDir)
        {
            Value.worldMoveDir = worldMoveDir;
            State.isMoving = worldMoveDir.sqrMagnitude > 0.01;
        }

        void IEnemyMoveSet.StopMoving()
        {
            Value.worldMoveDir = Vector3.zero;
            State.isMoving = false;
        }

        void IEnemyMoveSet.SetMovementSpeed(float speed)
        {
            Value.moveSpeed = speed;
        }

        Vector3 IEnemyMoveSet.DetectColCenter() => 
            _initColiderReady == true ? Com.capCol.center : _notReadyCenterValue;

/*         (bool cast, RaycastHit hit) IEnemyMoveSet.CheckObstacle(in Vector3 dir, float maxDistance)
        {
            #region CheckForwardCast
            bool cast = Physics.Raycast(
                origin: transform.TransformPoint(Com.capCol.center) + (dir.normalized * Com.capCol.radius),
                direction: dir + Vector3.down * 0.1f,
                hitInfo: out var hit,
                maxDistance: maxDistance,
                layerMask: -1,
                QueryTriggerInteraction.Collide);
            #endregion
            _debugVarBool[0] = cast;
            _debugVar[0] = hit.transform;
            return (cast, hit);
        } */

        bool IEnemyMoveSet.IsUnableMove() => State.isForwardBlocked || (State.isForwardBlocked && !State.isGrounded);

        (bool isChaseForwardBlocked, Vector3 chaseAvoidDir) IEnemyMoveSet.IsChaseCheckForward(float chaseForwardCheckDist, float chaseAvoidAngleInterV)
        {   
            bool cast = 
                #region ChaseCheckForwardCast
                Physics.CapsuleCast(
                    point1: CapBottomCenterPoint,
                    point2: CapTopCenterPoint,
                    radius: Value.castRadius,
                    direction: Value.worldMoveDir + Vector3.down * 0.1f,
                    hitInfo: out Value.chaseForwardHit,
                    maxDistance: chaseForwardCheckDist,
                    layerMask: (-1) - (1 << LayerMask.NameToLayer("Player")), // 장애물 탐지대상에서 플레이어는 제외함
                    QueryTriggerInteraction.Ignore);
                #endregion

            State.isChaseForwardBlocked = false;

            if (cast)
            {   
                
                float forwardObstacleAngle = Vector3.Angle(Value.chaseForwardHit.normal, Vector3.up);
                if (forwardObstacleAngle >= Check.maxSlopeAngle)
                {
                    State.isChaseForwardBlocked = true;
                    /* _checkAngleInterVRad = Mathf.Cos(chaseAvoidAngleInterV * Mathf.Deg2Rad * 0.5f); */
                    for(_checkAngle = chaseAvoidAngleInterV;
                        _checkAngle < 180;
                        _checkAngle = _checkAngle + chaseAvoidAngleInterV >= 180 ? 180 : _checkAngle + chaseAvoidAngleInterV)
                    {
                        bool rightCast =
                            #region ChaseAvoidCastRight
                            Physics.CapsuleCast(
                                point1: CapBottomCenterPoint,
                                point2: CapTopCenterPoint,
                                radius: Value.castRadius,
                                direction: (Quaternion.Euler(0, _checkAngle, 0) * Value.worldMoveDir) + Vector3.down * 0.1f,
                                hitInfo: out var rightHit,
                                maxDistance: chaseForwardCheckDist,
                                layerMask: (-1) - (1 << LayerMask.NameToLayer("Player")), // 장애물 탐지대상에서 플레이어는 제외함
                                QueryTriggerInteraction.Ignore);
                            #endregion
                        UnityEngine.Debug.DrawRay(Vector3.Lerp(CapTopCenterPoint, CapBottomCenterPoint, 0.5f), (Quaternion.Euler(0, _checkAngle, 0) * Value.worldMoveDir), Color.cyan);
                        float rightObstacleAngle = Vector3.Angle(rightHit.normal, Vector3.up);
                        if(rightObstacleAngle < Check.maxSlopeAngle || !rightCast)
                        {
                            Value.chaseAvoidDir = Quaternion.Euler(0, _checkAngle, 0) * Value.worldMoveDir;
                            break;
                        }
                        
                        bool leftCast =
                            #region ChaseAvoidCastLeft
                            Physics.CapsuleCast(
                                point1: CapBottomCenterPoint,
                                point2: CapTopCenterPoint,
                                radius: Value.castRadius,
                                direction: (Quaternion.Euler(0, -_checkAngle, 0) * Value.worldMoveDir) + Vector3.down * 0.1f,
                                hitInfo: out var leftHit,
                                maxDistance: chaseForwardCheckDist,
                                layerMask: (-1) - (1 << LayerMask.NameToLayer("Player")), // 장애물 탐지대상에서 플레이어는 제외함
                                QueryTriggerInteraction.Ignore);
                            #endregion
                        UnityEngine.Debug.DrawRay(Vector3.Lerp(CapTopCenterPoint, CapBottomCenterPoint, 0.5f), (Quaternion.Euler(0, -_checkAngle, 0) * Value.worldMoveDir), Color.cyan);
                        float leftObstacleAngle = Vector3.Angle(leftHit.normal, Vector3.up);
                        if(leftObstacleAngle < Check.maxSlopeAngle || !leftCast)
                        {
                            Value.chaseAvoidDir = Quaternion.Euler(0, -_checkAngle, 0) * Value.worldMoveDir;
                            break;
                        }

                        if(_checkAngle == 180) Value.chaseAvoidDir = Quaternion.Euler(0, 180, 0) * Value.worldMoveDir;
                    }
                }
            }
            return (State.isChaseForwardBlocked, Value.chaseAvoidDir);
        }

        bool IEnemyMoveSet.IsAvoidCheckForward(float chaseForwardCheckDist, in Vector3 targetDir)
        {
            bool cast = 
                #region AvoidCheckTargetCast
                Physics.CapsuleCast(
                    point1: CapBottomCenterPoint,
                    point2: CapTopCenterPoint,
                    radius: Value.castRadius,
                    direction: targetDir + Vector3.down * 0.1f,
                    hitInfo: out var hit,
                    maxDistance: chaseForwardCheckDist,
                    layerMask: (-1) - (1 << LayerMask.NameToLayer("Player")), // 장애물 탐지대상에서 플레이어는 제외함
                    QueryTriggerInteraction.Ignore);
                #endregion

            if(cast)
            {
                float obstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
                return obstacleAngle >= Check.maxSlopeAngle;
            }
            return false;
        }
        private void OnDrawGizmos()
        {
            if (!EDebug.isDrawGizmos || !_initColiderReady) return; //디버그 기즈모 표기를 꺼놨거나 collide 준비가 덜 된 경우 표시안함

            if (EDebug.hasDetectForward)
            { 
                Gizmos.color = EDebug.detectForwardDebugColor;
                Gizmos.DrawRay(CapTopCenterPoint, Value.worldMoveDir + Vector3.down * 0.1f);

            }
            else
            { 
                Gizmos.color = EDebug.forwardDebugColor;
                Gizmos.DrawRay(CapTopCenterPoint, Value.worldMoveDir + Vector3.down * 0.1f);
            }

            if (EDebug.hasDetectGround)
            { 
                Gizmos.color = EDebug.detectGroundDebugColor;
                Gizmos.DrawRay(CapBottomCenterPoint, Vector3.down);
            }
            else
            {
                Gizmos.color = EDebug.groundDebugColor;
                Gizmos.DrawRay(CapBottomCenterPoint, Vector3.down * Check.groundCheckDist);

            }

            if(EDebug.useDot)
            {
                Gizmos.color = EDebug.debugDotColor;
                foreach(Vector3 dotPos in EDebug.dotPositions)
                {
                    Gizmos.DrawWireSphere(dotPos, EDebug.dotSize);
                }
                // Gizmos.DrawSphere(EDebug.dotPositions[EDebug.dotPositions.Count],1f);
            }

            if(EDebug.useSubDot)
            {
                Gizmos.color = EDebug.debugSubDotColor;
                foreach(Vector3 dotPos in EDebug.subDotPositions)
                {
                    Gizmos.DrawWireSphere(dotPos, EDebug.dotSize);
                }
            }


        }

        private void UpdateValues()
        {
            //INFO: 비어있음
        }

        private void UpdateGravity()
        {
            if (State.isGrounded)
            {
                Value.gravity = 0;
            }
            else
                Value.gravity += _fixedDeltaTime * Option.gravityAccel;
        }

        private void CheckForward()
        {
            bool cast = 
                #region CheckForwardCast
                Physics.CapsuleCast(
                    point1: CapBottomCenterPoint,
                    point2: CapTopCenterPoint,
                    radius: Value.castRadius,
                    direction: Value.worldMoveDir + Vector3.down * 0.1f,
                    hitInfo: out Value.forwardHit,
                    maxDistance: Check.forwardCheckDist,
                    layerMask: -1,
                    QueryTriggerInteraction.Ignore);
                #endregion

            State.isForwardBlocked = false;
            EDebug.hasDetectForward = cast;

            if (cast)
            {
                float forwardObstacleAngle = Vector3.Angle(Value.forwardHit.normal, Vector3.up);
                State.isForwardBlocked = forwardObstacleAngle >= Check.maxSlopeAngle;
            }

            //UnityEngine.Debug.DrawRay(_meshCenter, Vector3.forward/*Value.moveDir*/ * hit.distance);
            //UnityEngine.Debug.Dra(_meshCenter + Vector3.forward/*Value.moveDir*/ * hit.distance, _meshExtend);
        }

        private void CheckGround()
        {
            Value.groundDist = float.MaxValue;
            Value.groundNormal = Vector3.up;
            Value.groundSlopeAngle = 0f;
            Value.forwardSlopeAngle = 0f;

            bool cast = 
                #region CheckGroundCast
                Physics.SphereCast(
                    origin: CapBottomCenterPoint,
                    radius: Value.castRadius,
                    direction: Vector3.down,
                    hitInfo: out Value.groundHit,
                    maxDistance: Check.groundCheckDist,
                    layerMask: Check.groundLayerMask,
                    QueryTriggerInteraction.Ignore);
                #endregion

            State.isGrounded = false;
            EDebug.hasDetectGround = cast;

            if (cast)
            {

                //지면 노멀벡터 초기화
                Value.groundNormal = Value.groundHit.normal;

                //현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Value.groundSlopeAngle = Vector3.Angle(Value.groundNormal, Vector3.up);
                Value.forwardSlopeAngle = Vector3.Angle(Value.groundNormal, Value.worldMoveDir) - 90f;

                State.isOnSteepSlope = Value.groundSlopeAngle >= Check.maxSlopeAngle;

                Value.groundDist = Mathf.Max(Value.groundHit.distance - Value.capRadiusDiff - Check.groundCheckThreshold, 0f);

                State.isGrounded = (Value.groundDist <= 0.0001f) && !State.isOnSteepSlope;
            }

            //월드 이동벡터 회전축
            Value.groundCross = Vector3.Cross(Value.groundNormal, Vector3.up);
        }

        private void CalculateMove()
        {
            //x,z 이동속도 계산
            if (State.isForwardBlocked && !State.isGrounded)
            {
                Value.hVelocity = Vector3.zero;
            }
            else
            {
                float speed = !State.isMoving ? 0f :
                                Value.moveSpeed;

                Value.hVelocity = Value.worldMoveDir * speed;
            }

            //지상이거나 지면에 가까운 높이일 때 x,z 벡터 회전
            if (State.isGrounded || Value.groundDist < Check.groundCheckDist)
            {
                if(State.isMoving && !State.isForwardBlocked)
                {
                    if (Option.slopeAccel > 0f)
                    {
                        bool isPlus = Value.forwardSlopeAngle >= 0f;
                        float absFsAngle = isPlus ? Value.forwardSlopeAngle : -Value.forwardSlopeAngle;
                        float accel = Option.slopeAccel * absFsAngle * 0.01111f + 1f;
                        Value.slopeAccel = !isPlus ? accel : 1f / accel;

                        Value.hVelocity *= Value.slopeAccel;
                    }

                    Value.hVelocity = 
                        Quaternion.AngleAxis(-Value.groundSlopeAngle, Value.groundCross) * Value.hVelocity;
                }
            }
        }

        private void ApplyMovementToRigidbody()
        {
            if (!EDebug.isDontMove)
            {
                Com.rb.velocity = Value.hVelocity + Vector3.up * Value.gravity;
            }
        }
    }
}