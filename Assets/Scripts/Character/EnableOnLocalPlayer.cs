using MLAPI;
using UnityEngine;

namespace PropHunt.Character
{

    public class EnableOnLocalPlayer : NetworkBehaviour
    {
        public GameObject enableOnLocalPlayer;

        public void Update()
        {
            enableOnLocalPlayer.SetActive(this.IsLocalPlayer);
        }
    }
}