﻿using System.Collections.Generic;
using Definitions;
using GameCycle;
using Gameplay.AI.Locators;
using Genes;
using Gameplay.Interaction;
using UnityEngine;
using Util;

namespace Gameplay
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EggBed : MonoBehaviour, INotificationProvider, ILocatorTarget, IInteractable
    {
        [SerializeField] private List<Egg> storedEggs = new();
        
        private SpriteRenderer spriteRenderer;

        public int EggsAmount => storedEggs.Count;
        public Egg GetEgg(int index) => storedEggs[index];
        
        
        
        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            RespawnManager.OnEggCollectionRequested += OnEggBedsCollectionRequested;
        }

        private void Start()
        {
            spriteRenderer.sprite = GlobalDefinitions.GetEggsBedSprite(EggsAmount);
            GlobalDefinitions.CreateNotification(this);
            BreedingManager.Instance.AddTotalEggsAmount(EggsAmount);
        }

        private void OnDestroy()
        {
            BreedingManager.Instance.AddTotalEggsAmount(-EggsAmount);
            RespawnManager.OnEggCollectionRequested -= OnEggBedsCollectionRequested;
            OnProviderDestroy?.Invoke();
        }

        public void AddEggs(List<Egg> eggs)
        {
            storedEggs.AddRange(eggs);
            BreedingManager.Instance.AddTotalEggsAmount(eggs.Count);
            UpdateAmount();
        }

        private void AddEgg(Egg egg)
        {
            storedEggs.Add(egg);
            BreedingManager.Instance.AddTotalEggsAmount(1);
            UpdateAmount();
        }

        public bool RemoveOne(out Egg egg)
        {
            egg = null;
            if (EggsAmount <= 0) return false;
            
            egg = storedEggs[Random.Range(0, EggsAmount)];
            storedEggs.Remove(egg);

            if (EggsAmount <= 0)
                Destroy(gameObject);
            else
            {
                UpdateAmount();
                BreedingManager.Instance.AddTotalEggsAmount(-1);
            }

            return true;
        }

        private void UpdateAmount()
        {
            spriteRenderer.sprite = GlobalDefinitions.GetEggsBedSprite(EggsAmount);
            OnDataUpdate?.Invoke();
        }

        private void OnEggBedsCollectionRequested(List<EggBed> eggBeds) => eggBeds.Add(this);


        
        // INotificationProvider 
        public event INotificationProvider.NotificationProviderEvent OnDataUpdate;
        public event INotificationProvider.NotificationProviderEvent OnProviderDestroy;
        public Transform Transform => transform;
        public string NotificationText => EggsAmount.ToString();


        // IInteractable


        public void Interact()
        {
            AddEgg(Player.Manager.Instance.HoldingEgg);
            Player.Manager.Instance.RemoveEgg();
        }

        public bool CanInteract() => Player.Manager.Instance.IsHoldingEgg && EggsAmount < 12;
        public float PopupDistance => 0.75f;
        public string ActionTitle => "Return egg";
        Vector3 IInteractable.Position => transform.position;
    }
}