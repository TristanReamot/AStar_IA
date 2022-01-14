using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using System.Linq;

public class PathMarker             //Les étapes du A-Star
{
    public MapLocation location;
    public GameObject marker;
    public PathMarker parent;

    public float G;         //Coût à partir du départ
    public float H;         //Coût jusqu'à l'arrivée
    public float F;         //G+H

    public PathMarker(MapLocation l, float g, float h, float f, GameObject marker, PathMarker p)
    {
        location = l;
        G = g;
        H = h;
        F = f;
        this.marker = marker;
        parent = p;
    }

    public override bool Equals(object obj)
    {
        if ((obj == null) || !this.GetType().Equals(obj.GetType()))
        {
            return false;
        }
        else
        {
            return location.Equals(((PathMarker)obj).location);
        }
    }

    public override int GetHashCode()
    {
        return 0;
    }
}

public class FindPathAStar : MonoBehaviour
{
    public MazeConstructor maze;
    public Material closedMaterial;
    public Material openMaterial;
    public GameObject introduce;
    public GameObject restart;
    public GameObject start;
    public GameObject end;
    public GameObject pathP;
    public GameObject IA;

    public bool starting = true;

    private ArrayList path = new ArrayList();

    static Vector3 currentPositionHolder;

    List<PathMarker> closed = new List<PathMarker>();
    List<PathMarker> open = new List<PathMarker>();

    PathMarker startNode;
    PathMarker goalNode;
    PathMarker lastPos;

    bool found = false;
    bool done = false;
    bool run = false;

    float timer;
    int currentNode;

    public void RemoveAllMarkers()
    {
        GameObject[] markers = GameObject.FindGameObjectsWithTag("marker");
        foreach (GameObject m in markers)
        {
            Destroy(m);
        }
    }

    //Configure les points de départ et d'arrivée, et démarre le A* par le point initial
    void BeginSearch()
    {
        starting = false;
        RemoveAllMarkers();
        path.Clear();

        Vector3 startLocation = new Vector3(maze.startCol * maze.hallWidth, 0, maze.startRow * maze.hallWidth);
        startNode = new PathMarker(new MapLocation(maze.startCol, maze.startRow), 0, 0, 0, Instantiate(start, startLocation, Quaternion.identity), null);

        Vector3 goalLocation = new Vector3(maze.goalCol * maze.hallWidth, 0, maze.goalRow * maze.hallWidth);
        goalNode = new PathMarker(new MapLocation(maze.goalCol, maze.goalRow), 0, 0, 0, Instantiate(end, goalLocation, Quaternion.identity), null);

        open.Clear();
        closed.Clear();

        open.Add(startNode);
        lastPos = startNode;        
    }

    //Run l'algorithme A* jusqu'à trouver l'arrivée
    //La coroutine permet d'afficher les étapes successivement et pas d'un seul coup
    IEnumerator Search(PathMarker thisNode)
    {
        while (!done)
        {
            foreach (MapLocation dir in maze.directions)
            {
                MapLocation neighbour = dir + thisNode.location;
                if (maze.data[neighbour.z, neighbour.x] == 1) continue;
                if (neighbour.z < 1 || neighbour.z >= maze.data.GetUpperBound(0) || neighbour.x < 1 || neighbour.x >= maze.data.GetUpperBound(1)) continue;
                if (IsClosed(neighbour)) continue;

                float G = Vector2.Distance(thisNode.location.ToVector(), neighbour.ToVector()) + thisNode.G;
                float H = Vector2.Distance(neighbour.ToVector(), goalNode.location.ToVector());
                float F = G + H;

                GameObject pathBlock = Instantiate(pathP, new Vector3(neighbour.x * maze.hallWidth, 0, neighbour.z * maze.hallWidth), Quaternion.identity);
                //TextMesh[] values = pathBlock.GetComponentsInChildren<TextMesh>();
                //values[0].text = "G: " + G.ToString("0.00");
                //values[1].text = "H: " + H.ToString("0.00");
                //values[2].text = "F: " + F.ToString("0.00");

                if (!UpdateMarker(neighbour, G, H, F, thisNode))
                {
                    open.Add(new PathMarker(neighbour, G, H, F, pathBlock, thisNode));
                }
            }

            open = open.OrderBy(p => p.F).ToList<PathMarker>();
            PathMarker pm = (PathMarker)open.ElementAt(0);
            closed.Add(pm);

            open.RemoveAt(0);
            pm.marker.GetComponent<Renderer>().material = closedMaterial;

            lastPos = pm;
            thisNode = lastPos;

            if (thisNode.Equals(goalNode)) { done = true; break; }      //Arrivée trouvée !

            yield return new WaitForSeconds(0.5f);
        }
        
    }

    //Met à jour les marqueurs avec la donnée du nouveau tour, et enregistre le marqueur "parent"
    bool UpdateMarker(MapLocation pos, float g, float h, float f, PathMarker prt)
    {
        foreach (PathMarker p in open)
        {
            if (p.location.Equals(pos))
            {
                p.G = g;
                p.H = h;
                p.F = f;
                p.parent = prt;
                return true;
            }
        }
        return false;
    }

    //Détermine si un marqueur a déjà été visité
    bool IsClosed (MapLocation marker)
    {
        foreach (PathMarker p in closed)
        {
            if (p.location.Equals(marker)) return true;
        }
        return false;
    }

    //Remonte la suite de marqueurs par les parents pour déterminer le chemin le plus court
    void GetPath()
    {
        RemoveAllMarkers();
        PathMarker begin = lastPos;
        
        while (!startNode.Equals(begin) && begin != null)
        {
            Instantiate(pathP, new Vector3(begin.location.x * maze.hallWidth, 0, begin.location.z * maze.hallWidth), Quaternion.identity);
            path.Add(new Vector3(begin.location.x * maze.hallWidth, 0, begin.location.z * maze.hallWidth));
            begin = begin.parent;
        }

        Instantiate(pathP, new Vector3(startNode.location.x * maze.hallWidth, 0, startNode.location.z * maze.hallWidth), Quaternion.identity);

    }

    //Retourne le chemin calculé par GetPath() dans le bon sens pour que l'IA la parcoure
    void RunPath()
    {
        path.Reverse();
        Vector3 goal = new Vector3(goalNode.location.x, 0, goalNode.location.z);
    }

    void CheckPath()
    {
        timer = 0;
        currentPositionHolder = (Vector3)path[currentNode];
    }

    void DisplayRestart()
    {
        restart.SetActive(true);
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && starting)        //Demande à l'utilisateur d'appuyer sur A pour lancer le processus.
        {
            introduce.SetActive(false);
            restart.SetActive(false);
            BeginSearch();
            StartCoroutine(Search(lastPos));
        }

        if (done)               //Si on a terminé de trouver le chemin, on peut setup la run
        {
            done = false;
            GetPath();
            RunPath();
            CheckPath();
            run = true;
        }

        if (run)                //Une fois le setup terminé, on peut lancer la run de l'IA
        {
            timer += Time.deltaTime;
            if (IA.transform.position != currentPositionHolder)
            {
                IA.transform.position = Vector3.Lerp(IA.transform.position, currentPositionHolder, timer);
            }
            else
            {
                if (currentNode < path.Count - 1)
                {
                    currentNode++;
                    CheckPath();
                }
                else
                {
                    currentNode = 0;
                    run = false;
                    Invoke("DisplayRestart", 2);
                }
            }
        }
    }
}
