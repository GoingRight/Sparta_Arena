using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Particle/ParticlePreset")]
public class ParticlePreset : ScriptableObject
{
    public Gradient colorOverLifetime;
    public GameObject ParticleSystem;
}
