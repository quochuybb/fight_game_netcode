using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraFollow : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;

    private CinemachineVirtualCamera _virtualCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner) return;          

        _virtualCamera = Instantiate(cameraPrefab);
        Transform target = transform.Find("CameraTarget");
        _virtualCamera.Follow = target;
        _virtualCamera.LookAt = target;

        _virtualCamera.transform.SetParent(null);
        
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        if (!IsOwner || _virtualCamera == null)
            return;
        _virtualCamera.Follow.position = Vector3.zero;
        _virtualCamera.LookAt.position = Vector3.zero;
    }

    private void OnDisable()
    {
        if (_virtualCamera != null && IsOwner)
        {
            _virtualCamera.Follow.position = Vector3.zero;
            _virtualCamera.LookAt.position = Vector3.zero;
        }
    }
    
}