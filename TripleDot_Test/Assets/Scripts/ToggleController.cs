using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ToggleAnimationController : MonoBehaviour
{
    [Header("Animator Reference")]
    public MultiUIAnimator animator;

    [Header("Animations to play when Toggle is ON")]
    public List<string> onAnimations = new();

    [Header("Animations to play when Toggle is OFF")]
    public List<string> offAnimations = new();

    private Toggle toggle;

    private bool isOn = true;

    public void ToggleValueChanged()
    {
        isOn = !isOn;
        PlayAnimations(isOn ? onAnimations : offAnimations);
    }

    private void PlayAnimations(List<string> animationNames)
    {
        if (animator == null) return;

        foreach (var animName in animationNames)
        {
            animator.PlayAnimationByName(animName);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ToggleAnimationController))]
public class ToggleAnimationControllerEditor : Editor
{
    SerializedProperty animatorProp;
    SerializedProperty onAnimationsProp;
    SerializedProperty offAnimationsProp;

    private void OnEnable()
    {
        animatorProp = serializedObject.FindProperty("animator");
        onAnimationsProp = serializedObject.FindProperty("onAnimations");
        offAnimationsProp = serializedObject.FindProperty("offAnimations");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(animatorProp);

        MultiUIAnimator animator = (MultiUIAnimator)animatorProp.objectReferenceValue;
        string[] animNames = animator != null ? animator.animations.ConvertAll(a => a.name).ToArray() : new string[0];

        EditorGUILayout.Space();
        DrawAnimationList("On Animations", onAnimationsProp, animNames);

        EditorGUILayout.Space();
        DrawAnimationList("Off Animations", offAnimationsProp, animNames);

        serializedObject.ApplyModifiedProperties();
    }

    void DrawAnimationList(string label, SerializedProperty listProp, string[] options)
    {
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty stringProp = listProp.GetArrayElementAtIndex(i);
            int currentIndex = System.Array.IndexOf(options, stringProp.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            EditorGUILayout.BeginHorizontal();
            if (options.Length > 0)
            {
                int newIndex = EditorGUILayout.Popup(currentIndex, options);
                stringProp.stringValue = options[newIndex];
            }
            else
            {
                EditorGUILayout.LabelField("No animations available");
            }

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                listProp.DeleteArrayElementAtIndex(i);
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Animation"))
        {
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
            listProp.GetArrayElementAtIndex(listProp.arraySize - 1).stringValue = options.Length > 0 ? options[0] : "";
        }
    }
}
#endif
