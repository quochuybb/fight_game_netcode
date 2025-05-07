using UnityEngine;

public class GenerateSpawner : MonoBehaviour
{
    [SerializeField] private GameObject BaseTele;
    [SerializeField] private GameObject Player;

    private void Awake()
    {
        //Instantiate(Player, new Vector3(1,0,0), Quaternion.identity);

    }
}
