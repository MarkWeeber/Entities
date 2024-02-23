namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationCurveKeyBuffer
    {
        public int Id;
        public int AnimatorInstanceId;
        public int AnimationId;
        public int CurveId;
        public float Time;
        public float Value;
    }
}