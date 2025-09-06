using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChefaoFSM : MonoBehaviour
{
   public enum Estados
   {
      Idle,
      Chase,
      Attack,
      SpecialAttack,
      Flee,
      Dead,
   }
   
   private Estados _estadoAtual =  Estados.Idle;
   
   [SerializeField] private float movimentSpeed = 3f;
   
   [Header("Referencias")]
   [SerializeField] private Transform playerPosition;
   [SerializeField] private ParticleSystem ataqueEspecial;
   [SerializeField] private ParticleSystem ataqueCorpoCorpo;
   [SerializeField] private GameObject projetilAtaqueDistancia;
   [SerializeField] private Text vidaAtualText;
   
   [Header("Configs Idle")]
   [SerializeField] private Transform idlePosition;

   [Header("Configs Chase")]
   [SerializeField] private float chaseDistance = 15f;
   
   [Header("Configs Attacks")]
   [SerializeField] private float rangeAtaqueCorpoCorpo = 3f;
   [SerializeField] private float cooldownCorpoCorpo = 1f;
   private float _tempoUltimoAtaqueCorpoCorpo = 0f;
   [SerializeField] private float rangeAtaqueDistancia = 10f;
   [SerializeField] private float cooldownAtaqueDistancia = 1f;
   private float _tempoUltimoAtaqueDistancia = 0f;
   [SerializeField] private float cooldownAtaqueEspecial = 1f;
   private float _tempoUltimoAtaqueEspecial = 0f;
   
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
         case Estados.Idle:
            IdleState();
            break;
         case Estados.Chase:
            ChaseState();
            break;
         case Estados.Attack:
            AttackState();
            break;
         case Estados.SpecialAttack:
            SpecialAttackState();
            break;
         case Estados.Flee:
            FleeState();
            break;
         case Estados.Dead:
            StateDead();
            break;
      }
   }

   private void IdleState()
   {
      if (VidaBaixa())
      {
         if (VidaCritica())
         {
            _estadoAtual = Estados.Flee;
         }
         else
         {
            _estadoAtual = Estados.SpecialAttack;
         }
      }
      else
      {
         if (PlayerAvistado())
         {
            _estadoAtual = Estados.Chase;
         }
         else
         {
            if (EstouNaBase())
            {
               //parado
            }
            else
            {
               transform.position = Vector2.MoveTowards(transform.position, idlePosition.position, movimentSpeed * Time.deltaTime);
            }
         }  
      }
   }

   private void ChaseState()
   {
      if (VidaBaixa())
      {
         if(VidaCritica())
         {
            _estadoAtual = Estados.Flee;
         }
         else
         {
            _estadoAtual = Estados.SpecialAttack;
         }
      }
      else
      {
         transform.position = Vector2.MoveTowards(transform.position, playerPosition.position, movimentSpeed * Time.deltaTime);
         if (!PlayerAvistado())
         {
            _estadoAtual = Estados.Idle;
         }

         if (PossoAtacarCorpoCorpo() || PossoAtacarDistancia())
         {
            _estadoAtual = Estados.Attack;
         }  
      }
   }

   private void AttackState()
   {
      if (VidaBaixa())
      {
         _estadoAtual = Estados.SpecialAttack;
      }
      else
      {
         if (PossoAtacarCorpoCorpo())
         {
            ataqueCorpoCorpo.Play();
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
            if (PlayerAvistado())
            {
               _estadoAtual = Estados.Chase;
            }
            else
            {
               _estadoAtual = Estados.Idle;
            }
         }
      }
   }

   private void SpecialAttackState()
   {
      if (PossoAtacarEspecial())
      {
         ataqueEspecial.Play();
         _tempoUltimoAtaqueEspecial =  Time.time + cooldownAtaqueEspecial;
      }
      if (VidaCritica())
      {
         _estadoAtual = Estados.Flee;
      }
   }

   
   private void FleeState()
   {
      transform.position = Vector2.MoveTowards(transform.position, fleePosition.position, movimentSpeed * Time.deltaTime);
      if (PossoCurar())
      {
         Curar();
      }

      if (DistanciaDoPlayer() <= chaseDistance * 0.5f)
      {
         _estadoAtual = Estados.Chase;
      }

      if (_vidaAtual == vidaMaxima)
      {
         _estadoAtual = Estados.Idle;
      }
   }

   private void StateDead()
   {
      Destroy(gameObject);
   }

   public void ReceberDano(int danoRecebido = 1)
   {
      _vidaAtual -= danoRecebido;
      vidaAtualText.text =  _vidaAtual.ToString();
      Debug.Log("Recebi dano, vida atual: " +  _vidaAtual);
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
   private bool PlayerAvistado()
   {
      if(DistanciaDoPlayer() <= chaseDistance)
      {
         return true;
      }
      return  false;
   }

   private bool EstouNaBase()
   {
      if (Vector2.Distance(idlePosition.position, transform.position) <= 0.1f)
      {
         return true;
      }
      
      return false;
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

   private bool PossoAtacarEspecial()
   {
      if (_tempoUltimoAtaqueEspecial <= Time.time)
      {
         return true;
      }
      return false;
   }

   private bool VidaBaixa()
   {
      if (_vidaAtual <= vidaMaxima * 0.5f)
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
