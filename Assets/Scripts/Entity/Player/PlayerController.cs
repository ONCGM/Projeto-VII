using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Enemies;
using Game;
using Items;
using UI;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Composites;
using UnityEngine.SceneManagement;

namespace Entity.Player {
    /// <summary>
    /// Controls anything related to the player.
    /// </summary>
    public class PlayerController : Entity {
        #pragma warning disable 0649
        [Header("Movement")] [SerializeField, Range(0f, 25f)]
        private float storeSpeed = 9f;

        [SerializeField, Range(0f, 25f)] private float worldSpeed = 15f;
        [SerializeField, Range(0f, 2f)] private float rotationSpeed = 0.1f;
        [SerializeField, Range(0f, 2f)] private float minRotationInput = 0.075f;
        private float playerSpeed = 1f;
        private Vector3 movementScale = Vector3.zero;

        [Header("Melee Attacks")] [SerializeField]
        private LayerMask enemyLayer = 11;

        [SerializeField] private SphereCollider swordCollider;
        [SerializeField, Range(1, 25)] private int meleeDamage = 10;
        [SerializeField, Range(0.01f, 3f)] private float meleeDamageComboMultiplier = .2f;
        private int comboNumber;

        [Header("Ranged Attacks")] [SerializeField, Range(1, 50)]
        private int rangedDamagePerBullet = 15;

        [SerializeField, Range(1, 9)] private int rangedBulletsPerAttack = 1;
        [SerializeField, Range(0f, 90f)] private float bulletSpreadAngle = 70f;
        [SerializeField, Range(2f, 55f)] private float rangedAttackMaxRange = 10f;
        [SerializeField] private Transform bulletSpawnPosition;
        [SerializeField] private GameObject bulletPrefab;

        [Header("Inventory")] [SerializeField] private int playerInventorySize = 10;

        private Inventory inventory;

        /// <summary>
        /// The player's inventory.
        /// </summary>
        public Inventory Inventory {
            get => inventory;
            set => inventory = value;
        }


        // Input
        private ActionInputs inputs;

        private static readonly List<int> ComboAnim = new List<int> {
            Animator.StringToHash("Combo_0"),
            Animator.StringToHash("Combo_1"),
            Animator.StringToHash("Combo_2")
        };

        private static readonly int SpecialAnim = Animator.StringToHash("Special");

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop;

        [SerializeField] private GameObject DamageCanvasPrefab;

        /// <summary>
        /// Is the player inside the shop? True for yes.
        /// </summary>
        public bool IsInsideShop {
            get => isInsideShop;
            set {
                isInsideShop = value;
                playerSpeed = (isInsideShop ? storeSpeed : worldSpeed);
                if(agent != null) agent.speed = playerSpeed;
                PlayerStatsUI.UpdateUiValues.Invoke();
            }
        }

        /// <summary>
        /// Allows the player to move or not.
        /// </summary>
        public bool CanMove { get; set; } = true;

        #pragma warning restore 0649

        #region Unity Events

        protected override void Awake() {
            base.Awake();
            inputs = new ActionInputs();
            inputs.Player.Attack.performed += ComboAttack;
            inputs.Player.Special.performed += SpecialAttack;
            if(ReferenceEquals(swordCollider, null)) swordCollider = GetComponentInChildren<SphereCollider>();
            playerSpeed = (isInsideShop ? storeSpeed : worldSpeed);
            agent.speed = playerSpeed;
            inventory = new Inventory(playerInventorySize, new List<InventoryItemEntry>());
            GameMaster.OnGameExecutionStateUpdated += UpdatePlayerMovementToMatchGameState;
            
            InvokeRepeating(nameof(RecoverStamina), 1f, 1f);
        }

        /// <summary>
        /// Sets the player movement state based on the game execution state.
        /// </summary>
        private void UpdatePlayerMovementToMatchGameState() {
            CanMove = (GameMaster.Instance.GameState == ExecutionState.Normal);
        }
        
        /// <summary>
        /// Recovers the player stamina at a steady rate.
        /// </summary>
        private void RecoverStamina() { 
            Stamina = Mathf.Clamp(Stamina + 2, 0, maxStamina);
            PlayerStatsUI.UpdateUiValues.Invoke();
        }

        // OnEnable Unity Event, enables input.
        private void OnEnable() {
            inputs.Enable();
        }

        // OnDisable Unity Event, disables input.
        private void OnDisable() {
            inputs.Disable();
        }

        // Unsubscribes from events.
        private void OnDestroy() {
            GameMaster.OnGameExecutionStateUpdated -= UpdatePlayerMovementToMatchGameState;
        }


        // Update events.
        private void Update() {
            GetInput();
        }
        
        private void FixedUpdate() {
            MovePlayer();
        }
        
        #endregion

        #region Entity Base Overrides

        public override void Kill() {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion
        
        private void GetInput() {
            movementScale = new Vector3(inputs.Player.Horizontal.ReadValue<float>(), 0f, inputs.Player.Vertical.ReadValue<float>());

            if(movementScale.magnitude < minRotationInput || !CanMove) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementScale), rotationSpeed);
        }

        /// <summary>
        /// Moves the player based on input using the nav mesh agent.
        /// </summary>
        private void MovePlayer() {
            if(agent.enabled && CanMove) agent.Move(movementScale * (playerSpeed * Time.deltaTime));
        }

        /// <summary>
        /// Player special attack. Fires a pistol.
        /// </summary>
        private void SpecialAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3) return;
            Stamina -= 4;
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Player_Attack_Special_Anim")) return;
            agent.enabled = false;
            anim.SetTrigger(SpecialAnim);
            PlayerStatsUI.UpdateUiValues.Invoke();
        }

        /// <summary>
        /// Called by animation to unlock player movement.
        /// </summary>
        public void SpecialAttackRelease() {
            var angleOffsetPerBullet = bulletSpreadAngle / rangedBulletsPerAttack;
            var position = bulletSpawnPosition.position;

            for(var i = 0; i < rangedBulletsPerAttack; i++) {
                var rotation = bulletSpawnPosition.rotation.eulerAngles;
                
                if(rangedBulletsPerAttack > 1) {
                    rotation.y -= bulletSpreadAngle * 0.5f;
                    rotation.y += angleOffsetPerBullet * i;
                }

                var bullet = Instantiate(bulletPrefab, position, Quaternion.Euler(rotation)).GetComponent<EntityBullet>();
                bullet.Damage = rangedDamagePerBullet;
                bullet.MaxRange = rangedAttackMaxRange;
                bullet.InitialPosition = position;
            }
            
            agent.enabled = true;
        }

        /// <summary>
        /// Player basic attack with three with combo.
        /// </summary>
        private void ComboAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3) return;
            Stamina -= 3;
            
            
            if(anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                comboNumber = 0;
                anim.ResetTrigger(ComboAnim[0]);
                anim.ResetTrigger(ComboAnim[1]);
                anim.ResetTrigger(ComboAnim[2]);
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
            
            PlayerStatsUI.UpdateUiValues.Invoke();
        }
        
        /// <summary>
        /// Called by animation to trigger collision check.
        /// </summary>
        public void ComboAttackCollision() {
            if(CheckCollisionSword(swordCollider)) {
                //TODO: Play audio and effects.
                Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                    .GetComponent<DamageCanvas>().damageValue = meleeDamage;
            }
        }
        
        /// <summary>
        /// Test collisions for melee hits.
        /// </summary>
        /// <param name="referenceSphere"> Weapon sphere collider. </param>
        private bool CheckCollisionSword(SphereCollider referenceSphere) {
            var hitAnEnemy = false;
            var damageAmount = Mathf.RoundToInt(meleeDamage * (1f + meleeDamageComboMultiplier * comboNumber));

            var contacts = new Collider[]{};
            Physics.OverlapSphereNonAlloc(referenceSphere.transform.position, referenceSphere.radius, contacts, enemyLayer, QueryTriggerInteraction.Ignore);

            foreach(var contact in contacts) {
                if(!contact.gameObject.GetComponent<Enemy>()) continue;
                contact.gameObject.GetComponent<Enemy>().Damage(damageAmount);
                hitAnEnemy = true;
            }

            return hitAnEnemy;
        }
    }
}