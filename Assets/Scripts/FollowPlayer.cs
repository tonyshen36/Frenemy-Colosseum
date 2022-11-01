using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform target;
    public float heightOffset = 7.0f;
    public float distOffset = 8.0f;

    private void Start()
    {
        transform.parent = null;
    }

    private void LateUpdate()
    {
        transform.position = new Vector3(target.position.x, target.position.y + heightOffset, target.position.z - distOffset);
        transform.LookAt(target.position);
    }
}
