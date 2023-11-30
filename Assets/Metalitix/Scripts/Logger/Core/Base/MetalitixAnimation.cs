using System.Collections.Generic;
using Metalitix.Core.Data.Runtime;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    public class MetalitixAnimation
    {
        private readonly Animator _animator;
        private readonly Dictionary<int, AnimationClip> _animationClips;
        
        // /// <summary>
        // /// Get current playing Animation data
        // /// </summary>
        // public AnimationData[] ActiveAnimations => _activeData.ToArray();

        private const string AnimationBaseLayer = "Base Layer";
        
        public MetalitixAnimation(Animator animator)
        {
            _animator = animator;
            var clips = _animator.runtimeAnimatorController.animationClips;
            _animationClips = new Dictionary<int, AnimationClip>();
            CreateAnimationData(clips);
        }
        
        private void CreateAnimationData(AnimationClip[] clips)
        {
            foreach (var clip in clips)
            {
                var hash = Animator.StringToHash(AnimationBaseLayer + $".{clip.name}");
                _animationClips.Add(hash, clip);
            }
        }

        public AnimationData[] GetAnimationData()
        {
            var tempData = new List<AnimationData>();
            
            for (var index = 0; index < _animationClips.Count; index++)
            {
                var data = ProceedAnimationDataFilling();
                tempData.Add(data);
            }

            return tempData.ToArray();
        }

        private AnimationClip GetClipFromHash(int hash)
        {
            return _animationClips.TryGetValue(hash, out var clip) ? clip : null;
        }

        private AnimationData ProceedAnimationDataFilling()
        {
            var layer = GetDominantLayer();
            var animState = _animator.GetCurrentAnimatorStateInfo(layer);
            var currentAnimHash = animState.fullPathHash;
            var currentClip = GetClipFromHash(currentAnimHash);
            
            if (animState.normalizedTime >= 1 && currentClip.isLooping)
            {
                _animator.Rebind();
                _animator.Update(0);
            }
            
            var animationData = new AnimationData(currentClip.name, currentClip.isLooping, currentClip.length);
            animationData.SetCurrentPlayedTime(animState.normalizedTime);
            return animationData;
        }

        private int GetDominantLayer()
        {
            var dominantIndex = 0;
            var dominantWeight = 0f;
            var weight = 0f;
            
            for (var index = 0; index < _animator.layerCount; index++) 
            {
                weight = _animator.GetLayerWeight(index);

                if (!(weight > dominantWeight)) continue;
                dominantWeight = weight;
                
                dominantIndex = index;
            }
            
            return dominantIndex;
        }
    }
}