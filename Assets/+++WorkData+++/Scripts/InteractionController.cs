using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
   [SerializeField] Camera playerCamera;
   [SerializeField] TextMeshProUGUI interactionText;
   [SerializeField] float interactionDistance = 5f;
   
   IInteractable currentTargetInteractable;
   IInteractable lastTargetInteractable;

   public void Update()
   {
      UpdateCurrentInteractable();
      UpdateInteractionText();
      CheckForInteractionInput();
   }

   void UpdateCurrentInteractable()
   {
      var ray = playerCamera.ViewportPointToRay(new Vector2(0.5f, 0.5f));

      if (Physics.Raycast(ray, out var hit, interactionDistance))
         currentTargetInteractable = hit.collider.GetComponent<IInteractable>();
      else
         currentTargetInteractable = null;

      if (lastTargetInteractable != null && lastTargetInteractable != currentTargetInteractable)
         lastTargetInteractable.EndInteract();

      lastTargetInteractable = currentTargetInteractable;
   }

   void UpdateInteractionText()
   {
      if (currentTargetInteractable == null)
      {
         interactionText.text = string.Empty;
         return;
      }

      interactionText.text = currentTargetInteractable.InteractMessage;
   }

   void CheckForInteractionInput()
   {
      if (currentTargetInteractable == null) return;

      if (Keyboard.current.eKey.wasPressedThisFrame)
         currentTargetInteractable.BeginInteract();

      if (Keyboard.current.eKey.isPressed && lastTargetInteractable != currentTargetInteractable)
         currentTargetInteractable.BeginInteract();

      if (Keyboard.current.eKey.wasReleasedThisFrame)
         currentTargetInteractable.EndInteract();
   }
}
