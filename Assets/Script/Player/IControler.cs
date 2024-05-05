using Game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Game.Definition;

public interface IControler
    {
        void EndPosCam(); //패리 모션이 끝난이후 tps카메라 변경명령

        //void SetActionConst(ActionConst cannotAction); //모션동안 다른 모션을 사용할 수 없는 명령 (선택)

        //void AddActionConst(ActionConst cannotAction); //모션동안 다른 모션을 사용할 수 없는 명령 (추가)

        //void SubActionConst(ActionConst cannotAction); //모션동안 다른 모션을 사용할 수 없는 명령 (제거)

        //void AllActionConst(bool action); //모션동안 다른 모션을 사용할 수 없는 명령 (일괄, 비활성화가능)

        //ActionConst IsActionConst();

    //void SendRootMotionRot(Quaternion rootMotionRot);
}