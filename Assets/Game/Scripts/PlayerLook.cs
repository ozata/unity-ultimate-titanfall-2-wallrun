using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    #region Variables
    public Transform player;
    public Transform cams;

    public float xSensitivity;
    public float ySensitivity;
    public float maxAngle;
    
    // Center of the camera
    private Quaternion camCenter;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        camCenter = cams.localRotation;
    }
        
    void Update()
    {
        SetY();
        SetX();
    }
    #endregion

    void SetX()
    {
            
        float inputX = Input.GetAxis("Mouse X") * Time.deltaTime * xSensitivity;
        // Adjustment
        Quaternion adj = Quaternion.AngleAxis(inputX, Vector3.up);
        Quaternion delta = player.localRotation * adj;
        player.localRotation = delta;
    }

    void SetY()
    {
        float inputY = Input.GetAxis("Mouse Y") * Time.deltaTime * ySensitivity;

        Quaternion adj = Quaternion.AngleAxis(inputY, -Vector3.right);
        Quaternion delta = cams.localRotation * adj;

        if (Quaternion.Angle(camCenter, delta) < maxAngle)
        {
            cams.localRotation = delta;
        }
    }
        
}