using UnityEngine;
using System.Collections;

public class FollowPlayer : MonoBehaviour
{
    private Vector3 offset;
    private GameObject player;

    void Start()
    {
        offset = transform.position;
    }

    void LateUpdate()
    {
        if (player == null)
        {
            player = GameObject.Find("Player(Clone)");
        }
        else
        {
            transform.position = player.transform.position + offset;
        }
    }
}