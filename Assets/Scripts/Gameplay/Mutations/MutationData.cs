﻿using System.Collections.Generic;
using System.Linq;
using Scriptable;

namespace Gameplay.Abilities
{
    public class MutationData
    {
        private readonly Dictionary<BasicMutation, int> mutations = new();

        public void Add(BasicMutation mutation, int lvl) => mutations.Add(mutation, lvl);
        public Dictionary<BasicMutation, int> GetAll() => mutations;

        public MutationData Randomize() 
            => new(mutations.ToDictionary(pair => pair.Key, pair => pair.Value));

        public MutationData(Dictionary<BasicMutation, int> mutations) => this.mutations = mutations;

        public MutationData()
        {
        }
    }
}