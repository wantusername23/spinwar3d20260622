using UnityEngine;
using Unity.Netcode;
using Unity.Collections;
using TMPro;

public class PlayerHud : NetworkBehaviour
{
    private NetworkVariable<NetworkString> playersName = new NetworkVariable<NetworkString>();
    private bool overlaySet = false;
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playersName.Value = $"Player {OwnerClientId}";
        }
    }
    public void SetOverlay()
    {
        var localPlayerOverlay = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        localPlayerOverlay.text = playersName.Value;
    }
    private void update()
    {
        if (!overlaySet && !string.IsNullOrEmpty(playersName.Value))
        {
            SetOverlay();
            overlaySet = true;
        }
    }
}
public struct NetworkString: INetworkSerializable
{
    private FixedString32Bytes info;
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref info);
    }

    public override string ToString()
    {
        return info.ToString();
    }
    public static implicit operator string (NetworkString s) => s.ToString();
    public static implicit operator NetworkString(string s) => new NetworkString() { info = new FixedString32Bytes(s) };
}