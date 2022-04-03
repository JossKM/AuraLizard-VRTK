using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Node))]
[CanEditMultipleObjects]
public class NodeEditor : Editor
{
    void OnEnable()
    {

    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
    }
}
