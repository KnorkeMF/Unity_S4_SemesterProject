using DefaultNamespace;
using UnityEngine;

public class SpawnButton : MonoBehaviour, IInteractable
{
    public string InteractMessage => objectInteractMessage;
    
    [SerializeField] GameObject spawnPrefab;
    
    [SerializeField] string objectInteractMessage;

    void Spawn()
    {
        var spawnedObject = Instantiate(spawnPrefab, transform.position + Vector3.up, Quaternion.identity);
        
        var randomSize =Random.Range(0.0f, 1.0f);
        spawnedObject.transform.localScale = Vector3.one * randomSize;
        
        var randomColour = new Color(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
        spawnedObject.GetComponent<MeshRenderer>().material.color = randomColour;
    }
    public void Interact()
    {
        Spawn();
    }

    public void BeginInteract()
    {
        Spawn();
    }

    public void EndInteract()
    {
    }
}
