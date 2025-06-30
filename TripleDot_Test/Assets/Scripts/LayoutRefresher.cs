using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LayoutRefresher : MonoBehaviour
{
    public RectTransform layoutGroup;

    private Dictionary<RectTransform, Vector3> lastScales = new();

    void Update()
    {
        bool needsUpdate = false;

        List<RectTransform> currentChildren = new();
        foreach (Transform child in layoutGroup)
        {
            if (child is RectTransform rectChild)
            {
                currentChildren.Add(rectChild);
            }
        }

        // Detect new childs
        foreach (RectTransform child in currentChildren)
        {
            if (!lastScales.ContainsKey(child))
            {
                lastScales[child] = child.localScale;
                needsUpdate = true;
            }
        }

        // Detect eleiminated childs
        List<RectTransform> removedChildren = new();
        foreach (var kvp in lastScales)
        {
            if (!currentChildren.Contains(kvp.Key))
            {
                removedChildren.Add(kvp.Key);
                needsUpdate = true;
            }
        }
        foreach (var removed in removedChildren)
        {
            lastScales.Remove(removed);
        }

        // Detect scale changes
        foreach (RectTransform child in currentChildren)
        {
            if (lastScales[child] != child.localScale)
            {
                lastScales[child] = child.localScale;
                needsUpdate = true;
            }
        }

      
        if (needsUpdate)
        {
            LayoutRebuilder.MarkLayoutForRebuild(layoutGroup);
        }
    }
}

