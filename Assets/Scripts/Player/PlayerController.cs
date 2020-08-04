using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.Composites;

namespace Player {
    public class PlayerController : MonoBehaviour {
        [Header("Movement")]
        [SerializeField] private float walkSpeed = 9f;
        [SerializeField] private float runSpeed = 15f;
        private float playerSpeed = 1f;
        private NavMeshAgent agent;
        private Vector3 movementScale = Vector3.zero;
        
        // Input
        private ActionInputs inputs;

        /// Is the player inside the shop? True for yes. 
        private bool isInsideShop = true;
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

        private void Awake() {
            inputs = new ActionInputs();
            agent = GetComponent<NavMeshAgent>();
            playerSpeed = (isInsideShop ? walkSpeed : runSpeed);
            agent.speed = playerSpeed;
        }

        private void OnEnable() {
            inputs.Enable();
        }

        private void OnDisable() {
            inputs.Disable();
        }

        private void Start() { }

        private void Update() {
            movementScale = new Vector3(inputs.Movement.Horizontal.ReadValue<float>(), 0f, inputs.Movement.Vertical.ReadValue<float>());
        }

        private void FixedUpdate() {
            agent.Move(movementScale * (playerSpeed * Time.deltaTime));
        }
    }
}