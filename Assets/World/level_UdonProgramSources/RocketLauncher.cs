
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

public class RocketLauncher : UdonSharpBehaviour
{
    public GameObject rocket;
    public Transform muzzle;
    public Text display;

    public int maxClip = 4;
    public float delayTime = 0.3f, reloadTime = 0.7f;

    private int clip = 0;
    private float delayTimer = 0f, reloadTimer = 0f;

    private bool shooting = false;

    void Start()
    {
        clip = maxClip;
    }

    void Update()
    {
        if (delayTimer < delayTime)
        {
            delayTimer += Time.deltaTime;
        } 
        else if (reloadTimer < reloadTime)
        {
            reloadTimer += Time.deltaTime;
        }
        else if (reloadTimer >= reloadTime && clip < maxClip)
        {
            reloadTimer = 0f;
            clip++;
        }

        display.text = clip + "/" + maxClip;

        if (shooting) {Shoot();}
    }

    public override void OnPickupUseDown()
    {
        shooting = true;
    }

    public override void OnPickupUseUp()
    {
        shooting = false;
    }

    private void Shoot()
    {
        if (delayTimer >= delayTime && clip > 0)
        {
            delayTimer = 0f;
            reloadTimer = 0f;
            clip--;
            Instantiate(rocket, muzzle.position, muzzle.rotation);
        }
    }
}
