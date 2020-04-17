using UnityEngine;

public class AreaHelper : MonoBehaviour
{
    public float radius;

    private void OnDrawGizmos()
    {
        var tf = transform;
        var position = tf.position;
        var scale = tf.localScale;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(position + Vector3.Scale(scale, new Vector3(0.5f, 0, 0.5f)), radius);
        Gizmos.DrawWireSphere(position + Vector3.Scale(scale, new Vector3(-0.5f, 0, 0.5f)), radius);
        Gizmos.DrawWireSphere(position + Vector3.Scale(scale, new Vector3(0.5f, 0, -0.5f)), radius);
        Gizmos.DrawWireSphere(position + Vector3.Scale(scale, new Vector3(-0.5f, 0, -0.5f)), radius);
    }
}