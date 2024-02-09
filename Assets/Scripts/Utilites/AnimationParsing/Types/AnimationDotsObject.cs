namespace Utils.Parser
{
    public class AnimationDotsObject
    {
        public Animationclip[] AnimationClip { get; set; }

        public class Animationclip
        {
            public string fileID { get; set; }
            public string m_ObjectHideFlags { get; set; }
            public M_Correspondingsourceobject m_CorrespondingSourceObject { get; set; }
            public M_Prefabinstance m_PrefabInstance { get; set; }
            public M_Prefabasset m_PrefabAsset { get; set; }
            public string m_Name { get; set; }
            public string serializedVersion { get; set; }
            public string m_Legacy { get; set; }
            public string m_Compressed { get; set; }
            public string m_UseHighQualityCurve { get; set; }
            public M_Rotationcurves[] m_RotationCurves { get; set; }
            public object[] m_CompressedRotationCurves { get; set; }
            public object[] m_EulerCurves { get; set; }
            public M_Positioncurves[] m_PositionCurves { get; set; }
            public M_Scalecurves[] m_ScaleCurves { get; set; }
            public M_Floatcurves[] m_FloatCurves { get; set; }
            public object[] m_PPtrCurves { get; set; }
            public string m_SampleRate { get; set; }
            public string m_WrapMode { get; set; }
            public M_Bounds m_Bounds { get; set; }
            public M_Clipbindingconstant m_ClipBindingConstant { get; set; }
            public M_Animationclipsettings m_AnimationClipSettings { get; set; }
            public object[] m_EditorCurves { get; set; }
            public object[] m_EulerEditorCurves { get; set; }
            public string m_HasGenericRootTransform { get; set; }
            public string m_HasMotionFloatCurves { get; set; }
            public object[] m_Events { get; set; }
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

        public class M_Bounds
        {
            public M_Center m_Center { get; set; }
            public M_Extent m_Extent { get; set; }
        }

        public class M_Center
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Extent
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Clipbindingconstant
        {
            public Genericbinding[] genericBindings { get; set; }
            public object[] pptrCurveMapping { get; set; }
        }

        public class Genericbinding
        {
            public string serializedVersion { get; set; }
            public string path { get; set; }
            public string attribute { get; set; }
            public Script script { get; set; }
            public string typeID { get; set; }
            public string customType { get; set; }
            public string isPPtrCurve { get; set; }
            public string isIntCurve { get; set; }
            public string isSerializeReferenceCurve { get; set; }
        }

        public class Script
        {
            public string fileID { get; set; }
        }

        public class M_Animationclipsettings
        {
            public string serializedVersion { get; set; }
            public M_Additivereferenceposeclip m_AdditiveReferencePoseClip { get; set; }
            public string m_AdditiveReferencePoseTime { get; set; }
            public string m_StartTime { get; set; }
            public string m_StopTime { get; set; }
            public string m_OrientationOffsetY { get; set; }
            public string m_Level { get; set; }
            public string m_CycleOffset { get; set; }
            public string m_HasAdditiveReferencePose { get; set; }
            public string m_LoopTime { get; set; }
            public string m_LoopBlend { get; set; }
            public string m_LoopBlendOrientation { get; set; }
            public string m_LoopBlendPositionY { get; set; }
            public string m_LoopBlendPositionXZ { get; set; }
            public string m_KeepOriginalOrientation { get; set; }
            public string m_KeepOriginalPositionY { get; set; }
            public string m_KeepOriginalPositionXZ { get; set; }
            public string m_HeightFromFeet { get; set; }
            public string m_Mirror { get; set; }
        }

        public class M_Additivereferenceposeclip
        {
            public string fileID { get; set; }
        }

        public class M_Rotationcurves
        {
            public Curve curve { get; set; }
            public string path { get; set; }
        }

        public class Curve
        {
            public string serializedVersion { get; set; }
            public M_Curve[] m_Curve { get; set; }
            public string m_PreInfinity { get; set; }
            public string m_PostInfinity { get; set; }
            public string m_RotationOrder { get; set; }
        }

        public class M_Curve
        {
            public string serializedVersion { get; set; }
            public string time { get; set; }
            public Value value { get; set; }
            public Inslope inSlope { get; set; }
            public Outslope outSlope { get; set; }
            public string tangentMode { get; set; }
            public string weightedMode { get; set; }
            public Inweight inWeight { get; set; }
            public Outweight outWeight { get; set; }
        }

        public class Value
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
            public string w { get; set; }
        }

        public class Inslope
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
            public string w { get; set; }
        }

        public class Outslope
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
            public string w { get; set; }
        }

        public class Inweight
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
            public string w { get; set; }
        }

        public class Outweight
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
            public string w { get; set; }
        }

        public class M_Positioncurves
        {
            public Curve1 curve { get; set; }
            public string path { get; set; }
        }

        public class Curve1
        {
            public string serializedVersion { get; set; }
            public M_Curve1[] m_Curve { get; set; }
            public string m_PreInfinity { get; set; }
            public string m_PostInfinity { get; set; }
            public string m_RotationOrder { get; set; }
        }

        public class M_Curve1
        {
            public string serializedVersion { get; set; }
            public string time { get; set; }
            public Value1 value { get; set; }
            public Inslope1 inSlope { get; set; }
            public Outslope1 outSlope { get; set; }
            public string tangentMode { get; set; }
            public string weightedMode { get; set; }
            public Inweight1 inWeight { get; set; }
            public Outweight1 outWeight { get; set; }
        }

        public class Value1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Inslope1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Outslope1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Inweight1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Outweight1
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Scalecurves
        {
            public Curve2 curve { get; set; }
            public string path { get; set; }
        }

        public class Curve2
        {
            public string serializedVersion { get; set; }
            public M_Curve2[] m_Curve { get; set; }
            public string m_PreInfinity { get; set; }
            public string m_PostInfinity { get; set; }
            public string m_RotationOrder { get; set; }
        }

        public class M_Curve2
        {
            public string serializedVersion { get; set; }
            public string time { get; set; }
            public Value2 value { get; set; }
            public Inslope2 inSlope { get; set; }
            public Outslope2 outSlope { get; set; }
            public string tangentMode { get; set; }
            public string weightedMode { get; set; }
            public Inweight2 inWeight { get; set; }
            public Outweight2 outWeight { get; set; }
        }

        public class Value2
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Inslope2
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Outslope2
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Inweight2
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class Outweight2
        {
            public string x { get; set; }
            public string y { get; set; }
            public string z { get; set; }
        }

        public class M_Floatcurves
        {
            public string serializedVersion { get; set; }
            public Curve3 curve { get; set; }
            public string attribute { get; set; }
            public object path { get; set; }
            public string classID { get; set; }
            public Script1 script { get; set; }
            public string flags { get; set; }
        }

        public class Curve3
        {
            public string serializedVersion { get; set; }
            public M_Curve3[] m_Curve { get; set; }
            public string m_PreInfinity { get; set; }
            public string m_PostInfinity { get; set; }
            public string m_RotationOrder { get; set; }
        }

        public class M_Curve3
        {
            public string serializedVersion { get; set; }
            public string time { get; set; }
            public string value { get; set; }
            public string inSlope { get; set; }
            public string outSlope { get; set; }
            public string tangentMode { get; set; }
            public string weightedMode { get; set; }
            public string inWeight { get; set; }
            public string outWeight { get; set; }
        }

        public class Script1
        {
            public string fileID { get; set; }
        }
    }
}
