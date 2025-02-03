using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class TimelineEditorWindow : EditorWindow
{
    private const float TIME_RULER_HEIGHT = 24f;
    private const float TRACK_HEIGHT = 25f;
    private const float AGENTS_LIST_WIDTH = 300f;
    private const float SEPARATOR_WIDTH = 1f;
    private const float HEADER_OFFSET = 45f;
    private const float MIN_TIME = 0f;
    private const float MAX_TIME = 10f;
    private const float EDITOR_UPDATE_STEP = 1f / 60f;
    private const float COMMAND_SPACING = 5f;
    private const float EDGE_DRAG_MARGIN = 5f; // Width of the drag zone on edges
    private const float MIN_PIXELS_PER_SECOND = 50f;
    private const float MAX_PIXELS_PER_SECOND = 200f;
    
    private float currentTime;
    private Vector2 scrollPosition;
    private float zoom = 1f;
    private bool isPlaying;
    private double lastUpdateTime;
    private enum DragType { None, LeftEdge, RightEdge, Whole }
    private DragType currentDragType = DragType.None;
    private bool isDragging = false;
    private BCommandData currentDragCommand;
    private SerializedProperty currentDragProperty;
    private float dragStartMouseX;
    private float dragStartTime;
    private float dragEndTime;
    private float currentDragStartTime; // New: for visual representation
    private float currentDragEndTime;   // New: for visual representation
    private bool isDraggingMarker = false;
    private float markerDragStartX;
    private float markerDragStartTime;
    private Vector2 timelineScrollPosition; // Add new field for timeline scroll
    private float pixelsPerSecond = 100f;

    [MenuItem("Window/Timeline Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<TimelineEditorWindow>("Timeline Editor");
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += OnEditorUpdate;
        Undo.undoRedoPerformed += OnUndoRedo;
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.newSceneCreated += OnNewSceneCreated;
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnEditorUpdate;
        Undo.undoRedoPerformed -= OnUndoRedo;
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorSceneManager.newSceneCreated -= OnNewSceneCreated;
        isPlaying = false;
        currentTime = 0f;
        TimelineManager.SetTime(0f);
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        ResetTimeline();
    }

    private void OnNewSceneCreated(Scene scene, NewSceneSetup setup, NewSceneMode mode)
    {
        ResetTimeline();
    }

    private void ResetTimeline()
    {
        isPlaying = false;
        currentTime = 0f;
        TimelineManager.SetTime(0f);
        Repaint();
    }

    private void OnEditorUpdate()
    {
        if (isPlaying)
        {
            double currentUpdateTime = EditorApplication.timeSinceStartup;
            double deltaTime = currentUpdateTime - lastUpdateTime;
            lastUpdateTime = currentUpdateTime;

            currentTime += (float)deltaTime;
            if (currentTime > MAX_TIME)
            {
                currentTime = 0f;
                isPlaying = false;
            }
            
            TimelineManager.SetTime(currentTime);
            Repaint();
        }
    }

    private void OnUndoRedo()
    {
        TimelineManager.RecalculateAll();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();

        // Left panel - Agents names
        EditorGUILayout.BeginVertical(GUILayout.Width(AGENTS_LIST_WIDTH));
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        
        GUILayout.Space(4); // Add top padding
        
        // Controls in the header space
        EditorGUILayout.BeginHorizontal();

        // Create a style for icon buttons
        var iconButtonStyle = new GUIStyle(GUI.skin.button);
        iconButtonStyle.padding = new RectOffset(1, 1, 1, 1);

        GUIContent zoomIcon = EditorGUIUtility.IconContent("d_ViewToolZoom");
        EditorGUILayout.LabelField(zoomIcon, GUILayout.Width(20), GUILayout.Height(20));
        pixelsPerSecond = GUILayout.HorizontalSlider(pixelsPerSecond, MIN_PIXELS_PER_SECOND, MAX_PIXELS_PER_SECOND, GUILayout.Width(100));

        GUILayout.FlexibleSpace(); // Add flexible space to push buttons to the right

        // Rewind to start button
        GUIContent rewindIcon = EditorGUIUtility.IconContent("Animation.FirstKey");
        rewindIcon.image = EditorGUIUtility.IconContent("Animation.FirstKey").image;
        if (GUILayout.Button(rewindIcon, iconButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        {
            currentTime = 0f;
            TimelineManager.SetTime(0f);
        }

        GUIContent playIcon = EditorGUIUtility.IconContent(isPlaying ? "d_PauseButton" : "d_PlayButton");
        if (GUILayout.Button(playIcon, iconButtonStyle, GUILayout.Width(20), GUILayout.Height(20)))
        {
            isPlaying = !isPlaying;
            if (isPlaying)
            {
                lastUpdateTime = EditorApplication.timeSinceStartup;
            }
        }
        
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(TIME_RULER_HEIGHT - EditorGUIUtility.singleLineHeight - 4); // Subtract the top padding
        
        Vector2 newLeftScroll = EditorGUILayout.BeginScrollView(
            scrollPosition, 
            GUIStyle.none, 
            GUIStyle.none, 
            GUILayout.ExpandHeight(true), 
            GUILayout.Width(AGENTS_LIST_WIDTH)
        );

        if (newLeftScroll.y != scrollPosition.y)
        {
            scrollPosition.y = newLeftScroll.y;
            Repaint();
        }
        
        for (int i = 0; i < agents.Length; i++)
        {
            SerializedObject serializedAgent = new SerializedObject(agents[i]);
            SerializedProperty commandsProperty = serializedAgent.FindProperty("commandsData");
            
            // Calculate total height for all commands
            float totalCommandsHeight = 0f;
            for (int j = 0; j < agents[i].commandsData.Count; j++)
            {
                var commandProperty = commandsProperty.GetArrayElementAtIndex(j);
                totalCommandsHeight += GetCommandHeight(commandProperty);
            }
            
            float trackHeight = TRACK_HEIGHT + totalCommandsHeight; // Agent name height + commands height
            
            var rect = EditorGUILayout.GetControlRect(false, trackHeight);
            EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f, 1));
            
            // Agent name
            var labelStyle = new GUIStyle(EditorStyles.label);
            labelStyle.alignment = TextAnchor.UpperLeft;
            labelStyle.normal.textColor = Color.white;
            labelStyle.padding.top = 7;

            // Create horizontal group for agent name and add button
            Rect agentHeaderRect = new Rect(rect.x, rect.y, rect.width, TRACK_HEIGHT);
            EditorGUI.LabelField(
                new Rect(agentHeaderRect.x, agentHeaderRect.y, agentHeaderRect.width - 25, TRACK_HEIGHT),
                agents[i].name,
                labelStyle
            );

            // Add button
            var addButtonRect = new Rect(agentHeaderRect.xMax - 20, agentHeaderRect.y + 4, 20, 20);
            if (GUI.Button(addButtonRect, "+", EditorStyles.miniButton))
            {
                var menu = new GenericMenu();
                var currentAgent = agents[i]; // Capture the current agent
                menu.AddItem(new GUIContent("Move/Move To"), false, () => AddCommand<MoveToData>(currentAgent));
                menu.AddItem(new GUIContent("Move/Move Add"), false, () => AddCommand<MoveAddData>(currentAgent));
                menu.AddItem(new GUIContent("Rotate/Rotate To"), false, () => AddCommand<RotateToData>(currentAgent));
                menu.AddItem(new GUIContent("Rotate/Rotate Add"), false, () => AddCommand<RotateAddData>(currentAgent));
                menu.AddItem(new GUIContent("Color/Set Color"), false, () => AddCommand<SetColorData>(currentAgent));
                menu.AddItem(new GUIContent("Other/Send Message"), false, () => AddCommand<SendMessageData>(currentAgent));
                menu.ShowAsContext();
            }

            // Command properties
            float currentY = rect.y + TRACK_HEIGHT;
            for (int j = 0; j < agents[i].commandsData.Count; j++)
            {
                SerializedProperty commandProperty = commandsProperty.GetArrayElementAtIndex(j);
                float commandHeight = GetCommandHeight(commandProperty);
                
                var commandRect = new Rect(
                    rect.x + 10,
                    currentY,
                    rect.width - 10,
                    commandHeight - COMMAND_SPACING
                );
                
                EditorGUI.PropertyField(commandRect, commandProperty, GUIContent.none);
                currentY += commandHeight;
                
                if (serializedAgent.hasModifiedProperties)
                {
                    serializedAgent.ApplyModifiedProperties();
                    EditorUtility.SetDirty(agents[i]);
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        // Vertical separator
        EditorGUILayout.BeginVertical(GUILayout.Width(SEPARATOR_WIDTH));
        var separatorRect = GUILayoutUtility.GetRect(SEPARATOR_WIDTH, position.height);
        EditorGUI.DrawRect(separatorRect, new Color(0.1f, 0.1f, 0.1f, 1));
        EditorGUILayout.EndVertical();
        
        // Right panel - Timeline
        EditorGUILayout.BeginVertical();
        DrawTimeRuler();
        
        // Handle dragging once per frame
        if (isDragging)
        {
            float timeScale = (position.width - AGENTS_LIST_WIDTH - SEPARATOR_WIDTH) / (MAX_TIME - MIN_TIME) * zoom;
            HandleDragging(timeScale);
        }

        DrawTimeline();
        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        if (Event.current.type == EventType.MouseUp)
        {
            EndDragging();
        }
    }

    private float GetCommandHeight(SerializedProperty commandProperty)
    {
        return EditorGUI.GetPropertyHeight(commandProperty, GUIContent.none) + COMMAND_SPACING;
    }

    private void DrawTimeRuler()
    {
        var rect = EditorGUILayout.GetControlRect(false, TIME_RULER_HEIGHT);
        rect.width = position.width - AGENTS_LIST_WIDTH - SEPARATOR_WIDTH;
        EditorGUI.DrawRect(rect, new Color(0.2f, 0.2f, 0.2f));
        
        float timeScale = pixelsPerSecond;
        
        // Create clipping rect for the ruler
        GUI.BeginClip(rect);
        
        // Adjust coordinates to be relative to clipping rect
        float scrollOffset = timelineScrollPosition.x;
        
        // Handle zoom with mouse wheel
        var currentEvent = Event.current;
        var mouseInRulerForZoom = currentEvent.mousePosition.x > 0 && 
                                 currentEvent.mousePosition.x < rect.width && 
                                 currentEvent.mousePosition.y > rect.y && 
                                 currentEvent.mousePosition.y < rect.y + rect.height;

        if (currentEvent.type == EventType.ScrollWheel && mouseInRulerForZoom)
        {
            // Calculate time at mouse position before zoom
            float timeAtMouse = (currentEvent.mousePosition.x + scrollOffset) / pixelsPerSecond;
            
            // Apply zoom
            float oldPixelsPerSecond = pixelsPerSecond;
            float zoomDelta = -currentEvent.delta.y * 10f;
            pixelsPerSecond = Mathf.Clamp(pixelsPerSecond + zoomDelta, MIN_PIXELS_PER_SECOND, MAX_PIXELS_PER_SECOND);
            
            // Adjust scroll to keep the time at mouse position
            if (oldPixelsPerSecond != pixelsPerSecond)
            {
                float newScrollOffset = (timeAtMouse * pixelsPerSecond) - currentEvent.mousePosition.x;
                timelineScrollPosition.x = newScrollOffset;
            }
            
            currentEvent.Use();
            Repaint();
        }
        
        for (float t = MIN_TIME; t <= MAX_TIME; t += 0.1f)
        {
            float x = (t * timeScale) - scrollOffset;
            bool isMajorLine = Mathf.RoundToInt(t * 10) % 5 == 0;
            float startY = isMajorLine ? rect.height * 0.2f : rect.height * 0.6f;
            
            Handles.color = isMajorLine ? Color.white : new Color(1f, 1f, 1f, 0.2f);
            
            Vector3[] points = new Vector3[] {
                new Vector3(x, startY, 0),
                new Vector3(x, rect.height, 0)
            };
            
            Handles.DrawAAPolyLine(2f, points);
        }
        
        // Draw time labels
        var style = new GUIStyle(EditorStyles.miniLabel);
        style.alignment = TextAnchor.UpperLeft;
        style.normal.textColor = new Color(1f, 1f, 1f, 0.2f);
        
        for (float t = MIN_TIME; t <= MAX_TIME; t += 1f)
        {
            float x = (t * timeScale) - scrollOffset;
            EditorGUI.LabelField(
                new Rect(x + 2, 2, 30, rect.height),
                t.ToString("0.0"),
                style
            );
        }

        // Current time marker
        float markerX = (currentTime * timeScale) - scrollOffset;
        var markerRect = new Rect(markerX - 4, 0, 8, rect.height);
        EditorGUI.DrawRect(markerRect, Color.white);

        // Handle marker dragging
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.ResizeHorizontal);
        
        var e = Event.current;
        // Convert mouse position to local coordinates
        Vector2 localMousePos = e.mousePosition;
        var mouseInRuler = localMousePos.x > 0 && localMousePos.x < rect.width && 
                           localMousePos.y > rect.y && localMousePos.y < rect.y + rect.height;
        
        switch (e.type)
        {
            case EventType.MouseDown:
                if (e.button == 0 && mouseInRuler)
                {
                    isDraggingMarker = true;
                    markerDragStartX = e.mousePosition.x;
                    // Set the time immediately on click
                    currentTime = Mathf.Clamp((e.mousePosition.x + scrollOffset) / timeScale, MIN_TIME, MAX_TIME);
                    TimelineManager.SetTime(currentTime);
                    markerDragStartTime = currentTime;
                    e.Use();
                }
                break;

            case EventType.MouseDrag:
                if (isDraggingMarker && mouseInRuler)
                {
                    float mouseDelta = e.mousePosition.x - markerDragStartX;
                    float timeDelta = mouseDelta / timeScale;
                    currentTime = Mathf.Clamp(markerDragStartTime + timeDelta, MIN_TIME, MAX_TIME);
                    TimelineManager.SetTime(currentTime);
                    Repaint();
                    e.Use();
                }
                break;

            case EventType.MouseUp:
                if (isDraggingMarker)
                {
                    isDraggingMarker = false;
                    e.Use();
                }
                break;
        }
        
        GUI.EndClip();
    }

    private void DrawTimeline()
    {
        float viewWidth = (MAX_TIME - MIN_TIME) * pixelsPerSecond;
        
        Vector2 newRightScroll = EditorGUILayout.BeginScrollView(
            new Vector2(timelineScrollPosition.x, scrollPosition.y),
            false,
            true,
            GUILayout.Width(position.width - AGENTS_LIST_WIDTH - SEPARATOR_WIDTH),
            GUILayout.Height(position.height - TIME_RULER_HEIGHT)
        );

        if (newRightScroll.y != scrollPosition.y)
        {
            scrollPosition.y = newRightScroll.y;
            Repaint();
        }
        timelineScrollPosition.x = newRightScroll.x;

        EditorGUILayout.BeginHorizontal(GUILayout.Width(viewWidth));
        
        var agents = Object.FindObjectsOfType<TimelineAgent>();
        EditorGUILayout.BeginVertical();
        foreach (var agent in agents)
        {
            DrawAgentTrack(agent);
        }
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
    }

    private void DrawAgentTrack(TimelineAgent agent)
    {
        SerializedObject serializedAgent = new SerializedObject(agent);
        SerializedProperty commandsProperty = serializedAgent.FindProperty("commandsData");
        
        float totalCommandsHeight = 0f;
        for (int j = 0; j < agent.commandsData.Count; j++)
        {
            var commandProperty = commandsProperty.GetArrayElementAtIndex(j);
            totalCommandsHeight += GetCommandHeight(commandProperty);
        }
        
        float trackHeight = TRACK_HEIGHT + totalCommandsHeight;
        var rect = EditorGUILayout.GetControlRect(false, trackHeight);
        EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));

        float timeScale = rect.width / (MAX_TIME - MIN_TIME) * zoom;
        float currentY = rect.y + TRACK_HEIGHT;

        for (int i = 0; i < agent.commandsData.Count; i++)
        {
            var command = agent.commandsData[i];
            var commandProperty = commandsProperty.GetArrayElementAtIndex(i);
            float commandHeight = GetCommandHeight(commandProperty) - COMMAND_SPACING;
            
            // Use drag values for visual representation if this command is being dragged
            float startTime = (isDragging && command == currentDragCommand) ? currentDragStartTime : command.startTime;
            float endTime = (isDragging && command == currentDragCommand) ? currentDragEndTime : command.endTime;
            
            float startX = rect.x + startTime * timeScale;
            float width = (endTime - startTime) * timeScale;
            
            var commandRect = new Rect(
                startX, 
                currentY,
                width,
                commandHeight
            );

            // Define drag zones
            var leftEdgeZone = new Rect(commandRect.x, commandRect.y, EDGE_DRAG_MARGIN, commandRect.height);
            var rightEdgeZone = new Rect(commandRect.xMax - EDGE_DRAG_MARGIN, commandRect.y, EDGE_DRAG_MARGIN, commandRect.height);
            var centerZone = new Rect(commandRect.x + EDGE_DRAG_MARGIN, commandRect.y, 
                                    commandRect.width - (EDGE_DRAG_MARGIN * 2), commandRect.height);

            EditorGUIUtility.AddCursorRect(leftEdgeZone, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(rightEdgeZone, MouseCursor.ResizeHorizontal);
            EditorGUIUtility.AddCursorRect(centerZone, MouseCursor.MoveArrow);

            var e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (leftEdgeZone.Contains(e.mousePosition))
                {
                    StartDragging(DragType.LeftEdge, command, commandProperty);
                    e.Use();
                }
                else if (rightEdgeZone.Contains(e.mousePosition))
                {
                    StartDragging(DragType.RightEdge, command, commandProperty);
                    e.Use();
                }
                else if (centerZone.Contains(e.mousePosition))
                {
                    StartDragging(DragType.Whole, command, commandProperty);
                    e.Use();
                }
            }

            EditorGUI.DrawRect(commandRect, new Color(0.4f, 0.6f, 0.8f));
            var commandName = command.GetType().Name.Replace("Data", "");
            EditorGUI.LabelField(commandRect, commandName, EditorStyles.miniLabel);
            
            currentY += commandHeight + COMMAND_SPACING;
        }
    }

    private void StartDragging(DragType type, BCommandData command, SerializedProperty property)
    {
        isDragging = true;
        currentDragType = type;
        currentDragCommand = command;
        currentDragProperty = property;
        dragStartMouseX = Event.current.mousePosition.x;
        dragStartTime = command.startTime;
        dragEndTime = command.endTime;
        currentDragStartTime = dragStartTime;
        currentDragEndTime = dragEndTime;
    }

    private void HandleDragging(float timeScale)
    {
        float mouseDelta = Event.current.mousePosition.x - dragStartMouseX - AGENTS_LIST_WIDTH - SEPARATOR_WIDTH + timelineScrollPosition.x;
        
        float timeDelta = Mathf.Round((mouseDelta / timeScale) / 0.05f) * 0.05f;

        switch (currentDragType)
        {
            case DragType.LeftEdge:
                currentDragStartTime = Mathf.Clamp(dragStartTime + timeDelta, 0, dragEndTime - 0.05f);
                currentDragEndTime = dragEndTime;
                currentDragProperty.FindPropertyRelative("startTime").floatValue = currentDragStartTime;
                break;

            case DragType.RightEdge:
                currentDragStartTime = dragStartTime;
                currentDragEndTime = Mathf.Clamp(dragEndTime + timeDelta, dragStartTime + 0.05f, MAX_TIME);
                currentDragProperty.FindPropertyRelative("endTime").floatValue = currentDragEndTime;
                break;

            case DragType.Whole:
                float duration = dragEndTime - dragStartTime;
                currentDragStartTime = Mathf.Clamp(dragStartTime + timeDelta, 0, MAX_TIME - duration);
                currentDragEndTime = currentDragStartTime + duration;
                currentDragProperty.FindPropertyRelative("startTime").floatValue = currentDragStartTime;
                currentDragProperty.FindPropertyRelative("endTime").floatValue = currentDragEndTime;
                break;
        }

        currentDragProperty.serializedObject.ApplyModifiedProperties();
        TimelineManager.RecalculateAll();
        Repaint();
    }

    private void EndDragging()
    {
        if (currentDragCommand != null)
        {
            // Find and select the agent GameObject
            var agents = Object.FindObjectsOfType<TimelineAgent>();
            foreach (var agent in agents)
            {
                if (agent.commandsData.Contains(currentDragCommand))
                {
                    Selection.activeGameObject = agent.gameObject;
                    break;
                }
            }
        }

        isDragging = false;
        currentDragType = DragType.None;
        currentDragCommand = null;
        currentDragProperty = null;
    }

    private void AddCommand<T>(TimelineAgent agent) where T : BCommandData, new()
    {
        var command = new T();
        Undo.RecordObject(agent, "Add Timeline Command");
        
        agent.commandsData.Add(command);
        EditorUtility.SetDirty(agent);
        
        TimelineManager.OnCommandAdded(agent, command);
    }
} 