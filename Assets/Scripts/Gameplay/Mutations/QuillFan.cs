﻿using UnityEngine;

namespace Gameplay.Abilities
{
    public class QuillFan : Ability
    {
        [SerializeField] private new ParticleSystem particleSystem;
        [Header("Particles amount")] 
        [SerializeField] private int amountLvl1;
        [SerializeField] private int amountLvl10;
        [Header("Fan duration")] 
        [SerializeField] private float durationLvl1;
        [SerializeField] private float durationLvl10;
        
        public override void OnLevelChanged(int lvl)
        {
            if(particleSystem.isPlaying) particleSystem.Stop();
            var emission = particleSystem.emission;
            var main = particleSystem.main;
            emission.rateOverTime = LerpLevel(amountLvl1, amountLvl10, lvl);
            main.duration = LerpLevel(durationLvl1, durationLvl10, lvl);
        }

        public override void Activate() => particleSystem.Play();
    }
}