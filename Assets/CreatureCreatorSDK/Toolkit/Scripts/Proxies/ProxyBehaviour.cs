using UnityEngine;

public class ProxyBehaviour : MonoBehaviour
{
    protected virtual void OnDrawGizmos()
    {
    }
    protected virtual void OnDrawGizmosSelected()
    {
    }

    public virtual bool IsValid()
    {
        return true;
    }
}
