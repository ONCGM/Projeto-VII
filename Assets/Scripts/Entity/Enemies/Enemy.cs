using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entity.Enemies {
    /// <summary>
    /// Base enemy class.
    /// </summary>
    public class Enemy : Entity {
        [Header("Player Settings")]
        [SerializeField] private string playerTag = "Player";
        [SerializeField] private LayerMask playerLayer = 10;
        
        //[Header("Enemy Settings")]
        //[SerializeField]
    }
}