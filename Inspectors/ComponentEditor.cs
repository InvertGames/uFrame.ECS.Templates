using Invert.Common;
using Invert.Core;
using Invert.Core.GraphDesigner;
using UnityEditor;
using UnityEngine;

[UnityEditor.CustomEditor(typeof(uFrame.ECS.EcsComponent),true)]
public class ComponentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        InvertApplication.SignalEvent<IDrawUnityInspector>(_ => _.DrawInspector(target));
    }
}