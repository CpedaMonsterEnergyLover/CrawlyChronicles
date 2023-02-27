﻿using Definitions;
using GameCycle;
using Gameplay;
using Gameplay.Interaction;
using Timeline;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Player
{
    public class Manager : MonoBehaviour, IDamageable
    {
        public static Manager Instance { get; private set; }

        [SerializeField] private ParticleSystem healingParticles;
        [SerializeField] private Animator spriteAnimator;
        [SerializeField] private Movement movement;
        [SerializeField] private MainMenu mainMenu;
        [SerializeField] private Text healthText;
        [SerializeField] private Text statsText;
        [SerializeField] private Healthbar healthbar;
        [SerializeField] private float healthbarOffsetY;
        [SerializeField] private float healthbarWidth;
        [SerializeField] private SpriteRenderer eggSpriteRenderer;
        [SerializeField] private AttackController attackController;
        [SerializeField] private PlayerStats baseStats = new();
        [SerializeField] private PlayerStats currentStats;
        
        private readonly int deadHash = Animator.StringToHash("PlayerSpriteDead");

        [field:SerializeField] public bool GodMode { get; private set; }
        public bool IsHoldingEgg { get; private set; }
        public Egg HoldingEgg { get; private set; }
        public bool AllowInteract => !attackController.IsAttacking;
        public static PlayerStats PlayerStats => Instance.currentStats;
        
        [SerializeField] private float health;
        
        public Manager() => Instance = this;

        private void Awake()
        {
            currentStats = baseStats;
            MainMenu.OnResetRequested += OnResetRequested;
        }
        
        private void Start()
        {
            healthbar.SetTarget(this);
            health = currentStats.MaxHealth;
            statsText.text = currentStats.Print(true);
            healthText.text = Mathf.CeilToInt(health).ToString();
            TimeManager.OnDayStart += OnDayStart;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R) && IsHoldingEgg) 
                DropEgg();
            else if(Input.GetKeyDown(KeyCode.K)) 
                Die();
            else if(Input.GetKeyDown(KeyCode.Escape) && Time.timeScale != 0) 
                mainMenu.Pause();
        }

        public void AddStats(PlayerStats stats)
        {
            currentStats.AddStats(baseStats, stats);
            statsText.text = currentStats.Print(true);
        }
        
        public void PickEgg(Egg egg)
        {
            IsHoldingEgg = true;
            HoldingEgg = egg;
            eggSpriteRenderer.enabled = true;
            attackController.enabled = false;
        }

        public void RemoveEgg()
        {
            IsHoldingEgg = false;
            HoldingEgg = null;
            eggSpriteRenderer.enabled = false;
            attackController.enabled = true;
        }
        
        private void DropEgg()
        {
            RemoveEgg();
            var egg = GlobalDefinitions.CreateEggDrop(HoldingEgg).transform;
            egg.position = (Vector3) Movement.Position + transform.up * 0.35f;
            egg.rotation = Quaternion.Euler(0, 0, Movement.Rotation);
        }

        public void AddHealthPercent(float percent) => AddHealth(currentStats.MaxHealth * percent);
        
        public void AddHealth(float amount)
        {
            if(health < 0 || health >= currentStats.MaxHealth) return;
            healingParticles.Play();
            health = Mathf.Clamp(health + amount, health, currentStats.MaxHealth);
            UpdateHealthbar();
        }
        
        public void Damage(float damage)
        {
            health -= PhysicsUtility.CalculateDamage(damage, currentStats.Armor);
            UpdateHealthbar();
            if (health <= float.Epsilon) Die();
        }

        private void UpdateHealthbar()
        {
            healthbar.SetValue(health / currentStats.MaxHealth);
            healthText.text = health.ToString("n2");
        }
        
        public void Die()
        {
            movement.enabled = false;
            spriteAnimator.Play(deadHash);
            DeathCounter.StopCounter();
            TimeManager.OnDayStart -= OnDayStart;
            Interactor.Abort();
            BreedingManager.Instance.Abort();
            attackController.ExpireCombo();
            attackController.enabled = false;
            if(IsHoldingEgg) DropEgg();
            if (RespawnManager.CollectEggBeds() > 0)
                RespawnManager.Respawn();
            else 
                mainMenu.ShowGameOver();
        }

        public void OnRespawn()
        {
            movement.enabled = true;
            attackController.enabled = true;
            StatRecorder.respawns++;
            health = currentStats.MaxHealth;
            UpdateHealthbar();
            TimeManager.OnDayStart += OnDayStart;
        }


        private void OnDestroy()
        {
            OnProviderDestroy?.Invoke();
            TimeManager.OnDayStart -= OnDayStart;
            MainMenu.OnResetRequested -= OnResetRequested;
        }

        private void OnDayStart(int day)
        {
            /*health = currentStats.MaxHealth;
            UpdateHealthbar();*/
        }

        private void OnResetRequested()
        {
            RemoveEgg();
            Movement.Teleport(new Vector2(15f, 15f));
            currentStats = baseStats;
            health = currentStats.MaxHealth;
            statsText.text = currentStats.Print(true);
            healthText.text = Mathf.CeilToInt(health).ToString();
            UpdateHealthbar();
        }


        // IDamageable
        public event IDamageable.DestructionProviderEvent OnProviderDestroy;
        public Transform Transform => transform;
        public float HealthbarOffsetY => healthbarOffsetY;
        public float HealthbarWidth => healthbarWidth;
    }
}