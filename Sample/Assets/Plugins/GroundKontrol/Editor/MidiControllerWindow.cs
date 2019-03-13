using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEditor;
using UnityEngine;


class MidiControllerWindow : EditorWindow
{
  [MenuItem("Window/MIDI Controller Mapping")]
  public static void ShowWindow()
  {
    var window = GetWindow<MidiControllerWindow>(typeof(MidiControllerWindow));
    window.NewMappings = new Dictionary<string, SelectedItem>();
  }

  private readonly List<MidiInput> _allInputs = MidiController.AllInputs();
  private static readonly int _width = 90;

  public Dictionary<string, SelectedItem> NewMappings = new Dictionary<string, SelectedItem>();

  Texture2D _whiteImage;
  Texture2D _blackImage;
  Boolean _showBlack = false;

  void Awake()
  {
    _whiteImage = AssetDatabase.LoadAssetAtPath("Assets/Plugins/GroundKontrol/korg-white.png", typeof(Texture2D)) as Texture2D;
    _blackImage = AssetDatabase.LoadAssetAtPath("Assets/Plugins/GroundKontrol/korg-black.png", typeof(Texture2D)) as Texture2D;
  }

  void OnGUI()
  {
    /**
     * Horizontal row of knobs 1-8
     * Photo of MIDI controller
     * Horizontal row of sliders 1-8
     *
     * Each knob/slider:
     * 1 unit per mapping
     * GameObject / Component / Name
     * Min / max / etc
     */

    var sliders = _allInputs.FindAll(i => i.Type == MidiInputType.Slider);
    var knobs = _allInputs.FindAll(i => i.Type == MidiInputType.Knob);

    var boundInputs = FindAllBoundInputs();

    EditorGUILayout.BeginHorizontal();

    GUILayout.Space(350);

    knobs.ForEach((k) =>
    {
      EditorGUILayout.BeginVertical(GUILayout.Width(_width));
      EditorGUILayout.LabelField(k.Name, GUILayout.Width(_width));

      if (GUILayout.Button("+ Add New", GUILayout.Width(_width)))
      {
        var item = new SelectedItem(k, null, new List<string>());
        NewMappings[k.Name] = item;
      }

      EditorGUILayout.Space();

      var items = boundInputs.FindAll(i => i.Item.Input.Name == k.Name);
      items.ForEach(LayoutItem);

      if (NewMappings.ContainsKey(k.Name))
      {
        LayoutItem(NewMappings[k.Name]);
      }

      EditorGUILayout.EndVertical();
    });

    EditorGUILayout.EndHorizontal();

    GUILayout.Label(_image());


    EditorGUILayout.BeginHorizontal();

    GUILayout.Space(10);
    var colorText = _showBlack ? "My controller is white!" : "My controller is black!";
    if (GUILayout.Button(colorText, GUILayout.Width(140)))
    {
      _showBlack = !_showBlack;
    }


    GUILayout.Space(200);

    sliders.ForEach((s) =>
    {
      EditorGUILayout.BeginVertical(GUILayout.Width(_width));
      EditorGUILayout.LabelField(s.Name, GUILayout.Width(_width));

      if (GUILayout.Button("+"))
      {
        var item = new SelectedItem(s, null, new List<string>());
        NewMappings[s.Name] = item;
      }

      EditorGUILayout.Space();

      var items = boundInputs.FindAll(i => i.Item.Input.Name == s.Name);
      items.ForEach(LayoutItem);

      EditorGUILayout.EndVertical();
    });

    EditorGUILayout.EndHorizontal();
  }

  public static List<SelectedItem> FindAllBoundInputs()
  {
    var items = new List<SelectedItem>();

    var objects = Resources.FindObjectsOfTypeAll<MidiController>();
    foreach (var obj in objects)
    {
      var components = obj.gameObject.GetComponents<Component>()
          .Select((m) => m.GetType().Name)
          .ToList();

      obj.Inputs.ForEach((i) =>
      {
        items.Add(new SelectedItem(i, obj.gameObject, components));
      });
    }

    return items;
  }

  private void LayoutItem(SelectedItem item)
  {
    EditorGUIUtility.labelWidth = 60.0f;

    if (GUILayout.Button("X", GUILayout.Width(20)))
    {
      var component = item.Object.GetComponent<MidiController>();
      component.Inputs = component.Inputs.FindAll(i => i != item.Item);
      if (component.Inputs.Count < 1)
      {
        DestroyImmediate(component);
      }
    }

    var selectedGameObj = (GameObject)EditorGUILayout.ObjectField(item.Object, typeof(GameObject), true, GUILayout.Width(_width));
    if (selectedGameObj != null && selectedGameObj != item.Object)
    {
      /* Modifying an object is slightly complicated       
      1. Add MidiController to the new obj if it doesn't have it
      2. Add a new SelectedItem to that MidiController
      3. Remove the old SelectedItem from the old MidiController
      4. Remove the MidiController component from the old object if SelectedItems are empty
       */

      var oldObject = item.Object;
      if (oldObject == null)
      {
        if (NewMappings.ContainsValue(item))
        {
          NewMappings.Remove(item.Item.Input.Name);
        }
      }
      else
      {
        var oldComponent = oldObject.GetComponent<MidiController>();
        oldComponent.Inputs = oldComponent.Inputs.FindAll(i => i != item.Item);
        if (oldComponent.Inputs.Count < 1)
        {
          DestroyImmediate(oldComponent);
        }
      }

      item.Object = selectedGameObj;

      var newComponent = item.Object.GetComponent<MidiController>();
      if (!newComponent)
      {
        newComponent = item.Object.AddComponent<MidiController>();
      }

      // TODO: Properly set item.Item
      item.Components = selectedGameObj.GetComponents<Component>()
          .Select((m) => m.GetType().Name)
          .ToList();

      newComponent.Inputs.Add(item.Item);
    }

    EditorGUILayout.BeginVertical();

    if (item.Components.Count > 0)
    {
      var componentIndex = EditorGUILayout.Popup("", item.ComponentIndex, item.Components.ToArray(),
          EditorStyles.popup, GUILayout.Width(_width));
      if (componentIndex != item.ComponentIndex)
      {
        item.SetComponentIndex(componentIndex);
      }
    }

    if (item.Members.Count > 0)
    {
      item.MemberIndex =
          EditorGUILayout.Popup("", item.MemberIndex, item.Members.ToArray(), EditorStyles.popup, GUILayout.Width(_width));
      item.Item.Member = item.Members.ElementAt(item.MemberIndex);

      item.Item.Range = EditorGUILayout.DelayedIntField("Range", item.Item.Range, GUILayout.Width(_width));

      EditorGUILayout.LabelField("Value: ", _getValue(item).ToString());
    }

    EditorGUILayout.EndVertical();
    EditorGUILayout.Space();
  }

  private static object _getValue(SelectedItem item) => MidiController.GetValue(item.Item, item.Object);

  private static bool _isNumber(Type t)
  {
    return t.IsPrimitive &&
           !(t == typeof(string) || t == typeof(bool) || t == typeof(char));
  }

  private Texture2D _image()
  {
    return _showBlack ? _blackImage : _whiteImage;
  }
}