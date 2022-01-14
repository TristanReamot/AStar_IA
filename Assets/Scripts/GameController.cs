using UnityEngine;

[RequireComponent(typeof(MazeConstructor))]

public class GameController : MonoBehaviour
{
    [SerializeField] private GameObject player;

    private MazeConstructor generator;

    private bool goalReached;

    public int mazeHeight = 13;
    public int mazeWidth = 15;

    void Start()
    {
        generator = GetComponent<MazeConstructor>();
        StartNewGame();
    }

    private void StartNewGame()
    {
        StartNewMaze();
    }

    private void StartNewMaze()
    {
        generator.GenerateNewMaze(mazeHeight, mazeWidth, OnStartTrigger, OnGoalTrigger);

        float x = generator.startCol * generator.hallWidth;
        float y = 1;
        float z = generator.startRow * generator.hallWidth;
        player.transform.position = new Vector3(x, y, z);

        goalReached = false;
        player.SetActive(true);
    }

    void Update()
    {
        if (!player)
        {
            return;
        }
    }

    void SetStartingTrue()
    {
        GetComponent<FindPathAStar>().starting = true;
    }

    void ClearMap()
    {
        GetComponent<FindPathAStar>().RemoveAllMarkers();
    }

    //Trigger sur l'arrivée qui relance un nouveau labyrinthe 2 sec après la fin
    private void OnGoalTrigger(GameObject trigger, GameObject other)
    {
        if (!goalReached)
        {
            goalReached = true;
            Destroy(trigger);
            Invoke("StartNewMaze", 2);
            Invoke("ClearMap", 2);
            Invoke("SetStartingTrue", 2);
        }       
    }

    private void OnStartTrigger(GameObject trigger, GameObject other)
    {
        //Trigger de départ si besoin.     
    }
}