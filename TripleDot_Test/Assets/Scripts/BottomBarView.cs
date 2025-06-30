using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
public class BottomBarView : MonoBehaviour
{
    [System.Serializable]
    public class MenuButtonEntry
    {
        public string name;
        public Button button;
        public MultiUIAnimator animator;

        public List<int> onSelectIndices = new();
        public List<int> onDeselectIndices = new();

        public bool isLocked = false;

        public GameObject lockedVisual;
        public GameObject unlockedVisual;
    }

    [Header("Menu Buttons")]
    public List<MenuButtonEntry> buttons = new();

    private int currentIndex = -1;

    private void Start()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int capturedIndex = i;

            if (Application.isPlaying)
            {
                buttons[i].button.onClick.AddListener(() => OnButtonClicked(capturedIndex));
            }

            UpdateLockVisual(buttons[i]);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying)
        {
            foreach (var button in buttons)
            {
                UpdateLockVisual(button);
            }
        }
    }
#endif

    private void Update()
    {
        if (Application.isPlaying && Input.GetMouseButtonDown(0) && !IsPointerOverAnyMenuButton())
        {
            DeselectCurrent();
            Closed();
        }
    }

    private void OnButtonClicked(int index)
    {
        if (index != currentIndex && index >= 0 && index < buttons.Count)
        {
            if (!buttons[index].isLocked)
            {
                SelectButton(index);
            }
        }
    }

    public void SelectButton(int index)
    {
        if (index < 0 || index >= buttons.Count || buttons[index].isLocked)
            return;

        DeselectCurrent();

        currentIndex = index;
        var current = buttons[currentIndex];

        if (current.animator && current.animator.animations != null)
        {
            foreach (int animIndex in current.onSelectIndices)
            {
                if (animIndex >= 0 && animIndex < current.animator.animations.Count)
                {
                    current.animator.PlayAnimation(current.animator.animations[animIndex]);
                }
            }
        }

        ContentActivated(index);
    }

    public void DeselectCurrent()
    {
        if (currentIndex >= 0 && currentIndex < buttons.Count)
        {
            var previous = buttons[currentIndex];

            if (previous.animator && previous.animator.animations != null)
            {
                foreach (int animIndex in previous.onDeselectIndices)
                {
                    if (animIndex >= 0 && animIndex < previous.animator.animations.Count)
                    {
                        previous.animator.PlayAnimation(previous.animator.animations[animIndex]);
                    }
                }
            }

            currentIndex = -1;
           
        }
    }

    public void SetButtonLocked(int index, bool locked)
    {
        if (index < 0 || index >= buttons.Count)
            return;

        buttons[index].isLocked = locked;
        UpdateLockVisual(buttons[index]);

        if (locked && index == currentIndex)
        {
            DeselectCurrent();
        }
    }

    public void UpdateLockVisual(MenuButtonEntry entry)
    {
        if (entry.lockedVisual != null)
            entry.lockedVisual.SetActive(entry.isLocked);

        if (entry.unlockedVisual != null)
            entry.unlockedVisual.SetActive(!entry.isLocked);

        if (entry.button != null)
            entry.button.interactable = !entry.isLocked;
    }

    private bool IsPointerOverAnyMenuButton()
    {
        if (EventSystem.current == null)
            return false;

        var pointerData = new PointerEventData(EventSystem.current) { position = Input.mousePosition };
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            foreach (var entry in buttons)
            {
                if (entry.button != null &&
                    (result.gameObject == entry.button.gameObject ||
                     result.gameObject.transform.IsChildOf(entry.button.transform)))
                    return true;
            }
        }

        return false;
    }
    protected virtual void ContentActivated(int index)
    {
        Debug.Log("Button Activated" + index);
    }
    protected virtual void Closed()
    {
        Debug.Log("Buttons Closed");
    }
}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(BottomBarView.MenuButtonEntry))]
public class MenuButtonEntryDrawer : PropertyDrawer
{
    private static readonly float LineHeight = EditorGUIUtility.singleLineHeight + 2f;
    private static readonly float GroupSpacing = 8f;

    private static readonly Dictionary<int, bool> FoldoutStates = new();

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        int id = property.propertyPath.GetHashCode();
        if (!FoldoutStates.ContainsKey(id))
            FoldoutStates[id] = false;

        FoldoutStates[id] = EditorGUI.BeginFoldoutHeaderGroup(
            new Rect(position.x, position.y, position.width, LineHeight),
            FoldoutStates[id],
            GetButtonName(property)
        );

        if (FoldoutStates[id])
        {
            EditorGUI.indentLevel++;

            var buttonProp = property.FindPropertyRelative("button");
            var animatorProp = property.FindPropertyRelative("animator");
            var isLockedProp = property.FindPropertyRelative("isLocked");
            var lockedVisualProp = property.FindPropertyRelative("lockedVisual");
            var unlockedVisualProp = property.FindPropertyRelative("unlockedVisual");
            var onSelectIndicesProp = property.FindPropertyRelative("onSelectIndices");
            var onDeselectIndicesProp = property.FindPropertyRelative("onDeselectIndices");

            Rect rect = new(position.x, position.y + LineHeight, position.width, LineHeight);

            EditorGUI.PropertyField(rect, buttonProp);
            rect.y += LineHeight;

            EditorGUI.PropertyField(rect, animatorProp);
            rect.y += LineHeight + GroupSpacing;

            EditorGUI.PropertyField(rect, isLockedProp);
            rect.y += LineHeight;

            EditorGUI.PropertyField(rect, lockedVisualProp);
            rect.y += LineHeight;

            EditorGUI.PropertyField(rect, unlockedVisualProp);
            rect.y += LineHeight + GroupSpacing;

            MultiUIAnimator animator = animatorProp.objectReferenceValue as MultiUIAnimator;
            if (animator != null && animator.animations != null)
            {
                string[] animationNames = new string[animator.animations.Count];
                for (int i = 0; i < animator.animations.Count; i++)
                {
                    animationNames[i] = animator.animations[i] != null
                        ? animator.animations[i].name
                        : $"Animation {i}";
                }

                DrawPopupList(ref rect, onSelectIndicesProp, animationNames, "On Select Animations");
                rect.y += GroupSpacing;
                DrawPopupList(ref rect, onDeselectIndicesProp, animationNames, "On Deselect Animations");
            }
            else
            {
                EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, LineHeight * 2),
                    "Assign a MultiUIAnimator with animations.",
                    MessageType.Warning);
                rect.y += LineHeight * 2;
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndFoldoutHeaderGroup();
    }

    private string GetButtonName(SerializedProperty property)
    {
        string name = property.FindPropertyRelative("name").stringValue;
        return string.IsNullOrEmpty(name) ? "Button" : name;
    }

    private void DrawPopupList(ref Rect rect, SerializedProperty listProp, string[] options, string label)
    {
        EditorGUI.LabelField(rect, label, EditorStyles.boldLabel);
        rect.y += LineHeight;

        for (int i = 0; i < listProp.arraySize; i++)
        {
            SerializedProperty indexProp = listProp.GetArrayElementAtIndex(i);
            indexProp.intValue = EditorGUI.Popup(rect, $"Animation {i + 1}", indexProp.intValue, options);
            rect.y += LineHeight;
        }

        Rect buttonRect = new(rect.x, rect.y, rect.width / 2f, LineHeight);

        if (GUI.Button(buttonRect, "+ Add"))
        {
            listProp.InsertArrayElementAtIndex(listProp.arraySize);
        }

        buttonRect.x += buttonRect.width;

        if (GUI.Button(buttonRect, "- Remove") && listProp.arraySize > 0)
        {
            listProp.DeleteArrayElementAtIndex(listProp.arraySize - 1);
        }

        rect.y += LineHeight;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        int id = property.propertyPath.GetHashCode();
        bool expanded = FoldoutStates.TryGetValue(id, out bool isOpen) && isOpen;

        if (!expanded)
            return LineHeight + 4;

        var animatorProp = property.FindPropertyRelative("animator");
        var onSelect = property.FindPropertyRelative("onSelectIndices");
        var onDeselect = property.FindPropertyRelative("onDeselectIndices");

        int baseLines = 6;
        float height = baseLines * LineHeight + GroupSpacing * 2;

        if (animatorProp.objectReferenceValue != null)
        {
            height += (onSelect.arraySize + onDeselect.arraySize + 6) * LineHeight;
            height += GroupSpacing;
        }
        else
        {
            height += LineHeight * 2;
        }

        return height;
    }
}
#endif
