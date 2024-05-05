// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// namespace Game
// {
//     public class EnemyDefinition : MonoBehaviour
//     {
//         [Serializable]
//         public class Components
//         {
//             public Transform enemyRoot;
//         }

//         [SerializeField] private Components _components => new();

//         private Components Com => _components;
        
//         public void Awake()
//         {
//             InitComponents();
//         }

//         public void InitComponents()
//         {
//             foreach(Transform child in transform)
//             {
//                 if(child.CompareTag("Root"))
//                 {
//                     Com.enemyRoot = child;
//                     break;
//                 } 
//             }
//         }
//     }
// }