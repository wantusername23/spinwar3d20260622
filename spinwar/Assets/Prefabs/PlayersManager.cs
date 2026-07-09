using DilmerGames.Core.Singletons;
using Unity.Netcode;

public class PlayersManager : Singleton<PlayersManager>
{
    private NetworkVariable<int> playersInGame = new NetworkVariable<int>();
    public int PlayersInGame
    {
        get
        {
            return playersInGame.Value;
        }
    }
    private void start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += (id) =>
        {
            if (IsServer)
            {
        
                playersInGame.Value++;
            }
        };
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            if (IsServer)
            {
                
                playersInGame.Value--;
            }
        };
    }
}
