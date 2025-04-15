using System.Collections;
using StarterAssets;
using UnityEngine;

public class MonsterTrapAlert : MonoBehaviour
{
    [Header("Trap Kind")]
    [SerializeField]
    private bool isSoundTrap;
    [SerializeField]
    private bool isDamageTrap;
    [SerializeField]
    private bool isSlowTrap;


    [Header("Slow Trap Settings")]
    [SerializeField] private float slowMultiplier = 0.5f;
    [SerializeField] private float slowDuration = 5f;

    [Header("Damage Trap Settings")]
    [SerializeField] private int damage = 10;

    [SerializeField] private DemonTargetAI1 demonTargetAI1;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (isSoundTrap)
            {
                GameObject Enemy = GameObject.FindGameObjectWithTag("Enemy");
                demonTargetAI1 = Enemy.GetComponent<DemonTargetAI1>();
                if (demonTargetAI1 != null)
                {
                    demonTargetAI1.RequestSoundAlert(this.gameObject);
                }
            }
            if(isDamageTrap)
            {
                PlayerHealth playerHealth = other.gameObject.GetComponent<PlayerHealth>();
                if(playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }
            if (isSlowTrap)
            {
                ThirdPersonController thirdPersonController = other.gameObject.GetComponent<ThirdPersonController>();
                thirdPersonController.ApplySlow(slowMultiplier, slowDuration);
            }
        }
    }

    


}
