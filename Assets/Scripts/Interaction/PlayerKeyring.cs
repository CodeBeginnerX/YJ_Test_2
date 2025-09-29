using System.Collections.Generic;
using UnityEngine;

namespace YJ.Interaction
{
    public class PlayerKeyring : MonoBehaviour
    {
        public static PlayerKeyring LocalPlayer { get; private set; }

        [SerializeField] private List<string> startingKeys = new List<string>();

        private readonly HashSet<string> _keys = new HashSet<string>();

        private void Awake()
        {
            if (LocalPlayer != null && LocalPlayer != this)
            {
                Debug.LogWarning("Multiple PlayerKeyring instances detected. The newest instance will be used as the local player.");
            }

            LocalPlayer = this;

            foreach (var key in startingKeys)
            {
                if (!string.IsNullOrWhiteSpace(key))
                {
                    _keys.Add(key);
                }
            }
        }

        private void OnDestroy()
        {
            if (LocalPlayer == this)
            {
                LocalPlayer = null;
            }
        }

        public bool HasKey(string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return true;
            }

            return _keys.Contains(keyId);
        }

        public void AddKey(string keyId)
        {
            if (string.IsNullOrWhiteSpace(keyId))
            {
                return;
            }

            if (_keys.Add(keyId))
            {
                Debug.Log($"Key collected: {keyId}");
            }
        }

        public static bool TryGetPlayerPosition(out Vector3 position)
        {
            if (LocalPlayer != null)
            {
                position = LocalPlayer.transform.position;
                return true;
            }

            position = Vector3.zero;
            return false;
        }
    }
}
