using UnityEngine;

// CameraController follows the local player by maintaining a fixed offset
// from the player's position each frame.
// Attach this script to the Main Camera in the scene.
// PlayerController calls SetTarget() on spawn to assign the correct player.
public class CameraController : MonoBehaviour
{

    [SerializeField] private float _distance = 6f;
    [SerializeField] private float _heightOffset = 1.5f;

    [SerializeField] private float _mouseSensitivity = 2f;

    [SerializeField] private float _maxPitch = 60f;
    [SerializeField] private float _minPitch = -20f;

    // The player transform this camera is following.
    // Null until a player calls SetTarget().
    private Transform _target;


    private float _yaw;
    private float _pitch;


    // Called by PlayerController.OnNetworkSpawn() on the owning client only.
    // This ensures the camera follows the correct player in a multi-player session.
    public void SetTarget(Transform target)
    {
        _target = target;
        _yaw = target.eulerAngles.y;
    }

    private void LateUpdate()
    {
        if (_target == null) return;

        //Read the Mouse input axis to rotate the camera around the player.
        float mouseX = Input.GetAxis("Mouse X") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * _mouseSensitivity;

        _yaw = _target.eulerAngles.y;
        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        Quaternion rotation = Quaternion.Euler(_pitch, _yaw, 0f);

        Vector3 targetPos = _target.position + Vector3.up * _heightOffset;
        transform.position = targetPos - rotation * Vector3.forward * _distance;
        transform.LookAt(targetPos);
    }
}
