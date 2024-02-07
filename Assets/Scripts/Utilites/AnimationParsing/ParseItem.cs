using System;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Utils.Parse
{
    public class ParseItem : MonoBehaviour
    {
        [SerializeField] private string PathToFile;

        private string fullPath;
        private string text;
        private string resultText;

        private void Start()
        {
            ParseFileToText();
        }

        private void ParseFileToText()
        {
            string applicationDataPath = Application.dataPath;
            fullPath = applicationDataPath + PathToFile;
            StreamReader streamReader = File.OpenText(fullPath);
            text = PrepareStringText(streamReader);
            streamReader.Close();
            ParseToJson();
        }

        private void ParseToJson()
        {
            var stringReader = new StringReader(text);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yamlObject = deserializer.Deserialize(stringReader);
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, yamlObject);
            resultText = stringWriter.ToString();
        }

        private string PrepareStringText(StreamReader streamReader)
        {
            string res = "";
            string line = null;
            string idContainer = "";
            Dictionary<string, string> dict = new Dictionary<string, string>();
            while ((line = streamReader.ReadLine()) != null)
            {
                if (line.Contains("%"))
                {
                    continue;
                }
                if (line.Contains("--- "))
                {
                    idContainer = line.Substring(line.IndexOf("&") + 1);
                    line = streamReader.ReadLine();
                    res += line + System.Environment.NewLine;
                    res += "  id: " + idContainer + System.Environment.NewLine;
                    continue;
                }
                res += line + System.Environment.NewLine;
            }
            return res;
        }

    }



    public class Rootobject
    {
        public Animatorstate AnimatorState { get; set; }
        public Animatorstatetransition AnimatorStateTransition { get; set; }
        public Animatorcontroller AnimatorController { get; set; }
        public Animatorstatemachine AnimatorStateMachine { get; set; }
    }

    public class Animatorstate
    {
        public long id { get; set; }
        public int serializedVersion { get; set; }
        public int m_ObjectHideFlags { get; set; }
        public M_Correspondingsourceobject m_CorrespondingSourceObject { get; set; }
        public M_Prefabinstance m_PrefabInstance { get; set; }
        public M_Prefabasset m_PrefabAsset { get; set; }
        public string m_Name { get; set; }
        public int m_Speed { get; set; }
        public int m_CycleOffset { get; set; }
        public M_Transitions[] m_Transitions { get; set; }
        public object[] m_StateMachineBehaviours { get; set; }
        public M_Position m_Position { get; set; }
        public int m_IKOnFeet { get; set; }
        public int m_WriteDefaultValues { get; set; }
        public int m_Mirror { get; set; }
        public int m_SpeedParameterActive { get; set; }
        public int m_MirrorParameterActive { get; set; }
        public int m_CycleOffsetParameterActive { get; set; }
        public int m_TimeParameterActive { get; set; }
        public M_Motion m_Motion { get; set; }
        public object m_Tag { get; set; }
        public object m_SpeedParameter { get; set; }
        public object m_MirrorParameter { get; set; }
        public object m_CycleOffsetParameter { get; set; }
        public object m_TimeParameter { get; set; }
    }

    public class M_Correspondingsourceobject
    {
        public int fileID { get; set; }
    }

    public class M_Prefabinstance
    {
        public int fileID { get; set; }
    }

    public class M_Prefabasset
    {
        public int fileID { get; set; }
    }

    public class M_Position
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class M_Motion
    {
        public int fileID { get; set; }
        public string guid { get; set; }
        public int type { get; set; }
    }

    public class M_Transitions
    {
        public long fileID { get; set; }
    }

    public class Animatorstatetransition
    {
        public long id { get; set; }
        public int m_ObjectHideFlags { get; set; }
        public M_Correspondingsourceobject1 m_CorrespondingSourceObject { get; set; }
        public M_Prefabinstance1 m_PrefabInstance { get; set; }
        public M_Prefabasset1 m_PrefabAsset { get; set; }
        public object m_Name { get; set; }
        public M_Conditions[] m_Conditions { get; set; }
        public M_Dststatemachine m_DstStateMachine { get; set; }
        public M_Dststate m_DstState { get; set; }
        public int m_Solo { get; set; }
        public int m_Mute { get; set; }
        public int m_IsExit { get; set; }
        public int serializedVersion { get; set; }
        public float m_TransitionDuration { get; set; }
        public int m_TransitionOffset { get; set; }
        public int m_ExitTime { get; set; }
        public int m_HasExitTime { get; set; }
        public int m_HasFixedDuration { get; set; }
        public int m_InterruptionSource { get; set; }
        public int m_OrderedInterruption { get; set; }
        public int m_CanTransitionToSelf { get; set; }
    }

    public class M_Correspondingsourceobject1
    {
        public int fileID { get; set; }
    }

    public class M_Prefabinstance1
    {
        public int fileID { get; set; }
    }

    public class M_Prefabasset1
    {
        public int fileID { get; set; }
    }

    public class M_Dststatemachine
    {
        public int fileID { get; set; }
    }

    public class M_Dststate
    {
        public long fileID { get; set; }
    }

    public class M_Conditions
    {
        public int m_ConditionMode { get; set; }
        public string m_ConditionEvent { get; set; }
        public float m_EventTreshold { get; set; }
    }

    public class Animatorcontroller
    {
        public long id { get; set; }
        public int m_ObjectHideFlags { get; set; }
        public M_Correspondingsourceobject2 m_CorrespondingSourceObject { get; set; }
        public M_Prefabinstance2 m_PrefabInstance { get; set; }
        public M_Prefabasset2 m_PrefabAsset { get; set; }
        public string m_Name { get; set; }
        public int serializedVersion { get; set; }
        public M_Animatorparameters[] m_AnimatorParameters { get; set; }
        public M_Animatorlayers[] m_AnimatorLayers { get; set; }
    }

    public class M_Correspondingsourceobject2
    {
        public int fileID { get; set; }
    }

    public class M_Prefabinstance2
    {
        public int fileID { get; set; }
    }

    public class M_Prefabasset2
    {
        public int fileID { get; set; }
    }

    public class M_Animatorparameters
    {
        public string m_Name { get; set; }
        public int m_Type { get; set; }
        public int m_DefaultFloat { get; set; }
        public int m_DefaultInt { get; set; }
        public int m_DefaultBool { get; set; }
        public M_Controller m_Controller { get; set; }
    }

    public class M_Controller
    {
        public int fileID { get; set; }
    }

    public class M_Animatorlayers
    {
        public int serializedVersion { get; set; }
        public string m_Name { get; set; }
        public M_Statemachine m_StateMachine { get; set; }
        public M_Mask m_Mask { get; set; }
        public object[] m_Motions { get; set; }
        public object[] m_Behaviours { get; set; }
        public int m_BlendingMode { get; set; }
        public int m_SyncedLayerIndex { get; set; }
        public int m_DefaultWeight { get; set; }
        public int m_IKPass { get; set; }
        public int m_SyncedLayerAffectsTiming { get; set; }
        public M_Controller1 m_Controller { get; set; }
    }

    public class M_Statemachine
    {
        public long fileID { get; set; }
    }

    public class M_Mask
    {
        public int fileID { get; set; }
    }

    public class M_Controller1
    {
        public int fileID { get; set; }
    }

    public class Animatorstatemachine
    {
        public long id { get; set; }
        public int serializedVersion { get; set; }
        public int m_ObjectHideFlags { get; set; }
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
        public int fileID { get; set; }
    }

    public class M_Prefabinstance3
    {
        public int fileID { get; set; }
    }

    public class M_Prefabasset3
    {
        public int fileID { get; set; }
    }

    public class M_Statemachinetransitions
    {
    }

    public class M_Anystateposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class M_Entryposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class M_Exitposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class M_Parentstatemachineposition
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }

    public class M_Defaultstate
    {
        public long fileID { get; set; }
    }

    public class M_Childstates
    {
        public int serializedVersion { get; set; }
        public M_State m_State { get; set; }
        public M_Position1 m_Position { get; set; }
    }

    public class M_State
    {
        public long fileID { get; set; }
    }

    public class M_Position1
    {
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
    }




}

