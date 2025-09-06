using UnityEngine;

public class ProjetilChefao : MonoBehaviour
{
   [HideInInspector] public Vector2 direction = Vector3.zero;
    void Update()
    {
        if (direction != Vector2.zero)
        {
            transform.Translate(direction * 2f * Time.deltaTime);
        }
    }
}
