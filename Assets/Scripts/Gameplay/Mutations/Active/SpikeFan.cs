﻿using System.Text;
using Gameplay.Enemies;
using UnityEngine;

namespace Gameplay.Abilities.Active
{
    public class SpikeFan : ActiveAbility
    {
        [SerializeField] private new ParticleSystem particleSystem;
        [Header("Particles amount")] 
        [SerializeField] private int amountLvl1;
        [SerializeField] private int amountLvl10;
        [Header("Fan duration")] 
        [SerializeField] private float durationLvl1;
        [SerializeField] private float durationLvl10;
        [Header("Stun and knockback")] 
        [SerializeField, Range(0, 1)] private float stunDuration = 0.5f;
        [SerializeField, Range(0, 10)]  private float knockbackPower = 0.5f;
        
        public override void OnLevelChanged(int lvl)
        {
            base.OnLevelChanged(lvl);
            if(particleSystem.isPlaying) particleSystem.Stop();
            var emission = particleSystem.emission;
            var main = particleSystem.main;
            emission.rateOverTime = LerpLevel(amountLvl1, amountLvl10, lvl);
            main.duration = LerpLevel(durationLvl1, durationLvl10, lvl);
        }
        
        private void OnParticleCollision(GameObject other)
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                enemy.Damage(1, knockbackPower, stunDuration);
            }
        }

        public override void Activate() => particleSystem.Play();
   
        public override string GetLevelDescription(int lvl)
        {
            float dur = LerpLevel(durationLvl1, durationLvl10, lvl);
            float amount = LerpLevel(amountLvl1, amountLvl10, lvl);
            StringBuilder sb = new StringBuilder();
            sb.Append("<color=orange>").Append("Cooldown").Append(": ").Append("</color>").Append(Scriptable.GetCooldown(lvl).ToString("n2")).Append("\n");
            sb.Append("<color=orange>").Append("Duration").Append(": ").Append("</color>").Append(dur.ToString("n2")).Append("\n");
            sb.Append("<color=orange>").Append("Bullets amount").Append(": ").Append("</color>").Append(amount.ToString("n2")).Append("\n");
            sb.Append("<color=orange>").Append("Stun duration").Append(": ").Append("</color>").Append(stunDuration.ToString("n2")).Append("\n");
            sb.Append("<color=orange>").Append("Knockback").Append(": ").Append("</color>").Append(knockbackPower.ToString("n2")).Append("\n");
            return sb.ToString();
        }
    }
}