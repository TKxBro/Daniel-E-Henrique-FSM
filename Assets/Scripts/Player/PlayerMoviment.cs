using UnityEngine;

public class PlayerMoviment : MonoBehaviour
{
    [SerializeField] private float movimentSpeed;

    private Vector2 _moviment;
    void Update()
    {
        Movimentar();
    }

    private void Movimentar()
    {
        _moviment.x = Input.GetAxis("Horizontal");
        _moviment.y = Input.GetAxis("Vertical");
        
        _moviment.Normalize();
        
        transform.Translate(_moviment.x * movimentSpeed * Time.deltaTime, _moviment.y * movimentSpeed * Time.deltaTime, 0);
    }
}
