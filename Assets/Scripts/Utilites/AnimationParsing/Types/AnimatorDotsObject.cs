namespace Utils.Parser
{
    public class AnimatorDotsObject
    {
        public Animatorstate[] AnimatorState { get; set; }
        public Animatorstatetransition[] AnimatorStateTransition { get; set; }
        public Animatorcontroller[] AnimatorController { get; set; }
        public Animatorstatemachine[] AnimatorStateMachine { get; set; }

        public class Animatorstate
        {
            public string fileID { get; set; }
        }

        public class Animatorstatetransition
        {
            public string fileID { get; set; }
        }

        public class Animatorcontroller
        {
            public string fileID { get; set; }
        }

        public class Animatorstatemachine
        {
            public string fileID { get; set; }
        }
    }
}
