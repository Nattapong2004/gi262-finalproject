using System.Collections.Generic;
using System.Security.Principal;
using System.Collections;
using UnityEngine;
using Unity.VisualScripting;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Size")]
    public int X = 10;
    public int Y = 10;

    [Header("Set Player")]
    public Player player;
    public Vector2Int playerStartPos;

    [Header("Set Wall")]
    public Identity Wall;

    [Header("Prefabs")]
    public GameObject[] floorsPrefab;
    public GameObject[] wallsPrefab;
    public GameObject[] monsterPrefab;
    

    [Header("Parent Objects")]
    public Transform floorParent;
    public Transform wallParent;
    public Transform monsterParent;
    

    [Header("Monster Settings")]
    public int monsterCount = 5;
    public int wallCount = 5;


    // ข้อมูลแผนที่
    public Identity[,] mapdata;

    // รายชื่อมอนสเตอร์ในแมพ
    public List<Monster> monstersOnMap = new List<Monster>();

    [HideInInspector] public string empty = "";
    [HideInInspector] public string wall = "wall";
    [HideInInspector] public string monster = "monster";
    [HideInInspector] public string playerOnMap = "player";
    [HideInInspector] public string collectItem = "collectItem";
    

    private void Awake()
    {
        CreateMap();
    }

    void Start()
    {
        SetUpPlayer();
        StartCoroutine(SetupMap());
    }

    IEnumerator SetupMap()
    {
        SetUpPlayer();
        PlaceItemsOnMap(monsterCount,monsterPrefab,monsterParent,monster);
        PlaceItemsOnMap(wallCount, wallsPrefab, wallParent, wall);
        yield return null;
    }

    // สร้างพื้นและกำแพง
    private void CreateMap()
    {
        mapdata = new Identity[X, Y];

        for (int x = -1; x < X + 1; x++)
        {
            for (int y = -1; y < Y + 1; y++)
            {
                if (x == -1 || x == X || y == -1 || y == Y)
                {
                    // สร้างกำแพงรอบนอก
                    int r = Random.Range(0, wallsPrefab.Length);
                    GameObject obj = Instantiate(wallsPrefab[r], new Vector3(x, y, 0), Quaternion.identity);
                    obj.transform.parent = wallParent;
                    obj.name = $"Wall_{x},{y}";
                }
                else
                {
                    // สร้างพื้น
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


    private void PlaceItemsOnMap(int count, GameObject[] prefab, Transform parent, string itemType, System.Action onComplete = null)
    {
        int placedCount = 0;
        int preventInfiniteLoop = 1000; // Increased loop prevention for safety

        while (placedCount < count)
        {
            if (--preventInfiniteLoop < 0)
            {
                Debug.LogWarning("Could not place all items. Map may be too full.");
                break;
            }

            int x = UnityEngine.Random.Range(0, X);
            int y = UnityEngine.Random.Range(0, Y);

            if (mapdata[x, y] == null)
            {
                SetUpItem(x, y, prefab, parent, itemType);
                placedCount++;
            }
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

