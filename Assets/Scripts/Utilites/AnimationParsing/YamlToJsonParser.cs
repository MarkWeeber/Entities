using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Utils.Parse
{
    public class YamlToJsonParser : MonoBehaviour
    {
        public RuntimeAnimatorController RunTimeAnimatorController;
        public AnimationClip AnimationClip;
        public string ResultText;
        public bool AnimationParseSuccess;
        public bool AnimatorParseSuccess;

        public void ParseAnimation()
        {
            if (AnimationParseSuccess)
            {
                Debug.Log("Animation Parse Success!");
            }
        }

        public void ParseAnimator()
        {
            if (AnimatorParseSuccess)
            {
                Debug.Log("Animator Parse Success!");
            }
        }

    }
}

