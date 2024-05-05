using Game;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngineInternal;
using static Game.Definition;

public class PlayerIK : MonoBehaviour
{
    private Animator anim;

    private Transform LeftHandMount;
    private Transform RightHandMount;

    [SerializeField] private LayerMask layerMask;

    //private IAnimation Animation => GetComponent<IAnimation>();

    //[HideInInspector] public IKWeapon UseIK;

    [Range(0f, 1f), Tooltip("발과 땅 사이의 거리")]
    [SerializeField] private float FootToGroundDist;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnAnimatorIK()
    {
        if (anim)
        {
            #region 손 IK
            //왼손 초기설정
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f); //포지션과
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f); //회전의 무게 설정

            //오른손 초기설정
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f); //포지션과
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f); //회전의 무게 설정

            //UseIK = Animation.IsUseIK();

            do
            {
                if (PublicPlayerState.useIK.HasFlag(IKWeapon.PistolFire)) // 총을 조준하고있는 상태에서 사격 시
                {
                    if (GameObject.Find("Pistol_Left_Hand") == null) break; // Pistol을 잡는 손을 찾지 못할 시 if에서 탈출

                    LeftHandMount = GameObject.Find("Pistol_Left_Hand").transform;
                    RightHandMount = GameObject.Find("Pistol_Right_Hand").transform;
                    #region 이전 코드
                    // == null ? GameObject.Find("mixamorig:LeftHand"/*"Null_Left_Hand"*/).transform : GameObject.Find("Pistol_Left_Hand").transform;
                    // == null ? GameObject.Find("mixamorig:RightHand"/*"Null_Right_Hand"*/).transform : GameObject.Find("Pistol_Right_Hand").transform;
                    #endregion

                    //왼손
                    anim.SetIKPosition(AvatarIKGoal.LeftHand, LeftHandMount.position);
                    anim.SetIKRotation(AvatarIKGoal.LeftHand, LeftHandMount.rotation);

                    //오른손
                    anim.SetIKPosition(AvatarIKGoal.RightHand, RightHandMount.position);
                    anim.SetIKRotation(AvatarIKGoal.RightHand, RightHandMount.rotation);
                }
            } while (false);

            #endregion

            #region 발 IK
            //왼발
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f); //포지션과
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f); //회전의 무게 설정

            Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out var hit, FootToGroundDist + 1f, layerMask))
                if(anim.GetBool("isGround"))
                {
                    Vector3 footPos = hit.point;
                    footPos.y += FootToGroundDist;
                    anim.SetIKPosition(AvatarIKGoal.LeftFoot, footPos);
                    anim.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }

            //오른발
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f); //포지션과
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f); //회전의 무게 설정

            ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up, Vector3.down);
            if (Physics.Raycast(ray, out hit, FootToGroundDist + 1f, layerMask))
                if (anim.GetBool("isGround"))
                {
                    Vector3 footPos = hit.point;
                    footPos.y += FootToGroundDist;
                    anim.SetIKPosition(AvatarIKGoal.RightFoot, footPos);
                    anim.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.LookRotation(transform.forward, hit.normal));
                }
            #endregion
        }
    }
}
