using CatsAndKills.Player;
using CatsAndKills.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CatsAndKills.World
{
    public static class PrototypeMissionBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateMissionIfNeeded()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (scene.name != "PlayableCombatSandbox")
                return;

            if (Object.FindFirstObjectByType<MissionDirector>() != null)
                return;

            PlayerMotor2D player = Object.FindFirstObjectByType<PlayerMotor2D>();
            if (player == null)
                return;

            GameObject missionGo = new GameObject("Mission Director");
            MissionDirector mission = missionGo.AddComponent<MissionDirector>();

            GameObject extraction = CreateExtraction(
                new Vector2(19f, -10.5f),
                mission);

            mission.Configure(extraction, null);

            CreateTerminal(
                new Vector2(18f, 7.5f),
                mission);

            CreateTrigger(
                "Warehouse Trigger",
                new Vector2(-4.5f, 0f),
                new Vector2(2f, 18f),
                mission,
                MissionTriggerType.Warehouse);

            CreateTrigger(
                "Administration Trigger",
                new Vector2(10.8f, 1f),
                new Vector2(2f, 18f),
                mission,
                MissionTriggerType.Administration);

            PrototypeHUD hud = Object.FindFirstObjectByType<PrototypeHUD>();
            hud?.BindMission(mission);
        }

        private static void CreateTerminal(Vector2 position, MissionDirector mission)
        {
            GameObject go = new GameObject("Archive Terminal");
            go.transform.position = position;
            go.transform.localScale = new Vector3(0.8f, 1.1f, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = new Color(0.12f, 0.75f, 0.72f);
            sr.sortingOrder = 8;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.size = Vector2.one;

            MissionTerminal terminal = go.AddComponent<MissionTerminal>();
            terminal.Configure(mission);
        }

        private static GameObject CreateExtraction(Vector2 position, MissionDirector mission)
        {
            GameObject go = new GameObject("Extraction Zone");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 1.8f;

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = MakeSolidSprite();
            sr.color = new Color(0.15f, 0.9f, 0.55f, 0.45f);
            sr.sortingOrder = 1;

            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.65f;

            MissionTrigger trigger = go.AddComponent<MissionTrigger>();
            trigger.Configure(mission, MissionTriggerType.Extraction);

            return go;
        }

        private static Sprite MakeSolidSprite()
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.name = "Runtime Mission Marker";
            Color[] pixels = { Color.white, Color.white, Color.white, Color.white };
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, 2f, 2f),
                new Vector2(0.5f, 0.5f),
                2f);
        }

        private static void CreateTrigger(
            string name,
            Vector2 position,
            Vector2 size,
            MissionDirector mission,
            MissionTriggerType type)
        {
            GameObject go = new GameObject(name);
            go.transform.position = position;

            BoxCollider2D col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;

            MissionTrigger trigger = go.AddComponent<MissionTrigger>();
            trigger.Configure(mission, type);
        }
    }
}
