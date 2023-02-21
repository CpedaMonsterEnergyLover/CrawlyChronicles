﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TutorialMenu : MonoBehaviour
    {
        [SerializeField] private GameObject rootGO;
        [SerializeField] private Transform stepsTransform;
        [SerializeField] private Text stepText;
        [SerializeField] private GameObject leftArrow;
        [SerializeField] private GameObject rightArrow;
 
        private List<GameObject> steps = new();
        private int currentStep;
        private int stepsAmount;

        private void CollectSteps()
        {
            steps.Clear();
            foreach (Transform t in stepsTransform) 
                steps.Add(t.gameObject);
            stepsAmount = steps.Count;
        }
        
        private void Update()
        {
            var axis = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 
                Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) ? 1 : 0;

            if (axis != 0) SelectTutorialStep(currentStep + axis);
        }

        public void Show()
        {
            enabled = true;
            if(steps.Count == 0) CollectSteps();
            SelectTutorialStep(0);
            rootGO.SetActive(true);
        }

        public void Close()
        {
            enabled = false;
            rootGO.SetActive(false);
        }

        private void SelectTutorialStep(int step)
        {
            step = Mathf.Clamp(step, 0, stepsAmount - 1);
            steps[currentStep].SetActive(false);
            currentStep = step;
            steps[currentStep].SetActive(true);
            leftArrow.SetActive(currentStep > 0);
            rightArrow.SetActive(currentStep < stepsAmount - 1);
            stepText.text = $"{currentStep + 1}/{stepsAmount}";
        }

        public void SelectRight() => SelectTutorialStep(currentStep + 1);
        public void SelectLeft() => SelectTutorialStep(currentStep - 1);

    }
}