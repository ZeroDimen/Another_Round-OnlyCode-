using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    [SerializeField] private Transform[] vfxPos;
    [SerializeField] private TextMeshProUGUI particleTitle;
    [SerializeField] private TextMeshProUGUI particleName;
    
    
    [SerializeField] GameObject[] particlePrefab;
    private List<GameObject> particlepool = new List<GameObject>();

    private int currentParticleIndex = 0;
    private int maxParticleIndex;
    
    private GameObject currentParticle;
    private void Start()
    {
        maxParticleIndex = particlePrefab.Length - 1;
        foreach (var particle in particlePrefab)
        {
            particle.gameObject.layer = 6;
        }
        
        foreach (var pos in vfxPos)
        {
            currentParticle = Instantiate(particlePrefab[0], pos);
            particlepool.Add(currentParticle);
        }
        NameVFX();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SetVFX(true);
        }
        else if (Input.GetKeyDown(KeyCode.T))
        {
            SetVFX(false);
        }
    }

    private void SetVFX(bool up)
    {
        if (up)
        {
            currentParticleIndex++;
            if (currentParticleIndex >= maxParticleIndex)
            {
                currentParticleIndex = 0;
            }
        }
        else
        {
            currentParticleIndex--;
            if (currentParticleIndex < 0)
            {
                currentParticleIndex = maxParticleIndex;
            }
        }

        foreach (var particle in particlepool)
        {
            Destroy(particle);
        }
        particlepool.Clear();
        foreach (var pos in vfxPos)
        {
            currentParticle = Instantiate(particlePrefab[currentParticleIndex], pos);
            particlepool.Add(currentParticle);
        }
        NameVFX();

    }

    private void NameVFX()
    {
        if (currentParticleIndex <= 16)
        {
            particleTitle.text = "AOE Magic Spells";
        }
        else if (currentParticleIndex <= 31)
        {
            particleTitle.text = "Magic Circles";
        }
        else if (currentParticleIndex <= 39)
        {
            particleTitle.text = "Map track markers VFX";
        }
        else if (currentParticleIndex <= 62)
        {
            particleTitle.text = "RPG VFX Bundle";
        }
        else if (currentParticleIndex <= 191)
        {
            particleTitle.text = "Elemental VFX Mega Bundle";
        }
        else if (currentParticleIndex <= 254)
        {
            particleTitle.text = "The Victory Stars FX Vol. 3";
        }

        particleName.text = $"{currentParticleIndex} : {particlePrefab[currentParticleIndex].name}";
    }
}
