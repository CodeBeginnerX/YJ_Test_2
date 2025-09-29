using UnityEngine;

namespace YJ.Interaction
{
    [RequireComponent(typeof(Collider))]
    public class KeyItem : MonoBehaviour
    {
        [SerializeField] private string keyId;
        [SerializeField] private bool destroyOnPickup = true;

        private void Reset()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnValidate()
        {
            var collider = GetComponent<Collider>();
            if (collider != null && !collider.isTrigger)
            {
                collider.isTrigger = true;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var keyring = other.GetComponentInParent<PlayerKeyring>();
            if (keyring == null)
            {
                return;
            }

            keyring.AddKey(keyId);

            if (destroyOnPickup)
            {
                Destroy(gameObject);
            }
        }
    }
}
