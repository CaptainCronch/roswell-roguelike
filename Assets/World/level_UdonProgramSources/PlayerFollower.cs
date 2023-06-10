
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class PlayerFollower : UdonSharpBehaviour
{
    private VRCPlayerApi player;

    void Start()
    {
        player = Networking.LocalPlayer;
    }

    public override void PostLateUpdate()
    {
        gameObject.transform.position = player.GetPosition();
        gameObject.transform.rotation = player.GetRotation();
    }
}
