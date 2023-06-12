
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class MovingPlatform : UdonSharpBehaviour
{
    public Transform platform;
    public Transform start, end;
    public float speed;

    private bool forward = true;

    void Update()
    {
        if (forward)
        {
            platform.position = Vector3.MoveTowards(platform.position, end.position, speed * Time.deltaTime);
        }
        else 
        {
            platform.position = Vector3.MoveTowards(platform.position, start.position, speed * Time.deltaTime);
        }
        

        if (platform.position == end.position || platform.position == start.position)
        {
            forward = !forward;
        }
    }
}
