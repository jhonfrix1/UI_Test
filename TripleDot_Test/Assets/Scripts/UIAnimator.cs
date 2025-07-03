using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEditor;

public class MultiUIAnimator : MonoBehaviour
{
    public List<UIAnimationStep> animations = new List<UIAnimationStep>();

    private void Start()
    {
        foreach (var anim in animations)
        {
            if (anim.triggerMode == UIAnimationStep.TriggerMode.OnStart)
                anim.Play();
            else if (anim.triggerMode == UIAnimationStep.TriggerMode.OnLoop)
                anim.Play(loop: true);
        }
    }

    public void PlayAnimationByName(string animName)
    {
        var anim = animations.Find(a => a.name == animName);
        if (anim != null) anim.Play();
    }

    public void PlayAnimation(UIAnimationStep anim)
    {
        if (anim != null) anim.Play();
    }
}

[System.Serializable]
public class GameObjectToggle
{
    public GameObject target;
    public bool setActive;
}

[System.Serializable]
public class ComponentToggle
{
    public Component target;
    public bool setEnabled;
}

[System.Serializable]
public class UIAnimationStep
{
    public enum TriggerMode { OnStart, OnLoop, OnCall }

    [Header("General")]
    public string name = "New Animation";
    public TriggerMode triggerMode = TriggerMode.OnCall;

    [Header("Targets")]
    public RectTransform rectTarget;
    public Graphic graphicTarget;
    public CanvasGroup canvasGroupTarget;

    [Header("Animation")]
    public bool animatePosition = false;
    public Vector3 targetPosition;

    public bool animateScale = false;
    public Vector3 targetScale = Vector3.one;

    public bool animateRotation = false;
    public Vector3 targetRotation;

    public bool animateColor = false;
    public Color targetColor = Color.white;

    public bool animateAlpha = false;
    [Range(0f, 1f)] public float targetAlpha = 1f;

    [Header("Timing")]
    public float duration = 1f;
    public float delay = 0f;

    [Header("Looping")]
    public bool loop = false;
    public int loopCount = 0;
    public LoopType loopType = LoopType.Restart;

    [Header("Easing")]
    public AnimationCurve easingCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("GameObject Activation/Deactivation")]
    public List<GameObjectToggle> toggleObjects = new();

    [Header("Component Activation/Deactivation")]
    public List<ComponentToggle> toggleComponents = new();

    [Header("Events")]
    public UnityEvent onComplete = new();

    [HideInInspector] public bool foldout = true;

    private Sequence sequence;

    public void Play(bool loop = false)
    {
        if (rectTarget == null && graphicTarget == null && canvasGroupTarget == null &&
            toggleObjects.Count == 0 && toggleComponents.Count == 0)
            return;

        if (delay > 0f)
        {
            if (animatePosition && rectTarget != null)
                rectTarget.anchoredPosition = targetPosition;

            if (animateScale && rectTarget != null)
                rectTarget.localScale = targetScale;

            if (animateRotation && rectTarget != null)
                rectTarget.localEulerAngles = targetRotation;

            if (animateColor && graphicTarget != null)
                graphicTarget.color = targetColor;

            if (animateAlpha && canvasGroupTarget != null)
                canvasGroupTarget.alpha = targetAlpha;
        }

        sequence?.Kill();
        sequence = DOTween.Sequence();

        if (animatePosition && rectTarget != null)
        {
            Tween t = rectTarget.DOAnchorPos(targetPosition, duration).SetDelay(delay);
            sequence.Join(t.SetEase(easingCurve));
        }

        if (animateScale && rectTarget != null)
        {
            Tween t = rectTarget.DOScale(targetScale, duration).SetDelay(delay);
            sequence.Join(t.SetEase(easingCurve));
        }

        if (animateRotation && rectTarget != null)
        {
            Tween t = rectTarget.DORotate(targetRotation, duration, RotateMode.FastBeyond360).SetDelay(delay);
            sequence.Join(t.SetEase(easingCurve));
        }

        if (animateColor && graphicTarget != null)
        {
            Tween t = graphicTarget.DOColor(targetColor, duration).SetDelay(delay);
            sequence.Join(t.SetEase(easingCurve));
        }

        if (animateAlpha && canvasGroupTarget != null)
        {
            Tween t = canvasGroupTarget.DOFade(targetAlpha, duration).SetDelay(delay);
            sequence.Join(t.SetEase(easingCurve));
        }

        if (loop || this.loop)
        {
            sequence.SetLoops(loopCount <= 0 ? -1 : loopCount, loopType);
        }

        sequence.OnComplete(() =>
        {
            onComplete?.Invoke();
        });

        sequence.Play();

        foreach (var toggle in toggleObjects)
        {
            if (toggle.target != null)
                toggle.target.SetActive(toggle.setActive);
        }

        foreach (var toggle in toggleComponents)
        {
            if (toggle.target != null)
            {
                if (toggle.target is Behaviour b)
                    b.enabled = toggle.setEnabled;
                else if (toggle.target is Renderer r)
                    r.enabled = toggle.setEnabled;
                else
                    toggle.target.gameObject.SetActive(toggle.setEnabled);
            }
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MultiUIAnimator))]
public class MultiUIAnimatorEditor : Editor
{
    SerializedProperty animationsProp;

    void OnEnable()
    {
        animationsProp = serializedObject.FindProperty("animations");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        int indexToRemove = -1;

        for (int i = 0; i < animationsProp.arraySize; i++)
        {
            SerializedProperty animProp = animationsProp.GetArrayElementAtIndex(i);
            SerializedProperty foldoutProp = animProp.FindPropertyRelative("foldout");
            SerializedProperty nameProp = animProp.FindPropertyRelative("name");

            EditorGUILayout.BeginHorizontal();
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, nameProp.stringValue, true);
            if (GUILayout.Button("Remove", GUILayout.Width(70)))
            {
                indexToRemove = i;
            }
            EditorGUILayout.EndHorizontal();

            if (foldoutProp.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(nameProp);
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("triggerMode"));
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("rectTarget"));
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("graphicTarget"));
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("canvasGroupTarget"));

                DrawToggleAndValue(animProp, "animatePosition", "targetPosition");
                DrawToggleAndValue(animProp, "animateScale", "targetScale");
                DrawToggleAndValue(animProp, "animateRotation", "targetRotation");
                DrawToggleAndValue(animProp, "animateColor", "targetColor");
                DrawToggleAndValue(animProp, "animateAlpha", "targetAlpha");

                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("duration"));
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("delay"));

                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("loop"));
                if (animProp.FindPropertyRelative("loop").boolValue)
                {
                    EditorGUILayout.PropertyField(animProp.FindPropertyRelative("loopCount"));
                    EditorGUILayout.PropertyField(animProp.FindPropertyRelative("loopType"));
                }

                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("easingCurve"));

                SerializedProperty togglesProp = animProp.FindPropertyRelative("toggleObjects");
                EditorGUILayout.LabelField("GameObject Toggles", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int j = 0; j < togglesProp.arraySize; j++)
                {
                    SerializedProperty toggleProp = togglesProp.GetArrayElementAtIndex(j);
                    SerializedProperty targetProp = toggleProp.FindPropertyRelative("target");
                    SerializedProperty setActiveProp = toggleProp.FindPropertyRelative("setActive");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(targetProp, GUIContent.none);
                    EditorGUILayout.PropertyField(setActiveProp, GUIContent.none, GUILayout.Width(60));
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        togglesProp.DeleteArrayElementAtIndex(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+ Add GameObject Toggle"))
                {
                    togglesProp.InsertArrayElementAtIndex(togglesProp.arraySize);
                }
                EditorGUI.indentLevel--;

                SerializedProperty componentTogglesProp = animProp.FindPropertyRelative("toggleComponents");
                EditorGUILayout.LabelField("Component Toggles", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int j = 0; j < componentTogglesProp.arraySize; j++)
                {
                    SerializedProperty toggleProp = componentTogglesProp.GetArrayElementAtIndex(j);
                    SerializedProperty targetProp = toggleProp.FindPropertyRelative("target");
                    SerializedProperty setEnabledProp = toggleProp.FindPropertyRelative("setEnabled");

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(targetProp, GUIContent.none);
                    EditorGUILayout.PropertyField(setEnabledProp, GUIContent.none, GUILayout.Width(60));
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        componentTogglesProp.DeleteArrayElementAtIndex(j);
                        break;
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (GUILayout.Button("+ Add Component Toggle"))
                {
                    componentTogglesProp.InsertArrayElementAtIndex(componentTogglesProp.arraySize);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(animProp.FindPropertyRelative("onComplete"));
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        if (indexToRemove >= 0)
        {
            animationsProp.DeleteArrayElementAtIndex(indexToRemove);
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Animation"))
        {
            animationsProp.arraySize++;
            var newAnim = animationsProp.GetArrayElementAtIndex(animationsProp.arraySize - 1);
            newAnim.FindPropertyRelative("name").stringValue = "New Animation";
            newAnim.FindPropertyRelative("duration").floatValue = 1f;
            newAnim.FindPropertyRelative("easingCurve").animationCurveValue = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
        if (GUILayout.Button("Remove Last"))
        {
            if (animationsProp.arraySize > 0)
                animationsProp.arraySize--;
        }
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    void DrawToggleAndValue(SerializedProperty animProp, string toggleName, string valueName)
    {
        SerializedProperty toggleProp = animProp.FindPropertyRelative(toggleName);
        SerializedProperty valueProp = animProp.FindPropertyRelative(valueName);

        EditorGUILayout.BeginHorizontal();
        toggleProp.boolValue = EditorGUILayout.ToggleLeft(toggleName.Replace("animate", ""), toggleProp.boolValue, GUILayout.Width(90));
        using (new EditorGUI.DisabledScope(!toggleProp.boolValue))
        {
            EditorGUILayout.PropertyField(valueProp, GUIContent.none);
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif
