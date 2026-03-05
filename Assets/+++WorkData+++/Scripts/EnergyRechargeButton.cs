using DefaultNamespace;
using UnityEngine;

public class EnergyRechargeButton : MonoBehaviour, IInteractable
{
    [Header("Hold Settings")]
    [SerializeField] private float holdTime = 10f;

    [Header("Target")]
    [SerializeField] private PlayerEnergy playerEnergy;

    private float holdTimer;
    private bool holding;

    public string InteractMessage
    {
        get
        {
            if (playerEnergy == null) return "No player energy set";
            if (!holding) return $"Hold [E] to charge ({holdTime:0}s)";
            return $"Charging... {holdTimer:0.0}/{holdTime:0.0}s";
        }
    }

    public void Interact()
    {
    }

    public void BeginInteract()
    {
        if (playerEnergy == null) return;

        holding = true;
        holdTimer = 0f;
    }

    public void EndInteract()
    {
        holding = false;
        holdTimer = 0f;
    }

    void Update()
    {
        if (!holding) return;
        if (playerEnergy == null) return;

        holdTimer += Time.deltaTime;

        if (holdTimer >= holdTime)
        {
            playerEnergy.RefillFull();
            holding = false;
            holdTimer = 0f;
        }
    }
}