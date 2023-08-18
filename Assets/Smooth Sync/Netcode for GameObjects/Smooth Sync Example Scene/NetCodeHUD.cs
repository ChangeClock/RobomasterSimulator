namespace SmoothPro.NetcodeNetworking
{
    using System.Collections;
    using Unity.Netcode;
    using UnityEngine;

    public class NetCodeHUD : MonoBehaviour
    {

        public GameObject HostButton;
        public GameObject ServerButton;
        public GameObject ClientButton;
        public GameObject DisconnectButton;

        public void OnEnable()
        {
            StartCoroutine(AssignDisconnectCallbackWhenReady());
        }

        public void OnDisable()
        {
            if (NetworkManager.Singleton)
            {
                StopAllCoroutines();
                NetworkManager.Singleton.OnClientDisconnectCallback -= this.OnDisconnect;
            }
        }

        private IEnumerator AssignDisconnectCallbackWhenReady()
        {
            while (NetworkManager.Singleton == null) yield return 0;
            NetworkManager.Singleton.OnClientDisconnectCallback += this.OnDisconnect;
        }

        public void OnDisconnect(ulong unused)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                this.SetMainMenuVisible(true);
            }
        }

        public void OnHostButtonPressed()
        {
            NetworkManager.Singleton.StartHost();
            this.SetMainMenuVisible(false);
        }

        public void OnServerButtonPressed()
        {
            NetworkManager.Singleton.StartServer();
            this.SetMainMenuVisible(false);
        }

        public void OnClientButtonPressed()
        {
            NetworkManager.Singleton.StartClient();
            this.SetMainMenuVisible(false);
        }

        public void OnDisconnectButtonPressed()
        {
            NetworkManager.Singleton.Shutdown();
            this.SetMainMenuVisible(true);
        }

        public void SetMainMenuVisible(bool visible)
        {
            this.DisconnectButton.SetActive(!visible);
            this.HostButton.SetActive(visible);
            this.ServerButton.SetActive(visible);
            this.ClientButton.SetActive(visible);
        }
    }
}