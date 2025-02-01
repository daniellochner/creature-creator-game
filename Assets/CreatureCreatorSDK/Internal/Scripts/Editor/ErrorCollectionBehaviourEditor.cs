using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ErrorCollectionBehaviour), true), CanEditMultipleObjects]
public class ErrorCollectionBehaviourEditor : Editor
{
	List<string> errors = new List<string>();

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		RunErrorChecks();
	}

	public void RunErrorChecks()
	{
		errors.Clear();

		((ErrorCollectionBehaviour)target).RunErrorChecks(ref errors);

		if(errors.Count > 0)
		{
			EditorGUILayout.BeginVertical("Box");

			var color = GUI.color;
			GUI.color = new Color(1, 1, 0);
			EditorGUI.indentLevel = 0;
			EditorGUILayout.LabelField($"{errors.Count} Unresolved Issue{(errors.Count == 1 ? "" : "s")}", EditorStyles.boldLabel);
			GUI.color = color;

			EditorGUI.indentLevel = 1;
			foreach(var error in errors)
			{
				EditorGUILayout.LabelField($"• {error}", EditorStyles.wordWrappedLabel);
			}

			EditorGUI.indentLevel = 0;
			EditorGUILayout.EndVertical();
		}
	}
}
