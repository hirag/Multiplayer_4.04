using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    public float walkSpeed = 8f;
    public float sprintSpeed = 14f;
    public float maxVelocityChange = 10f;
    public float airControl = 0.5f;
    public float jumpHeight = 10f;
    public float minHeight = -10f; // この高さ以下になると die メソッドを呼ぶ

    private Vector2 input;
    private Rigidbody rb;
    private bool sprinting;
    private bool jumping;
    private bool grounded;

    public InputActionAsset inputActionAsset;
    private InputAction moveAction;
    private InputAction sprintAction;
    private InputAction jumpAction;

    private PhotonView photonView;
    private Health health; // Health コンポーネントの参照

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        health = GetComponent<Health>(); // Health コンポーネントの参照を取得

        // Input Actionsの設定
        var playerActions = inputActionAsset.FindActionMap("Player");
        moveAction = playerActions.FindAction("Move");
        sprintAction = playerActions.FindAction("Sprint");
        jumpAction = playerActions.FindAction("Jump");
    }

    void OnEnable()
    {
        if (photonView.IsMine)
        {
            moveAction.Enable();
            sprintAction.Enable();
            jumpAction.Enable();
        }
    }

    void OnDisable()
    {
        if (photonView.IsMine)
        {
            moveAction.Disable();
            sprintAction.Disable();
            jumpAction.Disable();
        }
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        input = moveAction.ReadValue<Vector2>();
        sprinting = sprintAction.ReadValue<float>() > 0;
        jumping = jumpAction.ReadValue<float>() > 0;

        // 一定の高さ以下になった場合に die メソッドを呼ぶ
        if (transform.position.y < minHeight)
        {
            health.Die();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (photonView.IsMine)
        {
            grounded = true;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (grounded)
        {
            if (jumping)
            {
                rb.velocity = new Vector3(rb.velocity.x, jumpHeight, rb.velocity.z);
            }
            else if (input.magnitude > 0.1f)
            {
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed : walkSpeed), ForceMode.VelocityChange);
            }
            else
            {
                SlowDown();
            }
        }
        else
        {
            if (input.magnitude > 0.1f)
            {
                rb.AddForce(CalculateMovement(sprinting ? sprintSpeed * airControl : walkSpeed * airControl), ForceMode.VelocityChange);
            }
            else
            {
                SlowDown();
            }
        }

        grounded = false;
    }

    Vector3 CalculateMovement(float _speed)
    {
        Vector3 targetVelocity = new Vector3(input.x, 0, input.y);
        targetVelocity = transform.TransformDirection(targetVelocity);
        targetVelocity *= _speed;

        Vector3 velocity = rb.velocity;

        if (input.magnitude > 0.1f)
        {
            Vector3 velocityChange = targetVelocity - velocity;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
            velocityChange.y = 0;

            return velocityChange;
        }
        else
        {
            return Vector3.zero;
        }
    }

    void SlowDown()
    {
        var velocity = rb.velocity;
        velocity = new Vector3(velocity.x * 0.2f * Time.fixedDeltaTime, velocity.y, velocity.z * 0.2f * Time.fixedDeltaTime);
        rb.velocity = velocity;
    }
}
