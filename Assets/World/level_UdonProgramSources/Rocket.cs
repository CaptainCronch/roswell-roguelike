
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class Rocket : UdonSharpBehaviour
{
    public GameObject explosion;
    private Rigidbody rb;

    private float despawnTimer = 0f;

    public float speed = 15f, strength = 2f, radius = 2f;
    public float despawnTime = 5f;
    public LayerMask layers;

    void Start()
    {
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
        foreach (Collider collider in Physics.OverlapSphere(transform.position, radius, layers))
        {
            float distance = Vector3.Distance(collider.ClosestPointOnBounds(transform.position), transform.position);
            collider.gameObject.transform.parent.parent.gameObject.GetComponent<PlayerManager>().Explode(transform.position, distance, strength, radius);
        }
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
