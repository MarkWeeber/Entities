using UnityEngine;

namespace Utils.Parser
{
    public class YamlToJsonParser : MonoBehaviour
    {
        public RuntimeAnimatorController RunTimeAnimatorController;
        public AnimationClip AnimationClip;
        public string ResultText;
        public bool AnimationParseSuccess;
        public bool AnimatorParseSuccess;
        public AnimatorDotsAsset AnimatorDotsAssetTest;
        public AnimationDotsAsset AnimationDotsAssetTest;
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

        public void TestAnimatorDotsObjectAsset()
        {
            if (AnimatorDotsAssetTest != null)
            {
                var dostObject = AnimatorDotsAssetTest.AnimatorDOTSObject;
                for (int i = 0; i < dostObject.AnimatorState.Length; i++)
                {
                    var fileId = dostObject.AnimatorState[i].fileID;
                    Debug.Log(fileId);
                }
            }
        }

        public void TestAnimationDotsObjectAsset()
        {
            if (AnimationDotsAssetTest != null)
            {
                var dostObject = AnimationDotsAssetTest.AnimationDOTSObject;
                for (int i = 0; i < dostObject.AnimationClip.Length; i++)
                {
                    var fileId = dostObject.AnimationClip[i].fileID;
                    Debug.Log(fileId);
                }
            }
        }
    }
}

