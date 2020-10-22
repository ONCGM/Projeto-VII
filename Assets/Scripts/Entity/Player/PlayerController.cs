using System;
using System.Collections;
using System.Collections.Generic;
using Entity.Enemies;
using Game;
using Items;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Random = UnityEngine.Random;

namespace Entity.Player {
    /// <summary>
    /// Controls anything related to the player.
    /// </summary>
    public class PlayerController : Entity {
        #pragma warning disable 0649
        [Header("Movement")]
        [SerializeField, Range(0f, 25f)] private float movementSpeed = 15f;
        [SerializeField, Range(0f, 0.8f)] private float storeMovementSpeedReduction = 0.4f;
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

        private static readonly List<int> AnimCombo = new List<int> {
            Animator.StringToHash("Combo_0"),
            Animator.StringToHash("Combo_1"),
            Animator.StringToHash("Combo_2")
        };

        private static readonly int AnimSpecial = Animator.StringToHash("Special");
        private static readonly int AnimDead = Animator.StringToHash("Dead");
        private static readonly int AnimDamaged = Animator.StringToHash("Damaged");
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop;
        
        [Header("Leveling Settings")]
        private const string upgradeSettingsPath = "Scriptables/Player/Player_Upgrade_Settings";
        [SerializeField] private PlayerUpgradeSettings upgradeSettings;
        
        public override int Experience {
            get => experience;
            set {
                experience = value;
                PlayerStatsUI.UpdateUiValues?.Invoke();
            }
        }
        
        public override int Level {
            get => level;
            set {
                level = value;
                PlayerStatsUI.UpdateUiValues?.Invoke();
            }
        }
        
        // Which upgrades to use.
        private int currentUpgrades = 0;

        [Header("Other")]
        [SerializeField] private GameObject DamageCanvasPrefab;

        /// <summary>
        /// Is the player inside the shop? True for yes.
        /// </summary>
        public bool IsInsideShop {
            get => isInsideShop;
            set {
                isInsideShop = value;
                playerSpeed = (isInsideShop ? movementSpeed - (movementSpeed * storeMovementSpeedReduction) : movementSpeed);
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
        
        
        [Header("VFX")]
        [SerializeField] private VisualEffect swordSlash;


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
            Inventory = new Inventory(playerInventorySize, new List<InventoryItemEntry>());
            GameMaster.OnGameExecutionStateUpdated += UpdatePlayerMovementToMatchGameState;
            LoadGameMasterPlayerStats();
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
            anim.SetTrigger(AnimDead);
            Health = MaxHealth;
        }

        public override void Damage(int amount, Entity dealer) {
            currentGamepad.SetMotorSpeeds(0.8f, 0.8f);
            StartCoroutine(nameof(StopControllerVibration), 0.2f);
            anim.SetTrigger(AnimDamaged);
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
            anim.SetFloat(AnimSpeed, 0f);
            if(!agent.enabled || !CanMove) return; 
            agent.Move(movementScale * (playerSpeed * Time.deltaTime));
            anim.SetFloat(AnimSpeed, movementScale.magnitude);
        }
        
        #endregion
        
        #region Attack

        /// <summary>
        /// Recovers the player stamina at a steady rate.
        /// </summary>
        private void RecoverStamina() { 
            Stamina = Mathf.Clamp(Stamina + 2, 0, MaxStamina);
            PlayerStatsUI.UpdateUiValues.Invoke();
        }
        
        /// <summary>
        /// Player special attack. Fires a pistol.
        /// </summary>
        private void SpecialAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3 || isInsideShop) return;
            if(anim.GetCurrentAnimatorStateInfo(1).IsName("Player_Attack_Special_Anim")) return;
            Stamina -= 4;
            agent.enabled = false;
            anim.SetTrigger(AnimSpecial);
            PlayerStatsUI.UpdateUiValues.Invoke();
        }

        /// <summary>
        /// Called by animation to unlock player movement.
        /// </summary>
        public void SpecialAttackFire() {
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
        }
        
        /// <summary>
        /// Called by animation to unlock player movement.
        /// </summary>
        public void SpecialAttackRelease() {
            agent.enabled = true;
        }

        /// <summary>
        /// Player basic attack with three with combo.
        /// </summary>
        private void ComboAttack(InputAction.CallbackContext callbackContext) {
            if(Stamina < 3 || isInsideShop) return;
            Stamina -= 3;
            swordSlash.SendEvent("OnPlay");
            
            if(anim.GetCurrentAnimatorStateInfo(1).IsName("Attack Idle")) {
                comboNumber = 0;
                anim.ResetTrigger(AnimCombo[0]);
                anim.ResetTrigger(AnimCombo[1]);
                anim.ResetTrigger(AnimCombo[2]);
            }
            
            if(comboNumber < 1) {
                anim.SetTrigger(AnimCombo[comboNumber]);
                comboNumber++;
            } else if(comboNumber < 2) {
                anim.SetTrigger(AnimCombo[comboNumber]);
                comboNumber++;
            }  else if(comboNumber < 3) {
                anim.SetTrigger(AnimCombo[comboNumber]);
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
            swordSlash.SendEvent("OnStop");
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
            var damageAmount = Mathf.RoundToInt(meleeDamage * (1f + meleeDamageComboMultiplier * comboNumber));
            // ReSharper disable once Unity.PreferNonAllocApi
            var contacts = Physics.OverlapSphere(referenceSphere.transform.position, referenceSphere.radius,
                                                 enemyLayer, QueryTriggerInteraction.Collide);

            if(contacts.Length < 1) return false;
            
            foreach(var contact in contacts) {
                if(!contact.gameObject.GetComponent<Enemy>()) continue;
                contact.gameObject.GetComponent<Enemy>().Damage(damageAmount, this);
                return true;
            }

            return false;
        }
        
        #endregion
        
        #region Level, Data & Progression Management

        /// <summary>
        /// Updates the stats in the game master to reflect the player progression.
        /// </summary>
        public void UpdateGameMasterPlayerStats() {
            var stats = new PlayerStats {
                Health = Health,
                MaxHealth = MaxHealth,
                Stamina = Stamina,
                MaxStamina = MaxStamina,
                MeleeDamage = meleeDamage,
                RangedDamage = rangedDamagePerBullet,
                MovementSpeed = Mathf.RoundToInt(movementSpeed),
                Level = Level,
                Experience = Experience,
                TotalExperience = upgradeSettings.GetExperienceNeededForLevelUp(Level) + Experience,
                Coins = Coins,
                CurrentInventory = Inventory.ItemsInInventory,
                CurrentUpgradeLevel = 0
            };

            for(var i = 0; i < Level; i++) {
                if(i % upgradeSettings.upgradeEveryHowManyLevels == 1) stats.CurrentUpgradeLevel++;
            }
            
            GameMaster.Instance.PlayerStats = stats;
        }
        
        /// <summary>
        /// Updates the stats in the game master to reflect the player progression.
        /// </summary>
        public void LoadGameMasterPlayerStats() {
            var stats = GameMaster.Instance.PlayerStats;
            
            Health = stats.Health;
            MaxHealth = stats.MaxHealth;
            Stamina = stats.Stamina;
            MaxStamina = stats.MaxStamina;
            meleeDamage = stats.MeleeDamage;
            rangedDamagePerBullet = stats.RangedDamage;
            movementSpeed = stats.MovementSpeed;
            Level = stats.Level;
            Experience = stats.Experience;
            Coins = stats.Coins;
            Inventory = new Inventory(playerInventorySize, stats.CurrentInventory);

            currentUpgrades = stats.CurrentUpgradeLevel;
            
            ApplyLevelingUpgrades();
        }
        
        /// <summary>
        /// Adds experience to the player and trigger a level up if needed.
        /// </summary>
        /// <param name="amount"> Amount of experience to add. </param>
        public void AddExperience(int amount) {
            if(Level >= MaxLevel) return;
            
            var expToLevelUp = upgradeSettings.GetExperienceNeededForLevelUp(Level);
            Experience += amount;
                
            while(Experience >= expToLevelUp && Level < MaxLevel) {
                LevelUp();
                Experience -= expToLevelUp;
                expToLevelUp = upgradeSettings.GetExperienceNeededForLevelUp(Level);
            }
        }

        /// <summary>
        /// Levels the player up and triggers upgrades on player stats.
        /// </summary>
        private void LevelUp() {
            Level++;
            
            if(Level % upgradeSettings.upgradeEveryHowManyLevels == 1) {
                ApplyLevelingStats();
                ApplyLevelingUpgrades();
            }
        }
        
        /// <summary>
        /// Applies upgrades to the player's character basic stats.
        /// </summary>
        private void ApplyLevelingStats() {
            MaxHealth = Mathf.RoundToInt(MaxHealth * upgradeSettings.statsMultiplier);
            MaxStamina = Mathf.RoundToInt(MaxStamina * upgradeSettings.statsMultiplier);
            meleeDamage = Mathf.RoundToInt(meleeDamage * upgradeSettings.statsMultiplier);
            rangedDamagePerBullet = Mathf.RoundToInt(rangedDamagePerBullet * upgradeSettings.statsMultiplier);
        }

        /// <summary>
        /// Applies upgrades to the player's character attack stats.
        /// </summary>
        private void ApplyLevelingUpgrades() {
            meleeDamageComboMultiplier = upgradeSettings.meleeMultiplierUpgradeValues[currentUpgrades];
            rangedBulletsPerAttack = upgradeSettings.bulletsPerShotUpgradeValues[currentUpgrades];
            bulletSpreadAngle = upgradeSettings.bulletAngleUpgradeValues[currentUpgrades];
            rangedAttackMaxRange = upgradeSettings.bulletRangeUpgradeValues[currentUpgrades];
        }
        
        #endregion
    }
}