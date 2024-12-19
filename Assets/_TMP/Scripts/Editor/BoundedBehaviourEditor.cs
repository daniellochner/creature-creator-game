using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Redlabs.RedmatchSDK
{
	[CanEditMultipleObjects]
	[CustomEditor(typeof(BoundedBehaviour), true)]
	public class BoundedBehaviourEditor : Editor
	{
		private BoxBoundsHandle boxBoundsHandle = new BoxBoundsHandle();

		void OnSceneGUI()
		{
			BoundedBehaviour script = (BoundedBehaviour)target;

			boxBoundsHandle.center = GetWorldCenter();
			boxBoundsHandle.size = script.bounds.size;

			// Draw the handle
			EditorGUI.BeginChangeCheck();
			boxBoundsHandle.handleColor = script.BoundsOutlineColor;
			boxBoundsHandle.wireframeColor = script.BoundsOutlineColor;
			boxBoundsHandle.DrawHandle();
			if(EditorGUI.EndChangeCheck())
			{
				// Record the target object before setting new values so changes can be undone/redone
				Undo.RecordObject(script, "Change Bounds");

				// Copy the handle's updated data back to the target object
				Bounds newBounds = new Bounds();
				newBounds.center = boxBoundsHandle.center - script.transform.position;
				newBounds.size = boxBoundsHandle.size;
				script.bounds = newBounds;
			}
		}

		Vector3 GetWorldCenter()
		{
			BoundedBehaviour script = (BoundedBehaviour)target;
			return script.bounds.center + script.transform.position;
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if(GUILayout.Button("Expand to Fill Area"))
			{
				foreach(var target in targets)
				{
					BoundedBehaviour script = (BoundedBehaviour)target;

					Undo.RecordObject(target, "Expand to Fill Area");
					Undo.RecordObject(script.transform, "Expand to Fill Area");

					Vector3 min = script.bounds.min;
					Vector3 max = script.bounds.max;

					if(Physics.Raycast(GetWorldCenter(), Vector3.forward, out RaycastHit hitForward))
					{
						float distance = hitForward.point.z - GetWorldCenter().z;
						max.z = distance;
					}

					if(Physics.Raycast(GetWorldCenter(), Vector3.right, out RaycastHit hitRight))
					{
						float distance = hitRight.point.x - GetWorldCenter().x;
						max.x = distance;
					}

					if(Physics.Raycast(GetWorldCenter(), Vector3.back, out RaycastHit hitBack))
					{
						float distance = hitBack.point.z - GetWorldCenter().z;
						min.z = distance;
					}

					if(Physics.Raycast(GetWorldCenter(), Vector3.left, out RaycastHit hitLeft))
					{
						float distance = hitLeft.point.x - GetWorldCenter().x;
						min.x = distance;
					}

					Vector3 beforeCenter = script.bounds.center;

					script.bounds.SetMinMax(min, max);

					Vector3 afterCenter = script.bounds.center;

					Vector3 centerChange = afterCenter - beforeCenter;

					script.bounds.center = beforeCenter;

					script.transform.position += centerChange;

					EditorUtility.SetDirty(script);
					EditorUtility.SetDirty(script.transform);
				}
			}

			if(GUILayout.Button("Reset"))
			{
				foreach(var target in targets)
				{
					BoundedBehaviour script = (BoundedBehaviour)target;

					Undo.RecordObject(target, "Reset");

					script.bounds.center = Vector3.zero;
					script.bounds.extents = Vector3.one;

					EditorUtility.SetDirty(script);
				}
			}
		}
	}
}