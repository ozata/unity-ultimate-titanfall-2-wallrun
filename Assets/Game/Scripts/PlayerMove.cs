using System;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpSpeed;
    public float gravity;
    public float doubleJumpSpeed;
    public float movementMultiplier;
    public bool canJump;
    public bool canDoubleJump;

    public float airMultiplier;

    private float horizontalMovement;
    private float verticalMovement;

    private bool isGrounded;

    private Vector3 moveDirection;

    private Rigidbody rb;

    float groundDistance = 0.4f;

    [Header("Wall Run")] public float wallMoveSpeed;
    public float wallPullForce;
    public float wallDistance;
    public float wallRunGravity;
    public bool isWallRunning;
    public float wallRunJumpForceX, wallRunJumpForceY;
    private Vector3 wallRunDirection;
    private Transform currentWall;
    private bool wallLeft, wallRight;
    private RaycastHit leftWallHit, rightWallHit;
    private bool pulledToTheWall;
    private bool materialTurnedOriginal;

    [Header("Camera")] public Camera cam;
    public float camTilt;
    public float camTiltTime;

    public float wallRunTilt;

    [Header("LayerMasks")] public LayerMask wallMask;
    public LayerMask groundMask;

    [Header("Materials")] public Material blackMaterial;
    public Material greenMaterial;

    private void Start()
    {
        canJump = true;
        
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        Debug.DrawRay(transform.position, transform.right * wallDistance, Color.green);
        Debug.DrawRay(transform.position, -transform.right * wallDistance, Color.red);

        isGrounded = Physics.CheckSphere(transform.position - new Vector3(0, 1, 0), groundDistance, groundMask);

        HandleInput();

        var jumpPressed = Input.GetKeyDown(KeyCode.Space);
        if (jumpPressed && isGrounded)
        {
            Jump();
        }
        else if (jumpPressed && canDoubleJump)
        {
            DoubleJump();
        }


        WallRunning();
    }

    private void FixedUpdate()
    {
        //  Adding custom gravity
        MovePlayer();
        Gravity();
    }

    private void Gravity()
    {
        if (!isWallRunning)
        {
            rb.AddForce(new Vector3(0, -gravity, 0) * rb.mass);
        }
    }

    #region Wall Run

    private void WallRunning()
    {
        CheckWall();
        if (CanWallRun())
        {
            if (wallLeft || wallRight)
            {
                WallRun();
            }
            else
            {
                StopWallRun();
            }
        }
        else
        {
            StopWallRun();
        }
    }

    private void CheckWall()
    {
        wallLeft = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);
        wallRight = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);
    }

    private void WallRun()
    {
        //rb.useGravity = false;
        rb.AddForce(Vector3.down * wallRunGravity, ForceMode.Force);

        isWallRunning = true;
        
        if (wallLeft)
        {
            if (!pulledToTheWall)
            {
                pulledToTheWall = true;

                currentWall = leftWallHit.transform;

                ChangeMaterials(currentWall.GetComponent<MeshRenderer>(), greenMaterial);
                materialTurnedOriginal = false;
                
                Debug.Log("Left Normal X: " + leftWallHit.normal.x);
                if (leftWallHit.normal.x > 0)
                {
                    rb.AddForce(new Vector3(-wallPullForce, 0, 0));
                    wallRunDirection = leftWallHit.transform.forward;
                }
                else
                {
                    rb.AddForce(new Vector3(wallPullForce, 0, 0));
                    wallRunDirection = -leftWallHit.transform.forward;
                }
            }

            wallRunTilt = Mathf.Lerp(wallRunTilt, -camTilt, camTiltTime * Time.deltaTime);
        }
        else if (wallRight)
        {
            if (!pulledToTheWall)
            {
                pulledToTheWall = true;
                currentWall = rightWallHit.transform;
                
                ChangeMaterials(currentWall.GetComponent<MeshRenderer>(), greenMaterial);
                materialTurnedOriginal = false;
                
                if (rightWallHit.normal.x > 0)
                {
                    rb.AddForce(new Vector3(-wallPullForce, 0, 0));
                    wallRunDirection = -rightWallHit.transform.forward;
                }
                else
                {
                    rb.AddForce(new Vector3(wallPullForce, 0, 0));
                    wallRunDirection = rightWallHit.transform.forward;
                }
            }

            wallRunTilt = Mathf.Lerp(wallRunTilt, camTilt, camTiltTime * Time.deltaTime);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            Vector3 wallRunJumpDirection = transform.up + leftWallHit.normal;
            if (wallLeft)
            {
                wallRunJumpDirection = transform.up + leftWallHit.normal;
            }
            else if (wallRight)
            {
                wallRunJumpDirection = transform.up + rightWallHit.normal;
            }

            canDoubleJump = true;
            rb.AddForce(new Vector3(wallRunJumpDirection.x * wallRunJumpForceX, wallRunJumpDirection.y * wallRunJumpForceY), ForceMode.Force);
        }
    }

    private void StopWallRun()
    {
        if (currentWall != null && !materialTurnedOriginal)
        {
            materialTurnedOriginal = true;
            ChangeMaterials(currentWall.GetComponent<MeshRenderer>(), blackMaterial);
        }

        isWallRunning = false;
        pulledToTheWall = false;
        wallRunTilt = Mathf.Lerp(wallRunTilt, 0, camTiltTime * Time.deltaTime);
    }

    private bool CanWallRun()
    {
        return true;
    }

    #endregion

    void Jump()
    {
        if (canJump && !isWallRunning)
        {
            canDoubleJump = true;
            rb.velocity = new Vector3(rb.velocity.x, jumpSpeed, rb.velocity.z);
        }
    }

    void DoubleJump()
    {
        if (canDoubleJump && !isWallRunning)
        {
            canDoubleJump = false;
            rb.velocity = new Vector3(rb.velocity.x, doubleJumpSpeed, rb.velocity.z);
        }
    }

    private void MovePlayer()
    {
        if (isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (isWallRunning)
        {
            rb.AddForce(wallRunDirection * wallMoveSpeed * movementMultiplier, ForceMode.Acceleration);
        }
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * movementMultiplier * airMultiplier, ForceMode.Acceleration);
        }
    }

    private void HandleInput()
    {
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        moveDirection = transform.forward * verticalMovement + transform.right * horizontalMovement;
    }

    private void ChangeMaterials(MeshRenderer mr, Material newMat)
    {
        Material[] materials = mr.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i] = newMat;
        }
        mr.materials = materials;
    }


    private void OnDrawGizmos()
    {
    }
}