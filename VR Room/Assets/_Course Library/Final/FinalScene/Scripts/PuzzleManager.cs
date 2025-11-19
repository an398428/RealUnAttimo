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

    [Header("Wall to Disable")]
    [SerializeField] private GameObject wall;

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
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks
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
        if (wall != null)
        {
            // Play effects if assigned
            if (successParticles != null)
                successParticles.Play();

            if (successSound != null)
                successSound.Play();

            // Disable the wall
            wall.SetActive(false);
        }
    }
}
