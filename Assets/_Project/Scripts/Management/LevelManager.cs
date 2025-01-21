using System;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    public static Action<int, int> UnitCountUpdated { get; set; } = delegate { };

    private EntityManager _entityManager;
    private float _timeSinceLastUpdate;
    public GameObject panel;
    public Image panelImage;
    public TMP_Text panelText;
    public LevelsFinished levelsFinished;
    public int levelNum;


    private void Awake()
    {
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    void Update()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityQuery queryLevel = entityManager.CreateEntityQuery(typeof(Level));

        if (queryLevel.CalculateEntityCount() == 0)
        {
            return;
        }
        Entity levelEntity = queryLevel.GetSingletonEntity();
        Level level = entityManager.GetComponentData<Level>(levelEntity);

        if (_timeSinceLastUpdate > 0)
        {
            _timeSinceLastUpdate -= Time.deltaTime;
            return;
        }
        _timeSinceLastUpdate = 1f;
        int alliedUnits = 0;
        int enemyUnits = 0;

        EntityQuery query = _entityManager.CreateEntityQuery(ComponentType.ReadOnly<TeamData>());
        using (NativeArray<Entity> entities = query.ToEntityArray(Allocator.TempJob))
        {
            foreach (var entity in entities)
            {
                TeamData teamData = _entityManager.GetComponentData<TeamData>(entity);
                if (teamData.Value == 0)
                {
                    alliedUnits++;
                }
                else if (teamData.Value == 1)
                {
                    enemyUnits++;
                }
            }
        }

        UnitCountUpdated(alliedUnits, enemyUnits);

        query.Dispose();

        if (level.HasStarted && enemyUnits == 0)
        {
            panel.SetActive(true);
            panelImage.color = Color.green;
            panelText.text = "Allied units win!";
            levelsFinished.LevelCompleted(levelNum);
            Time.timeScale = 0f;
        }
        else if(level.Lose && level.HasStarted)
        {
            panel.SetActive(true);
            panelImage.color = Color.red;
            panelText.text = "Enemy units win!";
            levelsFinished.LevelCompleted(levelNum);
            Time.timeScale = 0f;
        }
    }

    public void ChangeScene()
    {
        panel.SetActive(false);
        SceneManager.LoadScene("WorldMap");
    }

}
