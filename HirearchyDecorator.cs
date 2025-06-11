#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Ultra-minimal config for HierarchyDecorator
/// </summary>
public class HierarchyDecoratorConfig : ScriptableObject
{
    // Row background colors
    public Color rowBg = Color.white;
    public Color rowHover = new Color(0.96f, 0.96f, 0.98f, 1f);
    public Color rowSelected = new Color(0.18f, 0.44f, 0.87f, 1f);

    // Row border
    // public Color leftBorder = new Color(0.18f, 0.44f, 0.87f, 1f);
    // public float leftBorderWidth = 3f;

    // Label customization
    public int labelFontSize = 15;
    public Color labelColor = new Color(0.08f, 0.08f, 0.08f, 1f);
    public Color labelInactiveColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color labelSelectedColor = Color.black;
    public FontStyle labelFontStyle = FontStyle.Bold;
    public FontStyle labelInactiveFontStyle = FontStyle.Normal;
    public TextAnchor labelAlignment = TextAnchor.MiddleCenter;

    // Tag/Layer label customization
    public int tagFontSize = 12;
    public Color tagColor = new Color(0.15f, 0.15f, 0.15f, 1f);
    public Color tagSelectedColor = Color.black;
    public TextAnchor tagAlignment = TextAnchor.MiddleCenter;

    // Child count badge customization
    public int badgeFontSize = 12;
    public Color badgeBg = new Color(0.85f, 0.85f, 0.95f, 1f);
    public Color badgeSelectedBg = new Color(1f, 1f, 1f, 0.32f);
    public Color badgeText = new Color(0.15f, 0.15f, 0.25f, 1f);
    public Color badgeSelectedText = new Color(0.18f, 0.44f, 0.87f, 1f);
    public Color badgeBorder = new Color(0.7f, 0.7f, 0.9f, 1f);
    public Color badgeSelectedBorder = new Color(0.18f, 0.44f, 0.87f, 1f);
    public float badgeCornerRadius = 7f;
    public float badgeBorderWidth = 1.5f;
    public Vector2 badgePadding = new Vector2(7f, 2f);

    // Static badge customization
    public int staticBadgeFontSize = 11;
    public Color staticBadgeBg = new Color(0.7f, 0.8f, 1f, 1f);
    public Color staticBadgeText = new Color(0.1f, 0.2f, 0.6f, 1f);
    public Color staticBadgeBorder = new Color(0.3f, 0.5f, 0.9f, 1f);
    public float staticBadgeCornerRadius = 7f;
    public float staticBadgeBorderWidth = 1.5f;
    public Vector2 staticBadgePadding = new Vector2(6f, 1.5f);

    // Icon and row layout
    public float iconSize = 14f;
    public float iconPad = 2f;
    public float bookmarkIconSize = 13f;
    public float pinIconSize = 13f;
    public float rowMinHeight = 28f;
    public float rowSpacing = 4f;

    // Tooltip customization
    public int tooltipFontSize = 12;
    public Color tooltipBg = new Color(1f, 1f, 1f, 0.98f);

    // Margin options
    public float rowVerticalMargin = 2f;
    public float rowHorizontalMargin = 0f;
    public float badgeRightMargin = 8f;
    public float badgeTopMargin = 0f;

    // Asset path
    public static string assetPath = "Assets/Editor/HierarchyDecoratorConfig.asset";
}

[InitializeOnLoad]
public static class HirearchyDecorator
{
    static HierarchyDecoratorConfig config;

    // --- Organize: Sorting mode ---
    enum SortMode { None, ByName, ByType, ByActive, ByChildCount }
    static SortMode sortMode = SortMode.None;

    // --- Organize: Filtering ---
    enum FilterMode { None, OnlyActive, OnlyInactive, OnlyWithTag, OnlyWithComponent }
    static FilterMode filterMode = FilterMode.None;
    static string filterTag = "";
    static string filterComponent = "";

    // --- Organize: Pinning ---
    static HashSet<int> pinnedIDs = new HashSet<int>();

    // --- Organize: Quick search ---
    static string quickSearch = "";

    // --- Organize: Add menu for sorting ---
    [MenuItem("Tools/Hierarchy Decorator/Sort/None")]
    static void SortNone() { sortMode = SortMode.None; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Sort/By Name")]
    static void SortByName() { sortMode = SortMode.ByName; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Sort/By Type")]
    static void SortByType() { sortMode = SortMode.ByType; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Sort/By Active State")]
    static void SortByActive() { sortMode = SortMode.ByActive; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Sort/By Child Count")]
    static void SortByChildCount() { sortMode = SortMode.ByChildCount; EditorApplication.RepaintHierarchyWindow(); }

    // --- Organize: Filtering ---
    [MenuItem("Tools/Hierarchy Decorator/Filter/None")]
    static void FilterNone() { filterMode = FilterMode.None; filterTag = ""; filterComponent = ""; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Filter/Only Active")]
    static void FilterOnlyActive() { filterMode = FilterMode.OnlyActive; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Filter/Only Inactive")]
    static void FilterOnlyInactive() { filterMode = FilterMode.OnlyInactive; EditorApplication.RepaintHierarchyWindow(); }
    [MenuItem("Tools/Hierarchy Decorator/Filter/Only With Tag...")]
    static void FilterOnlyWithTag()
    {
        filterTag = EditorUtility.DisplayDialogComplex("Filter by Tag", "Enter tag to filter:", "OK", "Cancel", "") == 0
            ? EditorUtility.DisplayDialog("Tag", "Enter tag to filter:", "OK") ? "" : "" // Placeholder, see below
            : "";
        filterMode = FilterMode.OnlyWithTag;
        EditorApplication.RepaintHierarchyWindow();
    }
    [MenuItem("Tools/Hierarchy Decorator/Filter/Only With Component...")]
    static void FilterOnlyWithComponent()
    {
        filterComponent = EditorUtility.DisplayDialogComplex("Filter by Component", "Enter component name to filter:", "OK", "Cancel", "") == 0
            ? EditorUtility.DisplayDialog("Component", "Enter component name to filter:", "OK") ? "" : "" // Placeholder, see below
            : "";
        filterMode = FilterMode.OnlyWithComponent;
        EditorApplication.RepaintHierarchyWindow();
    }

    // --- Organize: Pinning ---
    [MenuItem("Tools/Hierarchy Decorator/Pin Selected %#p")]
    static void PinSelected()
    {
        foreach (var id in Selection.instanceIDs)
            pinnedIDs.Add(id);
        EditorApplication.RepaintHierarchyWindow();
    }
    [MenuItem("Tools/Hierarchy Decorator/Unpin Selected %#u")]
    static void UnpinSelected()
    {
        foreach (var id in Selection.instanceIDs)
            pinnedIDs.Remove(id);
        EditorApplication.RepaintHierarchyWindow();
    }
    [MenuItem("Tools/Hierarchy Decorator/Clear All Pins")]
    static void ClearAllPins()
    {
        pinnedIDs.Clear();
        EditorApplication.RepaintHierarchyWindow();
    }

    // --- Organize: Quick search ---
    [MenuItem("Tools/Hierarchy Decorator/Quick Search %#f")]
    static void QuickSearch()
    {
        quickSearch = EditorUtility.DisplayDialog("Quick Search", "Enter name to search:", "OK") ? "" : ""; // Placeholder, see below
        EditorApplication.RepaintHierarchyWindow();
    }

    // --- Organize: Bookmarking ---
    static HashSet<int> bookmarkedIDs = new HashSet<int>();
    [MenuItem("Tools/Hierarchy Decorator/Bookmark Selected %#b")]
    static void BookmarkSelected()
    {
        foreach (var id in Selection.instanceIDs)
            bookmarkedIDs.Add(id);
        EditorApplication.RepaintHierarchyWindow();
    }
    [MenuItem("Tools/Hierarchy Decorator/Unbookmark Selected %#n")]
    static void UnbookmarkSelected()
    {
        foreach (var id in Selection.instanceIDs)
            bookmarkedIDs.Remove(id);
        EditorApplication.RepaintHierarchyWindow();
    }
    [MenuItem("Tools/Hierarchy Decorator/Clear All Bookmarks")]
    static void ClearAllBookmarks()
    {
        bookmarkedIDs.Clear();
        EditorApplication.RepaintHierarchyWindow();
    }

    // --- Organize: Tag color mapping ---
    static Dictionary<string, Color> tagColors = new Dictionary<string, Color>
    {
        { "Player", new Color(0.2f, 0.7f, 1f, 0.25f) },
        { "Enemy", new Color(1f, 0.4f, 0.3f, 0.25f) },
        { "Untagged", new Color(0.8f, 0.8f, 0.8f, 0.12f) }
    };

    // --- Organize: Sorting logic ---
    static void SortHierarchyIfNeeded()
    {
        if (sortMode == SortMode.None) return;
        foreach (var root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            SortChildrenRecursive(root.transform);
        }
    }

    static void SortChildrenRecursive(Transform parent)
    {
        // Get children
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < parent.childCount; i++)
            children.Add(parent.GetChild(i));

        // Sort children
        switch (sortMode)
        {
            case SortMode.ByName:
                children.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.OrdinalIgnoreCase));
                break;
            case SortMode.ByType:
                children.Sort((a, b) =>
                {
                    string at = a.GetComponent<Component>()?.GetType().Name ?? "";
                    string bt = b.GetComponent<Component>()?.GetType().Name ?? "";
                    return string.Compare(at, bt, System.StringComparison.OrdinalIgnoreCase);
                });
                break;
            case SortMode.ByActive:
                children.Sort((a, b) => b.gameObject.activeSelf.CompareTo(a.gameObject.activeSelf));
                break;
            case SortMode.ByChildCount:
                children.Sort((a, b) => b.childCount.CompareTo(a.childCount));
                break;
        }

        // Apply new sibling order
        for (int i = 0; i < children.Count; i++)
        {
            children[i].SetSiblingIndex(i);
            SortChildrenRecursive(children[i]);
        }
    }

    // --- Create New Config ---
    [MenuItem("Tools/Hierarchy Decorator/Create New Config")]
    static void CreateNewConfig()
    {
        string path = EditorUtility.SaveFilePanelInProject(
            "Create Hierarchy Decorator Config",
            "HierarchyDecoratorConfig.asset",
            "asset",
            "Choose location for new HierarchyDecoratorConfig asset"
        );
        if (!string.IsNullOrEmpty(path))
        {
            var newConfig = ScriptableObject.CreateInstance<HierarchyDecoratorConfig>();
            AssetDatabase.CreateAsset(newConfig, path);
            AssetDatabase.SaveAssets();
            // Set the config reference and assetPath for future use
            config = newConfig;
            HierarchyDecoratorConfig.assetPath = path;
            // Optionally select the new config in the Project window
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newConfig;
            // Force reload of config in case the window is open
            EditorApplication.RepaintHierarchyWindow();
        }
    }

    // Ensure config is always loaded from the correct assetPath
    static void EnsureConfig()
    {
        if (config == null)
        {
            config = AssetDatabase.LoadAssetAtPath<HierarchyDecoratorConfig>(HierarchyDecoratorConfig.assetPath);
            if (config == null)
            {
                // fallback: try to find any config in the project
                string[] guids = AssetDatabase.FindAssets("t:HierarchyDecoratorConfig");
                if (guids.Length > 0)
                {
                    string foundPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                    config = AssetDatabase.LoadAssetAtPath<HierarchyDecoratorConfig>(foundPath);
                    HierarchyDecoratorConfig.assetPath = foundPath;
                }
            }
        }
    }

    static void OnHierarchyGUI(int instanceID, Rect selectionRect)
    {
        EnsureConfig();
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null) return;

        // Determine if this object is selected in the hierarchy
        bool isSelected = Selection.instanceIDs != null && System.Array.IndexOf(Selection.instanceIDs, instanceID) >= 0;

        // --- Filtering logic ---
        if (filterMode == FilterMode.OnlyActive && !obj.activeSelf) return;
        if (filterMode == FilterMode.OnlyInactive && obj.activeSelf) return;
        if (filterMode == FilterMode.OnlyWithTag && !string.IsNullOrEmpty(filterTag) && obj.tag != filterTag) return;
        if (filterMode == FilterMode.OnlyWithComponent && !string.IsNullOrEmpty(filterComponent))
        {
            bool found = false;
            foreach (var comp in obj.GetComponents<Component>())
            {
                if (comp != null && comp.GetType().Name.ToLower().Contains(filterComponent.ToLower()))
                {
                    found = true;
                    break;
                }
            }
            if (!found) return;
        }
        if (!string.IsNullOrEmpty(quickSearch) && !obj.name.ToLower().Contains(quickSearch.ToLower())) return;

        // --- Pin highlight ---
        bool isPinned = pinnedIDs.Contains(instanceID);
        bool isBookmarked = bookmarkedIDs.Contains(instanceID);

        // --- Row background and highlight (draw first, before overlays) ---
        bool isRowHovered = selectionRect.Contains(Event.current.mousePosition);

        // Use config for background colors, fallback to defaults if config is null
        Color bgColor = isSelected
            ? (config != null ? config.rowSelected : new Color(0.18f, 0.44f, 0.87f, 1f))
            : (isRowHovered
                ? (config != null ? config.rowHover : new Color(0.96f, 0.96f, 0.98f, 1f))
                : (config != null ? config.rowBg : Color.white));

        EditorGUI.DrawRect(selectionRect, bgColor);

        // Pin highlight overlay
        if (isPinned)
        {
            Color pinColor = new Color(1f, 0.95f, 0.6f, 0.25f);
            EditorGUI.DrawRect(selectionRect, pinColor);
        }

        // Bookmark highlight overlay
        if (isBookmarked)
        {
            Color bookmarkColor = new Color(0.6f, 0.95f, 1f, 0.18f);
            EditorGUI.DrawRect(selectionRect, bookmarkColor);
        }

        // Tag color highlight overlay
        if (tagColors.TryGetValue(obj.tag, out var tagColor))
        {
            EditorGUI.DrawRect(selectionRect, tagColor);
        }

        // Subtle shadow on hover (not selected)
        if (isRowHovered && !isSelected)
        {
            Color shadow = new Color(0, 0, 0, 0.07f);
            EditorGUI.DrawRect(selectionRect, shadow);
        }

        // --- Draw separator line below each object for spacing ---
        // Draw after background, before content
        float separatorY = selectionRect.yMax - 1f;
        Color separatorColor = new Color(0f, 0f, 0f, 0.10f); // slightly more visible
        EditorGUI.DrawRect(new Rect(selectionRect.x, separatorY, selectionRect.width, 1f), separatorColor);

        // --- Bookmark icon (left of pin) ---
        float bookmarkIconSize = config != null ? config.bookmarkIconSize : 13f;
        float bookmarkIconPad = config != null ? config.iconPad : 2f;
        float leftStart = selectionRect.x + (config != null ? config.rowHorizontalMargin : 0f);
        float bookmarkIconX = leftStart;
        float bookmarkIconY = selectionRect.y + (selectionRect.height - bookmarkIconSize) * 0.5f;
        Rect bookmarkIconRect = new Rect(bookmarkIconX, bookmarkIconY, bookmarkIconSize, bookmarkIconSize);
        if (isBookmarked)
        {
            Texture bookmarkIcon = EditorGUIUtility.IconContent("d_Favorite").image;
            GUI.DrawTexture(bookmarkIconRect, bookmarkIcon, ScaleMode.ScaleToFit);
        }
        else if (bookmarkIconRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            bookmarkedIDs.Add(instanceID);
            Event.current.Use();
            EditorApplication.RepaintHierarchyWindow();
        }

        // --- Pin icon (right of bookmark) ---
        float pinIconSize = config != null ? config.pinIconSize : 13f;
        float pinIconPad = config != null ? config.iconPad : 2f;
        float pinIconX = bookmarkIconRect.xMax + pinIconPad;
        float pinIconY = bookmarkIconY;
        Rect pinIconRect = new Rect(pinIconX, pinIconY, pinIconSize, pinIconSize);
        if (isPinned)
        {
            Texture pinIcon = EditorGUIUtility.IconContent("d_Pin").image;
            if (pinIcon == null)
                pinIcon = EditorGUIUtility.IconContent("Favorite").image;
            GUI.DrawTexture(pinIconRect, pinIcon, ScaleMode.ScaleToFit);
        }
        else if (pinIconRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            pinnedIDs.Add(instanceID);
            Event.current.Use();
            EditorApplication.RepaintHierarchyWindow();
        }

        // --- Adjust leftPad for toggle ---
        float leftPad = pinIconRect.xMax + (config != null ? config.iconPad : 4f);
        float toggleSize = 16f;
        float toggleX = leftPad;
        float toggleY = selectionRect.y + (selectionRect.height - toggleSize) * 0.5f;
        var toggleRect = new Rect(toggleX, toggleY, toggleSize, toggleSize);

        // Toggle color logic
        bool toggleHovered = toggleRect.Contains(Event.current.mousePosition);
        Color toggleColor = obj.activeSelf
            ? (toggleHovered ? new Color(0.3f, 1f, 0.3f, 1f) : new Color(0.2f, 0.8f, 0.2f, 1f))
            : (toggleHovered ? new Color(1f, 0.5f, 0.3f, 1f) : new Color(0.85f, 0.2f, 0.2f, 1f));

        // Draw toggle (circle)
        Handles.BeginGUI();
        Handles.color = toggleColor;
        Vector2 toggleCenter = new Vector2(toggleRect.x + toggleRect.width / 2f, toggleRect.y + toggleRect.height / 2f);
        Handles.DrawSolidDisc(toggleCenter, Vector3.forward, toggleSize * 0.5f);
        Handles.color = Color.white;
        Handles.DrawWireDisc(toggleCenter, Vector3.forward, toggleSize * 0.5f);
        Handles.EndGUI();

        // Toggle click logic
        if (Event.current.type == EventType.MouseDown &&
            toggleRect.Contains(Event.current.mousePosition) &&
            Event.current.button == 0)
        {
            Undo.RecordObject(obj, "Toggle Active");
            obj.SetActive(!obj.activeSelf);
            EditorUtility.SetDirty(obj);
            if (!Application.isPlaying)
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(obj.scene);
            Event.current.Use();
        }

        // --- Show up to 3 component icons after prefab/lock ---
        float iconPad = config != null ? config.iconPad : 2f;
        float iconSize = config != null ? config.iconSize : 14f;
        float iconX = toggleRect.xMax + iconPad;
        float iconY = selectionRect.y + (selectionRect.height - iconSize) * 0.5f;
        float labelX = iconX;
        int compIconCount = 0;
        if (obj != null)
        {
            foreach (var comp in obj.GetComponents<Component>())
            {
                if (comp == null || comp is Transform) continue;
                Texture compIcon = EditorGUIUtility.ObjectContent(comp, comp.GetType()).image;
                if (compIcon != null)
                {
                    GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), compIcon, ScaleMode.ScaleToFit);
                    iconX += iconSize + iconPad;
                    labelX += iconSize + iconPad;
                    compIconCount++;
                    if (compIconCount >= 3) break;
                }
            }
        }

        // Lock and prefab icons
        bool isLocked =
                        (PrefabUtility.IsPartOfPrefabInstance(obj) && !PrefabUtility.IsAnyPrefabInstanceRoot(obj));
        if (isLocked)
        {
            Texture lockIcon = EditorGUIUtility.IconContent("LockIcon-On").image;
            GUI.color = new Color(1, 1, 1, 0.7f);
            GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), lockIcon, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
            iconX += iconSize + iconPad;
            labelX += iconSize + iconPad;
        }
        else if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
        {
            Texture prefabIcon = EditorGUIUtility.IconContent("Prefab Icon").image;
            GUI.color = new Color(1, 1, 1, 0.7f);
            GUI.DrawTexture(new Rect(iconX, iconY, iconSize, iconSize), prefabIcon, ScaleMode.ScaleToFit);
            GUI.color = Color.white;
            iconX += iconSize + iconPad;
            labelX += iconSize + iconPad;
        }

        // --- Show missing script warning icon if any ---
        bool hasMissingScript = false;
        foreach (var comp in obj.GetComponents<Component>())
        {
            if (comp == null)
            {
                hasMissingScript = true;
                break;
            }
        }
        if (hasMissingScript)
        {
            Texture warnIcon = EditorGUIUtility.IconContent("console.warnicon").image;
            float warnSize = 15f;
            float warnPad = 2f;
            GUI.DrawTexture(new Rect(iconX, iconY, warnSize, warnSize), warnIcon, ScaleMode.ScaleToFit);
            iconX += warnSize + warnPad;
            labelX += warnSize + warnPad;
        }

        // --- Show layer color swatch (if custom layer) ---
        int layer = obj.layer;
        if (layer > 0 && layer < 32)
        {
            Color layerColor = Color.HSVToRGB((layer * 0.07f) % 1f, 0.4f, 0.95f);
            float swatchSize = 10f;
            float swatchPad = 4f;
            Rect swatchRect = new Rect(labelX, iconY + 2f, swatchSize, swatchSize);
            EditorGUI.DrawRect(swatchRect, layerColor);
            labelX += swatchSize + swatchPad;
        }

        // --- Name label and static badge ---
        float minRowHeight = config != null ? config.rowMinHeight : 28f;
        float rowSpacing = config != null ? config.rowSpacing : 4f;
        float rowHeight = Mathf.Max(selectionRect.height, minRowHeight);

        // Add vertical margin
        float rowVerticalMargin = config != null ? config.rowVerticalMargin : 0f;
        float rowHorizontalMargin = config != null ? config.rowHorizontalMargin : 0f;
        Rect spacedRect = new Rect(
            selectionRect.x + rowHorizontalMargin,
            selectionRect.y + rowVerticalMargin,
            selectionRect.width - 2 * rowHorizontalMargin,
            rowHeight - 2 * rowVerticalMargin
        );

        Rect labelRect = new Rect(
            labelX,
            spacedRect.y,
            spacedRect.width - labelX,
            spacedRect.height
        );

        // Center the main label (now customizable)
        var labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize = config.labelFontSize + 2,
            fontStyle = obj.activeInHierarchy ? config.labelFontStyle : config.labelInactiveFontStyle,
            alignment = config.labelAlignment,
            clipping = TextClipping.Ellipsis,
            normal = { textColor = isSelected
                ? config.labelSelectedColor
                : (obj.activeInHierarchy
                    ? config.labelColor
                    : config.labelInactiveColor)
            }
        };

        // Static badge (keep centered, customizable)
        if (GameObjectUtility.GetStaticEditorFlags(obj) != 0)
        {
            string staticText = "S";
            var staticStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = config.staticBadgeFontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = config.staticBadgeText }
            };
            Vector2 staticSize = staticStyle.CalcSize(new GUIContent(staticText));
            staticSize += config.staticBadgePadding * 2f;
            float staticBadgeWidth = staticSize.x;
            float staticBadgeHeight = staticSize.y;
            var staticBadgeRect = new Rect(
                labelRect.x,
                labelRect.y + (labelRect.height - staticBadgeHeight) * 0.5f,
                staticBadgeWidth,
                staticBadgeHeight
            );
            DrawRoundedRect(
                staticBadgeRect,
                config.staticBadgeCornerRadius,
                config.staticBadgeBg,
                config.staticBadgeBorder,
                config.staticBadgeBorderWidth
            );
            GUI.Label(staticBadgeRect, staticText, staticStyle);
            labelRect.x += staticBadgeWidth + 4f;
            labelRect.width -= staticBadgeWidth + 4f;
        }

        // --- Responsive right-side layout ---
        float minRightSpace = 70f;
        float maxRightSpace = 140f;
        float rightSpace = Mathf.Clamp(spacedRect.width * 0.28f, minRightSpace, maxRightSpace);

        labelRect.width = Mathf.Max(40f, labelRect.width - rightSpace);

        GUI.Label(labelRect, obj.name, labelStyle);

        // --- Tag and Layer (right side, customizable alignment) ---
        var tagStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            fontSize = 12,
            alignment = config.labelAlignment,
            clipping = TextClipping.Ellipsis,
            normal = { textColor = isSelected ? config.labelSelectedColor : new Color(0.15f, 0.15f, 0.15f, 1f) }
        };

        float tagWidth = Mathf.Clamp(spacedRect.width * 0.18f, 50f, 100f);
        float tagPad = 38f;
        Rect tagRect = new Rect(
            spacedRect.xMax - tagWidth - tagPad,
            spacedRect.y,
            tagWidth,
            spacedRect.height
        );
        string tagLayer = $"Tag: {obj.tag} | Layer: {LayerMask.LayerToName(obj.layer)}";
        GUI.Label(tagRect, tagLayer, tagStyle);

        // --- Child count badge (centered, customizable) ---
        if (obj.transform.childCount > 0 && spacedRect.width > 80f)
        {
            string badge = obj.transform.childCount.ToString();
            var badgeStyle = new GUIStyle(EditorStyles.miniLabel)
            {
                fontSize = config.badgeFontSize,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold,
                normal = { textColor = isSelected ? config.badgeSelectedText : config.badgeText }
            };
            Vector2 badgeSize = badgeStyle.CalcSize(new GUIContent(badge));
            badgeSize += config.badgePadding * 2f;
            float badgeWidth = badgeSize.x;
            float badgeHeight = badgeSize.y;
            float badgeY = tagRect.y + (tagRect.height - badgeHeight) * 0.5f + (config != null ? config.badgeTopMargin : 0f);
            float badgeX = tagRect.xMax + (config != null ? config.badgeRightMargin : 8f);
            // Place badge immediately after tag label, not at far right
            Rect badgeRect = new Rect(
                badgeX,
                badgeY,
                badgeWidth,
                badgeHeight
            );

            DrawRoundedRect(
                badgeRect,
                config.badgeCornerRadius,
                isSelected ? config.badgeSelectedBg : config.badgeBg,
                isSelected ? config.badgeSelectedBorder : config.badgeBorder,
                config.badgeBorderWidth
            );

            GUI.Label(badgeRect, badge, badgeStyle);
        }

        // --- Show icon if GameObject is marked DontDestroyOnLoad ---
        if ((obj.scene.name == null || obj.scene.name == "") && selectionRect.width > 100f)
        {
            Texture ddolIcon = EditorGUIUtility.IconContent("d_UnityEditor.SceneView").image;
            float ddolSize = 13f;
            float ddolPad = 4f;
            Rect ddolRect = new Rect(selectionRect.xMax - 28f, selectionRect.y + (selectionRect.height - ddolSize) * 0.5f, ddolSize, ddolSize);
            GUI.DrawTexture(ddolRect, ddolIcon, ScaleMode.ScaleToFit);
        }
    }

    // Utility: Draw a rounded rectangle with background and border
    static void DrawRoundedRect(Rect rect, float radius, Color bg, Color border, float borderWidth)
    {
        // Draw background
        Handles.BeginGUI();
        Handles.DrawSolidRectangleWithOutline(
            new Vector3[]
            {
                new Vector3(rect.x + radius, rect.y),
                new Vector3(rect.xMax - radius, rect.y),
                new Vector3(rect.xMax, rect.y + radius),
                new Vector3(rect.xMax, rect.yMax - radius),
                new Vector3(rect.xMax - radius, rect.yMax),
                new Vector3(rect.x + radius, rect.yMax),
                new Vector3(rect.x, rect.yMax - radius),
                new Vector3(rect.x, rect.y + radius)
            },
            bg,
            border
        );
        // Draw border (approximate with rectangle)
        if (borderWidth > 0f)
        {
            Color prevColor = Handles.color;
            Handles.color = border;
            Handles.DrawAAPolyLine(borderWidth, new Vector3[]
            {
                new Vector3(rect.x + radius, rect.y),
                new Vector3(rect.xMax - radius, rect.y),
                new Vector3(rect.xMax, rect.y + radius),
                new Vector3(rect.xMax, rect.yMax - radius),
                new Vector3(rect.xMax - radius, rect.yMax),
                new Vector3(rect.x + radius, rect.yMax),
                new Vector3(rect.x, rect.yMax - radius),
                new Vector3(rect.x, rect.y + radius),
                new Vector3(rect.x + radius, rect.y)
            });
            Handles.color = prevColor;
        }
        Handles.EndGUI();
    }

    static HirearchyDecorator()
    {
        EnsureConfig();
        if (config == null)
        {
            // Ensure the Editor folder exists before creating the asset
            string editorFolder = System.IO.Path.GetDirectoryName(HierarchyDecoratorConfig.assetPath);
            if (!System.IO.Directory.Exists(editorFolder))
            {
                System.IO.Directory.CreateDirectory(editorFolder);
                AssetDatabase.Refresh();
            }
            config = ScriptableObject.CreateInstance<HierarchyDecoratorConfig>();
            AssetDatabase.CreateAsset(config, HierarchyDecoratorConfig.assetPath);
            AssetDatabase.SaveAssets();
        }
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        EditorApplication.hierarchyWindowChanged += SortHierarchyIfNeeded;
    }
}

[CustomEditor(typeof(HierarchyDecoratorConfig))]
public class HierarchyDecoratorConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var config = (HierarchyDecoratorConfig)target;

        EditorGUILayout.LabelField("Row Background Colors", EditorStyles.boldLabel);
        config.rowBg = EditorGUILayout.ColorField("Row BG", config.rowBg);
        config.rowHover = EditorGUILayout.ColorField("Row Hover", config.rowHover);
        config.rowSelected = EditorGUILayout.ColorField("Row Selected", config.rowSelected);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Label Customization", EditorStyles.boldLabel);
        config.labelFontSize = EditorGUILayout.IntField("Font Size", config.labelFontSize);
        config.labelColor = EditorGUILayout.ColorField("Label Color", config.labelColor);
        config.labelInactiveColor = EditorGUILayout.ColorField("Inactive Color", config.labelInactiveColor);
        config.labelSelectedColor = EditorGUILayout.ColorField("Selected Color", config.labelSelectedColor);
        config.labelFontStyle = (FontStyle)EditorGUILayout.EnumPopup("Font Style", config.labelFontStyle);
        config.labelInactiveFontStyle = (FontStyle)EditorGUILayout.EnumPopup("Inactive Font Style", config.labelInactiveFontStyle);
        config.labelAlignment = (TextAnchor)EditorGUILayout.EnumPopup("Alignment", config.labelAlignment);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tag/Layer Label Customization", EditorStyles.boldLabel);
        config.tagFontSize = EditorGUILayout.IntField("Tag Font Size", config.tagFontSize);
        config.tagColor = EditorGUILayout.ColorField("Tag Color", config.tagColor);
        config.tagSelectedColor = EditorGUILayout.ColorField("Tag Selected Color", config.tagSelectedColor);
        config.tagAlignment = (TextAnchor)EditorGUILayout.EnumPopup("Tag Alignment", config.tagAlignment);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Child Count Badge", EditorStyles.boldLabel);
        config.badgeFontSize = EditorGUILayout.IntField("Badge Font Size", config.badgeFontSize);
        config.badgeBg = EditorGUILayout.ColorField("Badge BG", config.badgeBg);
        config.badgeSelectedBg = EditorGUILayout.ColorField("Badge Selected BG", config.badgeSelectedBg);
        config.badgeText = EditorGUILayout.ColorField("Badge Text", config.badgeText);
        config.badgeSelectedText = EditorGUILayout.ColorField("Badge Selected Text", config.badgeSelectedText);
        config.badgeBorder = EditorGUILayout.ColorField("Badge Border", config.badgeBorder);
        config.badgeSelectedBorder = EditorGUILayout.ColorField("Badge Selected Border", config.badgeSelectedBorder);
        config.badgeCornerRadius = EditorGUILayout.FloatField("Badge Corner Radius", config.badgeCornerRadius);
        config.badgeBorderWidth = EditorGUILayout.FloatField("Badge Border Width", config.badgeBorderWidth);
        config.badgePadding = EditorGUILayout.Vector2Field("Badge Padding", config.badgePadding);
        config.badgeRightMargin = EditorGUILayout.FloatField("Badge Right Margin", config.badgeRightMargin);
        config.badgeTopMargin = EditorGUILayout.FloatField("Badge Top Margin", config.badgeTopMargin);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Static Badge", EditorStyles.boldLabel);
        config.staticBadgeFontSize = EditorGUILayout.IntField("Static Badge Font Size", config.staticBadgeFontSize);
        config.staticBadgeBg = EditorGUILayout.ColorField("Static Badge BG", config.staticBadgeBg);
        config.staticBadgeText = EditorGUILayout.ColorField("Static Badge Text", config.staticBadgeText);
        config.staticBadgeBorder = EditorGUILayout.ColorField("Static Badge Border", config.staticBadgeBorder);
        config.staticBadgeCornerRadius = EditorGUILayout.FloatField("Static Badge Corner Radius", config.staticBadgeCornerRadius);
        config.staticBadgeBorderWidth = EditorGUILayout.FloatField("Static Badge Border Width", config.staticBadgeBorderWidth);
        config.staticBadgePadding = EditorGUILayout.Vector2Field("Static Badge Padding", config.staticBadgePadding);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Icon and Row Layout", EditorStyles.boldLabel);
        config.iconSize = EditorGUILayout.FloatField("Icon Size", config.iconSize);
        config.iconPad = EditorGUILayout.FloatField("Icon Pad", config.iconPad);
        config.bookmarkIconSize = EditorGUILayout.FloatField("Bookmark Icon Size", config.bookmarkIconSize);
        config.pinIconSize = EditorGUILayout.FloatField("Pin Icon Size", config.pinIconSize);
        config.rowMinHeight = EditorGUILayout.FloatField("Row Min Height", config.rowMinHeight);
        config.rowSpacing = EditorGUILayout.FloatField("Row Spacing", config.rowSpacing);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Tooltip", EditorStyles.boldLabel);
        config.tooltipFontSize = EditorGUILayout.IntField("Tooltip Font Size", config.tooltipFontSize);
        config.tooltipBg = EditorGUILayout.ColorField("Tooltip BG", config.tooltipBg);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Margins", EditorStyles.boldLabel);
        config.rowVerticalMargin = EditorGUILayout.FloatField("Row Vertical Margin", config.rowVerticalMargin);
        config.rowHorizontalMargin = EditorGUILayout.FloatField("Row Horizontal Margin", config.rowHorizontalMargin);

        if (GUI.changed)
        {
            EditorUtility.SetDirty(config);
        }
    }
}
#endif
