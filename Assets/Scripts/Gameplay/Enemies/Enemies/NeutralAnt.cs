﻿using System.Collections;
using Definitions;
using Gameplay.AI;
using Gameplay.Food;
using Genes;
using Gameplay.Interaction;
using Timeline;
using UnityEngine;
using Util;

namespace Gameplay.Enemies
{
    public class NeutralAnt : Enemy, IContinuouslyInteractable
    {
        [SerializeField] private ParticleSystem breedingParticles;
        [SerializeField] private Animator breedAnimator;

        private delegate void NeutralAntEvent(Vector2 position);
        private static event NeutralAntEvent OnNeutralDamaged;

        private static readonly int BreedAnimHash = Animator.StringToHash("NeutralAntBodyBreeding");
        private static readonly int IdleAnimHash = Animator.StringToHash("NeutralAntBodyIdle");

        private Coroutine interestRoutine;
        private bool hungry = true;
        private bool aggressive;
        public bool CanBreed { get; set; } = true;

        [field:SerializeField] public TrioGene TrioGene { get; private set; } = TrioGene.Zero;

        
        
        protected override void Start()
        {
            SubEvents();
            int entropy = GlobalDefinitions.BreedingPartnersGeneEntropy;
            TrioGene = BreedingManager.Instance.TrioGene.Randomize(entropy);
            TrioGene.AddGene((GeneType) Random.Range(0,3), Random.Range(2 * entropy, 4 * entropy));
            
            base.Start();
        }

        public override void OnMapEntered()
        {
            stateController.SetState(AIState.Wander);
        }

        public override void OnPlayerLocated()
        {
            if (aggressive)
                AttackPlayer();
            else
            {
                StopInterest();
                if(TimeManager.IsDay && CanBreed) interestRoutine = StartCoroutine(InterestRoutine());
            }
        }

        public override void OnEggsLocated(EggBed eggBed)
        {
        }

        protected override void OnStunEnd()
        {
            if(!TimeManager.IsDay) OnNightStart(0);
        }

        public override void OnFoodLocated(FoodBed foodBed)
        {
            if (!hungry || foodBed is RadioactiveFungi) return;
            stateController.SetState(AIState.Follow, 
                followTarget: foodBed,
                () => {
                    GlobalDefinitions.CreateRandomGeneDrop(transform.position);
                    foodBed.Eat();
                    hungry = false;
                    StartCoroutine(HungerRoutine());
                    stateController.SetState(AIState.Wander);
                });
        }

        protected override void OnDamageTaken()
        {
            OnNeutralDamaged?.Invoke(rb.position);
        }

        public void Interact()
        {
            BreedingManager.Instance.OpenBreedingMenu(this);
            CanBreed = false;
        }

        private IEnumerator InterestRoutine()
        {
            stateController.TakeMoveControl();

            float t = 0;
            while (t < 2f)
            {
                rb.RotateTowardsPosition(Player.Movement.Position, 5);
                t += Time.deltaTime;
                yield return null;
            }
            
            stateController.ReturnMoveControl();
            interestRoutine = null;
            stateController.SetState(AIState.Wander);
        }

        private void StopInterest()
        {
            if(interestRoutine is not null) StopCoroutine(interestRoutine);
            stateController.ReturnMoveControl();
        }

        private IEnumerator HungerRoutine()
        {
            yield return new WaitForSeconds(10);
            hungry = true;
        }

        private void OnNightStart(int day)
        {
            StopInterest();
            stateController.SetEtherial(true);
            CanBreed = false;
            stateController.SetState(AIState.Flee);
        }

        private void OnNeutralDamage(Vector2 pos)
        {
            if(Vector2.Distance(rb.position, pos) > 7.5f || stateController.CurrentState == AIState.Enter) return;
            StopInterest();
            aggressive = true;
            minimapIcon.color = Color.red;
            CanBreed = false;
            AttackPlayer();
            OnNeutralDamaged -= OnNeutralDamage;
        }
        
        private void SubEvents()
        {
            OnNeutralDamaged += OnNeutralDamage;
            TimeManager.OnNightStart += OnNightStart;
        }

        private void UnsubEvents()
        {
            OnNeutralDamaged -= OnNeutralDamage;
            TimeManager.OnNightStart -= OnNightStart;
        }

        protected override void OnDestroy()
        {
            UnsubEvents();
            base.OnDestroy();
        }


        // IInteractable
        public bool CanInteract() => CanBreed && BreedingManager.Instance.CanBreed;

        public void OnInteractionStart()
        {
            UnsubEvents();
            stateController.SetState(AIState.None);
            StopInterest();
            rb.RotateTowardsPosition(Player.Movement.Position, 360);
            breedAnimator.Play(BreedAnimHash);
            breedingParticles.Play();
            BreedingManager.Instance.PlayBreedingAnimation();
        }

        public void OnInteractionStop()
        {
            breedAnimator.Play(IdleAnimHash);
            stateController.SetState(AIState.Wander);
            breedingParticles.Stop();
            BreedingManager.Instance.PlayIdleAnimation();
            if(TimeManager.IsDay) SubEvents();
            else OnNightStart(0);
            if(!CanBreed) stateController.SetState(AIState.Flee);
        }

        public float InteractionTime => 3f;
        public float PopupDistance => 0.75f;
        public string ActionTitle => "Breed";
        Vector3 IInteractable.Position => transform.position;
    }
}