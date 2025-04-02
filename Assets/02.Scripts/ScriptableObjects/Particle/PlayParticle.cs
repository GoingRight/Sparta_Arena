using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayParticle : MonoBehaviour
{
    [Header("ParticleSystem")]
    public ParticleSystem particle;
    public bool isPlaying = false;
    public float delay = 5;

    [Header("Particle 초기 세팅")]
    public float duration = 0.6f;
    public Gradient colorOverLifetime;


    float timer = 0f;
    bool firstPlayDone = false;
    Vector3 prevPosition = Vector3.zero;

    private void Start()
    {
        ParticleSetting();
        prevPosition = transform.position;
    }


    void Update()
    {
        Vector3 moveDir = (transform.position - prevPosition).normalized;
        isPlaying = (transform.position - prevPosition).magnitude >= 0.01f;
        prevPosition = transform.position;

        if (isPlaying)
        {
            if (!firstPlayDone)
            {
                particle.Play();
                //SetParticleDirection(moveDir);
                firstPlayDone = true;
                timer = 0f;
                Debug.Log("Particle1");
            }
            else
            {
                timer += Time.deltaTime;
                if (timer >= delay)
                {
                    //SetParticleDirection(moveDir);
                    particle.Play();
                    timer = 0f;
                    Debug.Log("Particle2");
                }
            }
        }
        else
        {
            firstPlayDone = false;
            timer = 0f;
        }
    }

    void SetParticleDirection(Vector3 direction)
    {

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        particle.Play();
    }

    /// <summary>
    /// 파티클 기본 세팅
    /// </summary>
    void ParticleSetting()
    {
        particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = particle.main;
        main.duration = duration;

        var col = particle.colorOverLifetime;
        col.enabled = true;
        col.color = colorOverLifetime;
    }
}
