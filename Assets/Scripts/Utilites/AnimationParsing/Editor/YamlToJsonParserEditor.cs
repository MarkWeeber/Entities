using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utils.Parse;
using Utils.Parser;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

[CustomEditor(typeof(YamlToJsonParser))]
public class YamlToJsonParserEditor : Editor
{
    private enum ObjectType
    {
        Animator = 0,
        Animation = 1,
    }

    public override void OnInspectorGUI()
    {
        YamlToJsonParser parser = (YamlToJsonParser)target;

        GUI.enabled = false;
        EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour((YamlToJsonParser)target), typeof(YamlToJsonParser), false);
        GUI.enabled = true;

        parser.AnimationClip = EditorGUILayout.ObjectField("Animation Clip", parser.AnimationClip, typeof(AnimationClip), false) as AnimationClip;
        if (GUILayout.Button("Parse Animation"))
        {
            parser.AnimatorParseSuccess = ParseObject(parser, ObjectType.Animation);
            parser.ParseAnimation();
        }

        parser.RunTimeAnimatorController = EditorGUILayout.ObjectField("Animator Clip", parser.RunTimeAnimatorController, typeof(RuntimeAnimatorController), false) as RuntimeAnimatorController;
        if (GUILayout.Button("Parse Animator"))
        {
            parser.AnimatorParseSuccess = ParseObject(parser, ObjectType.Animator);
            parser.ParseAnimator();
        }

    }

    private bool ParseObject(YamlToJsonParser parser, ObjectType objectType)
    {
        int instanceId = 0;
        if (objectType == ObjectType.Animator)
        {
            instanceId = parser.RunTimeAnimatorController.GetInstanceID();
        }
        else if (objectType == ObjectType.Animation)
        {
            instanceId = parser.AnimationClip.GetInstanceID();
        }
        var assetPath = AssetDatabase.GetAssetPath(instanceId);
        StreamReader streamReader = File.OpenText(assetPath);
        string correctFormattedString = CorrectFormatForYAML(streamReader);
        streamReader.Close();
        return ParseYamlToJson(parser, correctFormattedString, out parser.ResultText, objectType);
    }

    private string CorrectFormatForYAML(StreamReader streamReader)
    {
        string resultText = "";
        string line = null;
        string idContainer = "";
        string rootParameter = "";
        Dictionary<string, string> rootAndContent = new Dictionary<string, string>();
        while ((line = streamReader.ReadLine()) != null)
        {
            if (line.Contains("%"))
            {
                continue;
            }
            if (line.Contains("--- ")) // root parameter
            {
                idContainer = line.Substring(line.IndexOf("&") + 1);
                line = streamReader.ReadLine();
                rootParameter = line;
                idContainer = "  - fileID: " + idContainer;
                TryAddToDictionary(rootAndContent, rootParameter, idContainer);
            }
            else
            {
                TryAddToDictionary(rootAndContent, rootParameter, "  " + line);
            }
        }
        foreach (var item in rootAndContent)
        {
            string value = item.Key + System.Environment.NewLine + item.Value;
            resultText += value;
        }
        return resultText;
    }

    private bool ParseYamlToJson(YamlToJsonParser parser, string formattedTextForYaml, out string result, ObjectType objectType)
    {
        try
        {
            var stringReader = new StringReader(formattedTextForYaml);
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var yamlObject = deserializer.Deserialize(stringReader);
            var serializer = new Newtonsoft.Json.JsonSerializer();
            var stringWriter = new StringWriter();
            serializer.Serialize(stringWriter, yamlObject);
            result = stringWriter.ToString();
            SaveAsset(parser, result, objectType);
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
            result = "";
            return false;
        }
    }

    private void TryAddToDictionary(Dictionary<string, string> dictionary, string key, string value)
    {
        string valueWithEndLine = value + System.Environment.NewLine;
        if (dictionary.ContainsKey(key))
        {
            dictionary[key] += valueWithEndLine;
        }
        else
        {
            dictionary.Add(key, valueWithEndLine);
        }
    }

    private void SaveAsset(YamlToJsonParser parser, string contnet, ObjectType objectType)
    {
        int instanceId = 0;
        if (objectType == ObjectType.Animator)
        {
            var asset = ScriptableObject.CreateInstance<AnimatorDotsAsset>();
            //asset.Content = contnet;
            asset.AnimatorDOTSObject = JsonUtility.FromJson<AnimatorDOTSObject>(contnet);
            instanceId = parser.RunTimeAnimatorController.GetInstanceID();
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            assetPath = assetPath.Replace(".controller", "DOTS.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
        }
        else if (objectType == ObjectType.Animation)
        {
            var asset = ScriptableObject.CreateInstance<AnimationDotsAsset>();
            //asset.Content = contnet;
            asset.AnimationDOTSObject = JsonUtility.FromJson<AnimationDOTSObject>(contnet);
            instanceId = parser.AnimationClip.GetInstanceID();
            var assetPath = AssetDatabase.GetAssetPath(instanceId);
            assetPath = assetPath.Replace(".anim", "DOTS.asset");
            AssetDatabase.CreateAsset(asset, assetPath);
        }
    }
}
