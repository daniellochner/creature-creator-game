﻿// Creature Creator - https://github.com/daniellochner/Creature-Creator
// Copyright (c) Daniel Lochner

using System;
using UnityEngine;

namespace DanielLochner.Assets.CreatureCreator
{
    [RequireComponent(typeof(CreatureAnimator))]
    [RequireComponent(typeof(CreatureConstructor))]
    [RequireComponent(typeof(CreatureCamera))]
    public class CreatureMover : MonoBehaviour
    {
        #region Fields
        [Header("Movement")]
        [SerializeField] private bool requestToMove;
        [SerializeField] private float moveSpeed;
        [SerializeField] private float turnSpeed;
        [SerializeField] private float moveSmoothTime;
        [SerializeField] private float turnSmoothTime;
        [SerializeField] private float angleToMove;
        [SerializeField] private float thresholdWalkSpeed;
        [SerializeField] private GameObject targetPrefab;
        
        [Header("Platform")]
        [SerializeField] private Platform platform;
        [SerializeField] private float positionSmoothing;
        [SerializeField] private float rotationSmoothing;

        [Header("Debug")]
        [SerializeField, ReadOnly] private Vector3 velocity;
        [SerializeField, ReadOnly] private Vector3 angularVelocity;

        private Animator targetAnimator;
        private CapsuleCollider capsuleCollider;
        private GameObject targetGO;
        private new Rigidbody rigidbody;

        private bool isMovable, canMove, canTurn;
        private Vector3 keyboardForward, keyboardRight, moveDisplacement, direction;
        private InputMode inputMode;
        private Vector3 targetPosition;
        #endregion

        #region Properties        
        public CreatureConstructor Constructor { get; private set; }
        public CreatureAnimator Animator { get; private set; }
        public CreatureCamera Camera { get; private set; }

        public Platform Platform
        {
            get => platform;
            set => platform = value;
        }

        public Action<Vector3> OnMoveRequest { get; set; }
        public Action<float> OnTurnRequest { get; set; }

        public bool CanInput
        {
            get
            {
                return EditorManager.Instance.IsPlaying && !InputDialog.Instance.IsOpen && !ConfirmationDialog.Instance.IsOpen && !InformationDialog.Instance.IsOpen;
            }
        }
        public bool IsMovable
        {
            get => isMovable;
            set
            {
                isMovable = value;

                moveDisplacement = Vector3.zero;
                targetPosition = transform.position;

                // Physics
                foreach (Transform bone in Constructor.Bones)
                {
                    bone.GetComponent<Rigidbody>().isKinematic = isMovable;
                }
                rigidbody.constraints = isMovable ? RigidbodyConstraints.FreezeRotation : RigidbodyConstraints.FreezeAll; // Setting "isKinematic" to false will invoke OnTriggerExit().
                rigidbody.useGravity = isMovable;
            }
        }
        #endregion

        #region Methods
        private void Awake()
        {
            Initialize();
        }
        private void Update()
        {
            if (!IsMovable)
            {
                HandlePlatform();
            }
            else if (CanInput)
            {
                HandleInput();
            }
        }
        private void FixedUpdate()
        {
            if (IsMovable)
            {
                HandleMovement();
            }
        }
        private void OnDisable()
        {
            if (targetAnimator != null)
            {
                targetAnimator.SetBool("IsHolding", false);
            }
        }

        private void Initialize()
        {
            Constructor = GetComponent<CreatureConstructor>();
            Animator = GetComponent<CreatureAnimator>();
            Camera = GetComponent<CreatureCamera>();

            capsuleCollider = GetComponent<CapsuleCollider>();
            rigidbody = GetComponent<Rigidbody>();
        }

        private void HandleInput()
        {
            bool kInput = InputUtility.GetKey(KeybindingsManager.Data.WalkForwards) || InputUtility.GetKey(KeybindingsManager.Data.WalkBackwards) || InputUtility.GetKey(KeybindingsManager.Data.WalkLeft) || InputUtility.GetKey(KeybindingsManager.Data.WalkRight);
            bool pInput = Input.GetMouseButton(1);

            bool setK = kInput || (!pInput && inputMode == InputMode.Keyboard);
            bool setP = pInput || (!kInput && inputMode == InputMode.Pointer);

            if (setK)
            {
                inputMode = InputMode.Keyboard;
            }
            else if (setP)
            {
                inputMode = InputMode.Pointer;
            }

            direction = Vector3.zero;
            canTurn = false;
            canMove = false;

            switch (inputMode)
            {
                #region Keyboard
                case InputMode.Keyboard:

                    if (!InputUtility.GetKey(KeybindingsManager.Data.FreeLook) || !kInput) // Free-look when holding ALT.
                    {
                        keyboardForward = Vector3.ProjectOnPlane(Camera.Camera.transform.forward, transform.up);
                        keyboardRight = Vector3.ProjectOnPlane(Camera.Camera.transform.right, transform.up);
                    }

                    int vAxisRaw = InputUtility.GetKey(KeybindingsManager.Data.WalkForwards) ? 1 : (InputUtility.GetKey(KeybindingsManager.Data.WalkBackwards) ? -1 : 0);
                    int hAxisRaw = InputUtility.GetKey(KeybindingsManager.Data.WalkRight) ? 1 : (InputUtility.GetKey(KeybindingsManager.Data.WalkLeft) ? -1 : 0);

                    Vector3 vertical = keyboardForward * vAxisRaw;
                    Vector3 horizontal = keyboardRight * hAxisRaw;

                    direction = (vertical + horizontal).normalized;
                    canTurn = true;
                    canMove = kInput && !InputUtility.GetKey(KeybindingsManager.Data.StopMove);

                    break;
                #endregion

                #region Pointer
                case InputMode.Pointer:

                    if (pInput && Physics.Raycast(RectTransformUtility.ScreenPointToRay(Camera.Camera, Input.mousePosition), out RaycastHit raycastHit, Mathf.Infinity, LayerMask.GetMask("Ground")))
                    {
                        Vector3 position = raycastHit.point;
                        Quaternion rotation = Quaternion.LookRotation(raycastHit.normal, transform.up);

                        if (Input.GetMouseButtonDown(1) || targetGO == null)
                        {
                            targetGO = Instantiate(targetPrefab, position, rotation, Dynamic.WorldCanvas);
                            targetAnimator = targetGO.GetComponent<Animator>();
                        }

                        targetGO.transform.SetPositionAndRotation(position, rotation);
                        targetPosition = position;
                    }

                    Vector3 displacement = Vector3.ProjectOnPlane(targetPosition - transform.position, transform.up);

                    direction = displacement.normalized;
                    canTurn = displacement.magnitude > moveSpeed * moveSmoothTime;
                    canMove = canTurn;

                    break;
                    #endregion
            }

            if (targetAnimator != null)
            {
                targetAnimator.SetBool("IsHolding", pInput && !kInput);
            }
        }
        private void HandleMovement()
        {
            if (direction != Vector3.zero)
            {
                float angle = Vector3.SignedAngle(transform.forward, direction, transform.up);
                if (canTurn)
                {
                    RequestTurn(angle);
                }
                canMove &= Mathf.Abs(angle) < angleToMove;
            }
            RequestMove(canMove ? direction : Vector3.zero);
        }
        private void HandlePlatform()
        {
            if (Platform != null) transform.LerpTo(Platform.Position, positionSmoothing);
        }

        public void RequestMove(Vector3 direction)
        {
            if (requestToMove)
            {
                OnMoveRequest?.Invoke(direction);
            }
            else
            {
                Move(direction);
            }
        }
        public void RequestTurn(float angle)
        {
            if (requestToMove)
            {
                OnTurnRequest?.Invoke(angle);
            }
            else
            {
                Turn(angle);
            }
        }
        public void Move(Vector3 direction)
        {
            Vector3 displacement = direction * moveSpeed * Time.fixedDeltaTime;
            moveDisplacement = Vector3.SmoothDamp(moveDisplacement, displacement, ref velocity, moveSmoothTime);
            rigidbody.position += moveDisplacement;
        }
        public void Turn(float angle)
        {
            Quaternion rotation = Quaternion.Euler(0f, angle * turnSpeed * Mathf.Deg2Rad * Time.fixedDeltaTime, 0f);
            rigidbody.rotation *= rotation;
        }

        public void Teleport(Vector3 position)
        {
            transform.position = targetPosition = position;

            moveDisplacement = Vector3.zero;
        }
        public void Teleport(Platform platform, bool start = false)
        {
            Platform = platform;
            Teleport(Platform.Position);

            if (start)
            {
                transform.rotation = Platform.Rotation;
                Camera.Root.SetPositionAndRotation(platform.Position, platform.Rotation);
            }
        }
        #endregion

        #region Enums
        public enum InputMode
        {
            None,
            Pointer,
            Keyboard
        }
        #endregion
    }
}