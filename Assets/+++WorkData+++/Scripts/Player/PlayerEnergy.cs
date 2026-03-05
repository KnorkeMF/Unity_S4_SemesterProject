using UnityEngine;

public class PlayerEnergy : MonoBehaviour
{
    [Header("Energie Settings")]
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energy;
    [SerializeField] private float drainPerSecond =2f;
    
    public float Energy01 => maxEnergy <= 0f ? 0f : energy/maxEnergy;
    public bool isEmpty => energy <= 0.001f;

    void Awake()
    {
        energy = maxEnergy;
    }

    void Update()
    {
        energy -= drainPerSecond * Time.deltaTime;
        if (energy < 0f) energy = 0f;
    }

    public void RefillFull()
    {
        energy = maxEnergy;
    }

    public void AddEnergy(float amount)
    {
        energy = Mathf.Clamp(energy + amount, 0f, maxEnergy);
    }
   
}
