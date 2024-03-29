﻿using System.Collections.Generic;
using System.Linq;
using Definitions;
using GameCycle;
using Gameplay;
using Gameplay.Abilities;
using Genes;
using Player;
using Scriptable;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UI
{
    public class MutationMenu : MonoBehaviour
    {
        private static MutationMenu instance;

        [SerializeField] private RespawnManager respawnManager;
        [SerializeField] private AbilityTooltip tooltip;
        [SerializeField] private Transform currentMutationsTransform;
        [SerializeField] private Transform newMutationsTransform;
        [SerializeField] private BasicAbilityButton basicAbilityButtonPrefab;
        [SerializeField] private MutationButton mutationButtonPrefab;
        [SerializeField] private GeneDisplay geneDisplay;
        [SerializeField] private List<BasicMutation> allMutations = new();
        [SerializeField] private MutationsRerollButton rerollButton;

        [SerializeField] private Egg hatchingEgg = new(TrioGene.Zero, new MutationData());

        private MutationTarget mutationTarget;
        private TrioGene genesLeft;
        private readonly Dictionary<BasicMutation, BasicAbilityButton> basicAbilityButtons = new();
        private Dictionary<BasicMutation, int> current = new();
        private TrioGene rerollCost;
        
        [Header("Debug")]
        public BasicMutation debug_MutationToAdd;
        public int debug_MutationLevelToAdd;
        
        public Egg HatchingEgg => hatchingEgg;



        private MutationMenu() => instance = this;

        public static void Show(MutationTarget target, Egg egg) => instance.ShowNonStatic(target, egg);
        
        private void ShowNonStatic(MutationTarget target, Egg egg)
        {
            Time.timeScale = 0;
            mutationTarget = target;
            tooltip.Clear();
            hatchingEgg = egg;
            genesLeft = egg.Genes;
            geneDisplay.UpdateTrioText(genesLeft);
            current = egg.MutationData.GetAll();
            ShowCurrentMutations();
            UpdateRerollButton();
            CreateMutations();
            
            gameObject.SetActive(true);
        }

        private void CreateMutations()
        {
            Dictionary<BasicMutation, int> variants = new();
            HashSet<BasicMutation> maxed = new();
            foreach (var (basicMutation, lvl) in current)
                if (lvl == 9) maxed.Add(basicMutation);
            var available = LinqUtility.ToHashSet(allMutations);
            available.ExceptWith(maxed);

            int amount = Random.Range(3, 13);
            if (amount > available.Count) amount = available.Count;
            while (amount > 0)
            {
                BasicMutation chosenOne = available.ToArray()[Random.Range(0, available.Count)];
                if (variants.ContainsKey(chosenOne))
                {
                    if(variants.Count == available.Count) break;
                    continue;
                }

                int lvl = 0;
                if (current.ContainsKey(chosenOne)) 
                    lvl = current[chosenOne] + 1;
                variants.Add(chosenOne, lvl);
                amount--;
            }
            
            foreach (var (mutation, lvl) in variants)
            {
                var btn = Instantiate(mutationButtonPrefab, newMutationsTransform);
                btn.SetMutation(mutation, lvl);
                btn.GetComponent<AbilityTooltipProvider>().SetTooltip(tooltip);
                btn.SetAffordable(CanAfford(mutation, lvl));
                btn.OnClick = OnClick;
            }
        }

        private void ShowCurrentMutations()
        {
            foreach (var (mutation, level) in current) 
                CreateBasicAbilityButton(mutation, level);
        }

        private void OnClick(BasicMutation mutation, int level)
        {
            int cost = GlobalDefinitions.GetMutationCost(level);
            
            if (current.ContainsKey(mutation))
            {
                current[mutation] = level;
                basicAbilityButtons[mutation].UpdateLevelText(level);
            }
            else
            {
                CreateBasicAbilityButton(mutation, level);
                current.Add(mutation, level);
            }

            genesLeft.SetGene(mutation.GeneType, genesLeft.GetGene(mutation.GeneType) - cost);
            geneDisplay.UpdateTrioText(genesLeft);
            RefreshAffrodable();
            UpdateRerollButton();
            StatRecorder.timesMutated++;
        }

        private void CreateBasicAbilityButton(BasicMutation mutation, int level)
        {
            var btn = Instantiate(basicAbilityButtonPrefab, currentMutationsTransform);
            btn.SetVisuals(mutation);
            btn.UpdateLevelText(level);
            btn.GetComponent<AbilityTooltipProvider>().SetTooltip(tooltip);
            basicAbilityButtons.Add(mutation, btn);
        }

        private bool CanAfford(BasicMutation mutation, int level) =>
            genesLeft.GetGene(mutation.GeneType) >= GlobalDefinitions.GetMutationCost(level);

        private void RefreshAffrodable()
        {
            foreach (Transform t in newMutationsTransform)
            {
                var btn = t.GetComponent<MutationButton>();
                btn.SetAffordable(CanAfford(btn.Scriptable, btn.Level));
            }
        }

        public void Hatch()
        {
            Egg mutated = new Egg(genesLeft, new MutationData(
                current.ToDictionary(
                    pair => pair.Key, 
                    pair => pair.Value)));
            gameObject.SetActive(false);
            ClearAll();
            if(mutationTarget is MutationTarget.Egg) 
                respawnManager.Respawn(hatchingEgg, mutated);
            else if (mutationTarget is MutationTarget.Player)
            {
                BreedingManager.Instance.SetTrioGene(mutated.Genes);
                AbilityController.UpdateAbilities(mutated);
                Time.timeScale = 1;
            }
        }
        
        public void Reroll()
        {
            genesLeft.SetGene(0, genesLeft.GetGene(0) - rerollCost.GetGene(0));
            genesLeft.SetGene(1, genesLeft.GetGene(1) - rerollCost.GetGene(1));
            genesLeft.SetGene(2, genesLeft.GetGene(2) - rerollCost.GetGene(2));
            geneDisplay.UpdateTrioText(genesLeft);
            foreach (Transform t in newMutationsTransform) 
                Destroy(t.gameObject);
            UpdateRerollButton();
            CreateMutations();
        }
        
        private void UpdateRerollButton()
        {
            rerollCost = genesLeft.AsRerollCost(BreedingManager.Instance.MutationRerollCost);
            rerollButton.SetCost(rerollCost, genesLeft);
        }


        // Utils
        public void ClearAll()
        {
            foreach (Transform t in currentMutationsTransform) Destroy(t.gameObject);
            foreach (Transform t in newMutationsTransform) Destroy(t.gameObject);
            basicAbilityButtons.Clear();
            current.Clear();
        }

        public void ResetEgg()
        {
            genesLeft = TrioGene.Zero;
            hatchingEgg = new Egg(TrioGene.Zero, new MutationData());
        }

        public void Refresh()
        {
            ClearAll();
            ShowNonStatic(mutationTarget, hatchingEgg);
        }

        public void Save()
        {
            hatchingEgg = new Egg(genesLeft, new MutationData(current));
        }
    }

    public enum MutationTarget
    {
        Egg,
        Player
    }
}