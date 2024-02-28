namespace ParseUtils
{
    [System.Serializable]
    public struct AnimationCurveItem
    {
        public int Id;
        public int AnimatorInstanceId;
        public int AnimationId;
        public string Path;
        public string PropertyName;
    }
}