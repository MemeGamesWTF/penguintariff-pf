using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 66;

    public GameObject GameOverScreen, GameWinScreen, InfoScreen, PauseMenuScreen;
    public Button PauseButton;
    public bool GameState = false;
    private bool isPaused = false;

    private ScoreObj Score;

    public Transform Player;
    public Camera mainCamera;
    public GameObject circlePrefab;

    public GameObject[] circlePrefabs;
    public List<GameObject> activeCircles = new List<GameObject>();
    public float respawnHeight = 6f;
    public float fallSpeed = 1f;
    private int totalCircles = 5;
    public float minSpawnHeight = 5f;
    public float maxSpawnHeight = 7f;
    private Dictionary<GameObject, float> circleFallTimers = new Dictionary<GameObject, float>();

    public Text ScoreText;
    private int currentScore;
    public AudioSource tapsound;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ScoreText.text = "0";
        InfoScreen.SetActive(true);
        PauseMenuScreen.SetActive(false);
        PauseButton.onClick.AddListener(TogglePause);
        InitializeCircles();
    }

    void Update()
    {
        if (!GameState && !isPaused)
            return;

        if (!GameState)
            return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            HandleInput(touch.position);
        }
        else if (Input.GetMouseButton(0))
        {
            HandleInput(Input.mousePosition);
        }

        for (int i = 0; i < activeCircles.Count; i++)
        {
            GameObject circle = activeCircles[i];
            if (circle != null)
            {
                if (circleFallTimers.ContainsKey(circle) && Time.time < circleFallTimers[circle])
                    continue;

                circle.transform.position += Vector3.down * fallSpeed * Time.deltaTime;

                if (circle.transform.position.y < -5f)
                {
                    StartCoroutine(ResetCircleWithDelay(circle));
                }
            }
        }
    }

    private void HandleInput(Vector3 inputPosition)
    {
        // Play tap sound when input is detected
        if (tapsound != null)
        {
            tapsound.Play();
        }
        else
        {
            Debug.LogWarning("Tap sound is not assigned in the GameManager!");
        }

        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(inputPosition.x, inputPosition.y, mainCamera.nearClipPlane));
        Player.position = Vector3.Lerp(
            Player.position,
            new Vector3(Mathf.Clamp(worldPosition.x, -1.9f, 1.9f), Player.position.y, Player.position.z),
            10f * Time.deltaTime
        );
    }

    public void AddScore()
    {
        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += 10;
            ScoreText.text = Score.score.ToString();
            Score.score += 10;
        }
        else
        {
            ScoreText.text = "0";
        }
    }

    public void GameWin()
    {
        GameState = false;
        GameWinScreen.SetActive(true);
        SendScore((int)Score.score, 66);
    }

    public void GameOVer()
    {
        GameState = false;
        GameOverScreen.SetActive(true);
        Debug.Log(Score.score);
        SendScore((int)Score.score, 111);
    }

    public void GameResetScreen()
    {
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        PauseMenuScreen.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        GameState = true;
        Player.position = new Vector3(0f, Player.position.y, Player.position.z);

        foreach (var circle in activeCircles)
        {
            Destroy(circle);
        }
        activeCircles.Clear();

        InitializeCircles();
        Debug.Log("Game Restarted!");
    }

    public void AddScore(float f)
    {
        Score.score += f;
    }

    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        return SpawnBounds.transform.TransformPoint(randomPoint);
    }

    private void InitializeCircles()
    {
        if (circlePrefabs.Length == 0)
        {
            Debug.LogWarning("No Circle Prefabs Assigned!");
            return;
        }

        foreach (GameObject circle in activeCircles)
        {
            if (circle != null)
            {
                Destroy(circle);
            }
        }
        activeCircles.Clear();

        for (int i = 0; i < totalCircles; i++)
        {
            bool uniqueCircleFound = false;
            GameObject newCircle = null;

            for (int attempts = 0; attempts < 10; attempts++)
            {
                float randomX = Random.Range(-2f, 2f);
                float randomY = Random.Range(minSpawnHeight, maxSpawnHeight);
                Vector2 spawnPosition = new Vector2(randomX, randomY);

                bool positionOccupied = false;
                foreach (GameObject circle in activeCircles)
                {
                    if (Vector2.Distance(circle.transform.position, spawnPosition) < 0.5f)
                    {
                        positionOccupied = true;
                        break;
                    }
                }

                if (positionOccupied)
                    continue;

                int randomIndex = Random.Range(0, circlePrefabs.Length);
                GameObject selectedPrefab = circlePrefabs[randomIndex];

                bool prefabExists = false;
                foreach (GameObject circle in activeCircles)
                {
                    if (circle.name.Contains(selectedPrefab.name))
                    {
                        prefabExists = true;
                        break;
                    }
                }

                if (!prefabExists)
                {
                    newCircle = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);
                    uniqueCircleFound = true;
                    break;
                }
            }

            if (uniqueCircleFound && newCircle != null)
            {
                activeCircles.Add(newCircle);
                circleFallTimers[newCircle] = Time.time + Random.Range(0f, 6f);
            }
            else
            {
                Debug.LogWarning("Could not find a unique circle after multiple attempts.");
            }
        }
    }

    private IEnumerator ResetCircleWithDelay(GameObject circle)
    {
        float delay = Random.Range(0f, 0.1f);
        yield return new WaitForSeconds(delay);

        Vector2 newPosition = Vector2.zero;
        bool positionValid = false;

        while (!positionValid)
        {
            float randomX = Random.Range(-2f, 2f);
            float randomY = Random.Range(minSpawnHeight, maxSpawnHeight);
            newPosition = new Vector2(randomX, randomY + 0.5f);

            Collider2D overlap = Physics2D.OverlapCircle(newPosition, 0.5f);
            if (overlap == null)
            {
                positionValid = true;
            }
        }

        circle.transform.position = newPosition;
        circleFallTimers[circle] = Time.time + Random.Range(0f, 6f);
    }

    public void RestartGame()
    {
        Score.score = 0;
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        PauseMenuScreen.SetActive(false);
        isPaused = false;
        Time.timeScale = 1f;
        
        Player.position = new Vector3(0f, Player.position.y, Player.position.z);

        foreach (var circle in activeCircles)
        {
            Destroy(circle);
        }
        activeCircles.Clear();

        InitializeCircles();
        GameState = true;
        Debug.Log("Game Restarted!");
    }

    // Pause Menu Methods
    public void PauseGame()
    {
        if (!GameState || GameOverScreen.activeSelf || GameWinScreen.activeSelf) return;
        
        isPaused = true;
        GameState = false;
        Time.timeScale = 0f;
        PauseMenuScreen.SetActive(true);
        // PauseButton.gameObject.SetActive(false);
    }

    public void ResumeGame()
    {
        isPaused = false;
        GameState = true;
        Time.timeScale = 1f;
        PauseMenuScreen.SetActive(false);
        // PauseButton.gameObject.SetActive(true);
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public struct ScoreObj
    {
        public float score;
    }
}