using System.Collections;
using UnityEngine;

public class PassThroughPlatform : MonoBehaviour
{
    private Collider2D _collider2D;
    private bool _playerOnPlatform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _collider2D = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_playerOnPlatform && Input.GetKeyDown(KeyCode.S))
        {
            StartCoroutine(nameof(HandleLayerCollision));
        }
    }
    private IEnumerator HandleLayerCollision()
    {
        Physics.IgnoreLayerCollision(6, 8, true);
        yield return new WaitForSeconds(0.5f);
        Physics.IgnoreLayerCollision(6, 8, false);
    }
    private void SetPlayerOnPlatform(Collision2D other, bool value)
    {
        if (other.gameObject.TryGetComponent<PlayerController>(out _))
        {
            _playerOnPlatform = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        SetPlayerOnPlatform(collision, true);
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        SetPlayerOnPlatform(collision, false);
    }
}
