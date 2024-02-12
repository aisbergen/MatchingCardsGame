using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public AddButtons addButtonsScript;
    public int levels;
    public int current_level;
    [SerializeField] private Sprite bgImage;
    [SerializeField] private Sprite level2Background;
    [SerializeField] private Sprite level3Background;
    [SerializeField] private Sprite level4Background;

    public Sprite[] timeSprites; // Array of time sprites
    private Image timerImage;
    private int currentSpriteIndex = 0;

    public List<Button> btns = new List<Button>();

    public Sprite[] puzzles;
    public List<Sprite> gamePuzzles = new List<Sprite>();
    private bool firstGuess, secondGuess;
    public int countGuesses;
    public int countCorrectGuesses;
    public int gameGuesses;
    public int firstGuessIndex, secondGuessIndex;
    public string firstGuessPuzzle, secondGuessPuzzle;

    public GameObject gameWin_Popup;
    public GameObject gameOverPopup;

    public Button hintButton; // Reference to the Hint button
    private int remainingHints = 3;
    public Text hintsRemainingText;

    public Text timerText; // Reference to the UI Text component to display the timer
    private float timeRemaining = 30f;




    void Start()
    {
        SetLevel(); addButtonsScript.CreateBtns();
        GetButtons();
        AddListeners();
        AddGamePuzzles();
        Shuffle(gamePuzzles);

        timerImage = transform.Find("Time").GetComponent<Image>();

        StartCoroutine(ChangeTimerSprite());




        StartCoroutine(ShowCardsForTwoSeconds());


        hintButton.onClick.AddListener(ShowHint);

    }
    IEnumerator ChangeTimerSprite()
    {
        float timePerSprite = 30f / timeSprites.Length;

        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(timePerSprite);

            if (currentSpriteIndex < timeSprites.Length - 1)
            {
                currentSpriteIndex++;
                timerImage.sprite = timeSprites[currentSpriteIndex];
            }
        }

        // If the time runs out, restart the timer and reset the sprite index
        timeRemaining = 30f;
        currentSpriteIndex = 0;
        timerImage.sprite = timeSprites[currentSpriteIndex];
        StartCoroutine(ChangeTimerSprite());
    }
    void FixedUpdate()
    {
        // Update the timer each frame if the game is running
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            // Game over when the time runs out
            Debug.Log("Time's up!");
            GameOver(); // Trigger game over when time runs out
        }

    }
    void GameOver()
    {
        // Implement your game over logic here
        gameOverPopup.SetActive(true);
        Invoke("Restart_Scene", 1f);
        //Time.timeScale = 0f; // Pause the game if needed
    }

    void GameWin()
    {
        // Implement your game win logic here
        gameWin_Popup.SetActive(true);
        current_level++; PlayerPrefs.SetInt("current_lvl", current_level);
        Invoke("Restart_Scene", 1f); print("win");
        //Time.timeScale = 0f; // Pause the game if needed
    }

    void Update()
    {
        // ... (your existing code)

        // Update the timer each frame if the game is running
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            UpdateTimerDisplay();
        }
        else
        {
            // Game over when the time runs out
            Debug.Log("Time's up!");
            GameOver(); // Trigger game over when time runs out
        }
    }

    void Restart_Scene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    private void UpdateTimerDisplay()
    {
        // Display the time remaining in the UI Text component
        timerText.text = "Time: " + Mathf.RoundToInt(timeRemaining).ToString();
    }


    private void Awake()
    {
        puzzles = Resources.LoadAll<Sprite>("cards");
    }
    void GetButtons()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("puzzleBtn");
        for (int i = 0; i < objects.Length; i++)
        {
            btns.Add(objects[i].GetComponent<Button>());
            btns[i].image.sprite = bgImage;
        }
    }
    void AddGamePuzzles()
    {
        int looper = btns.Count;
        int index = 0;
        for (int i = 0; i < looper; i++)
        {
            if (index == looper / 2)
            {
                index = 0;
            }
            gamePuzzles.Add(puzzles[index]);
            index++;
        }
    }
    void AddListeners()
    {
        foreach (Button btn in btns)
        {
            btn.onClick.AddListener(() => PickPuzzle());
        }
    }

    public void PickPuzzle()
    {
        //string name = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name;

        if (!firstGuess)
        {
            firstGuess = true;
            firstGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);
            firstGuessPuzzle = gamePuzzles[firstGuessIndex].name;

            btns[firstGuessIndex].image.sprite = gamePuzzles[firstGuessIndex];
        }
        else if (!secondGuess)
        {
            secondGuess = true;
            secondGuessIndex = int.Parse(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name);

            secondGuessPuzzle = gamePuzzles[secondGuessIndex].name;

            btns[secondGuessIndex].image.sprite = gamePuzzles[secondGuessIndex];

            if (firstGuessPuzzle == secondGuessPuzzle)
            {
                print("puzzle");
            }
            else
            {
                print("don't match");
            }
            countGuesses++;
            StartCoroutine(checkThePuzzleMatch());
        }
    }
    IEnumerator checkThePuzzleMatch()
    {
        yield return new WaitForSeconds(0.5f);
        if (firstGuessPuzzle == secondGuessPuzzle)
        {

            btns[firstGuessIndex].interactable = false;
            btns[secondGuessIndex].interactable = false;

            btns[firstGuessIndex].image.color = new Color(0, 0, 0, 0);
            btns[secondGuessIndex].image.color = new Color(0, 0, 0, 0);

            CheckTheGameFinished();
        }
        else
        {
            btns[firstGuessIndex].image.sprite = bgImage;
            btns[secondGuessIndex].image.sprite = bgImage;
        }
        yield return new WaitForSeconds(0.5f);
        firstGuess = secondGuess = false;
    }
    IEnumerator ShowCardsForTwoSeconds()
    {
        // Show cards face-up for 2 seconds
        foreach (Button btn in btns)
        {
            int index = int.Parse(btn.name);
            btn.image.sprite = gamePuzzles[index];
        }

        yield return new WaitForSeconds(2f);

        // Flip cards back to the background image
        foreach (Button btn in btns)
        {
            btn.image.sprite = bgImage;
        }

        // Allow player interaction after showing the cards
        foreach (Button btn in btns)
        {
            btn.interactable = true;
        }
        gameGuesses = gamePuzzles.Count / 2;
    }
    void CheckTheGameFinished()
    {
        countCorrectGuesses++;

        if (countCorrectGuesses == gameGuesses)
        {
            print("game finish");
            GameWin();
            print("it took you " + countGuesses + "");
        }
    }
    public void NextBtnClick()
    {
        print("next click");
    }
    public void RetryBtnClick()
    {
        print("retry");
    }
    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
    // New method for handling the hint functionality
    private void ShowHint()
    {
        if (remainingHints > 0)
        {
            StartCoroutine(ShowCardsForOneSecond());
            remainingHints--;
            hintsRemainingText.text = "Hints Remaining: " + remainingHints;
        }
        else
        {
            hintButton.interactable = false;
        }
    }

    private IEnumerator ShowCardsForOneSecond()
    {
        foreach (Button btn in btns)
        {
            int index = int.Parse(btn.name);
            btn.image.sprite = gamePuzzles[index];
        }

        yield return new WaitForSeconds(1f);

        foreach (Button btn in btns)
        {
            btn.image.sprite = bgImage;
        }
    }

    public void SetLevel()
    {
        current_level = PlayerPrefs.GetInt("current_lvl");
        if (current_level <= levels)
        {
            switch (current_level)
            {
                case 0:
                    addButtonsScript.btns_amount = 4;
                    bgImage = level2Background;
                    break;

                case 1:
                    addButtonsScript.btns_amount = 6;
                    bgImage = level3Background;
                    break;

                case 2:
                    addButtonsScript.btns_amount = 8;
                    bgImage = level4Background;
                    break;
            }
        }
        else
        {
            current_level = 0;
            PlayerPrefs.SetInt("current_lvl", current_level);
            Restart_Scene();
        }
    }
}
