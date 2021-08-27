using UnityEngine;
using Mirror;
using PropHunt.Game.Communication;
using System;
using PropHunt.Environment.Sound;
using UnityEngine.SceneManagement;
using System.Collections;
using PropHunt.Utils;
using PropHunt.Game.Level;

namespace PropHunt.Game.Flow
{
    public class GameSceneManager : NetworkBehaviour
    {
        [Header("Scene Management")]

        [Scene]
        public string lobbyScene;

        public string gameScene { get; private set; }

        public GameLevelLibrary levelLibrary;

        public INetworkService newtworkService;

        private GameTimer gamePhaseTimer;

        private static bool loaded = false;

        /// <summary>
        /// Buffer time between setup and in game phases
        /// </summary>
        public float bufferTime = 1.0f;

        /// <summary>
        /// Buffer time that has elapsed so far
        /// </summary>
        private float bufferElapsed = 0.0f;

        public void Start()
        {
            newtworkService = new NetworkService(this);
            if (string.IsNullOrEmpty(gameScene))
            {
                gameScene = levelLibrary.DefaultLevel.levelName;
            }
        }

        public void ChangeScene(string newScene)
        {
            gameScene = newScene;
        }

        public override void OnStartServer()
        {
            GameManager.OnGamePhaseChange += HandleGamePhaseChange;
            GameManager.ChangePhase(GamePhase.Reset);
        }

        public override void OnStopServer()
        {
            GameManager.OnGamePhaseChange -= HandleGamePhaseChange;
            GameManager.ChangePhase(GamePhase.Disabled);
        }

        /// <summary>
        /// Stop and destory the current game phase timer
        /// </summary>
        public void ClearTimer()
        {
            if (this.gamePhaseTimer != null)
            {
                this.gamePhaseTimer.StopTimer();
                NetworkServer.Destroy(this.gamePhaseTimer.gameObject);
                this.gamePhaseTimer = null;
            }
        }

        /// <summary>
        /// Clear out previous timer and create a new one
        /// </summary>
        public void SetupTimer()
        {
            ClearTimer();
            this.gamePhaseTimer = GameObject.Instantiate(CustomNetworkManager.Instance.timerPrefab);
            NetworkServer.Spawn(this.gamePhaseTimer.gameObject);
        }

        public void HandleGamePhaseChange(object sender, GamePhaseChange change)
        {
            // Cleanpu from previous game state
            switch (change.previous)
            {
                // Do things differently based on the new phase
                case GamePhase.Lobby:
                    break;
                case GamePhase.Setup:
                    break;
                case GamePhase.InGame:
                    ClearTimer();
                    break;
                case GamePhase.Score:
                    ClearTimer();
                    break;
                case GamePhase.Reset:
                    break;
            }
            // Handle whenever the game state changes
            switch (change.next)
            {
                // Do things differently based on the new phase
                case GamePhase.Lobby:
                    break;
                case GamePhase.Setup:
                    bufferElapsed = 0.0f;
                    NetworkServer.SetAllClientsNotReady();
                    CustomNetworkManager.Instance.ServerChangeScene(gameScene);
                    // Once loading is complete, go to InGame
                    break;
                case GamePhase.InGame:
                    // Setup a timer for the game
                    SetupTimer();
                    // Transition to score phase upon completion of timer
                    this.gamePhaseTimer.OnFinish += (source, args) => GameManager.ChangePhase(GamePhase.Score);
                    // Start the timer with a time of 5 minutes
                    this.gamePhaseTimer.StartTimer(60 * 5);
                    break;
                case GamePhase.Score:
                    // Setup a timer for the score phase
                    SetupTimer();
                    // Transition to reset phase upon completion of timer
                    this.gamePhaseTimer.OnFinish += (source, args) => GameManager.ChangePhase(GamePhase.Reset);
                    // Start the timer with a time of 30 seconds
                    this.gamePhaseTimer.StartTimer(30);
                    break;
                case GamePhase.Reset:
                    // Start loading the lobby scene
                    CustomNetworkManager.Instance.ServerChangeScene(lobbyScene);
                    // Once laoding is complete, go to lobby
                    break;
            }
        }

        public void Update()
        {
            // Only run this on server
            if (!newtworkService.activeNetworkServer)
            {
                return;
            }
            switch (GameManager.gamePhase)
            {
                case GamePhase.Setup:
                    bool allReady = true;
                    foreach (NetworkConnection client in NetworkServer.connections.Values)
                    {
                        if (!client.isReady)
                        {
                            allReady = false;
                        }
                    }
                    if (allReady)
                    {
                        bufferElapsed += Time.deltaTime;
                    }
                    // As soon as scene is loaded, move to in game
                    if (bufferElapsed >= bufferTime &&
                        allReady &&
                        (NetworkManager.loadingSceneAsync == null || NetworkManager.loadingSceneAsync.isDone))
                    {
                        GameManager.ChangePhase(GamePhase.InGame);
                    }
                    break;
                case GamePhase.Reset:
                    // As soon as scene is loaded, move to in game
                    if (NetworkManager.loadingSceneAsync == null || NetworkManager.loadingSceneAsync.isDone)
                    {
                        GameManager.ChangePhase(GamePhase.Lobby);
                    }
                    break;
            }
        }
    }
}
