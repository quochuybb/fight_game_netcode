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
    
}