using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Metalitix.Scripts.Runtime.Logger.Core.Data.Base
{
    [Serializable] [DataContract]
    public class AnimationData
    {
        [DataMember] public string name { get; }
        [DataMember] public float progress { get; private set; }
        [DataMember] public bool loop { get; }
        [DataMember] public float weight { get; private set; }

        public float length { get; private set; }
        public bool isPlaying { get; private set; }
        public bool isEnded { get; private set; }
        public float currentNormalizedTime { get; private set; }
        public float lastNormalizedTime { get; private set; }
        
        [JsonConstructor]
        public AnimationData(string name, bool loop, float length, float weight)
        {
            this.name = name;
            this.loop = loop;
            this.length = length;
            this.weight = weight;
        }
        
        public AnimationData(string name, bool loop, float length)
        {
            this.name = name;
            this.loop = loop;
            this.length = length;
        }
        
        public AnimationData(AnimationClip clip)
        {
            name = clip.name;
            loop = clip.isLooping;
            length = clip.length;
        }

        public void SetWeight(float weight)
        {
            this.weight = weight;
        }

        public void SetIsPlayingNow(bool state)
        {
            isPlaying = state;
        }

        public void SetCurrentPlayedTime(float normalizedTime)
        {
            if (lastNormalizedTime != 0)
                currentNormalizedTime = normalizedTime - lastNormalizedTime;
            else
                currentNormalizedTime = normalizedTime;

            var value = length * currentNormalizedTime;
            progress = Mathf.InverseLerp(0, length, value);
            
            CheckForAnimationEnded(normalizedTime);
        }

        private void CheckForAnimationEnded(float normalizedTime)
        {
            var progressAbs = Math.Abs(progress - 1f);

            if (progressAbs < 0.001f)
            {
                lastNormalizedTime = normalizedTime;

                if (loop) return;

                isEnded = true;
                isPlaying = false;
            }
        }
    }
}