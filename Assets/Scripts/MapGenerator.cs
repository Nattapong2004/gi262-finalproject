using System.Collections.Generic;
using System.Security.Principal;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;


/// - ป้องกันไม่ให้ไอเท็มวางให้แน่นหรือใกล้กันมากเกินไป
/// - ตรวจสอบว่าไม่มีการปิดกั้นเส้นทางระหว่างผู้เล่นและทางออก
public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int X = 10;
    public int Y = 10;

    [Header("Set Player")]
    public Player player;
    public Vector2Int playerStartPos;
    public Vector2Int exitStartPos;
    private Vector2Int exitPos = new Vector2Int(-100, -100);

    [Header("Set Wall")]
    public Identity Wall;

    [Header("Prefabs")]
    public GameObject[] floorsPrefab;
    public GameObject[] wallsPrefab;
    public GameObject[] monsterPrefab;
    public GameObject[] fruitPrefab;
    public GameObject[] exitPrefab;
    public GameObject[] npcPrefab;
    

    [Header("Parent Objects")]
    public Transform floorParent;
    public Transform wallParent;
    public Transform monsterParent;
    public Transform fruitParent;
    public Transform exitParent;
    public Transform npcParent;
    

    [Header("Monster Settings")]
    public int monsterCount = 5;
    public int wallCount = 5;
    public int fruitCount = 3;
    [Tooltip("Minimum Chebyshev distance between fruits (1 = no adjacent fruits)")]
    public int fruitMinDistance = 1;
    [Tooltip("Minimum Chebyshev distance between monsters (1 = no adjacent monsters)")]
    public int monsterMinDistance = 1;


    public Identity[,] mapdata;

    public List<Monster> monstersOnMap = new List<Monster>();

    [HideInInspector] public string empty = "";
    [HideInInspector] public string wall = "wall";
    [HideInInspector] public string monster = "monster";
    [HideInInspector] public string playerOnMap = "player";
    [HideInInspector] public string collectItem = "collectItem";
    [HideInInspector] public string fruit = "fruit";
    [HideInInspector] public string exit = "exit";
    [HideInInspector] public string npc = "npc";
    

    private void Awake()
    {
        CreateMap();
    }

    void Start()
    {
        SetUpPlayer();
        StartCoroutine(SetupMap());
    }

    
    ///วาง NPC เหนือผู้เล่น
    ///วางสัตว์ประสาด ผนัง และผลไม้ (ตามลำดับ)
    IEnumerator SetupMap()
    {
        SetUpPlayer();

        if (exitStartPos != Vector2Int.zero)
        {
            exitPos = exitStartPos;
        }
        else
        {
            exitPos = new Vector2Int(X - 1, Y - 1);
        }

        SetUpItem(exitPos.x, exitPos.y, exitPrefab, exitParent, exit);

        Vector2Int npcPos = new Vector2Int(playerStartPos.x, playerStartPos.y + 2);
        if (npcPos.x >= 0 && npcPos.x < X && npcPos.y >= 0 && npcPos.y < Y)
        {
            if (mapdata[npcPos.x, npcPos.y] == null)
            {
                SetUpItem(npcPos.x, npcPos.y, npcPrefab, npcParent, npc);
            }
            else
            {
                Debug.LogWarning($"Could not place NPC at {npcPos} because tile is occupied.");
            }
        }
        else
        {
            Debug.LogWarning($"NPC position {npcPos} is out of map bounds; NPC not placed.");
        }

        PlaceItemsOnMap(monsterCount, monsterPrefab, monsterParent, monster);
        PlaceItemsOnMap(wallCount, wallsPrefab, wallParent, wall);
        PlaceItemsOnMap(fruitCount, fruitPrefab, fruitParent, fruit);

        yield return null;
    }

    
    private void CreateMap()
    {
        mapdata = new Identity[X, Y];

        for (int x = -1; x < X + 1; x++)
        {
            for (int y = -1; y < Y + 1; y++)
            {
                if (x == -1 || x == X || y == -1 || y == Y)
                {
                    int r = Random.Range(0, wallsPrefab.Length);
                    GameObject obj = Instantiate(wallsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
                    obj.transform.parent = wallParent;
                    obj.name = $"Wall_{x},{y}";
                }
                else
                {
                    int r = Random.Range(0, floorsPrefab.Length);
                    GameObject obj = Instantiate(floorsPrefab[r], new Vector3(x, y, 1), Quaternion.identity);
                    obj.transform.parent = floorParent;
                    obj.name = $"Floor_{x},{y}";
                    mapdata[x, y] = null;
                }
            }
        }
    }

    
    public Identity GetMapData(float x, float y)
    {
        if (x >= X || x < 0 || y >= Y || y < 0)
        {
            return Wall;
        }

        return mapdata[(int)x, (int)y];
    }

    
    /// ใช้ป้องกันวางผนังที่จะปิดกั้นทางระหว่างผู้เล่นและทางออก
    private bool IsPathAvailable(Vector2Int start, Vector2Int goal, Vector2Int simulatedBlockedPos)
    {
        if (start.x < 0 || goal.x < 0) return false;

        bool[,] visited = new bool[X, Y];
        Queue<Vector2Int> q = new Queue<Vector2Int>();
        q.Enqueue(start);
        visited[start.x, start.y] = true;

        int[] dx = new int[] { 1, -1, 0, 0 };
        int[] dy = new int[] { 0, 0, 1, -1 };

        while (q.Count > 0)
        {
            Vector2Int cur = q.Dequeue();
            if (cur == goal) return true;

            for (int i = 0; i < 4; i++)
            {
                int nx = cur.x + dx[i];
                int ny = cur.y + dy[i];

                if (nx < 0 || ny < 0 || nx >= X || ny >= Y) continue;
                if (visited[nx, ny]) continue;

                if (simulatedBlockedPos.x == nx && simulatedBlockedPos.y == ny) continue;

                bool walkable = false;
                if (mapdata[nx, ny] == null) walkable = true;
                if (nx == start.x && ny == start.y) walkable = true;
                if (nx == goal.x && ny == goal.y) walkable = true;

                if (!walkable) continue;

                visited[nx, ny] = true;
                q.Enqueue(new Vector2Int(nx, ny));
            }
        }

        return false;
    }


    ///วางไอเท็มแบบกระจายทั่วแผนที่แต่ไม่ให้ติดกับผู้เล่นและทางออก
    private void PlaceItemsOnMap(int count, GameObject[] prefab, Transform parent, string itemType, System.Action onComplete = null)
    {
        int placedCount = 0;

        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int xi = 0; xi < X; xi++)
        {
            for (int yi = 0; yi < Y; yi++)
            {
                if (mapdata[xi, yi] != null) continue;

                if (Mathf.Abs(xi - playerStartPos.x) <= 1 && Mathf.Abs(yi - playerStartPos.y) <= 1) continue;

                if (exitPos.x >= 0 && exitPos.y >= 0)
                {
                    if (Mathf.Abs(xi - exitPos.x) <= 1 && Mathf.Abs(yi - exitPos.y) <= 1) continue;
                }

                candidates.Add(new Vector2Int(xi, yi));
            }
        }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            Vector2Int tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        foreach (var pos in candidates)
        {
            if (placedCount >= count) break;

            int x = pos.x;
            int y = pos.y;

            if (mapdata[x, y] != null) continue;

            if (itemType == wall && exitPos.x >= 0 && exitPos.y >= 0)
            {
                if (!IsPathAvailable(playerStartPos, exitPos, new Vector2Int(x, y)))
                {
                    continue;
                }
            }

            if (itemType == fruit)
            {
                bool tooClose = false;
                for (int nx = x - fruitMinDistance; nx <= x + fruitMinDistance && !tooClose; nx++)
                {
                    for (int ny = y - fruitMinDistance; ny <= y + fruitMinDistance; ny++)
                    {
                        if (nx < 0 || ny < 0 || nx >= X || ny >= Y) continue;
                        Identity id = mapdata[nx, ny];
                        if (id != null && id.Name == fruit)
                        {
                            tooClose = true;
                            break;
                        }
                    }
                }

                if (tooClose) continue;
            }

            if (itemType == monster)
            {
                bool tooCloseM = false;
                for (int nx = x - monsterMinDistance; nx <= x + monsterMinDistance && !tooCloseM; nx++)
                {
                    for (int ny = y - monsterMinDistance; ny <= y + monsterMinDistance; ny++)
                    {
                        if (nx < 0 || ny < 0 || nx >= X || ny >= Y) continue;
                        Identity id = mapdata[nx, ny];
                        if (id != null && id.Name == monster)
                        {
                            tooCloseM = true;
                            break;
                        }
                    }
                }

                if (tooCloseM) continue;
            }

            SetUpItem(x, y, prefab, parent, itemType);
            placedCount++;
        }

        if (placedCount < count)
        {
            Debug.LogWarning($"Could only place {placedCount} of {count} requested items of type {itemType}.");
        }

        onComplete?.Invoke();
    }

    
    public void SetUpItem(int x, int y, GameObject[] _itemsPrefab, Transform parrent, string _name)
    {
        int r = Random.Range(0, _itemsPrefab.Length);
        GameObject obj = Instantiate(_itemsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
        obj.transform.parent = parrent;
        mapdata[x, y] = obj.GetComponent<Identity>();
        mapdata[x, y].positionX = x;
        mapdata[x, y].positionY = y;
        mapdata[x, y].mapGenerator = this;
        if (_name != collectItem)
        {
            mapdata[x, y].Name = _name;
        }
        if (_name == monster)
        {
            monstersOnMap.Add(obj.GetComponent<Monster>());
        }
        obj.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
    }
    
    public void SetUpItem(int x, int y, GameObject _itemsPrefab, Transform parrent, string _name)
    {
        _itemsPrefab.transform.parent = parrent;
        mapdata[x, y] = _itemsPrefab.GetComponent<Identity>();
        mapdata[x, y].positionX = x;
        mapdata[x, y].positionY = y;
        mapdata[x, y].mapGenerator = this;
        if (_name != collectItem)
        {
            mapdata[x, y].Name = _name;
        }
        if (_name == monster)
        {
            monstersOnMap.Add(_itemsPrefab.GetComponent<Monster>());
        }
        _itemsPrefab.name = $"Object_{mapdata[x, y].Name} {x}, {y}";
    }
    
    
    private void SetUpPlayer()
    {
        player.mapGenerator = this;
        player.positionX = playerStartPos.x;
        player.positionY = playerStartPos.y;
        player.transform.position = new Vector3(playerStartPos.x, playerStartPos.y, -0.1f);
        mapdata[playerStartPos.x, playerStartPos.y] = player;
    }

}

