﻿using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Genes
{
    [Serializable]
    public struct TrioGene
    {
        [SerializeField] private int[] trio;

        public int GetGene(GeneType geneType) => trio[(int) geneType];
        public int GetGene(int geneType) => trio[geneType];
        public void SetGene(GeneType geneType, int amount) => trio[(int) geneType] = amount;
        public void AddGene(GeneType geneType) => trio[(int) geneType]++;
        public void AddGene(GeneType geneType, int amount) => trio[(int) geneType] += amount;
        
        public int Aggressive
        {
            get => trio[0];
            set => trio[0] = value;
        }

        public int Defensive
        {
            get => trio[1];
            set => trio[1] = value;
        }

        public int Universal
        {
            get => trio[2];
            set => trio[2] = value;
        }

        public TrioGene(int aggressive, int defensive, int universal)
        {
            trio = new[]
            {
                Math.Abs(aggressive),
                Math.Abs(defensive),
                Math.Abs(universal)
            };
        }
        
        public TrioGene Randomize(int entropy)
        {
            return new TrioGene(
                Random.Range(Aggressive - entropy, Aggressive + entropy + 1),
                Random.Range(Defensive - entropy, Defensive + entropy + 1),
                Random.Range(Universal - entropy, Universal + entropy + 1));
        }

        public static TrioGene Median(TrioGene first, TrioGene second)
        {
            return new TrioGene(
                first.Aggressive + second.Aggressive / 2,
                first.Defensive + second.Defensive / 2,
                first.Universal + second.Universal / 2);
        }

        public static TrioGene Zero => new(0, 0, 0);

        public static TrioGene One => new(1, 1, 1);
    }
}