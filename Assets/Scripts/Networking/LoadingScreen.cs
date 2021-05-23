using System.Collections;
using UnityEngine;
namespace SteamNetworking.GUI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField]
        protected Transform loadingIcon;
        [SerializeField]
        protected float rotationSpeed;

        protected void Update()
        {
            loadingIcon.Rotate(0, 0, Time.unscaledDeltaTime * rotationSpeed);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        public static void Instantiate()
        {
            Instantiate(Resources.Load<GameObject>("Loading Screen Canvas"));
        }
    }
}