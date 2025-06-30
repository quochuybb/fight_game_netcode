using System;
using Unity.Netcode;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class CameraFollow : NetworkBehaviour
{
    [SerializeField] private CinemachineVirtualCamera cameraPrefab;
    [SerializeField] private GameObject mainCamera;

    private CinemachineVirtualCamera _virtualCamera;
    public static CameraFollow instance;

    private void Awake()
    {
        instance = this;
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

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
        if (_virtualCamera == null) return;
        base.OnNetworkDespawn();
        mainCamera.transform.position = Vector3.zero;
        Destroy(_virtualCamera.gameObject);

    }

    public void OnResetCamera()
    {
    }

    
}