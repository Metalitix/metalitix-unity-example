using System.Threading;
using System.Threading.Tasks;
using Metalitix.Core.Data.Containers;
using Metalitix.Core.Data.Runtime;
using Metalitix.Core.Tools;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    [ExecuteInEditMode]
    public class MetalitixScene : MonoBehaviour
    {
        private Animator _animator;
        private MetalitixAnimation _metalitixAnimation;

        private CancellationTokenSource _animCancellation = new CancellationTokenSource();

        private const float AnimationSpeed = 3f;
        private const int DelayForAnimReset = 500;
        
        private void Awake()
        {
            if (!TryGetComponent<Animator>(out var animator)) return;
            
            _animator = animator;
            _metalitixAnimation = new MetalitixAnimation(_animator);
        }
        
        private void Start()
        {
            var parent = transform.parent;

            if (parent != null && parent.TryGetComponent<MetalitixCamera>(out var metalitixCamera))
            {
                MetalitixDebug.LogWarning(this, MetalitixRuntimeLogs.SceneElementIsParented);
            }
        }

        public bool IsAnimatedScene()
        {
            return _animator != null;
        }
        
        public async void UpdateAnimation(string animName, float progress, bool loop)
        {
            if (progress >= 1f && loop)
            {
                _animCancellation.Cancel();
                SetAnimationProgress(animName, 0f);
                await Task.Delay(DelayForAnimReset);
                _animCancellation = new CancellationTokenSource();
                return;
            }
            
            var currentProgress = GetAnimationProgress(animName);
            LoopAnimationProgress(animName, currentProgress, progress);
        }
        
        public AnimationData[] GetAnimationData()
        {
            return !IsAnimatedScene() ? null : _metalitixAnimation.GetAnimationData();
        }

        private async void LoopAnimationProgress(string animName, float start, float end)
        {
            float t = 0f;

            while (t < 1f)
            {
                float progress = Mathf.Lerp(start, end, t);
                
                if(!_animCancellation.IsCancellationRequested)
                    SetAnimationProgress(animName, progress);
                
                t += Time.deltaTime * AnimationSpeed;
                await Task.Yield();
            }
        }

        private void SetAnimationProgress(string animName, float progress)
        {
            if (_animator != null)
            {
                _animator.Play(animName, 0, progress);
                _animator.speed = 0f;
                _animator.Update(Time.deltaTime);
            }
        }

        private float GetAnimationProgress(string clipName)
        {
            if (_animator == null) return 0f;

            AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

            // Check if the current state matches the specified animation clip
            if (stateInfo.IsName(clipName))
            {
                // Calculate the normalized progress of the animation clip
                float progress = stateInfo.normalizedTime % 1f;
                return progress;
            }
            
            return 0f;
        }
    }
}