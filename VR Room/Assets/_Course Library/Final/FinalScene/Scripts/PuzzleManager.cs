using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SocketPuzzleManager : MonoBehaviour
{
    [Header("Socket References")]
    [SerializeField] private XRSocketInteractor mushroomSocket;
    [SerializeField] private XRSocketInteractor crystalSocket;
    [SerializeField] private XRSocketInteractor flowerSocket;

    [Header("Objects to Disable on Win")]
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject planeToDisable;

    [Header("Objects to Enable on Win")]
    [SerializeField] private GameObject teleportAreaToEnable;
    [SerializeField] private GameObject wizardCharacter; // <--- NEW: Drag your Wizard here

    [Header("Optional Effects")]
    [SerializeField] private ParticleSystem successParticles;
    [SerializeField] private AudioSource successSound;
    [SerializeField] private float wallDisappearDelay = 0.5f;

    private bool mushroomPlaced = false;
    private bool crystalPlaced = false;
    private bool flowerPlaced = false;
    private bool puzzleCompleted = false;

    private void Start()
    {
        // Subscribe to socket events
        if (mushroomSocket != null)
        {
            mushroomSocket.selectEntered.AddListener(OnMushroomPlaced);
            mushroomSocket.selectExited.AddListener(OnMushroomRemoved);
        }

        if (crystalSocket != null)
        {
            crystalSocket.selectEntered.AddListener(OnCrystalPlaced);
            crystalSocket.selectExited.AddListener(OnCrystalRemoved);
        }

        if (flowerSocket != null)
        {
            flowerSocket.selectEntered.AddListener(OnFlowerPlaced);
            flowerSocket.selectExited.AddListener(OnFlowerRemoved);
        }
        
        // SAFETY CHECK: Ensure objects intended to appear later are hidden at start
        if (teleportAreaToEnable != null)
        {
            teleportAreaToEnable.SetActive(false);
        }

        if (wizardCharacter != null) // <--- NEW: Ensure Wizard is hidden at start
        {
            wizardCharacter.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (mushroomSocket != null)
        {
            mushroomSocket.selectEntered.RemoveListener(OnMushroomPlaced);
            mushroomSocket.selectExited.RemoveListener(OnMushroomRemoved);
        }

        if (crystalSocket != null)
        {
            crystalSocket.selectEntered.RemoveListener(OnCrystalPlaced);
            crystalSocket.selectExited.RemoveListener(OnCrystalRemoved);
        }

        if (flowerSocket != null)
        {
            flowerSocket.selectEntered.RemoveListener(OnFlowerPlaced);
            flowerSocket.selectExited.RemoveListener(OnFlowerRemoved);
        }
    }

    private void OnMushroomPlaced(SelectEnterEventArgs args)
    {
        mushroomPlaced = true;
        CheckPuzzleCompletion();
    }

    private void OnMushroomRemoved(SelectExitEventArgs args)
    {
        mushroomPlaced = false;
    }

    private void OnCrystalPlaced(SelectEnterEventArgs args)
    {
        crystalPlaced = true;
        CheckPuzzleCompletion();
    }

    private void OnCrystalRemoved(SelectExitEventArgs args)
    {
        crystalPlaced = false;
    }

    private void OnFlowerPlaced(SelectEnterEventArgs args)
    {
        flowerPlaced = true;
        CheckPuzzleCompletion();
    }

    private void OnFlowerRemoved(SelectExitEventArgs args)
    {
        flowerPlaced = false;
    }

    private void CheckPuzzleCompletion()
    {
        if (!puzzleCompleted && mushroomPlaced && crystalPlaced && flowerPlaced)
        {
            puzzleCompleted = true;
            Invoke(nameof(DisableWall), wallDisappearDelay);
        }
    }

    private void DisableWall()
    {
        // Play effects if assigned
        if (successParticles != null)
            successParticles.Play();

        if (successSound != null)
            successSound.Play();

        // 1. Disable the wall
        if (wall != null)
        {
            wall.SetActive(false);
        }

        // 2. Disable the extra plane
        if (planeToDisable != null)
        {
            planeToDisable.SetActive(false);
        }

        // 3. Enable the Teleport Area
        if (teleportAreaToEnable != null)
        {
            teleportAreaToEnable.SetActive(true);
        }

        // 4. Enable the Wizard
        if (wizardCharacter != null) // <--- NEW: Activate the Wizard
        {
            wizardCharacter.SetActive(true);
        }
    }
}