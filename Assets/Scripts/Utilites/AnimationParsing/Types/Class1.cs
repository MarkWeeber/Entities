using System;
using System.Collections.Generic;
using System.Text;

namespace Assets.Scripts.Utilites.AnimationParsing.Types
{
    class Class1
    {

        public class Rootobject
        {
            public Animatorstate[] AnimatorState { get; set; }
            public Animatorstatetransition[] AnimatorStateTransition { get; set; }
            public Animatorcontroller[] AnimatorController { get; set; }
            public Animatorstatemachine[] AnimatorStateMachine { get; set; }
        }

        public class Animatorstate
        {
            public string fileID { get; set; }
            public string serializedVersion { get; set; }
            public string m_ObjectHideFlags { get; set; }
            public M_Correspondingsourceobject m_CorrespondingSourceObject { get; set; }
            public M_Prefabinstance m_PrefabInstance { get; set; }
            public M_Prefabasset m_PrefabAsset { get; set; }
            public string m_Name { get; set; }
            public string m_Speed { get; set; }
            public string m_CycleOffset { get; set; }
            public M_Transitions[] m_Transitions { get; set; }
            public object[] m_StateMachineBehaviours { get; set; }
            public M_Position m_Position { get; set; }
            public string m_IKOnFeet { get; set; }
            public string m_WriteDefaultValues { get; set; }
            public string m_Mirror { get; set; }
            public string m_SpeedParameterActive { get; set; }
            public string m_MirrorParameterActive { get; set; }
            public string m_CycleOffsetParameterActive { get; set; }
            public string m_TimeParameterActive { get; set; }
            public M_Motion m_Motion { get; set; }
            public object m_Tag { get; set; }
            public string m_SpeedParameter { get; set; }
            public string m_MirrorParameter { get; set; }
            public string m_CycleOffsetParameter { get; set; }
            public string m_TimeParameter { get; set; }
        }

        public class M_Correspondingsourceobject
        {
            public string fileID { get; set; }
        }

        public class M_Prefabinstance
        {
            public string fileID { get; set; }
        }

        public class M_Prefabasset
        {
            public string fileID { get; set; }
        }

        public class M_Position
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Motion
        {
            public string fileID { get; set; }
            public string guid { get; set; }
            public string type { get; set; }
        }

        public class M_Transitions
        {
            public string fileID { get; set; }
        }

        public class Animatorstatetransition
        {
            public string fileID { get; set; }
            public string m_ObjectHideFlags { get; set; }
            public M_Correspondingsourceobject1 m_CorrespondingSourceObject { get; set; }
            public M_Prefabinstance1 m_PrefabInstance { get; set; }
            public M_Prefabasset1 m_PrefabAsset { get; set; }
            public object m_Name { get; set; }
            public M_Conditions[] m_Conditions { get; set; }
            public M_Dststatemachine m_DstStateMachine { get; set; }
            public M_Dststate m_DstState { get; set; }
            public string m_Solo { get; set; }
            public string m_Mute { get; set; }
            public string m_IsExit { get; set; }
            public string serializedVersion { get; set; }
            public string m_TransitionDuration { get; set; }
            public string m_TransitionOffset { get; set; }
            public string m_ExitTime { get; set; }
            public string m_HasExitTime { get; set; }
            public string m_HasFixedDuration { get; set; }
            public string m_InterruptionSource { get; set; }
            public string m_OrderedInterruption { get; set; }
            public string m_CanTransitionToSelf { get; set; }
        }

        public class M_Correspondingsourceobject1
        {
            public string fileID { get; set; }
        }

        public class M_Prefabinstance1
        {
            public string fileID { get; set; }
        }

        public class M_Prefabasset1
        {
            public string fileID { get; set; }
        }

        public class M_Dststatemachine
        {
            public string fileID { get; set; }
        }

        public class M_Dststate
        {
            public string fileID { get; set; }
        }

        public class M_Conditions
        {
            public string m_ConditionMode { get; set; }
            public string m_ConditionEvent { get; set; }
            public string m_EventTreshold { get; set; }
        }

        public class Animatorcontroller
        {
            public string fileID { get; set; }
            public string m_ObjectHideFlags { get; set; }
            public M_Correspondingsourceobject2 m_CorrespondingSourceObject { get; set; }
            public M_Prefabinstance2 m_PrefabInstance { get; set; }
            public M_Prefabasset2 m_PrefabAsset { get; set; }
            public string m_Name { get; set; }
            public string serializedVersion { get; set; }
            public M_Animatorparameters[] m_AnimatorParameters { get; set; }
            public M_Animatorlayers[] m_AnimatorLayers { get; set; }
        }

        public class M_Correspondingsourceobject2
        {
            public string fileID { get; set; }
        }

        public class M_Prefabinstance2
        {
            public string fileID { get; set; }
        }

        public class M_Prefabasset2
        {
            public string fileID { get; set; }
        }

        public class M_Animatorparameters
        {
            public string m_Name { get; set; }
            public string m_Type { get; set; }
            public string m_DefaultFloat { get; set; }
            public string m_DefaultInt { get; set; }
            public string m_DefaultBool { get; set; }
            public M_Controller m_Controller { get; set; }
        }

        public class M_Controller
        {
            public string fileID { get; set; }
        }

        public class M_Animatorlayers
        {
            public string serializedVersion { get; set; }
            public string m_Name { get; set; }
            public M_Statemachine m_StateMachine { get; set; }
            public M_Mask m_Mask { get; set; }
            public object[] m_Motions { get; set; }
            public object[] m_Behaviours { get; set; }
            public string m_BlendingMode { get; set; }
            public string m_SyncedLayerIndex { get; set; }
            public string m_DefaultWeight { get; set; }
            public string m_IKPass { get; set; }
            public string m_SyncedLayerAffectsTiming { get; set; }
            public M_Controller1 m_Controller { get; set; }
        }

        public class M_Statemachine
        {
            public string fileID { get; set; }
        }

        public class M_Mask
        {
            public string fileID { get; set; }
        }

        public class M_Controller1
        {
            public string fileID { get; set; }
        }

        public class Animatorstatemachine
        {
            public string fileID { get; set; }
            public string serializedVersion { get; set; }
            public string m_ObjectHideFlags { get; set; }
            public M_Correspondingsourceobject3 m_CorrespondingSourceObject { get; set; }
            public M_Prefabinstance3 m_PrefabInstance { get; set; }
            public M_Prefabasset3 m_PrefabAsset { get; set; }
            public string m_Name { get; set; }
            public M_Childstates[] m_ChildStates { get; set; }
            public object[] m_ChildStateMachines { get; set; }
            public object[] m_AnyStateTransitions { get; set; }
            public object[] m_EntryTransitions { get; set; }
            public M_Statemachinetransitions m_StateMachineTransitions { get; set; }
            public object[] m_StateMachineBehaviours { get; set; }
            public M_Anystateposition m_AnyStatePosition { get; set; }
            public M_Entryposition m_EntryPosition { get; set; }
            public M_Exitposition m_ExitPosition { get; set; }
            public M_Parentstatemachineposition m_ParentStateMachinePosition { get; set; }
            public M_Defaultstate m_DefaultState { get; set; }
        }

        public class M_Correspondingsourceobject3
        {
            public string fileID { get; set; }
        }

        public class M_Prefabinstance3
        {
            public string fileID { get; set; }
        }

        public class M_Prefabasset3
        {
            public string fileID { get; set; }
        }

        public class M_Statemachinetransitions
        {
        }

        public class M_Anystateposition
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Entryposition
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Exitposition
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Parentstatemachineposition
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Defaultstate
        {
            public string fileID { get; set; }
        }

        public class M_Childstates
        {
            public string serializedVersion { get; set; }
            public M_State m_State { get; set; }
            public M_Position1 m_Position { get; set; }
        }

        public class M_State
        {
            public string fileID { get; set; }
        }

        public class M_Position1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

    }
}
