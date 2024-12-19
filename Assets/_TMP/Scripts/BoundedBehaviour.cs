using UnityEngine;

public class BoundedBehaviour : MonoBehaviour
{
	public virtual Color BoundsColor { get; private set; }
	public virtual Color BoundsOutlineColor { get; private set; }
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 2);

	void OnDrawGizmos()
	{
		Gizmos.color = BoundsColor;
		Gizmos.DrawCube(transform.position + bounds.center, bounds.size);
	}

	
}