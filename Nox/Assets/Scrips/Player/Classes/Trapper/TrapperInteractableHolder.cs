using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TrapperInteractableHolder : MonoBehaviour
{
    [SerializeField] private float spawnSpeed = 1f;
    [SerializeField] private int childCount;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        childCount = transform.childCount;

        // Disable all children initially
        for (int i = 0; i < childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnRoutine());
        }
        
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            ActivateRandomChild();
            yield return new WaitForSeconds(spawnSpeed);
        }
    }

    private void ActivateRandomChild()
    {
        List<int> inactiveIndices = new List<int>();

        for (int i = 0; i < childCount; i++)
        {
            if (!transform.GetChild(i).gameObject.activeSelf)
            {
                inactiveIndices.Add(i);
            }
        }

        if (inactiveIndices.Count == 0)
            return; // All children are active — do nothing

        int randomIndex = inactiveIndices[Random.Range(0, inactiveIndices.Count)];
        photonView.RPC("RPC_SetActiveState", RpcTarget.All, randomIndex);
    }

    [PunRPC]
    private void RPC_SetActiveState(int childIndex)
    {
        if (childIndex >= 0 && childIndex < transform.childCount)
        {
            transform.GetChild(childIndex).gameObject.SetActive(true);
        }
    }
}
