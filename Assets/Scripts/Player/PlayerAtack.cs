using UnityEngine;

public class PlayerAtack : MonoBehaviour
{
    [SerializeField] private ChefaoFSM chefaoFsm;
    [SerializeField] private GuardiaoFSM guardiaoFsm;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Atacar();
        }
    }

    private void Atacar()
    {
        if (chefaoFsm != null)
        {
            chefaoFsm.ReceberDano();
        }
        else if (guardiaoFsm != null)
        {
            guardiaoFsm.ReceberDano();
        }
    }
}
