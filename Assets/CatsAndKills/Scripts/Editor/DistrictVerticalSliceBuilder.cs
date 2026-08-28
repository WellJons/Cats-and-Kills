#if UNITY_EDITOR
using CatsAndKills.AI;
using CatsAndKills.Narrative;
using CatsAndKills.UI;
using CatsAndKills.Visual;
using CatsAndKills.World;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.EditorTools
{
    public static class DistrictVerticalSliceBuilder
    {
        private const string RootName =
            "District Vertical Slice";

        public static void Apply(
            ProductionArtPack pack)
        {
            if (pack == null)
                return;

            GameObject old =
                GameObject.Find(
                    RootName);

            if (old != null)
                Object.DestroyImmediate(old);

            GameObject root =
                new GameObject(
                    RootName);

            EnsureNarrativeSystems();

            NavigationGrid2D nav =
                Object.FindAnyObjectByType<
                    NavigationGrid2D>();

            ConfigureExistingFactions();

            MissionDirector mission =
                Object.FindAnyObjectByType<
                    MissionDirector>();

            if (mission == null)
            {
                GameObject missionGo =
                    new GameObject(
                        "District Mission");

                mission =
                    missionGo.AddComponent<
                        MissionDirector>();
            }

            mission.ConfigureNarrativeMode(
                true);

            DistrictVerticalSliceDirector district =
                root.AddComponent<
                    DistrictVerticalSliceDirector>();

            district.Configure(
                mission);

            CreateStoryTrigger(
                root.transform);

            CreatePopulation(
                root.transform,
                pack,
                nav);

            CreatePropaganda(
                root.transform,
                pack);

            CreateNeonDistrictDetails(
                root.transform,
                pack);

            CreateMarketDetails(
                root.transform,
                pack);
        }

        private static void EnsureNarrativeSystems()
        {
            if (Object.FindAnyObjectByType<
                    NarrativeWorldState>() == null)
            {
                new GameObject(
                    "Narrative World State")
                    .AddComponent<
                        NarrativeWorldState>();
            }

            if (Object.FindAnyObjectByType<
                    NarrativeDialogueSystem>() == null)
            {
                new GameObject(
                    "Narrative Dialogue")
                    .AddComponent<
                        NarrativeDialogueSystem>();
            }
        }

        private static void ConfigureExistingFactions()
        {
            foreach (TacticalEnemyAgent agent in
                     Object.FindObjectsByType<
                         TacticalEnemyAgent>(
                         FindObjectsSortMode.None))
            {
                if (agent == null)
                    continue;

                GameObject root =
                    agent.gameObject;

                WorldFactionMember2D member =
                    root.GetComponent<
                        WorldFactionMember2D>();

                if (member == null)
                {
                    member =
                        root.AddComponent<
                            WorldFactionMember2D>();
                }

                bool gang =
                    root.name.StartsWith(
                        "Warehouse");

                member.Configure(
                    gang
                        ? WorldFaction.Gang
                        : WorldFaction.Security,
                    false);
            }
        }

        private static void CreateStoryTrigger(
            Transform parent)
        {
            GameObject go =
                new GameObject(
                    "Story Trigger // Warehouse Alley");

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                new Vector2(
                    -22f,
                    0.4f);

            BoxCollider2D collider =
                go.AddComponent<
                    BoxCollider2D>();

            collider.isTrigger = true;
            collider.size =
                new Vector2(
                    8f,
                    2.2f);

            DistrictStoryTrigger trigger =
                go.AddComponent<
                    DistrictStoryTrigger>();

            trigger.Configure(
                "slice_mechanic_done",
                "slice_ambush_started",
                WorldFaction.Gang);
        }

        private static void CreatePopulation(
            Transform parent,
            ProductionArtPack pack,
            NavigationGrid2D nav)
        {
            DialogueNodeData[] vendorDialogue =
                BuildVendorDialogue();

            DialogueNodeData[] mechanicDialogue =
                BuildMechanicDialogue();

            CreateCivilian(
                parent,
                "Street Vendor",
                new Vector2(-44.1f, -18.8f),
                pack.pistolier,
                nav,
                new Color(0.92f, 0.82f, 0.70f),
                false,
                0f,
                "ТОРГОВЕЦ",
                vendorDialogue,
                "slice_vendor_done");

            CreateCivilian(
                parent,
                "Workshop Mechanic",
                new Vector2(-17.1f, -20.2f),
                pack.rifleman,
                nav,
                new Color(0.72f, 0.84f, 0.90f),
                false,
                0f,
                "МЕХАНИК",
                mechanicDialogue,
                null);

            Vector2[] crowd =
            {
                new Vector2(-45f, -14.5f),
                new Vector2(-39f, -14.0f),
                new Vector2(-32f, -25.8f),
                new Vector2(-22f, -26.0f),
                new Vector2(-14f, -12.2f),
                new Vector2(-9f, -10.6f),
                new Vector2(-7f, -4.5f),
                new Vector2(-2f, -5.2f),
                new Vector2(3f, -4.3f),
                new Vector2(8f, -2.6f),
                new Vector2(13f, 0.4f),
                new Vector2(15f, -7.6f),
                new Vector2(16f, -24.8f),
                new Vector2(35f, -7.0f),
                new Vector2(38f, 1.0f),
                new Vector2(12f, 21.8f)
            };

            for (int i = 0;
                 i < crowd.Length;
                 i++)
            {
                DirectionalSpriteSet set =
                    i % 3 == 0
                        ? pack.pistolier
                        : i % 3 == 1
                            ? pack.rifleman
                            : pack.demolitionist;

                Color tint =
                    Color.Lerp(
                        new Color(
                            0.62f,
                            0.68f,
                            0.76f),
                        new Color(
                            0.80f,
                            0.66f,
                            0.58f),
                        (i % 5) /
                        4f);

                CreateCivilian(
                    parent,
                    "Civilian " +
                    (i + 1).ToString("00"),
                    crowd[i],
                    set,
                    nav,
                    tint,
                    true,
                    Random.Range(
                        2.4f,
                        5.0f),
                    null,
                    null,
                    null);
            }

            DialogueNodeData[] worker =
            {
                new DialogueNodeData
                {
                    id = "start",
                    speaker = "РАБОЧИЙ",
                    text =
                        "Ночная смена сегодня не вышла со склада. Начальство говорит — авария сети. Только патрулей почему-то стало вдвое больше.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Ты сам в это веришь?",
                                nextNodeId =
                                    "answer",
                                valueKey =
                                    "city_civilian_trust",
                                valueDelta = 1
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Не моё дело.",
                                closeDialogue = true
                            }
                        }
                },
                new DialogueNodeData
                {
                    id = "answer",
                    speaker = "РАБОЧИЙ",
                    text =
                        "Я верю, что зарплату опять задержат. А остальное лучше обсуждать подальше от городских микрофонов."
                }
            };

            CreateCivilian(
                parent,
                "Night Shift Worker",
                new Vector2(-11.5f, -12.5f),
                pack.pistolier,
                nav,
                new Color(
                    0.64f,
                    0.72f,
                    0.82f),
                false,
                0f,
                "РАБОЧИЙ",
                worker,
                null);
        }

        private static void CreateCivilian(
            Transform parent,
            string name,
            Vector2 position,
            DirectionalSpriteSet set,
            NavigationGrid2D nav,
            Color tint,
            bool wander,
            float wanderRadius,
            string dialogueName,
            DialogueNodeData[] dialogue,
            string completionFlag)
        {
            if (set == null)
                return;

            GameObject root =
                new GameObject(
                    name);

            root.transform.SetParent(
                parent,
                false);

            root.transform.position =
                position;

            Rigidbody2D body =
                root.AddComponent<
                    Rigidbody2D>();

            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation =
                RigidbodyInterpolation2D.Interpolate;

            body.mass = 0.55f;

            CircleCollider2D collider =
                root.AddComponent<
                    CircleCollider2D>();

            collider.radius = 0.27f;

            WorldFactionMember2D faction =
                root.AddComponent<
                    WorldFactionMember2D>();

            faction.Configure(
                WorldFaction.Civilian,
                false);

            GameObject visualGo =
                new GameObject(
                    "Civilian 3-4 Visual");

            visualGo.transform.SetParent(
                root.transform,
                false);

            visualGo.transform.localScale =
                Vector3.one *
                Random.Range(
                    0.86f,
                    0.98f);

            SpriteRenderer renderer =
                visualGo.AddComponent<
                    SpriteRenderer>();

            renderer.sprite =
                set.GetIdle(
                    CharacterDirection8.South);

            renderer.color = tint;

            ThreeQuarterCharacterVisual2D visual =
                visualGo.AddComponent<
                    ThreeQuarterCharacterVisual2D>();

            visual.Configure(
                set,
                renderer,
                null,
                body);

            DepthSortedSprite2D depth =
                visualGo.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { renderer },
                5000,
                -0.58f);

            CreateCivilianShadow(
                root.transform,
                packFallbackShadow: null);

            if (wander &&
                nav != null)
            {
                EnemyMotor2D motor =
                    root.AddComponent<
                        EnemyMotor2D>();

                motor.Configure(
                    nav,
                    Random.Range(
                        1.15f,
                        1.65f));

                CityCivilian2D civilian =
                    root.AddComponent<
                        CityCivilian2D>();

                civilian.Configure(
                    nav,
                    motor,
                    wanderRadius);
            }

            if (dialogue != null &&
                dialogue.Length > 0)
            {
                DialogueInteractable2D talk =
                    root.AddComponent<
                        DialogueInteractable2D>();

                talk.Configure(
                    dialogueName ??
                    name,
                    "ПОГОВОРИТЬ [E]",
                    "start",
                    dialogue,
                    completionFlag);
            }
        }

        private static void CreateCivilianShadow(
            Transform parent,
            Sprite packFallbackShadow)
        {
            Sprite shadow =
                packFallbackShadow != null
                    ? packFallbackShadow
                    : GeneratedArtFactory.Get(
                        "soft_shadow");

            if (shadow == null)
                return;

            GameObject go =
                new GameObject(
                    "Civilian Shadow");

            go.transform.SetParent(
                parent,
                false);

            go.transform.localPosition =
                new Vector3(
                    0f,
                    -0.10f,
                    0f);

            go.transform.localScale =
                new Vector3(
                    0.82f,
                    0.48f,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<
                    SpriteRenderer>();

            sr.sprite = shadow;
            sr.color =
                new Color(
                    0f,
                    0f,
                    0f,
                    0.48f);

            sr.sortingOrder = 3;
        }

        private static DialogueNodeData[]
            BuildVendorDialogue()
        {
            return new[]
            {
                new DialogueNodeData
                {
                    id = "start",
                    speaker = "ТОРГОВЕЦ",
                    text =
                        "С таким ошейником я бы под камерой долго не стоял. Патруль сегодня проверяет документы у каждого второго.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Что случилось?",
                                nextNodeId =
                                    "rumor",
                                setFlag =
                                    "slice_vendor_asked_patrol",
                                valueKey =
                                    "city_civilian_trust",
                                valueDelta = 1
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Мне нужен человек, который разбирается в электронике.",
                                nextNodeId =
                                    "mechanic",
                                setFlag =
                                    "slice_vendor_direct"
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Следи лучше за своим ларьком.",
                                closeDialogue = true,
                                setFlag =
                                    "slice_vendor_rude",
                                valueKey =
                                    "city_civilian_trust",
                                valueDelta = -1
                            }
                        }
                },
                new DialogueNodeData
                {
                    id = "rumor",
                    speaker = "ТОРГОВЕЦ",
                    text =
                        "Официально — проверка после аварии в складском секторе. Неофициально — оттуда с ночи никто не вышел. Но это я тебе не говорил.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Кто может посмотреть ошейник?",
                                nextNodeId =
                                    "mechanic",
                                valueKey =
                                    "city_civilian_trust",
                                valueDelta = 1
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Понятно.",
                                closeDialogue = true
                            }
                        }
                },
                new DialogueNodeData
                {
                    id = "mechanic",
                    speaker = "ТОРГОВЕЦ",
                    text =
                        "За мастерской стоит механик. Обычно чинит генераторы и чужие проблемы. Скажи, что тебя отправили с улицы — имя моё лучше не называй."
                }
            };
        }

        private static DialogueNodeData[]
            BuildMechanicDialogue()
        {
            return new[]
            {
                new DialogueNodeData
                {
                    id = "start",
                    speaker = "МЕХАНИК",
                    text =
                        "Вижу, зачем пришёл. Повреждение корпуса, следы перегрева... такой ошейник я бы даже трогать не стал без причины.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Я уже был у склада. Теперь объясняй.",
                                nextNodeId =
                                    "afterfight",
                                requiredFlag =
                                    "slice_ambush_cleared",
                                setFlag =
                                    "slice_mechanic_afterfight"
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Мне сказали, ты разбираешься в ошейниках.",
                                nextNodeId =
                                    "collar",
                                forbiddenFlag =
                                    "slice_ambush_cleared",
                                setFlag =
                                    "slice_mechanic_done",
                                valueKey =
                                    "mechanic_trust",
                                valueDelta = 1
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Есть работа?",
                                nextNodeId =
                                    "job",
                                forbiddenFlag =
                                    "slice_ambush_cleared",
                                setFlag =
                                    "slice_mechanic_done"
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Не сейчас.",
                                closeDialogue = true
                            }
                        }
                },
                new DialogueNodeData
                {
                    id = "collar",
                    speaker = "МЕХАНИК",
                    text =
                        "Разобраться могу. Снять — нет. Сначала принеси мне блок диагностики со старого склада севернее. Тогда хотя бы поймём, что в нём сломалось.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Ладно. Проверю склад.",
                                closeDialogue = true,
                                setFlag =
                                    "slice_mechanic_agreed",
                                valueKey =
                                    "mechanic_trust",
                                valueDelta = 1
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Если это ловушка — вернусь к тебе.",
                                closeDialogue = true,
                                setFlag =
                                    "slice_mechanic_threatened",
                                valueKey =
                                    "mechanic_trust",
                                valueDelta = -1
                            }
                        }
                },
                new DialogueNodeData
                {
                    id = "job",
                    speaker = "МЕХАНИК",
                    text =
                        "Нужен диагностический блок со склада. И да — официально склад закрыт. Решай сам, насколько тебе нужна помощь."
                },
                new DialogueNodeData
                {
                    id = "afterfight",
                    speaker = "МЕХАНИК",
                    text =
                        "Чёрт. Я знал, что там кто-то крутится. Не знал, что они будут ждать именно тебя.",
                    choices =
                        new[]
                        {
                            new DialogueChoiceData
                            {
                                text =
                                    "Ты отправил меня туда вслепую.",
                                closeDialogue = true,
                                setFlag =
                                    "slice_mechanic_confronted",
                                valueKey =
                                    "mechanic_trust",
                                valueDelta = -2
                            },
                            new DialogueChoiceData
                            {
                                text =
                                    "Мне нужен ответ про ошейник. Остальное потом.",
                                closeDialogue = true,
                                setFlag =
                                    "slice_mechanic_focused",
                                valueKey =
                                    "mechanic_trust",
                                valueDelta = 1
                            }
                        }
                }
            };
        }

        private static void CreatePropaganda(
            Transform parent,
            ProductionArtPack pack)
        {
            CreatePoster(
                parent,
                pack.propagandaPoster,
                "Poster // Gate",
                new Vector2(
                    -43.1f,
                    -15.1f),
                "ЕДИНСТВО. ТРУД. ВОССТАНОВЛЕНИЕ. Военная администрация благодарит граждан за сотрудничество.",
                "poster_gate_seen");

            CreatePoster(
                parent,
                pack.propagandaPoster,
                "Poster // Collar",
                new Vector2(
                    -18.1f,
                    -16.5f),
                "ОШЕЙНИК — ВАШ ДОКУМЕНТ. Повреждение, отключение или попытка снятия подлежат немедленному докладу.",
                "poster_collar_seen");

            CreatePoster(
                parent,
                pack.propagandaPoster,
                "Poster // Curfew",
                new Vector2(
                    13.0f,
                    -2.0f),
                "КОМЕНДАНТСКИЙ ЧАС 22:00. Порядок сохраняет жизни. Нарушение режима создаёт угрозу всему сектору.",
                "poster_curfew_seen");

            CreatePoster(
                parent,
                pack.propagandaPoster,
                "Poster // Rumours",
                new Vector2(
                    36.5f,
                    3.0f),
                "СЛУХИ МЕШАЮТ ВОССТАНОВЛЕНИЮ. Используйте только официальные каналы городской сети.",
                "poster_rumours_seen");
        }

        private static void CreatePoster(
            Transform parent,
            Sprite sprite,
            string name,
            Vector2 position,
            string slogan,
            string flag)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                Vector3.one *
                0.48f;

            SpriteRenderer sr =
                go.AddComponent<
                    SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 5100;

            BoxCollider2D collider =
                go.AddComponent<
                    BoxCollider2D>();

            collider.isTrigger = true;
            collider.size =
                new Vector2(
                    Mathf.Max(
                        0.8f,
                        sprite.bounds.size.x),
                    Mathf.Max(
                        0.8f,
                        sprite.bounds.size.y));

            PropagandaPoster2D poster =
                go.AddComponent<
                    PropagandaPoster2D>();

            poster.Configure(
                slogan,
                flag);
        }

        private static void CreateNeonDistrictDetails(
            Transform parent,
            ProductionArtPack pack)
        {
            CreateNeonSign(
                parent,
                "NEON // Repair",
                "РЕМОНТ",
                new Vector2(
                    -16.5f,
                    -15.5f),
                new Color(
                    0.18f,
                    0.75f,
                    1f));

            CreateNeonSign(
                parent,
                "NEON // Bar",
                "БАР 17",
                new Vector2(
                    -8.5f,
                    -12.0f),
                new Color(
                    1f,
                    0.18f,
                    0.55f));

            CreateNeonSign(
                parent,
                "NEON // Night",
                "24 / НОЧНАЯ СМЕНА",
                new Vector2(
                    5.0f,
                    -4.5f),
                new Color(
                    0.15f,
                    0.84f,
                    0.92f));

            CreateNeonSign(
                parent,
                "NEON // Network",
                "ГОРОДСКАЯ СЕТЬ",
                new Vector2(
                    30f,
                    1.5f),
                new Color(
                    1f,
                    0.14f,
                    0.22f));
        }

        private static void CreateNeonSign(
            Transform parent,
            string name,
            string text,
            Vector2 position,
            Color color)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            TextMesh mesh =
                go.AddComponent<
                    TextMesh>();

            mesh.text = text;
            mesh.fontSize = 52;
            mesh.characterSize = 0.055f;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment =
                TextAlignment.Center;
            mesh.color = color;

            MeshRenderer renderer =
                go.GetComponent<
                    MeshRenderer>();

            if (renderer != null)
                renderer.sortingOrder = 5150;

            Light2D light =
                go.AddComponent<
                    Light2D>();

            light.lightType =
                Light2D.LightType.Point;

            light.color = color;
            light.intensity = 0.75f;
            light.pointLightOuterRadius = 3.4f;
            light.pointLightInnerRadius = 0.4f;
        }

        private static void CreateMarketDetails(
            Transform parent,
            ProductionArtPack pack)
        {
            CreateStaticSprite(
                parent,
                "Vendor Stall // Counter",
                pack.barricade,
                new Vector2(
                    -44.0f,
                    -20.2f),
                0.48f);

            CreateStaticSprite(
                parent,
                "Vendor Stall // Boxes",
                pack.crateStack,
                new Vector2(
                    -45.3f,
                    -21.0f),
                0.38f);

            CreateStaticSprite(
                parent,
                "Street Debris",
                pack.debris,
                new Vector2(
                    -39.0f,
                    -26.0f),
                0.25f);

            CreateStaticSprite(
                parent,
                "Workshop Cables",
                pack.cableBundle,
                new Vector2(
                    -16.2f,
                    -18.2f),
                0.42f);

            CreateStaticSprite(
                parent,
                "Public Terminal",
                pack.terminal,
                new Vector2(
                    -11.4f,
                    -8.8f),
                0.44f);
        }

        private static void CreateStaticSprite(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            float scale)
        {
            if (sprite == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                Vector3.one *
                scale;

            SpriteRenderer sr =
                go.AddComponent<
                    SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;

            DepthSortedSprite2D depth =
                go.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                5000,
                0f);
        }
    }
}
#endif
