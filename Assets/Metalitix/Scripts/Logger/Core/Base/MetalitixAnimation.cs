using System.Collections.Generic;
using System.Linq;
using Metalitix.Core.Data.Runtime;
using UnityEngine;

namespace Metalitix.Scripts.Logger.Core.Base
{
    [RequireComponent(typeof(Animator))]
    public class MetalitixAnimation : MonoBehaviour
    {
        private Animator _animator;
        private AnimationData[] _animationData;
        private List<AnimationData> _activeData;
        
        private Dictionary<int, AnimationClip> _animationClips;
        
        public AnimationData[] AnimationData => _animationData;
        
        /// <summary>
        /// Get current playing Animation data
        /// </summary>
        public AnimationData[] ActiveAnimations => _activeData.ToArray();

        private const string AnimationBaseLayer = "Base Layer";
        
        private void Awake()
        {
            _animator = GetComponent<Animator>();

            var clips = _animator.runtimeAnimatorController.animationClips;
            
            InitializeCollections(clips);
            CreateAnimationData(clips);
        }

        private void InitializeCollections(AnimationClip[] clips)
        {
            _animationClips = new Dictionary<int, AnimationClip>();
            _animationData = new AnimationData[clips.Length];
            _activeData = new List<AnimationData>();
        }

        private void CreateAnimationData(AnimationClip[] clips)
        {
            var index = 0;

            foreach (var clip in clips)
            {
                var hash = Animator.StringToHash(AnimationBaseLayer + $".{clip.name}");
                _animationClips.Add(hash, clip);
                _animationData[index] = new AnimationData(clip);
                index++;
            }
        }

        private void Update()
        {
            foreach (var clip in _animationData)
            {
                ProceedAnimationDataFilling(clip);

                if (clip.isPlaying && !_activeData.Contains(clip))
                {
                    _activeData.Add(clip);
                }
            }
        }

        private AnimationClip GetClipFromHash(int hash)
        {
            return _animationClips.TryGetValue(hash, out var clip) ? clip : null;
        }

        private void ProceedAnimationDataFilling(AnimationData data)
        {
            if (data.isEnded)
            {
                if (_activeData.Contains(data))
                    _activeData.Remove(data);
                return;
            }
            
            var layer = GetDominantLayer();
            var animState = _animator.GetCurrentAnimatorStateInfo(layer);
            var animCLip = _animator.GetCurrentAnimatorClipInfo(layer);
            
            if(animCLip.Length == 0) return;
            
            var clipWeights = animCLip
                .Where(c => c.clip.name == data.name)
                .Select(c => c.weight);
            
            if(!clipWeights.Any()) return;

            var clipWeight = clipWeights.First();
            
            var isAnimatorStatePlayingClip = animState.IsName(data.name);
            
            data.SetIsPlayingNow(isAnimatorStatePlayingClip);
            data.SetWeight(clipWeight);

            var currentAnimHash = animState.fullPathHash;

            if (currentAnimHash == 0) return;
            
            var currentClip = GetClipFromHash(currentAnimHash);
            
            if (currentClip == null) return;

            data.SetCurrentPlayedTime(animState.normalizedTime);
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