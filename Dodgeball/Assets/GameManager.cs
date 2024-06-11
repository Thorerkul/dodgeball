using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public GameObject referenceBall;
    public GameObject playerPrefab; // Reference to the player prefab
    public Transform[] spawnPoints; // Array of spawn points
    private Dictionary<Gamepad, bool> spawnedGamepads = new Dictionary<Gamepad, bool>(); // Dictionary to track spawned gamepads
    private int playerCount = 0; // Counter to keep track of the number of players

    public int timeBeforeBallSpawns;
    public bool hasSpawnedBall = false;
    public bool canSpawnedBall = false;
    public int playersNeeded;
    private float timePlayersJoined;
    public Transform[] ballSpawnPoints;

    private void Start()
    {
        // Enable unpaired device activity listening
        ++InputUser.listenForUnpairedDeviceActivity;
    }

    private void Update()
    {
        if (playerCount >= playersNeeded && !hasSpawnedBall)
        {
            timePlayersJoined = Time.time;
            canSpawnedBall = true;
        }

        if (canSpawnedBall && !hasSpawnedBall && (timePlayersJoined + timeBeforeBallSpawns) <= Time.time)
        {
            for (int i = 0; i < ballSpawnPoints.Length; i++)
            {
                GameObject newball = Instantiate(referenceBall);
                newball.transform.position = ballSpawnPoints[i].transform.position;
            }
            canSpawnedBall = false;
            hasSpawnedBall = true;
        }
    }

    public void OnButtonPress(InputAction.CallbackContext context)
    {
        // Check if the A button (South button) was pressed this frame
        if (context.performed && playerCount < 4)
        {
            var currentGamepad = Gamepad.current;
            // Check if the current gamepad has not been used to spawn a player yet
            if (!spawnedGamepads.ContainsKey(currentGamepad) || !spawnedGamepads[currentGamepad])
            {
                // Spawn a player and pair it with the gamepad
                SpawnPlayer(currentGamepad);
                // Mark the current gamepad as used to spawn a player
                spawnedGamepads[currentGamepad] = true;
            }
        }
    }

    private void SpawnPlayer(Gamepad gamepad)
    {
        // Instantiate the player prefab at the next spawn point
        var player = PlayerInput.Instantiate(playerPrefab, controlScheme: "Gamepad", pairWithDevice: gamepad);
        player.transform.position = spawnPoints[playerCount % spawnPoints.Length].position;
        if (playerCount % 2 == 0)
        {
            player.GetComponent<PlayerController>().isOnRedTeam = true;
        }
        else
        {
            player.GetComponent<PlayerController>().isOnRedTeam = false;
        }
        playerCount++;
    }
}
