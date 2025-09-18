using UnityEngine;
using UnityEngine.UI;

public class SinglePlayerManager : MonoBehaviour
{
    public void HandleStartButton()
    {
        NetworkRunnerHandler.Instance.CreatePVERoom();
    }
}
