using Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    public float speed=10f;
    public float runSpeed = 15f;
    public float currentSpeed;
    public float rayGroundDetectionRange = 0.3f;
    Rigidbody rb;
    float horizontal;
    float vertical;
    float ascend;
    public float ascendSpeed=5f;
    [SerializeField]
    RaycastHit hit;
    Player player;
    public LayerMask groundLayer;
    bool ascending;
    public bool running, isRecovering;
    public float stamina = 100;
    public float currentStamina;
    public float staminaRecoveryDelay = 1f;
    public float staminaRecoveryTimer = 0f;

    public CinemachineFreeLook freeLook;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        currentSpeed = speed;
        currentStamina = stamina;
    }

    void Update()
    {
        if (ScoreManager.instance.isGameEnd) return;
        player.grounded = Physics.Raycast(transform.position, -transform.up, out hit, rayGroundDetectionRange, groundLayer);
        player.animator.SetBool("isFlying", !player.grounded);
        if (player.isTrapped) 
        {
            horizontal = 0f;
            vertical = 0f;
            return; 
        }
        vertical = (Input.GetKey(KeyCode.W) ? 1 :0) - (Input.GetKey(KeyCode.S) && player.grounded ? 1 : 0);
        horizontal = Input.GetAxis("Horizontal");
        ascend = (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!ascending && !player.flying)
            {
                rb.useGravity = false;
                ascending = true;
            }else if (ascending && !player.flying)
            {
                rb.useGravity = false; 
                ascending = false;
            }

            if (player.flying)
            {
                player.flying = false;
            }
        }
        if (Input.GetKey(KeyCode.LeftShift) && currentStamina > 0 && staminaRecoveryTimer <= 0 && (player.flying || ascending))
        {
            running = true;
        }
        else
        {
            running = false;
        }

        if (vertical != 0 && running)
        {
            currentStamina = Mathf.Clamp(currentStamina - (20f * Time.deltaTime), 0, stamina);
            currentSpeed = runSpeed;

            if (currentStamina <= 0 && !isRecovering)
            {
                isRecovering = true; 
                staminaRecoveryTimer = staminaRecoveryDelay; 
            }
        }
        else
        {
            currentSpeed = speed;

            if (isRecovering)
            {
                staminaRecoveryTimer -= Time.deltaTime;

                if (staminaRecoveryTimer <= 0)
                {
                    isRecovering = false; 
                }
            }
            else
            {
                currentStamina = Mathf.Clamp(currentStamina + (15f * Time.deltaTime), 0, stamina);
            }
        }

        GameEventManager.instance.uIEvents.UpdateStaminaGauge(currentStamina/stamina);
        GameEventManager.instance.uIEvents.BirdFly(player.flying);
        Debug.DrawRay(transform.position, -transform.up * rayGroundDetectionRange, Color.red);
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = transform.forward * vertical * currentSpeed;
        player.animator.SetFloat("speed",rb.velocity.magnitude);
        player.animator.speed = rb.velocity.magnitude > runSpeed ? 1.5f : 1;
        Vector3 currentRot = transform.rotation.eulerAngles;
        currentRot.y += horizontal * 2;
        transform.rotation = Quaternion.Euler(currentRot);
        if (player.flying && !player.isTrapped )
        {
            if (!Physics.Raycast(transform.position, -transform.up, out hit, 20f, groundLayer) && ascend >0 )
            {
                ascend = 0;
            }
            moveDirection += transform.up * (ascend * ascendSpeed);
            rb.velocity = moveDirection;

            if (ascend < 0 && player.grounded)
            {
                rb.useGravity = true;
                player.flying = false;
            }
        }else if (ascending)
        {
            moveDirection += transform.up * 2;
            rb.velocity = moveDirection;
            if(!player.grounded && !Physics.Raycast(transform.position, -transform.up, out hit, 2f, groundLayer)) 
            { 
                player.flying = true;
                ascending = false;
            }
        }
        else
        {
            if (!player.grounded || player.isTrapped)
            {
                rb.useGravity = true;
                moveDirection += -transform.up * speed;
            }
            
            if(!player.isTrapped)
            {
                rb.velocity = moveDirection;
            }
        }
    }
}
