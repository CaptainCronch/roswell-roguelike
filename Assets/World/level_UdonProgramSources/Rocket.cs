
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Rocket : UdonSharpBehaviour
{
    public GameObject explosion;
    private Rigidbody rb;
    private Collider col;

    private float despawnTimer = 0f;

    public float speed = 15f;
    public float despawnTime = 5f;

    void Start()
    {
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        rb.AddRelativeForce(Vector3.forward * speed, ForceMode.Impulse);
    }

    void Update(){
        if (despawnTimer >= despawnTime){
            Destroy(gameObject);
        } else {
            despawnTimer += Time.deltaTime;
        }
    }

    void OnCollisionEnter(Collision collision){
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
