using System;
using System.Collections.Generic;
using System.Reflection;
using MidiJack;
using UnityEngine;

[Serializable]
public enum MidiInputType
{
	Slider,
	Knob
}

[Serializable]
public class MidiInput
{
	[SerializeField]
	public MidiInputType Type;
	
	[SerializeField]
	public int Number;

	public MidiInput(MidiInputType type, int number)
	{
		Type = type;
		Number = number;
	}

	public int KnobNumber
	{
		get
		{
			switch (Type)
			{
				case MidiInputType.Knob:
					return Number + 16;
				case MidiInputType.Slider:
					return Number;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}

	public string Name
	{
		get { return string.Format(@"{0} {1}", Type, Number + 1); }
	}
}

[Serializable]
public class InputConfiguration
{
	public string Component;
	public string Member;
	
	// TODO: Range as a min/max instead of just a max?
	public int Range = 1;
	
	[SerializeField]
	public MidiInput Input;

	public InputConfiguration(MidiInput input)
	{
		Debug.Log("Creating InputConfiguration with MidiInput");
		Input = input;
	}
	
	public InputConfiguration(string component, string member, int range, MidiInput input)
	{
		Component = component;
		Member = member;
		Range = range;
		Input = input;
	}
}

public class MidiController : MonoBehaviour
{
	private readonly Dictionary<float, float> _previousValues = new Dictionary<float, float>();
	
	public List<InputConfiguration> Inputs = new List<InputConfiguration>();

	public static List<MidiInput> AllInputs()
	{
		var list = new List<MidiInput>();
		for (var i = 0; i < 8; i++)
		{
			list.Add(new MidiInput(MidiInputType.Knob, i));
		}
		
		for (var i = 0; i < 8; i++)
		{
			list.Add(new MidiInput(MidiInputType.Slider, i));
		}

		return list;
	}

	void Update()
	{
		var shouldChange = !(MidiMaster.GetKnob(MidiChannel.All, 43) > 0.0);

		Inputs.ForEach((i) => UpdateValue(i, shouldChange));
	}

	private void UpdateValue(InputConfiguration i, bool shouldChange)
	{
		if (i.Component == null || i.Member == null)
		{
			return;
		}
		
		// TODO: Cache these lookups from update to update?
		var component = GetComponent(i.Component);
		var type = component.GetType();
		MemberInfo member = type.GetField(i.Member);
		
		if (member == null)
		{
			member = type.GetProperty(i.Member);
		}

		if (member == null)
		{
			Debug.Log("No member! Returning!");
			return;
		}

        
		var previousValue = 0.0f;
		if (_previousValues.ContainsKey(i.Input.KnobNumber))
		{
			previousValue = _previousValues[i.Input.KnobNumber];
		}

		var knobValue = MidiMaster.GetKnob(MidiChannel.All, i.Input.KnobNumber) * i.Range;
		var difference = knobValue - previousValue;
		var newValue = (float) GetValue(member, component) + difference;

		_previousValues[i.Input.KnobNumber] = knobValue;
		
		if (shouldChange)
		{
			SetValue(member, component, newValue);
		}
	}
	
	// Via https://stackoverflow.com/questions/12680341/how-to-get-both-fields-and-properties-in-single-call-via-reflection
	public static void SetValue(MemberInfo member, object owner, object value)
	{
		switch (member.MemberType)
		{
			case MemberTypes.Property:
				((PropertyInfo)member).SetValue(owner, value, null);
				break;
			case MemberTypes.Field:
				((FieldInfo)member).SetValue(owner, value);
				break;
			default:
				throw new Exception("Property must be of type FieldInfo or PropertyInfo");
		}
	}

	public static object GetValue(MemberInfo member, object owner)
	{
		switch (member.MemberType)
		{
			case MemberTypes.Property:
				return ((PropertyInfo)member).GetValue(owner, null);
			case MemberTypes.Field:
				return ((FieldInfo)member).GetValue(owner);
			default:
				throw new Exception("Property must be of type FieldInfo or PropertyInfo");
		}
	}

	public static object GetValue(InputConfiguration input, GameObject owner)
	{
		var component = owner.GetComponent(input.Component);
		var type = component.GetType();
		MemberInfo member = type.GetField(input.Member);
		
		if (member == null)
		{
			member = type.GetProperty(input.Member);
		}

		if (member == null)
		{
			Debug.Log("No member! Returning!");
			return null;
		}

		return GetValue(member, component);
	}

	private static Type GetType(MemberInfo member)
	{
		switch (member.MemberType)
		{
			case MemberTypes.Field:
				return ((FieldInfo)member).FieldType;
			case MemberTypes.Property:
				return ((PropertyInfo)member).PropertyType;
			case MemberTypes.Event:
				return ((EventInfo)member).EventHandlerType;
			default:
				throw new ArgumentException("MemberInfo must be if type FieldInfo, PropertyInfo or EventInfo");
		}
	}
}