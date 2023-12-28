using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Entity.Enemies;
using FMODUnity;
using Game;
using Items;
using Town;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Utility;
using Random = UnityEngine.Random;

namespace Entity.Player {
    /// <summary>
    /// Controls anything related to the player.
    /// </summary>
    public class PlayerController : Entity {
        #pragma warning disable 0649
        [Header("Movement")] 
        [SerializeField] private bool isDead;
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

        [SerializeField] private Inventory inventory;
        

        /// <summary>
        /// The player's inventory.
        /// </summary>
        public Inventory Inventory {
            get => inventory;
        }
        
        /// <summary>
        /// Holds info on the items that the player got in the island trip.
        /// </summary>
        public Inventory PlayerIslandInventory { get; set; } 

        // Input
        private ActionInputs inputs;
        private Gamepad currentGamepad;
        private WaitForSeconds waitTime = new WaitForSeconds(0f);

        private static readonly List<int> AnimCombo = new List<int> {
            Animator.StringToHash("Combo_0"),
            Animator.StringToHash("Combo_1"),
            Animator.StringToHash("Combo_2")
        };

        
        private static readonly int AnimReset = Animator.StringToHash("Death Reset");
        private static readonly int AnimSpecial = Animator.StringToHash("Special");
        private static readonly int AnimDead = Animator.StringToHash("Dead");
        private static readonly int AnimDamaged = Animator.StringToHash("Damaged");
        private static readonly int AnimSpeed = Animator.StringToHash("Speed");

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop;
        
        [Header("Leveling Settings")]
        [SerializeField, Range(0.1f, 1f)] private float healthRegenOnLevelUpMultiplier = 0.35f;
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
        
        // Sound Stuff
        // Dictionary for the different sound values.
        public enum footstepSound { SAND, WOOD }
        // Current type of ground the player is stepping on.
        private footstepSound currentTypeOfGround = footstepSound.WOOD;

        // The EventEmitter variable for the player sounds.
        [Header("Sound Emitters")]
        [SerializeField] private StudioEventEmitter playerFootstepEmitter;
        
        [Header("Sound Events")]
        [SerializeField, EventRef] private string footstepEvent;
        [SerializeField, EventRef] private string specialAttackEvent;
        [SerializeField, EventRef] private string meleeSwingEvent;
        [SerializeField, EventRef] private string meleeHitEvent;

        [Header("Sound Parameters")] 
        [SerializeField, ParamRef] private string footstepParam = "GroundType";
        [SerializeField] private string sandTag = "Sand";
        [SerializeField] private string woodTag = "Wood";
        
        // How far under the player should we check for the ground.
        [SerializeField] private Vector3 groundDistance = new Vector3(0f, 1f, 0f);
        
        [Header("Other")]
        [SerializeField] private GameObject DamageCanvasPrefab;
        [SerializeField] private GameObject deathCanvasPrefab;

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
        
        /// <summary>
        /// Allows the player to move or not.
        /// </summary>
        public bool CanMove { get; set; } = true;
        
        /// <summary>
        /// Overrides the ability of the player to move or not.
        /// </summary>
        public bool CanMoveOverride { get; set; } = true;

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
            inventory = new Inventory(playerInventorySize, new List<InventoryItemEntry>());
            LoadGameMasterPlayerStats();
            inventory.OnInventoryUpdate?.Invoke();
            inventory.OnInventoryUpdate.AddListener(UpdateGameMasterPlayerStats);
            PlayerIslandInventory = new Inventory(playerInventorySize, new List<InventoryItemEntry>());
            GameMaster.OnGameExecutionStateUpdated += UpdatePlayerMovementToMatchGameState;
            InvokeRepeating(nameof(RecoverStamina), 1f, 1f);
        }

        // "Respawns" the player.
        public void ResetPlayer() {
            anim.ResetTrigger(AnimDead);
            anim.ResetTrigger(AnimDamaged);
            anim.ResetTrigger(AnimSpecial);
            anim.SetTrigger(AnimReset);
            Health = maxHealth;
            CanMoveOverride = true;
            CanMove = true;
            transform.position = FindObjectOfType<PlayerSpawnPositionBasedOnLastScene>().portSpawnPosition.position;
            isDead = false;
            agent.enabled = true;
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

        /// <summary>
        /// Opens the death canvas.
        /// </summary>
        public void DisplayDeathCanvas() {
            Instantiate(deathCanvasPrefab);
        }
        
        public override void Kill() {
            if(isDead) return;
            anim.SetTrigger(AnimDead);
            CanMoveOverride = false;
            GameMaster.Instance.MasterSaveData.playerDeathCount++;
            GameMaster.Instance.SaveGame();
            isDead = true;
        }

        public override void Damage(int amount, Entity dealer) {
            if(isDead) return;
            VibrateController(0.2f, 0.8f, 0.8f);
            anim.SetTrigger(AnimDamaged);
            agent.enabled = true;
            if(dealer.GetComponent<Enemy>()) LastEnemyToHitPlayer = dealer.GetComponent<Enemy>();
            base.Damage(amount, dealer);
        }

        #endregion
        
        #region Input & Movement
        
        /// <summary>
        /// Sets the player movement state based on the game execution state.
        /// </summary>
        private void UpdatePlayerMovementToMatchGameState(ExecutionState state) {
            CanMove = (GameMaster.Instance.GameState == ExecutionState.Normal);
        }
        
        private void GetInput() {
            movementScale = new Vector3(inputs.Player.Horizontal.ReadValue<float>(), 0f, inputs.Player.Vertical.ReadValue<float>());

            if(movementScale.magnitude < minRotationInput || !CanMove || !CanMoveOverride) return;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementScale), rotationSpeed);
        }

        /// <summary>
        /// Vibrates the controller with the given parameters.
        /// </summary>
        /// <param name="time"> How long to vibrate the controller for. Defaults to 0.42 seconds.</param>
        /// <param name="lowFreq"> Strong vibrations. Defaults to 0.3. </param>
        /// <param name="highFreq"> Weak vibrations. Defaults to 0.5. </param>
        private void VibrateController(float time = 0.42f, float lowFreq = 0.3f, float highFreq = 0.5f) {
            currentGamepad?.SetMotorSpeeds(lowFreq, highFreq);
            StartCoroutine(nameof(StopControllerVibration), time);
        }
        
        /// <summary>
        /// Stops a controller from vibrating.
        /// </summary>
        /// <param name="time"> How long until vibration stops. Leave blank for immediately stopping it.</param>
        private IEnumerator StopControllerVibration(float time = 0f) {
            waitTime = new WaitForSeconds(time);
            yield return waitTime;
            currentGamepad?.SetMotorSpeeds(0f, 0f);
        }

        /// <summary>
        /// Moves the player based on input using the nav mesh agent.
        /// </summary>
        private void MovePlayer() {
            anim.SetFloat(AnimSpeed, 0f);
            if(!agent.enabled || !CanMove || !CanMoveOverride) return; 
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
            PlayerStatsUI.UpdateUiValues?.Invoke();
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
            PlayerStatsUI.UpdateUiValues?.Invoke();
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
            
            PlaySfx(specialAttackEvent);
            VibrateController(0.2f, 0.6f, 0.4f);
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
            PlaySfx(meleeSwingEvent);
            
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
            
            PlayerStatsUI.UpdateUiValues?.Invoke();
        }
        
        /// <summary>
        /// Called by animation to trigger collision check.
        /// </summary>
        public void ComboAttackCollision() {
            swordSlash.SendEvent("OnStop");
            if(!CheckCollisionSword(swordCollider)) return;
            //TODO: Play effects.
            PlaySfx(meleeHitEvent);
            Instantiate(DamageCanvasPrefab, transform.position, Quaternion.identity)
                .GetComponent<DamageCanvas>().damageValue = 
                Mathf.RoundToInt(meleeDamage * (1f + meleeDamageComboMultiplier * comboNumber));

            VibrateController(0.2f, 0.42f, 0.42f); 
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
                if(!contact.gameObject.GetComponent<Entity>()) continue;
                if(contact.gameObject.GetComponent<PlayerController>()) continue;
                contact.gameObject.GetComponent<Entity>().Damage(damageAmount, this);
                return true;
            }

            return false;
        }
        
        #endregion
        
        #region Level, Coins, Data & Progression Management

        /// <summary>
        /// Adds coins to the player coin count.
        /// </summary>
        public void AddCoins(int amount) {
            Coins += amount;
            UpdateGameMasterPlayerStats();
        }
        
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
                InventorySize = playerInventorySize,
                CurrentUpgradeLevel = 0
            };

            GameMaster.Instance.MasterSaveData.currentInventoryIds =
                ItemIdToItemEntry.ReturnIdsFromEntries(Inventory.ItemsInInventory);

            for(var i = 0; i < Level; i++) {
                if(i % upgradeSettings.upgradeEveryHowManyLevels == 1) stats.CurrentUpgradeLevel++;
            }
            
            GameMaster.Instance.PlayerStats = stats;
            GameMaster.Instance.MasterSaveData.currentPlayerStats = stats;
        }
        
        /// <summary>
        /// Updates the stats in the game master to reflect the player progression.
        /// </summary>
        public void LoadGameMasterPlayerStats() {
            PlayerStats stats = GameMaster.Instance.PlayerStats;

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
            playerInventorySize = stats.InventorySize;
            
            inventory.ItemsInInventory = 
                GameMaster.Instance.MasterSaveData.currentInventoryIds.Count > 0 ? 
                    new List<InventoryItemEntry>(GameMaster.Instance.ItemConverter.ReturnEntriesFromIds(GameMaster.Instance.MasterSaveData.currentInventoryIds)) :
                    new List<InventoryItemEntry>();

            inventory.OnInventoryUpdate?.Invoke();

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
            
            UpdateGameMasterPlayerStats();
        }

        /// <summary>
        /// Levels the player up and triggers upgrades on player stats.
        /// </summary>
        private void LevelUp() {
            Level++;
            UpdateGameMasterPlayerStats();
            
            if(Level % upgradeSettings.upgradeEveryHowManyLevels != 1) return;
            ApplyLevelingStats();
            ApplyLevelingUpgrades();
            UpdateGameMasterPlayerStats();
        }
        
        /// <summary>
        /// Applies upgrades to the player's character basic stats.
        /// </summary>
        private void ApplyLevelingStats() {
            MaxHealth = Mathf.RoundToInt(MaxHealth * upgradeSettings.statsMultiplier);
            Health += Mathf.RoundToInt(MaxHealth * healthRegenOnLevelUpMultiplier);
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
            playerInventorySize = upgradeSettings.inventorySizeUpgradeValues[currentUpgrades];
            inventory.InventorySize = playerInventorySize;
        }
        
        #endregion
        
        #region Sound
        
        /// <summary>
        /// Plays a sound attached to the player position.
        /// </summary>
        /// <param name="soundEvent"> Event to play. </param>
        public void PlaySfx(string soundEvent) => RuntimeManager.PlayOneShotAttached(soundEvent, gameObject);

        /// <summary>
        ///  Plays footstep sound.
        /// </summary>
        public void PlayFootstep() {
            CheckGround();
            
            if(playerFootstepEmitter.Event != null) {
                playerFootstepEmitter.Play();
                playerFootstepEmitter.EventInstance.setParameterByName(footstepParam, (int) currentTypeOfGround);
            } else {
                if(string.IsNullOrEmpty(footstepEvent)) return;
                playerFootstepEmitter.Event = footstepEvent;
                playerFootstepEmitter.Play();
                playerFootstepEmitter.EventInstance.setParameterByName(footstepParam, (int) currentTypeOfGround);
            }
        }

        // Sets the current type of ground.
        public void SetCurrentGround(footstepSound type) => currentTypeOfGround = type;
        
        // Checks what kind of ground it is.
        public void CheckGround() {
            Physics.Linecast((transform.position + groundDistance / 2), transform.position - groundDistance, out var hitInfo);
        
            // Sets ground type for footstep sound.
            if(hitInfo.collider == null) return;
            
            if(hitInfo.collider.gameObject.CompareTag(sandTag)) {
                SetCurrentGround(footstepSound.SAND);
            } else if(hitInfo.collider.gameObject.CompareTag(woodTag)) {
                SetCurrentGround(footstepSound.WOOD);
            }
        }
        #endregion
    }
}