using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

internal class SelectedItem
{
  public GameObject Object;
  public int ComponentIndex = 0;
  public int MemberIndex = 0;

  public List<string> Components;
  public List<string> Members;

  public readonly InputConfiguration Item;

  public bool Open = true;

  public SelectedItem(MidiInput input, GameObject obj, List<string> components)
  {
    Item = new InputConfiguration(input);
    Object = obj;
    Components = components;

    Members = obj == null ? new List<string>() : GetMembers(Object.GetComponent(Components[0])); 
  }

  public SelectedItem(InputConfiguration config, GameObject obj, List<string> components)
  {
    Item = config;
    Object = obj;

    Components = components;
    Members = GetMembers(Object.GetComponent(config.Component));

    ComponentIndex = components.IndexOf(Item.Component);
    MemberIndex = Members.IndexOf(Item.Member);
  }

  public void SetComponentIndex(int index)
  {
    ComponentIndex = index;
    MemberIndex = 0;
    var componentName = Components.ElementAt(ComponentIndex);
    Item.Component = componentName;

    Members = GetMembers(Object.GetComponent(componentName));
  }

  private static List<string> GetMembers(Component component)
  {
    if (!component)
    {
      return new List<string>();
    }
    // We care about both properties and fields, so we grab them separately

    var properties = component.GetType().GetProperties(System.Reflection.BindingFlags.Public |
                                                       System.Reflection.BindingFlags.Instance |
                                                       System.Reflection.BindingFlags.DeclaredOnly
        )
        .Where(o => _isNumber(o.PropertyType));

    var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public |
                                               System.Reflection.BindingFlags.Instance |
                                               System.Reflection.BindingFlags.DeclaredOnly
        )
        .Where(o => _isNumber(o.FieldType));


    return fields.Select((m) => m.Name).Concat(properties.Select((m) => m.Name)).ToList();
  }

  private static bool _isNumber(Type t)
  {
    return t.IsPrimitive &&
           !(t == typeof(string) || t == typeof(bool) || t == typeof(char));
  }
}

[CustomEditor(typeof(MidiController))]
public class MidiControllerInspector : Editor
{
  private readonly List<SelectedItem> _items = new List<SelectedItem>();

  private int _selectedInput = 0;

  private List<string> _components = new List<string>();

  private readonly List<MidiInput> _allInputs = MidiController.AllInputs();

  private void OnEnable()
  {
    var t = (MidiController)target;

    _components = t.gameObject.GetComponents<Component>()
        .Select((m) => m.GetType().Name)
        .ToList();

    t.Inputs.ForEach((i) =>
    {
      var component = t.GetComponent(i.Component);
      _items.Add(new SelectedItem(i, t.gameObject, _components));
    });
  }

  public override void OnInspectorGUI()
  {
    foreach (var item in _items)
    {
      LayoutItem(item);
    }

    PromptForNewLayoutItem();

    ((MidiController)target).Inputs = _items.Select((i) => i.Item).ToList();
  }

  private void LayoutItem(SelectedItem item)
  {
    item.Open = (EditorGUILayout.Foldout(item.Open, item.Item.Input.Name));
    if (!item.Open) return;

    EditorGUILayout.BeginVertical();

    var componentIndex = EditorGUILayout.Popup("Component", item.ComponentIndex, item.Components.ToArray(), EditorStyles.popup);
    if (componentIndex != item.ComponentIndex)
    {
      item.SetComponentIndex(componentIndex);
    }

    // TODO: Buttons as boolean toggles?

    if (item.Members.Count > 0 && item.MemberIndex >= 0)
    {
      item.MemberIndex =
          EditorGUILayout.Popup("Property", item.MemberIndex, item.Members.ToArray(), EditorStyles.popup);
      item.Item.Member = item.Members.ElementAt(item.MemberIndex);

      item.Item.Range = EditorGUILayout.DelayedIntField("Range", item.Item.Range);
    }

    if (GUILayout.Button("Remove"))
    {
      // TODO: Lol, don't remove while enumerating
      _items.Remove(item);
    }

    EditorGUILayout.EndVertical();
  }

  private void PromptForNewLayoutItem()
  {
    var inputs = _availableInputs();
    EditorGUI.BeginDisabledGroup(inputs.Count == 0);

    var strings = _inputsToStringList(inputs);

    EditorGUILayout.Separator();

    _selectedInput = EditorGUILayout.Popup("Add New Mapping", 0, strings, EditorStyles.popup);

    if (_selectedInput == 0) return;

    var input = inputs.ElementAt(_selectedInput - 1);
    _items.Add(new SelectedItem(input, ((MidiController)target).gameObject, _components));
    _selectedInput = 0;

    EditorGUI.EndDisabledGroup();
  }

  private List<MidiInput> _availableInputs()
  {
    // TODO: This is no longer working as expected
    var currentInputs = _items.Select((i) => i.Item.Input);
    var all = _allInputs.Except(currentInputs).ToList();
    return all.ToList();
  }

  private static string[] _inputsToStringList(IEnumerable<MidiInput> list)
  {
    var strings = list.Select(i => i.Name).ToList();
    strings.Insert(0, "Select an Input");
    return strings.ToArray();
  }
}