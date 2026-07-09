using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(OwnerClientId + "; randomNumber: " + randomNumber.Value);
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.N))
        {
            // randomNumber.Value = Random.Range(0, 100);
            TestServerRpc();
        }
    }
    [ServerRpc]
    private void TestServerRpc()
    {
        Debug.Log("TestServerRpc " + OwnerClientId);
    }
    [ClientRpc]
    private void TestClientRpc()
    {
        Debug.Log("TestCllientRpc" );
    }
}
