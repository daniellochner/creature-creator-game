using UnityEngine;
using System;
using System.Collections;

// Source: http://www.brechtos.com/hiding-or-disabling-inspector-properties-using-propertydrawers-within-unity-5/

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct, Inherited = true)]
public class ConditionalHideAttribute : PropertyAttribute
{
	public string ConditionalSourceField = "";
	public string ConditionalSourceField2 = "";
	public string[] ConditionalSourceFields = new string[] { };
	public bool[] ConditionalSourceFieldInverseBools = new bool[] { };
	public bool HideInInspector = false;
	public bool Inverse = false;
	public bool UseOrLogic = false;
	public int RequiredEnumIndex = 0;

	public bool InverseCondition1 = false;
	public bool InverseCondition2 = false;


	// Use this for initialization
	public ConditionalHideAttribute(string conditionalSourceField)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = false;
		this.Inverse = false;
	}

	public ConditionalHideAttribute(string conditionalSourceField, int requiredEnumIndex)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = false;
		this.RequiredEnumIndex = requiredEnumIndex;
		this.Inverse = false;
	}

	public ConditionalHideAttribute(string conditionalSourceField, int requiredEnumIndex, bool hideInInspector)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = hideInInspector;
		this.RequiredEnumIndex = requiredEnumIndex;
		this.Inverse = false;
	}

	public ConditionalHideAttribute(string conditionalSourceField, int requiredEnumIndex, bool hideInInspector, bool inverse)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = hideInInspector;
		this.RequiredEnumIndex = requiredEnumIndex;
		this.Inverse = inverse;
	}

	public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = hideInInspector;
		this.Inverse = false;
	}

	public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector, bool inverse)
	{
		this.ConditionalSourceField = conditionalSourceField;
		this.HideInInspector = hideInInspector;
		this.Inverse = inverse;
	}

	public ConditionalHideAttribute(bool hideInInspector = false)
	{
		this.ConditionalSourceField = "";
		this.HideInInspector = hideInInspector;
		this.Inverse = false;
	}

	public ConditionalHideAttribute(string[] conditionalSourceFields, bool[] conditionalSourceFieldInverseBools, bool hideInInspector, bool inverse)
	{
		this.ConditionalSourceFields = conditionalSourceFields;
		this.ConditionalSourceFieldInverseBools = conditionalSourceFieldInverseBools;
		this.HideInInspector = hideInInspector;
		this.Inverse = inverse;
	}

	public ConditionalHideAttribute(string[] conditionalSourceFields, bool hideInInspector, bool inverse)
	{
		this.ConditionalSourceFields = conditionalSourceFields;
		this.HideInInspector = hideInInspector;
		this.Inverse = inverse;
	}
}