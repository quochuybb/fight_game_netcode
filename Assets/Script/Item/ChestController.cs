using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class ChestController : NetworkBehaviour
{
    public UnityEvent<Transform> onOpenChest = new UnityEvent<Transform>();

    [SerializeField] private float chestOpenTime = 3f;
    private float timer = 0f;

    [SerializeField] private Vector3[] spawnPoints =
    {
        new Vector3(-147, -5, 0),
        new Vector3(-155, -20, 0),
        new Vector3(-158, -9, 0),
        new Vector3(-170, -5, 0),
        new Vector3(-170, -16, 0),
        new Vector3(-160, -15, 0)
    };
    [SerializeField] private Vector3 currentPoint;

    private readonly NetworkVariable<Vector3> chestPosition = new NetworkVariable<Vector3>(
        writePerm: NetworkVariableWritePermission.Server);

    private void Start()
    {
        if (IsClient)
        {
            chestPosition.OnValueChanged += (oldPos, newPos) =>
            {
                transform.position = newPos;
            };
        }
    }

    private void Update()
    {
        if (!IsServer) return; 

        timer += Time.deltaTime;
        if (timer > chestOpenTime)
        {
            timer = 0;
            onOpenChest.Invoke(transform);
            MoveChest();
        }
    }

    private void MoveChest()
    {
        int idx = Random.Range(0, spawnPoints.Length);

        while (spawnPoints[idx] == currentPoint)
        {
            idx = Random.Range(0, spawnPoints.Length);
        }

        currentPoint = spawnPoints[idx];

        transform.position = currentPoint;
        chestPosition.Value = currentPoint;
    }

}