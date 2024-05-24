using UnityEngine;

public class hitTest : MonoBehaviour
{
    [SerializeField] private BoxCollider2D _boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("PlayerAttack"))
        {
            Debug.Log("yaaay");
        }
    }

}
