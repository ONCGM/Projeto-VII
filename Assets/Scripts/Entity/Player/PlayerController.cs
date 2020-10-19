using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Enemies;
using Game;
using Items;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Entity.Player {
    /// <summary>
    /// Controls anything related to the player.
    /// </summary>
    public class PlayerController : Entity {
        #pragma warning disable 0649
        [Header("Movement")]
        [SerializeField, Range(0f, 25f)] private float storeSpeed = 9f;
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
        private Gamepad currentGamepad;
        private WaitForSeconds waitTime = new WaitForSeconds(0f);

        private static readonly List<int> ComboAnim = new List<int> {
            Animator.StringToHash("Combo_0"),
            Animator.StringToHash("Combo_1"),
            Animator.StringToHash("Combo_2")
        };

        private static readonly int SpecialAnim = Animator.StringToHash("Special");

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop;
        
        [Header("Settings")]
        private const string upgradeSettingsPath = "Scriptables/Player/Player_Upgrade_Settings";
        [SerializeField] private PlayerUpgradeSettings upgradeSettings;
        
        [Header("Other")]
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
                PlayerStatsUI.UpdateUiValues?.Invoke();
            }
        }

        // TODO set stats to be based on player settings and such.
        
        /// <summary>
        /// Allows the player to move or not.
        /// </summary>
        public bool CanMove { get; set; } = true;

        /// <summary>
        /// The last enemy that dealt damage to the player.
        /// </summary>
        public Enemy LastEnemyToHitPlayer { get; set; }

        #pragma warning restore 0649

        #region Unity Events

        protected override void Awake() {
            base.Awake();
            inputs = new ActionInputs();
            currentGamepad = Gamepad.current;
            inputs.Player.Attack.performed += ComboAttack;
            inputs.Player.Special.performed += SpecialAttack;
            if(ReferenceEquals(swordCollider, null)) swordCollider = GetComponentInChildren<SphereCollider>();
            upgradeSettings = Resources.Load<PlayerUpgradeSettings>(upgradeSettingsPath);
            agent.speed = playerSpeed;
            agent.autoTraverseOffMeshLink = false;
            inventory = new Inventory(playerInventorySize, new List<InventoryItemEntry>());
            GameMaster.OnGameExecutionStateUpdated += UpdatePlayerMovementToMatchGameState;

            InvokeRepeating(nameof(RecoverStamina), 1f, 1f);
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
            // ReSharper disable once DelegateSubtraction
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
            Debug.Log("DEAD!");
            Health = MaxHealth;
        }

        public override void Damage(int amount, Entity dealer) {
            currentGamepad.SetMotorSpeeds(0.8f, 0.8f);
            StartCoroutine(nameof(StopControllerVibration), 0.2f);
            if(dealer.GetComponent<Enemy>()) LastEnemyToHitPlayer = dealer.GetComponent<Enemy>();
            base.Damage(amount, dealer);
        }

        #endregion
        
        #region Input & Movement
        
        /// <summary>
        /// Sets the player movement state based on the game execution state.
        /// </summary>
        private void UpdatePlayerMovementToMatchGameState() {
            CanMove = (GameMaster.Instance.GameState == ExecutionState.Normal);
        }
        
        private void GetInput() {
            movementScale = new Vector3(inputs.Player.Horizontal.ReadValue<float>(), 0f, inputs.Player.Vertical.ReadValue<float>());

            if(movementScale.magnitude < minRotationInput || !CanMove) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementScale), rotationSpeed);
        }

        /// <summary>
        /// Stops a controller from vibrating.
        /// </summary>
        /// <param name="time"> How long until vibration stops. Leave blank for immediately stopping it.</param>
        private IEnumerator StopControllerVibration(float time = 0f) {
            waitTime = new WaitForSeconds(time);
            yield return waitTime;
            currentGamepad.SetMotorSpeeds(0f, 0f);
        }

        /// <summary>
        /// Moves the player based on input using the nav mesh agent.
        /// </summary>
        private void MovePlayer() {
            if(agent.enabled && CanMove) agent.Move(movementScale * (playerSpeed * Time.deltaTime));
        }
        
        #endregion
        
        #region Attack

        /// <summary>
        /// Recovers the player stamina at a steady rate.
        /// </summary>
        private void RecoverStamina() { 
            Stamina = Mathf.Clamp(Stamina + 2, 0, maxStamina);
            PlayerStatsUI.UpdateUiValues.Invoke();
        }
        
        /// <summary>
        /// Player special attack. Fires a pistol.
        /// </summary>
        private void SpecialAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3 || isInsideShop) return;
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
                bullet.BulletOwner = this;
            }
            
            currentGamepad.SetMotorSpeeds(0.6f, 0.4f);
            StartCoroutine(nameof(StopControllerVibration), 0.2f);
            
            agent.enabled = true;
        }

        /// <summary>
        /// Player basic attack with three with combo.
        /// </summary>
        private void ComboAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3 || isInsideShop) return;
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
            if(!CheckCollisionSword(swordCollider)) return;
            //TODO: Play audio and effects.
            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                .GetComponent<DamageCanvas>().damageValue = 
                Mathf.RoundToInt(meleeDamage * (1f + meleeDamageComboMultiplier * comboNumber));
                
            currentGamepad.SetMotorSpeeds(0.42f, 0.42f);
            StartCoroutine(nameof(StopControllerVibration), 0.2f);
        }
        
        /// <summary>
        /// Test collisions for melee hits.
        /// </summary>
        /// <param name="referenceSphere"> Weapon sphere collider. </param>
        private bool CheckCollisionSword(SphereCollider referenceSphere) {
            var hitAnEnemy = false;
            var damageAmount = Mathf.RoundToInt(meleeDamage * (1f + meleeDamageComboMultiplier * comboNumber));
            // ReSharper disable once Unity.PreferNonAllocApi
            var contacts = Physics.OverlapSphere(referenceSphere.transform.position, referenceSphere.radius,
                                                 enemyLayer, QueryTriggerInteraction.Collide);

            foreach(var contact in contacts) {
                if(!contact.gameObject.GetComponent<Enemy>()) continue;
                contact.gameObject.GetComponent<Enemy>().Damage(damageAmount, this);
                hitAnEnemy = true;
            }

            return hitAnEnemy;
        }
        
        #endregion
        
        #region Level Management

        /// <summary>
        /// Adds experience to the player and trigger a level up if needed.
        /// </summary>
        /// <param name="amount"> Amount of experience to add. </param>
        public void AddExperience(int amount) {
            var expToLevelUp = upgradeSettings.GetExperienceNeededForLevelUp(Level);
            Experience += amount;
                
            while(Experience >= expToLevelUp) {
                Experience -= expToLevelUp;
                Level++;
                expToLevelUp = upgradeSettings.GetExperienceNeededForLevelUp(Level);
            }
        }
        
        #endregion
    }
}