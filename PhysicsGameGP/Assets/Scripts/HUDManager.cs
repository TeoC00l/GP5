using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI currentAbilityText;
    [SerializeField] private TextMeshProUGUI nextAbilityText;
    private void Awake()
    {
        
    }

    private void Update()
    {
        Queue<Abilities> abilitiesQueue = new Queue<Abilities>(playerController.GetAbilityQueue());
        string abilityText = abilitiesQueue.Dequeue().ToString();
        currentAbilityText.text = abilityText;
        
        abilityText = abilitiesQueue.Dequeue().ToString();
        nextAbilityText.text = abilityText;
    }
}
