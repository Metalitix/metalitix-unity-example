using Metalitix.Scripts.Logger.Core.Base;
using UnityEngine;

namespace Metalitix.Examples.Scripts
{
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SphereCollider))]
    [RequireComponent(typeof(MetalitixAnimation))]
    public class DoorExample : MonoBehaviour
    {
        [SerializeField] private string isOnKey;

        private Animator _animator;
        
        private void Start()
        {
            _animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            _animator.SetBool(isOnKey, true);
        }

        private void OnTriggerExit(Collider other)
        {
            _animator.SetBool(isOnKey, false);
        }
    }
}
