using System;
using System.Collections;
using System.Collections.Generic;
using Game;
using Items;
using Store;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Entity.NPC {
    /// <summary>
    /// Controls the behaviour of a npc.
    /// </summary>
    public class NpcController : Entity {
        #pragma warning disable 0649
        [Header("Settings")] 
        [SerializeField, Range(0f, 1f)] private float chanceToBuy = 0.5f; 
        [SerializeField, Range(1, 10)] public int howManyItemsToBrowse = 2;
        [SerializeField, Range(1, 100)] public int minimumCoinsToStay = 25;
        [SerializeField, Range(0.1f, 5f)] public float buyAnimationWait = 1f;
        [SerializeField] private Transform carryPosition;
        [Header("References")]
        [SerializeField] private List<Transform> npcBuyWaypoints = new List<Transform>();
        [SerializeField] private List<Transform> npcPayWaypoints = new List<Transform>();
        [SerializeField] private List<Transform> npcRandomWaypoints = new List<Transform>();

        /// <summary>
        /// The stats of this npc.
        /// </summary>
        public NpcStats Stats { get; set; }
        
        /// <summary>
        /// Has this npc bought an item.
        /// </summary>
        public bool HasChosenAnItem { get; private set; }
        public bool HasPaidForItem { get; private set; }

        private StoreController store;
        private static readonly int Speed = Animator.StringToHash("Speed");
        private static readonly int Think = Animator.StringToHash("Think");
        private static readonly int Grab = Animator.StringToHash("Grab_Item");
        private static readonly int Drop = Animator.StringToHash("Drop_Item");
        
        // Waits
        private WaitUntil waitUntilPathComplete;
        private WaitForSeconds waitOnPurchase;

        /// <summary>
        /// The npc chosen item.
        /// </summary>
        public ItemStore ChosenItem { get; set; }

        #pragma warning restore 0649

        // Gets references and stuff.
        protected override void Awake() {
            base.Awake();
            store = FindObjectOfType<StoreController>();
            npcBuyWaypoints = store.npcBuyWaypoints;
            npcPayWaypoints = store.npcPayWaypoints;
            npcRandomWaypoints = store.npcRandomWaypoints;
            waitUntilPathComplete = new WaitUntil(() => Vector3.Distance(agent.destination, agent.transform.position) < 0.25f);
            waitOnPurchase = new WaitForSeconds(buyAnimationWait);
        }

        /// <summary>
        /// Animation values.
        /// </summary>
        private void Update() {
            anim.SetFloat(Speed, agent.velocity.magnitude);
        }

        /// <summary>
        /// Moves the npc to a specified position.
        /// </summary>
        private void SetDestinationToWaypoint(Transform waypoint) => agent.SetDestination(waypoint.position);
        
        /// <summary>
        /// Configures the npc.
        /// </summary>
        public void SetNpcsValuesBasedOnStats() {
            Coins = Mathf.RoundToInt(Stats.amountOfCoins * GameMaster.Instance.GameDifficulty);
            chanceToBuy = Mathf.Max(Stats.chanceToBuySomething * GameMaster.Instance.GameDifficultyReversed, 0.1f);
            howManyItemsToBrowse = Stats.howManyItemsToBrowse;

            Level = Mathf.Max(Random.Range(GameMaster.Instance.PlayerStats.Level - Stats.levelVariationBasedOnPlayerLevel, 
                                           GameMaster.Instance.PlayerStats.Level + Stats.levelVariationBasedOnPlayerLevel), 1);

            for(var i = 0; i < Level; i++) Coins = Mathf.CeilToInt(Coins * Stats.basicStatsMultiplier);

            StartCoroutine(nameof(NpcAi));
        }

        /// <summary>
        /// Controls the behaviour of the npcs.
        /// </summary>
        private IEnumerator NpcAi() {
            var purchaseTries = Mathf.RoundToInt(Random.Range((Stats.howManyItemsToBrowse - Stats.howManyItemsToBrowse * 0.5f), (Stats.howManyItemsToBrowse + 1)));
            SetDestinationToWaypoint(npcRandomWaypoints[Random.Range(0, npcRandomWaypoints.Count)]);

            for(var i = 0; i < purchaseTries; i++) {
                if(HasChosenAnItem) break;
                
                if(Stats.chanceToBuySomething >= Random.value) {
                    ChosenItem = store.PickItem(Coins);

                    if(ChosenItem != null) {
                        StartCoroutine(PurchaseItem(ChosenItem));
                        HasChosenAnItem = true;
                        
                        yield break;
                    }
                }

                SetDestinationToWaypoint(npcRandomWaypoints[Random.Range(0, npcRandomWaypoints.Count)]);
                    
                yield return waitUntilPathComplete;
                    
                anim.SetTrigger(Think);
                    
                yield return waitOnPurchase;
            }
            
            if(HasChosenAnItem) yield break;

            StartCoroutine(GoToExit());
        }

        /// <summary>
        /// Buys an item.
        /// </summary>
        private IEnumerator PurchaseItem(ItemStore item) {
            if(HasChosenAnItem) yield break;
            
            SetDestinationToWaypoint(item.itemWaypoint);
                        
            yield return waitUntilPathComplete;
            
            anim.SetTrigger(Think);
            
            yield return waitOnPurchase;
            
            anim.SetTrigger(Grab);
            
            yield return waitOnPurchase;
                    
            item.transform.parent = carryPosition;
            item.transform.localPosition = Vector3.zero;
            SetDestinationToWaypoint(npcPayWaypoints[Random.Range(0, npcPayWaypoints.Count)]);

            yield return new WaitUntil(() => Vector3.Distance(agent.destination, agent.transform.position) < 0.2f);
            
            yield return waitOnPurchase;
            
            anim.SetTrigger(Drop);
            item.transform.position = store.itemPurchasePosition.position;
            
            yield return waitOnPurchase;

            Coins -= store.BuyItem(item);
            HasPaidForItem = true;

            yield return waitOnPurchase;
            
            anim.SetTrigger(Grab);
            item.transform.localPosition = Vector3.zero;
            
            yield return waitOnPurchase;

            StartCoroutine(nameof(GoToExit));
        }

        #region Overrides

        // NPCs shouldn't take damage. Only plays a hurt anim for feedback.
        public override void Damage(int amount, Entity dealer) {
            return;
        }

        // NPCS don't die, so they're just removed when not needed.
        public override void Kill() => StartCoroutine(nameof(GoToExit));

        /// <summary>
        /// Goes to exit then deletes this npc.
        /// </summary>
        private IEnumerator GoToExit() {
            SetDestinationToWaypoint(store.npcSpawnPoints[Random.Range(0, store.npcSpawnPoints.Count)]);
            yield return waitUntilPathComplete;
            yield return waitOnPurchase;
            store.RemoveNpc(this);
            Destroy(gameObject);
        }

        #endregion

        /// <summary>
        /// Leaves the store.
        /// </summary>
        public void LeaveStore() {
            if(HasPaidForItem) return;
            StopAllCoroutines();
            
            if(HasChosenAnItem) {
                anim.SetTrigger(Drop);
                ChosenItem.transform.position = store.itemPurchasePosition.position;
            }
            
            StartCoroutine(nameof(GoToExit));
        }
    }
}
