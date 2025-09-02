using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    [Header("Movement Settings")]
    [SerializeField] private float speed = 6.0f;
    [SerializeField] private float turnSmoothTime = 0.1f;
    [SerializeField] private float footstepInterval = 0.5f;

    [SerializeField] private CharacterController characterController;
    public Transform cam;
    public Animator animator;

    private float turnSmoothVelocity ;

    //Dialogue
    public static bool isInDialogue;
    private AudioSource audioSource;
  

    //Movement 
    private float timeSinceLastFootstep;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        timeSinceLastFootstep = footstepInterval;
    }
    private void Update()
    {
        if (!isInDialogue)
        {
            //Only move if not in dialogue
            animator.SetTrigger("idleTrigger");
            MovePlayer();
        }
        else
        {
            //If you are in dialogue even if you begin interaction while walking you will 
            //trigger the idle animation 
            TriggerIdleAnimation();
        }
    }
    void MovePlayer()
    {
        //Get user input 
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        //Check if shift is pressed to run 
        bool isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        //Calculate Speed based on whether the player is running or not 
        float currentSpeed = isRunning ? speed * 2.0f : speed;

        //Find mouse direction 
        Vector3 direction = new Vector3(horizontal, 0.0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0.0f, angle, 0.0f);

            //Player moves in direction of camera 
            Vector3 moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
            characterController.Move(currentSpeed * Time.deltaTime * moveDirection.normalized);
            animator.SetFloat("Speed", direction.magnitude / currentSpeed);

            //Play footstep sounds 
            timeSinceLastFootstep += Time.deltaTime;
            if (timeSinceLastFootstep >= footstepInterval)
            {
                PlayFootstepSound();
                timeSinceLastFootstep = 0;
            }
        }
        else animator.SetFloat("Speed", 0);
    }
    private void PlayFootstepSound()
    { 
        audioSource.Play();
    }
    public void TriggerIdleAnimation()
    {
        animator.SetTrigger("idleTrigger");
        animator.SetFloat("Speed", 0);
    }
}
