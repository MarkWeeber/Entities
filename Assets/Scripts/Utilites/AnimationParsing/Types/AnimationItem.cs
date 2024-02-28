namespace ParseUtils
{
    [System.Serializable]
    public partial struct AnimationItem
    {
        public int AnimationInstanceId;
        public int AnimatorInstanceId;
        public string Name;
        public bool Looped;
        public float Length;
    }
}