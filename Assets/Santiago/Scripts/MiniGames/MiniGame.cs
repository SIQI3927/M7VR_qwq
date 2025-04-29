using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using System.Threading.Tasks;

public class MiniGame : NetworkBehaviour
{
    [SerializeField]
    public List<GameObject> objectsForTheGame;

    [SerializeField]
    public NetworkVariable<float> networkRoundDuration = new NetworkVariable<float>();

    [SerializeField]
    public float roundTime = 10f;

    // Start is called before the first frame update
    void Start()
    {
        foreach (var obj in objectsForTheGame)
        {
            obj.gameObject.SetActive(false);
        }
    }

    public virtual async Task OnGame()
    {

    }
}
