using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GuardiaoFSM : MonoBehaviour
{
    public enum Estados
   {
      Patrol,
      Alert,
      Chase,
      Attack,
      CallAllies,
      Retreat,
      Dead,
   }
   
   private Estados _estadoAtual =  Estados.Patrol;
   
   [SerializeField] private float movimentSpeed = 3f;
   
   [Header("Referencias")]
   [SerializeField] private Transform playerPosition;
   [SerializeField] private ParticleSystem ataqueEspecial;
   [SerializeField] private ParticleSystem ataqueCorpoCorpo;
   [SerializeField] private GameObject projetilAtaqueDistancia;
   [SerializeField] private Text vidaAtualText;


   [Header("Configs Patrol")]
   [SerializeField] private List<Transform> patrolPositions;
   private int _patrolIndex = 0;
   
   [Header("Configs Alert")]
   [SerializeField] private float alertDistance = 20f;

   [Header("Configs Chase")]
   [SerializeField] private float chaseDistance = 10f;
   
   [Header("Configs Attacks")]
   [SerializeField] private float rangeAtaqueCorpoCorpo = 3f;
   [SerializeField] private float cooldownCorpoCorpo = 1f;
   private float _tempoUltimoAtaqueCorpoCorpo = 0f;
   [SerializeField] private float rangeAtaqueDistancia = 10f;
   [SerializeField] private float cooldownAtaqueDistancia = 1f;
   private float _tempoUltimoAtaqueDistancia = 0f;
   
   [Header("Configs CallReinforments")]
   [SerializeField] private List<GameObject> allies;
   private bool _alreadyCalledReinforcement = false;

   
   [Header("Configs Flee")]
   [SerializeField] private Transform fleePosition;
   [SerializeField] private int healPerSecond = 1;
   private float _tempoUltimaCura = 0f;
   
   [Header("Configs Vida")]
   [SerializeField] private int vidaMaxima = 10;
   private int _vidaAtual;

   private void Start()
   {
      _vidaAtual = vidaMaxima;
   }

   private void Update()
   {
      switch (_estadoAtual)
      {
         case Estados.Patrol:
            PatrolState();
            break;
         case Estados.Alert:
            AlertState();
            break;
         case Estados.Chase:
            ChaseState();
            break;
         case Estados.Attack:
            AttackState();
            break;
         case Estados.CallAllies:
            CallReinforcements();
            break;
         case Estados.Retreat:
            FleeState();
            break;
         case Estados.Dead:
            StateDead();
            break;
      }
   }

   private void PatrolState()
   {
      if (VidaBaixa() && !_alreadyCalledReinforcement)
      {
         _estadoAtual = Estados.CallAllies;
      }
      else if (VidaCritica())
      {
         _estadoAtual = Estados.Retreat;
      }
      else
      {
         if (DistanciaDoPlayer() < alertDistance)
         {
            _estadoAtual = Estados.Alert;
         }
         else //patrulhar
         {
               transform.position = Vector2.MoveTowards(transform.position, patrolPositions[_patrolIndex].position, movimentSpeed * Time.deltaTime);
               if (Vector2.Distance(transform.position, patrolPositions[_patrolIndex].position) <= 0.1f)
               {
                  if (_patrolIndex + 1 >= patrolPositions.Count)
                  {
                     _patrolIndex = 0;
                  }
                  else
                  {
                     _patrolIndex++;
                  }
               }
         }  
      }
   }

   private void AlertState()
   {
      Debug.Log("Olhando player");
      
      if (VidaBaixa() && !_alreadyCalledReinforcement)
      {
         _estadoAtual = Estados.CallAllies;
      }
      else if (VidaCritica())
      {
         _estadoAtual = Estados.Retreat;
      }
      
      if (DistanciaDoPlayer() > alertDistance)
      {
         _estadoAtual = Estados.Patrol;
      }

      if (DistanciaDoPlayer() < chaseDistance)
      {
         _estadoAtual = Estados.Chase;
      }
      
   }
   private void ChaseState()
   {
      if (VidaBaixa() && !_alreadyCalledReinforcement)
      {
         _estadoAtual = Estados.CallAllies;
      }
      else if (VidaCritica())
      {
         _estadoAtual = Estados.Retreat;
      }
      else
      {
         transform.position = Vector2.MoveTowards(transform.position, playerPosition.position, movimentSpeed * Time.deltaTime);
         if (DistanciaDoPlayer() > chaseDistance)
         {
            _estadoAtual = Estados.Alert;
         }

         if (PossoAtacarCorpoCorpo() || PossoAtacarDistancia())
         {
            _estadoAtual = Estados.Attack;
         }  
      }
   }

   private void AttackState()
   {
      if (VidaBaixa() && !_alreadyCalledReinforcement)
      {
         _estadoAtual = Estados.CallAllies;
      }
      else if (VidaCritica())
      {
         _estadoAtual = Estados.Retreat;
      }
      else
      {
         if (PossoAtacarCorpoCorpo())
         {
            ataqueCorpoCorpo.Play();
            _tempoUltimoAtaqueCorpoCorpo = Time.time + cooldownCorpoCorpo;
         }
         else if (PossoAtacarDistancia())
         {
            DispararProjetil();
            _tempoUltimoAtaqueDistancia = Time.time + cooldownAtaqueDistancia;
         }

         if (!PossoAtacarDistancia() && !PossoAtacarCorpoCorpo())
         {
            _estadoAtual = Estados.Chase;
         }
      }
   }

   private void CallReinforcements()
   {
      _alreadyCalledReinforcement = true;
      foreach (GameObject aliado in allies)
      {
         aliado.SetActive(true);
      }
      _estadoAtual = Estados.Patrol;
   }

   
   private void FleeState()
   {
      transform.position = Vector2.MoveTowards(transform.position, fleePosition.position, movimentSpeed * Time.deltaTime);
      if (PossoCurar())
      {
         Curar();
      }

      if (_vidaAtual == vidaMaxima) //ignora o player até se recuperar totalmente
      {
         _estadoAtual = Estados.Patrol;
      }
   }

   private void StateDead()
   {
      Destroy(gameObject);
   }

   public void ReceberDano(int danoRecebido = 1)
   {
      _vidaAtual -= danoRecebido;
      vidaAtualText.text = _vidaAtual.ToString();
      if (_vidaAtual == 0)
      {
         _estadoAtual = Estados.Dead;
      }
   }
   
   public void Curar()
   {
      _vidaAtual++;
      _vidaAtual = Mathf.Clamp(_vidaAtual, 0, vidaMaxima);
      vidaAtualText.text = _vidaAtual.ToString();
      Debug.Log("Curei. Vida atual: " + _vidaAtual);
      _tempoUltimaCura = Time.time + 1f;
   }

   private void DispararProjetil()
   {
      GameObject projetil = Instantiate(projetilAtaqueDistancia, transform.position, transform.rotation);
      Vector2 direcaoProjetil = playerPosition.position - transform.position;
      ProjetilChefao scriptProjetil = projetil.GetComponent<ProjetilChefao>();
      
      if (scriptProjetil != null)
      {
         scriptProjetil.direction = direcaoProjetil;
      }
   }
   
   #region booleanas e afins

   private float DistanciaDoPlayer()
   {
      if (playerPosition == null)
      {
         return Mathf.Infinity;
      }
      return Vector2.Distance(playerPosition.position, transform.position);
   }
   private bool PossoAtacarCorpoCorpo()
   {
      if (DistanciaDoPlayer() <= rangeAtaqueCorpoCorpo)
      {
         if (_tempoUltimoAtaqueCorpoCorpo <= Time.time)
         {
            return true;
         }
      }
      return false;
   }

   private bool PossoAtacarDistancia()
   {
      if (DistanciaDoPlayer() <= rangeAtaqueDistancia && DistanciaDoPlayer() >= rangeAtaqueCorpoCorpo)
      {
         if (_tempoUltimoAtaqueDistancia <= Time.time)
         {
            return true;   
         }
      }
      return false;
   }

   private bool VidaBaixa()
   {
      if (_vidaAtual <= vidaMaxima * 0.4f)
      {
         return true;
      }
      return false;
   }

   private bool VidaCritica()
   {
      if (_vidaAtual <= vidaMaxima * 0.2f)
      {
         return true;
      }
      return false;
   }

   private bool PossoCurar()
   {
      if (_tempoUltimaCura <= Time.time)
      {
         return true;
      }
      return false;
   }
   
   #endregion
}
