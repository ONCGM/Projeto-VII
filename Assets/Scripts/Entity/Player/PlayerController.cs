using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;

namespace Entity.Player {
    public class PlayerController : Entity {
        [Header("Movement")]
        [SerializeField, Range(0f, 25f)] private float walkSpeed = 9f;
        [SerializeField, Range(0f, 25f)] private float runSpeed = 15f;
        [SerializeField, Range(0f, 2f)] private float rotationSpeed = 0.1f;
        [SerializeField, Range(0f, 2f)] private float minRotationInput = 0.075f;
        private float playerSpeed = 1f;
        private Vector3 movementScale = Vector3.zero;

        [Header("Attacks")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private SphereCollider swordCollider;
        private int comboNumber;
        
        // Input
        private ActionInputs inputs;

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop = true;

        private static readonly List<int> ComboAnim = new List<int>{Animator.StringToHash("Combo_0"),
                                                                Animator.StringToHash("Combo_1"),
                                                                Animator.StringToHash("Combo_2")};

        /// <summary>
        /// Is the player inside the shop? True for yes.
        /// </summary>
        public bool IsInsideShop {
            get => isInsideShop;
            set {
                isInsideShop = value;
                playerSpeed = (isInsideShop ? walkSpeed : runSpeed);
                if(agent != null) agent.speed = playerSpeed;
            }
        }

        protected override void Awake() {
            base.Awake();
            inputs = new ActionInputs();
            inputs.Player.Attack.performed += ComboAttack;
            if(ReferenceEquals(swordCollider, null)) swordCollider = GetComponentInChildren<SphereCollider>();
            playerSpeed = (isInsideShop ? walkSpeed : runSpeed);
            agent.speed = playerSpeed;
        }

        #region Enable Disable Evens
        
        // OnEnable Unity Event, enables input.
        private void OnEnable() {
            inputs.Enable();
        }

        // OnDisable Unity Event, disables input.
        private void OnDisable() {
            inputs.Disable();
        }
        #endregion
        
        private void Start() { }

        private void Update() {
            GetInput();
        }

        private void GetInput() {
            movementScale = new Vector3(inputs.Player.Horizontal.ReadValue<float>(), 0f, inputs.Player.Vertical.ReadValue<float>());

            if(movementScale.magnitude < minRotationInput) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementScale), rotationSpeed);
        }

        private void FixedUpdate() {
            MovePlayer();
        }

        /// <summary>
        /// Moves the player based on input using the nav mesh agent.
        /// </summary>
        private void MovePlayer() {
            agent.Move(movementScale * (playerSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Player basic attack with three with combo.
        /// </summary>
        private void ComboAttack(InputAction.CallbackContext callbackContext) {
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                comboNumber = 0;
            }
            
            if(comboNumber < 1) {
                anim.SetTrigger(ComboAnim[comboNumber]);
                comboNumber++;
            } else if(comboNumber < 2) {
                anim.SetTrigger(ComboAnim[comboNumber]);
                comboNumber++;
            }  else if(comboNumber < 3) {
                anim.SetTrigger(ComboAnim[comboNumber]);
                comboNumber = 0;
            } else {
                comboNumber = 0;
            }
        }
        
        /// <summary>
        /// Called by animation to trigger collision check.
        /// </summary>
        public void ComboAttackCollision() {
            if(CheckCollisionSword(swordCollider)) {
                // Play audio and effects.
            }
        }
        
        /// <summary>
        /// Test collisions for melee hits.
        /// </summary>
        /// <param name="referenceSphere"> Weapon sphere collider. </param>
        private bool CheckCollisionSword(SphereCollider referenceSphere) {
            bool hitAnEnemy = false;
            
            Collider[] contacts = Physics.OverlapSphere(referenceSphere.transform.position, referenceSphere.radius,
                                                        enemyLayer, QueryTriggerInteraction.Ignore);

            foreach(var contact in contacts) {
                // Damage Enemies.
                if(true) {
                    // damaged an enemy
                    hitAnEnemy = true;
                }
            }

            return hitAnEnemy;
        }
    }
}