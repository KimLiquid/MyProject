using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor.ShaderGraph;
using UnityEditorInternal;
using UnityEngine;
using static Game.Definition;
using static UnityEngine.Rendering.DebugUI;

namespace Game
{
    public class EnemyMoveSet : MonoBehaviour, IEnemyMoveSet
    {
        public enum CollideType : byte
        {
            [Tooltip("박스")]
            box = 0,

            [Tooltip("캡슐")]
            capsule = 1
        }

        //public enum MeshMthod : byte
        //{
        //    [Tooltip("경계로 계산")]
        //    useBounds = 0,

        //    [Tooltip("꼭짓점으로 계산")]
        //    useVertex = 1
        //}

        [Serializable]
        public class Components
        {
            // public Transform enemyRoot;

            public Rigidbody rb;
            //public Transform target;
            //public SphereCollider targetDetectCol;
            public CapsuleCollider capCol; // CollideType가 캡슐일때만 사용

            public BoxCollider boxCol; // CollideType가 박스일때만 사용

            // public Transform meshCenter;
            // public BoxCollider meshBoundCol; //EDebug.isMeshBoundVisbleToCol = true; 일 경우 사용
        }

        [Serializable]
        public class EnemyOption
        {
            [Tooltip("충돌 타입")]
            public CollideType collideType = 0;

            [Tooltip("콜라이더타입이 캡슐일 시 직접 설정 여부")]
            public bool setColliderSelf = false;

            //[Tooltip("매쉬 계산방식")]
            //public MeshMthod meshMthod = 0;

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

            public Vector3 meshCenter; // 해당 오브젝트의 중심
            public Vector3 meshExtend; // 해당 오브젝트의 반지름

            public Vector3 meshScaledExtend; // 해당 오브젝트의 크키에 비례한 반지름
            public float meshFwdExtend; // 해당 오브젝트의 중심과 정면사이의 길이
            public float meshDownExtend; // 해당 오브젝트의 중심과 바닥면사이의 길이

            public Vector3 worldMeshCenter; // 월드좌표에서의 해당 오브젝트의 중심

            public float castRadius; // 레이케스트 시 캡슐의 반지름
            public float capRadiusDiff; // 캡슐의 반지름과 레이케스트 시 캡슐의 반지름의 차이

            public Vector3 groundNormal;
            public Vector3 groundCross;
            public float groundSlopeAngle;
            public float forwardSlopeAngle;
            public float slopeAccel;
            public float groundDist;

            public float gravity;
            public Vector3 hVelocity;
        }

        [Serializable]
        public class EnemyState
        {
            public bool isMoving; // 이동중인가

            public bool isForwardBlocked; // 앞 길이 막혀있는가
            public bool isGrounded; // 공중에 떠있는가

            public bool isOnSteepSlope; // 등반 불가능한 경사로인가
        }

        [Serializable]
        public class EnemyDebug
        {
            // [Tooltip("매쉬의 경계를 콜라이더로 표기 (에디터에서만 수정가능, )")]
            // public bool isMeshBoundVisbleToCol = false;

            [Tooltip("이동기능 제거")]
            public bool isDontMove = false;
            [Space]

            [Tooltip("디버그 기즈모 표기 여부")]
            public bool isDrawGizmos = true;
            [Space]

            [Tooltip("정면 감지 이전 선/큐브 색상")]
            public Color forwardDebugColor/* { get; }*/ = new Color(0, 0, 1, 0.5f);
            [Tooltip("지면 감지 이전 선/큐브 색상")]
            public Color groundDebugColor/* { get; }*/ = new Color(0, 1, 0, 0.5f);
            
            [Space]

            [Tooltip("정면 감지 선/큐브 색상")]
            public Color detectForwardDebugColor/* { get; }*/ = new Color(1, 0, 1, 0.5f);
            [Tooltip("지면 감지 선/큐브 색상")]
            public Color detectGroundDebugColor/* { get; }*/ = new Color(1, 1, 0, 0.5f);

            [Space]
            public bool hasDetectForward; //정면 감지 여부
            public bool hasDetectGround; //지면 감지 여부

            public RaycastHit forwardHit;
            public RaycastHit groundHit;

            [Space]
            
            [Tooltip("디버그로 점을 찍을 때 색상")]
            public Color debugDotColor = new Color(1, 0, 0, 0.5f);
            
            [Tooltip("디버그로 점을 찍을 때 색상(보조)")]
            public Color debugSubDotColor = new Color(0, 1, 1, 0.5f);

            public bool useDot; // 점 사용 여부

            public Vector3 dotPosition; // 점 위치

            public bool useSubDot; // 보조 점 사용 여부

            public Vector3 subDotPosition; // 보조 점 위치
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
        private EnemyDefinition.Components ShareCom => new();

        private float _fixedDeltaTime;

        // private Vector3 CapTopCenterPoint;
        // private Vector3 CapBottomCenterPoint;
        private IEnemy Control => GetComponent<IEnemy>();

        private Vector3 CapTopCenterPoint
            => Option.collideType == CollideType.capsule ? new Vector3(transform.position.x, transform.position.y + (Com.capCol != null ? Com.capCol.height - Com.capCol.radius : 0), transform.position.z) : Vector3.zero;
        private Vector3 CapBottomCenterPoint
            => Option.collideType == CollideType.capsule ? new Vector3(transform.position.x, transform.position.y + (Com.capCol != null ? Com.capCol.radius : 0), transform.position.z) : Vector3.zero;
        // private Vector3 BoxMeshCenterPoint
        //     => Option.collideType == CollideType.box ? transform.TransformPoint(Value.meshCenter) : Vector3.zero;

        private void OnEnable()
        {
            InitComponents();
            InitCollidePoint();
        }

        private void FixedUpdate()
        {
            UpdateValues();
            UpdateGravity();

            CheckGround();
            CheckForward();

            CalculateMove();
            ApplyMovementToRigidbody();

            _fixedDeltaTime = Time.fixedDeltaTime;
        }

        private void InitComponents()
        {
            // while(true)
            // {
            //     if(ShareCom.enemyRoot != null)
            //         break;
            // } // Com.enemyRoot가 null이 아닐때까지 대기

            ShareCom.enemyRoot.TryGetComponent(out Com.rb);
            if (Com.rb == null) Com.rb = ShareCom.enemyRoot.AddComponent<Rigidbody>();

            // if(EDebug.isMeshBoundVisbleToCol && Option.collideType == CollideType.box)
            // {
            //     TryGetComponent(out Com.meshBoundCol);
            //     if(Com.meshBoundCol == null) Com.meshBoundCol = gameObject.AddComponent<BoxCollider>();

            //     Com.meshBoundCol.isTrigger = true;
            // }

            #region 안쓰는코드
            //if (Com.rb == null) Com.rb = gameObject.AddComponent<Rigidbody>();
            //TryGetComponent(out Com.targetDetectCol);
            //if (Com.targetDetectCol == null) Com.targetDetectCol = gameObject.AddComponent<SphereCollider>();
            //TryGetComponent(out Com.col);
            //if (Com.col == null) Com.col = gameObject.AddComponent<MeshCollider>();
            #endregion

            if (Option.collideType == CollideType.box)
            {
                ShareCom.enemyRoot.TryGetComponent(out Com.boxCol);
                if (Com.boxCol == null) Com.boxCol = ShareCom.enemyRoot.AddComponent<BoxCollider>();
                // Com.meshCenter = transform.Find("MeshCenter");
                // if (Com.meshCenter == null)  // 해당 오브젝트 자식에 "MeshCenter"가 없는지 확인
                // {
                //     Com.meshCenter = new GameObject("MeshCenter").transform; // 없으면 생성 후
                //     Com.meshCenter.SetParent(transform); // 해당 오브젝트의 자식으로 설정
                //     Com.meshCenter = transform.Find("MeshCenter"); // 그 이후 다시 "MeshCenter"을 찾음
                // }
            }
            else if (Option.collideType == CollideType.capsule)
            {
                ShareCom.enemyRoot.TryGetComponent(out Com.capCol);
                if (Com.capCol == null) Com.capCol = ShareCom.enemyRoot.AddComponent<CapsuleCollider>();
            }
            #region 안쓰는코드
            //Com.target = GameObject.FindGameObjectWithTag("CamControler").transform;
            //Com.waitPatrol = new(Stats.patrolCooldown);
            //Com.waitGroggy = new(Stats.groggyTime);
            #endregion
            
            Com.rb.constraints = RigidbodyConstraints.FreezeRotation;
            Com.rb.interpolation = RigidbodyInterpolation.Interpolate;
            Com.rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            Com.rb.useGravity = false;
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
            
            switch (Option.collideType)
            {
                case CollideType.box:
                    var rArr = GetComponentsInChildren<Renderer>(); //Renderer 확인
                    if (rArr.Length > 0)
                    {
                        Quaternion currentRotation = ShareCom.enemyRoot.rotation;
                        Vector3 currentScale = ShareCom.enemyRoot.localScale;
                        ShareCom.enemyRoot.rotation = Quaternion.Euler(Vector3.zero);
                        ShareCom.enemyRoot.localScale = Vector3.one;
                        Bounds sumBounds = rArr[0].bounds/*new(transform.position, Vector3.zero)*/;

                        foreach (var r in rArr)
                        {
                            sumBounds.Encapsulate(r.bounds);
                            /*_meshCenter = transform.InverseTransformPoint(r.bounds.center);
                            _meshExtend = r.bounds.extents;*/
                        }
                        /*_meshCenter = transform.InverseTransformPoint(sumBounds.center);
                        _meshExtend = sumBounds.extents;*/
                        Com.boxCol.center = sumBounds.center - ShareCom.enemyRoot.position;
                        Com.boxCol.size = Vector3.Scale(sumBounds.size, ShareCom.enemyRoot.localScale);
                        // Value.meshCenter = sumBounds.center - transform.position;
                        // Value.meshExtend = sumBounds.extents;

                        ShareCom.enemyRoot.rotation = currentRotation;
                        ShareCom.enemyRoot.localScale = currentScale;
                    }

                    // Value.worldMeshCenter = transform.TransformPoint(Value.meshCenter);

                    // Com.meshCenter.localPosition = Value.meshCenter;
                    /*_meshFwdExtend = sumBounds.size;*//*Mathf.Abs(transform.InverseTransformPoint(sumBounds.max).z) + Mathf.Abs(transform.InverseTransformPoint(sumBounds.center).z)*/
                    //_meshDownExtend = Mathf.Abs(sumBounds.max.y - sumBounds.center.y);

                    // Value.meshScaledExtend = Vector3.Scale(Value.meshExtend, transform.localScale);
                    Value.meshFwdExtend = Value.meshScaledExtend.z;
                    Value.meshDownExtend = Value.meshScaledExtend.y;

                    /* if (EDebug.isMeshBoundVisbleToCol)
                    {
                        Com.meshBoundCol.center = Value.meshCenter;
                        Com.meshBoundCol.size = Value.meshScaledExtend * 2;
                    } */
                    break;

                case CollideType.capsule:
                    if (!Option.setColliderSelf) // 직접설정을 하지 않을 시 자동으로 지정 
                    {
                        /*Vector3 maxVertex = new(float.MinValue, float.MinValue, float.MinValue);
                        Vector3 minVertex = new(float.MaxValue, float.MaxValue, float.MaxValue);*/
                        float maxHeight = -1f;

                        //SkinnedMeshRenderer 확인
                        var smrArr = GetComponentsInChildren<SkinnedMeshRenderer>();
                        if (smrArr.Length > 0)
                        {
                            foreach (var smr in smrArr) //모든 SkinnedMeshRenderer 에서
                            {
                                foreach (var vertex in smr.sharedMesh.vertices) //모든 메쉬의 꼭짓점 확인
                                {
                                    if (maxHeight < vertex.y)
                                        maxHeight = vertex.y;
                                    /*if (maxVertex.x < vertex.x)
                                        maxVertex.x = vertex.x;
                                    if (maxVertex.y < vertex.y)
                                        maxVertex.y = vertex.y;
                                    if (maxVertex.z < vertex.z)
                                        maxVertex.z = vertex.z;

                                    if (minVertex.x > vertex.x)
                                        minVertex.x = vertex.x;
                                    if (minVertex.y > vertex.y)
                                        minVertex.y = vertex.y;
                                    if (minVertex.z > vertex.z)
                                        minVertex.z = vertex.z;*/
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
                                    foreach (var vertex in mf.mesh.vertices) //모든 메쉬의 꼭짓점 확인
                                    {
                                        if (maxHeight < vertex.y)
                                            maxHeight = vertex.y;
                                        /*if (maxVertex.x < vertex.x)
                                            maxVertex.x = vertex.x;
                                        if (maxVertex.y < vertex.y)
                                            maxVertex.y = vertex.y;
                                        if (maxVertex.z < vertex.z)
                                            maxVertex.z = vertex.z;

                                        if (minVertex.x > vertex.x)
                                            minVertex.x = vertex.x;
                                        if (minVertex.y > vertex.y)
                                            minVertex.y = vertex.y;
                                        if (minVertex.z > vertex.z)
                                            minVertex.z = vertex.z;*/
                                    }
                                }
                            }
                        }

                        //캡슐 콜라이더 값 설정
                        if (maxHeight <= 0) maxHeight = 1f;

                        float center = maxHeight * 0.5f;

                        Com.capCol.height = maxHeight;
                        Com.capCol.center = Vector3.up * center;
                        Com.capCol.radius = 0.2f;
                    }

                    Value.castRadius = Com.capCol.radius * 0.9f;
                    Value.capRadiusDiff = Com.capCol.radius - Value.castRadius + 0.05f;

                    // CapTopCenterPoint =
                    //     new Vector3(transform.position.x, transform.position.y + Com.capCol.height - Com.capCol.radius, transform.position.z);
                    // CapBottomCenterPoint = 
                    //     new Vector3(transform.position.x, transform.position.y + Com.capCol.radius, transform.position.z);


                    /*UnityEngine.Debug.DrawLine(maxVertex, minVertex);
                    Value.meshCenter = Vector3.Lerp(maxVertex, minVertex, 0.5f);
                    Value.meshExtend = maxVertex;*/
                    break;
            }

            //EnemyControl.DetectColCenter = Value.meshCenter;
            /*Com.testCol = gameObject.AddComponent<BoxCollider>();
            Com.testCol.center = _meshCenter;
            Com.testCol.size = _meshSize;*/
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

        Vector3 IEnemyMoveSet.DetectColCenter() => Value.meshCenter;

        bool IEnemyMoveSet.IsUnableMove() => State.isForwardBlocked && !State.isGrounded;

        private void OnDrawGizmos()
        {
            if (!EDebug.isDrawGizmos) return; //디버그 기즈모 표기를 꺼놓은경우 표시안함

            if (EDebug.hasDetectForward)
            {
                Gizmos.color = EDebug.detectForwardDebugColor;
                if(Option.collideType == CollideType.box)
                {
                    Gizmos.DrawRay(Value.worldMeshCenter, Value.worldMoveDir * EDebug.forwardHit.distance);
                    Gizmos.matrix = Matrix4x4.TRS(Value.worldMeshCenter + Value.worldMoveDir * EDebug.forwardHit.distance, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(Vector3.zero, Value.meshExtend * 2);
                }
                else
                    Gizmos.DrawRay(CapTopCenterPoint, Value.worldMoveDir + Vector3.down * 0.1f);

            }
            else
            {
                Gizmos.color = EDebug.forwardDebugColor;
                if (Option.collideType == CollideType.box)
                    Gizmos.DrawRay(Value.worldMeshCenter, Value.worldMoveDir * (Value.meshFwdExtend + Check.forwardCheckDist));
                else
                    Gizmos.DrawRay(CapTopCenterPoint, Value.worldMoveDir + Vector3.down * 0.1f);
            }

            if (EDebug.hasDetectGround)
            {
                Gizmos.color = EDebug.detectGroundDebugColor;
                if (Option.collideType == CollideType.box)
                {
                    Gizmos.DrawRay(Value.worldMeshCenter, Vector3.down * /*Mathf.Max(*/EDebug.groundHit.distance/* - Check.groundCheckThreshold, 0f)*/);
                    Gizmos.matrix = Matrix4x4.TRS(Value.worldMeshCenter + Vector3.down * EDebug.groundHit.distance, transform.rotation, transform.lossyScale);
                    Gizmos.DrawCube(Vector3.zero, Value.meshExtend * 2);
                }
                else
                    Gizmos.DrawRay(CapBottomCenterPoint, Vector3.down);
            }
            else
            {
                //Gizmos.matrix = Matrix4x4.TRS(Value.worldMeshCenter, transform.rotation, transform.lossyScale);
                //Gizmos.matrix = Matrix4x4.Rotate(transform.rotation);
                Gizmos.color = EDebug.groundDebugColor;
                if (Option.collideType == CollideType.box)
                    Gizmos.DrawRay(Value.worldMeshCenter, Vector3.down * (Value.meshDownExtend + Check.groundCheckDist));
                else
                    Gizmos.DrawRay(CapBottomCenterPoint, Vector3.down);

            }

            if(EDebug.useDot)
            {
                Gizmos.color = EDebug.debugDotColor;
                Gizmos.DrawSphere(EDebug.dotPosition,1f);
            }

            if(EDebug.useSubDot)
            {
                Gizmos.color = EDebug.debugSubDotColor;
                Gizmos.DrawSphere(EDebug.subDotPosition,1f);
            }
        }

        private void UpdateValues()
        {
            Value.worldMeshCenter = transform.TransformPoint(Value.meshCenter);
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

        private void CheckGround()
        {
            Value.groundDist = float.MaxValue;
            Value.groundNormal = Vector3.up;
            Value.groundSlopeAngle = 0f;
            Value.forwardSlopeAngle = 0f;

            bool cast = 
                Option.collideType == CollideType.box ?
                Physics.BoxCast(Value.worldMeshCenter, Value.meshScaledExtend, Vector3.down,
                out var hit, transform.rotation, /*_meshDownExtend + */Check.groundCheckDist, Check.groundLayerMask, QueryTriggerInteraction.Ignore)
                :
                Physics.SphereCast(CapBottomCenterPoint, Value.castRadius, Vector3.down,
                out hit, Check.groundCheckDist, Check.groundLayerMask , QueryTriggerInteraction.Ignore);
            // Debug.DrawRay(CapBottomCenterPoint, Vector3.down, Color.green);

            State.isGrounded = false;
            EDebug.hasDetectGround = cast;

            if (cast)
            {
                EDebug.groundHit = hit;

                //지면 노멀벡터 초기화
                Value.groundNormal = hit.normal;

                //현재 위치한 지면의 경사각 구하기(캐릭터 이동방향 고려)
                Value.groundSlopeAngle = Vector3.Angle(Value.groundNormal, Vector3.up);
                Value.forwardSlopeAngle = Vector3.Angle(Value.groundNormal, Value.worldMoveDir) - 90f;

                State.isOnSteepSlope = Value.groundSlopeAngle >= Check.maxSlopeAngle;

                Value.groundDist = Mathf.Max(hit.distance - Value.capRadiusDiff - Check.groundCheckThreshold, 0f);

                State.isGrounded = (Value.groundDist <= 0.0001f) && !State.isOnSteepSlope;
            }

            //월드 이동벡터 회전축
            Value.groundCross = Vector3.Cross(Value.groundNormal, Vector3.up);

            #region .
            // PublicCurrentValue.groundDist = float.MaxValue;
            // Value.groundNormal = Vector3.up;
            // Value.groundSlopeAngle = 0f;
            // Value.forwardSlopeAngle = 0f;

            // bool cast = 
            //     Option.collideType == CollideType.box ? //충돌타입이 박스일경우
            //     Physics.BoxCast(Value.worldMeshCenter, Value.meshScaledExtend, Vector3.down, out var hit,
            //     transform.rotation, /*_meshDownExtend + */Check.groundCheckDist, -1, QueryTriggerInteraction.Ignore)
            //     : //충돌타입이 캡슐일경우
            //     Physics.SphereCast(CapBottomCenterPoint, Value.castRadius, Vector3.down,
            //     out hit, Check.groundCheckDist, Check.groundLayerMask, QueryTriggerInteraction.Ignore);

            // State.isGrounded = false;
            // if (cast)
            // {
            //     EDebug.groundHit = hit;
            //     EDebug.hasDetectGround = true;

            //     //지면 노멀벡터 초기화
            //     Value.groundNormal = hit.normal;

            //     //현재 위치한 지면의 경사각 구하기(몹 이동방향 고려)
            //     Value.groundSlopeAngle = Vector3.Angle(Value.groundNormal, Vector3.up);
            //     Value.forwardSlopeAngle = Vector3.Angle(Value.groundNormal, Value.moveDir) - 90f;

            //     State.isOnSteepSlope = Value.groundSlopeAngle >= Check.maxSlopeAngle;

            //     Value.groundDist =
            //         Option.collideType == CollideType.box ? //충돌타입이 박스일경우
            //         Mathf.Max(hit.distance - Check.groundCheckThreshold, 0f)
            //         : //충돌타입이 캡슐일경우
            //         Mathf.Max(hit.distance - Value.capRadiusDiff - Check.groundCheckThreshold, 0f);

            //     State.isGrounded = (Value.groundDist <= 0.0001f) && !State.isOnSteepSlope;
            // }
            // else EDebug.hasDetectGround = false;

            // Value.groundCross = Vector3.Cross(Value.groundNormal, Vector3.up);
            #endregion
        }

        private void CheckForward()
        {
            bool cast =
                Option.collideType == CollideType.box ? //충돌타입이 박스일경우
                Physics.BoxCast(Value.worldMeshCenter, Value.meshScaledExtend, Value.worldMoveDir, out var hit,
                transform.rotation, Value.meshFwdExtend + Check.forwardCheckDist, -1, QueryTriggerInteraction.Ignore)
                : //충돌타입이 캡슐일경우
                Physics.CapsuleCast(CapBottomCenterPoint, CapTopCenterPoint, Value.castRadius, Value.worldMoveDir + Vector3.down * 0.1f,
                out hit, Check.forwardCheckDist, -1, QueryTriggerInteraction.Ignore);

            State.isForwardBlocked = false;
            EDebug.hasDetectForward = cast;

            if (cast)
            {
                EDebug.forwardHit = hit;

                float forwardObstacleAngle = Vector3.Angle(hit.normal, Vector3.up);
                State.isForwardBlocked = forwardObstacleAngle >= Check.maxSlopeAngle;
            }

            //UnityEngine.Debug.DrawRay(_meshCenter, Vector3.forward/*Value.moveDir*/ * hit.distance);
            //UnityEngine.Debug.Dra(_meshCenter + Vector3.forward/*Value.moveDir*/ * hit.distance, _meshExtend);
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