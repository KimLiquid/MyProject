using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public interface IEnemy
    {
        Transform GetEnemyRoot(); // enemyRoot 값을 리턴
        void UpdateHealth(float damage);
        void UpdateStamina(float damage);
    }
}