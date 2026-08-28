#if UNITY_EDITOR
using CatsAndKills.FX;
using CatsAndKills.Visual;
using CatsAndKills.World;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatsAndKills.EditorTools
{
    public static class ConceptVisualPolishBuilder
    {
        private const string RootName =
            "Concept Visual Polish";

        private const string LitMaterialPath =
            "Assets/CatsAndKills/Generated/IntegratedConcept/Data/ConceptSpriteLit.mat";

        public static Material GetOrCreateLitMaterial()
        {
            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(
                    LitMaterialPath);

            if (material != null)
                return material;

            Shader shader =
                Shader.Find(
                    "Universal Render Pipeline/2D/Sprite-Lit-Default");

            if (shader == null)
                return null;

            material =
                new Material(shader);

            AssetDatabase.CreateAsset(
                material,
                LitMaterialPath);

            return material;
        }

        public static void Apply(
            ProductionArtPack pack)
        {
            if (pack == null)
                return;

            GameObject old =
                GameObject.Find(RootName);

            if (old != null)
                Object.DestroyImmediate(old);

            GameObject root =
                new GameObject(RootName);

            RemoveLegacyPresentationObjects();
            RemoveLegacyPrototypeGameplayGeometry();
            DisableLegacyWallVisuals();
            DisableLegacyInternalWallColliders();

            Material lit =
                GetOrCreateLitMaterial();

            CreateConceptFloor(
                root.transform,
                pack);

            AddStructuralPass(
                root.transform,
                pack,
                lit);

            AddPropPass(
                root.transform,
                pack,
                lit);

            AddFloorDetailPass(
                root.transform,
                pack);

            AddLightingPass(
                root.transform,
                lit);

            AddFogPass(
                root.transform,
                pack);

            if (root.GetComponent<AlarmLighting2D>() == null)
                root.AddComponent<AlarmLighting2D>();

            ApplyMaterialToConceptSprites(
                lit);
        }

        private static void RemoveLegacyPresentationObjects()
        {
            var toDestroy =
                new System.Collections.Generic.HashSet<GameObject>();

            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null)
                    continue;

                GameObject go =
                    sr.gameObject;

                string n =
                    go.name;

                string parent =
                    go.transform.parent != null
                        ? go.transform.parent.name
                        : string.Empty;

                bool legacyVisual =
                    n == "Floor" ||
                    n.StartsWith("Crate ") ||
                    n.StartsWith("Fuel Drum ") ||
                    n.StartsWith("3-4 Decor //") ||
                    parent.StartsWith("3-4 Decor //") ||
                    n.Contains("Floor Zone") ||
                    parent.Contains("Floor Zone") ||
                    n.Contains("Hazard //") ||
                    parent.Contains("Hazard //") ||
                    n.Contains("Light Pool //") ||
                    parent.Contains("Light Pool //");

                if (!legacyVisual)
                    continue;

                GameObject target = go;

                if (go.transform.parent != null &&
                    (parent.StartsWith("3-4 Decor //") ||
                     parent.Contains("Floor Zone") ||
                     parent.Contains("Hazard //") ||
                     parent.Contains("Light Pool //")))
                {
                    target =
                        go.transform.parent.gameObject;
                }

                toDestroy.Add(target);
            }

            foreach (GameObject go in toDestroy)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
        }

        private static void RemoveLegacyPrototypeGameplayGeometry()
        {
            var roots =
                new System.Collections.Generic.List<GameObject>();

            foreach (BoxCollider2D collider in
                     Object.FindObjectsByType<BoxCollider2D>(
                         FindObjectsSortMode.None))
            {
                if (collider == null)
                    continue;

                GameObject go =
                    collider.gameObject;

                string n =
                    go.name;

                bool prototypeObject =
                    n.StartsWith("Crate ") ||
                    n.StartsWith("Fuel Drum ") ||
                    n.StartsWith("Prototype ");

                if (prototypeObject &&
                    !roots.Contains(go))
                {
                    roots.Add(go);
                }
            }

            foreach (GameObject go in roots)
            {
                if (go != null)
                    Object.DestroyImmediate(go);
            }
        }

        private static void DisableLegacyWallVisuals()
        {
            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null)
                    continue;

                string n =
                    sr.gameObject.name;

                string parent =
                    sr.transform.parent != null
                        ? sr.transform.parent.name
                        : string.Empty;

                // The legacy wall renderers are tiled prototype geometry.
                // The concept wall sprite is a full illustration, so keeping
                // these renderers visible causes the repeated giant strips.
                if (n == "Wall Top" ||
                    n == "Wall Side" ||
                    n == "Wall Shadow" ||
                    n == "Prop Shadow" ||
                    n == "Floor" ||
                    n.Contains("Floor Zone") ||
                    parent.Contains("Floor Zone") ||
                    n.Contains("Hazard //") ||
                    parent.Contains("Hazard //"))
                {
                    sr.enabled = false;
                }
            }
        }

        private static void DisableLegacyInternalWallColliders()
        {
            foreach (BoxCollider2D collider in
                     Object.FindObjectsByType<BoxCollider2D>(
                         FindObjectsSortMode.None))
            {
                if (collider == null)
                    continue;

                Transform wallRoot =
                    collider.transform;

                bool prototypeWall =
                    wallRoot.Find("Wall Top") != null ||
                    wallRoot.Find("Wall Side") != null;

                if (!prototypeWall)
                    continue;

                string n =
                    wallRoot.gameObject.name;

                // Keep the outer world boundary only. Interior prototype walls
                // do not line up with the concept-art wall pieces and therefore
                // become invisible blockers after the visual conversion.
                bool outerBoundary =
                    n == "North" ||
                    n == "South" ||
                    n == "West" ||
                    n == "East";

                if (!outerBoundary)
                    collider.enabled = false;
            }
        }

        private static void CreateConceptFloor(
            Transform parent,
            ProductionArtPack pack)
        {
            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            if (square == null)
                return;

            CreateGroundRect(
                parent,
                "District Base",
                Vector2.zero,
                new Vector2(96f, 64f),
                new Color(
                    0.075f,
                    0.085f,
                    0.115f,
                    1f),
                -1500);

            // Roads are intentionally simple and continuous. No giant sampled
            // texture is stretched across the entire map anymore.
            CreateGroundRect(
                parent,
                "West Main Road",
                new Vector2(-28f, -20f),
                new Vector2(38f, 13f),
                new Color(
                    0.095f,
                    0.105f,
                    0.135f,
                    1f),
                -1495);

            CreateGroundRect(
                parent,
                "Central Boulevard",
                new Vector2(0f, -7f),
                new Vector2(58f, 12f),
                new Color(
                    0.088f,
                    0.098f,
                    0.128f,
                    1f),
                -1495);

            CreateGroundRect(
                parent,
                "East Service Road",
                new Vector2(31f, -8f),
                new Vector2(14f, 42f),
                new Color(
                    0.090f,
                    0.100f,
                    0.132f,
                    1f),
                -1495);

            CreateGroundRect(
                parent,
                "North Service Road",
                new Vector2(0f, 23f),
                new Vector2(42f, 9f),
                new Color(
                    0.086f,
                    0.100f,
                    0.135f,
                    1f),
                -1495);

            // The square is visually distinct from the roads and reads as a
            // real location rather than an empty piece of floor texture.
            CreateGroundRect(
                parent,
                "Central Plaza",
                new Vector2(0f, 2f),
                new Vector2(25f, 20f),
                new Color(
                    0.145f,
                    0.145f,
                    0.185f,
                    1f),
                -1490);

            CreateGroundRect(
                parent,
                "Plaza Inner Stone",
                new Vector2(0f, 2f),
                new Vector2(17f, 12f),
                new Color(
                    0.175f,
                    0.170f,
                    0.205f,
                    1f),
                -1489);

            CreateGroundRect(
                parent,
                "Warehouse Apron",
                new Vector2(-25f, 1f),
                new Vector2(27f, 9f),
                new Color(
                    0.105f,
                    0.125f,
                    0.145f,
                    1f),
                -1490);

            CreateGroundRect(
                parent,
                "Administration Courtyard",
                new Vector2(24f, 1f),
                new Vector2(21f, 10f),
                new Color(
                    0.125f,
                    0.115f,
                    0.145f,
                    1f),
                -1490);

            // Sidewalk bands make building footprints readable.
            CreateGroundRect(
                parent,
                "West Sidewalk",
                new Vector2(-32f, -12f),
                new Vector2(28f, 3.2f),
                new Color(
                    0.18f,
                    0.19f,
                    0.22f,
                    1f),
                -1488);

            CreateGroundRect(
                parent,
                "East Sidewalk",
                new Vector2(25f, -1f),
                new Vector2(25f, 3.0f),
                new Color(
                    0.17f,
                    0.18f,
                    0.22f,
                    1f),
                -1488);

            CreateGroundRect(
                parent,
                "North Sidewalk",
                new Vector2(-5f, 16f),
                new Vector2(48f, 3.0f),
                new Color(
                    0.16f,
                    0.18f,
                    0.22f,
                    1f),
                -1488);
        }

        private static void CreateGroundRect(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color,
            int order)
        {
            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            if (square == null)
                return;

            GameObject go =
                new GameObject(
                    "Ground // " + name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = square;
            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size = size;
            sr.color = color;
            sr.sortingOrder = order;
        }

        private static void CreateFloorTintZone(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 size,
            Color color)
        {
            Sprite square =
                GeneratedArtFactory.Get("ui_square");

            if (square == null)
                return;

            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = square;
            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size = size;
            sr.color = color;
            sr.sortingOrder = -1490;
        }

        private static void AddStructuralPass(
            Transform parent,
            ProductionArtPack pack,
            Material lit)
        {
            // WEST ENTRY: visible immediately when the level starts.
            CreateBuilding(
                parent,
                "Security Gatehouse",
                new Vector2(-38f, -21f),
                new Vector2(8f, 8f),
                null,
                null,
                lit,
                0f,
                float.NaN,
                -1.2f,
                0f,
                new Color(0.11f, 0.15f, 0.21f, 0.98f));

            CreateBuilding(
                parent,
                "Workshop",
                new Vector2(-27f, -17f),
                new Vector2(17f, 13f),
                null,
                null,
                lit,
                float.NaN,
                2.5f,
                float.NaN,
                0f,
                new Color(0.10f, 0.14f, 0.19f, 0.98f));

            // SOUTH-WEST shop creates a second route into the square.
            CreateBuilding(
                parent,
                "Corner Store",
                new Vector2(-9f, -17f),
                new Vector2(11f, 9f),
                null,
                null,
                lit,
                1.5f,
                float.NaN,
                float.NaN,
                0f,
                new Color(0.13f, 0.14f, 0.20f, 0.98f));

            // NORTH-WEST warehouse is a large enterable combat space.
            CreateBuilding(
                parent,
                "Warehouse 04",
                new Vector2(-25f, 11f),
                new Vector2(24f, 17f),
                null,
                null,
                lit,
                -4f,
                float.NaN,
                1.5f,
                float.NaN,
                new Color(0.10f, 0.15f, 0.20f, 0.98f));

            // NORTH-EAST clinic provides an alternate path around the plaza.
            CreateBuilding(
                parent,
                "Clinic",
                new Vector2(8f, 14f),
                new Vector2(12f, 10f),
                null,
                null,
                lit,
                0f,
                float.NaN,
                -1f,
                float.NaN,
                new Color(0.13f, 0.15f, 0.22f, 0.98f));

            CreateBuilding(
                parent,
                "Administration",
                new Vector2(26f, 12f),
                new Vector2(21f, 18f),
                null,
                null,
                lit,
                -2f,
                float.NaN,
                -2f,
                float.NaN,
                new Color(0.16f, 0.12f, 0.20f, 0.98f));

            CreateBuilding(
                parent,
                "Barracks",
                new Vector2(27f, -18f),
                new Vector2(18f, 14f),
                null,
                null,
                lit,
                float.NaN,
                -2f,
                1.5f,
                float.NaN,
                new Color(0.11f, 0.14f, 0.20f, 0.98f));

            CreateBuilding(
                parent,
                "North Checkpoint",
                new Vector2(-5f, 24f),
                new Vector2(14f, 9f),
                null,
                null,
                lit,
                0f,
                float.NaN,
                float.NaN,
                1f,
                new Color(0.11f, 0.14f, 0.21f, 0.98f));

            // Plaza barriers are real collision, not decorative hazard strips.
            CreateBuildingWallSegment(
                parent,
                "Plaza West Security",
                new Vector2(-13f, 2f),
                new Vector2(0.65f, 8f),
                null,
                lit);

            CreateBuildingWallSegment(
                parent,
                "Plaza East Security",
                new Vector2(13f, 2f),
                new Vector2(0.65f, 8f),
                null,
                lit);

            CreateBuildingWallSegment(
                parent,
                "Plaza North Security",
                new Vector2(0f, 12f),
                new Vector2(8f, 0.65f),
                null,
                lit);

            CreateBuildingWallSegment(
                parent,
                "Plaza South Security",
                new Vector2(0f, -8f),
                new Vector2(7f, 0.65f),
                null,
                lit);
        }

        private static void CreateBuilding(
            Transform parent,
            string name,
            Vector2 center,
            Vector2 size,
            Sprite interiorSprite,
            Sprite wallSprite,
            Material material,
            float southDoorX,
            float northDoorX,
            float westDoorY,
            float eastDoorY,
            Color roofColor)
        {
            GameObject root =
                new GameObject(
                    "Building // " + name);

            root.transform.SetParent(
                parent,
                false);

            root.transform.position =
                center;

            CreateBuildingInteriorFloor(
                root.transform,
                size,
                new Color(
                    Mathf.Clamp01(roofColor.r + 0.08f),
                    Mathf.Clamp01(roofColor.g + 0.08f),
                    Mathf.Clamp01(roofColor.b + 0.09f),
                    1f));

            const float thickness = 0.72f;
            const float doorWidth = 2.7f;

            CreateHorizontalBuildingSide(
                root.transform,
                name + " South",
                -size.y * 0.5f,
                size.x,
                thickness,
                southDoorX,
                doorWidth,
                null,
                material);

            CreateHorizontalBuildingSide(
                root.transform,
                name + " North",
                size.y * 0.5f,
                size.x,
                thickness,
                northDoorX,
                doorWidth,
                null,
                material);

            CreateVerticalBuildingSide(
                root.transform,
                name + " West",
                -size.x * 0.5f,
                size.y,
                thickness,
                westDoorY,
                doorWidth,
                null,
                material);

            CreateVerticalBuildingSide(
                root.transform,
                name + " East",
                size.x * 0.5f,
                size.y,
                thickness,
                eastDoorY,
                doorWidth,
                null,
                material);

            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            GameObject roofEdge =
                new GameObject(
                    name + " Roof Edge");

            roofEdge.transform.SetParent(
                root.transform,
                false);

            SpriteRenderer edgeRenderer =
                roofEdge.AddComponent<SpriteRenderer>();

            edgeRenderer.sprite = square;
            edgeRenderer.drawMode =
                SpriteDrawMode.Tiled;

            edgeRenderer.size =
                new Vector2(
                    size.x + 0.35f,
                    size.y + 0.35f);

            edgeRenderer.color =
                new Color(
                    0.035f,
                    0.045f,
                    0.070f,
                    1f);

            edgeRenderer.sortingOrder =
                8998;

            GameObject roof =
                new GameObject(
                    name + " Roof");

            roof.transform.SetParent(
                root.transform,
                false);

            SpriteRenderer roofRenderer =
                roof.AddComponent<SpriteRenderer>();

            roofRenderer.sprite = square;
            roofRenderer.drawMode =
                SpriteDrawMode.Tiled;

            roofRenderer.size =
                new Vector2(
                    Mathf.Max(1f, size.x - 0.30f),
                    Mathf.Max(1f, size.y - 0.30f));

            roofRenderer.color =
                roofColor;

            roofRenderer.sortingOrder =
                8999;

            // Simple roof machinery makes the outside silhouette read as a
            // roof instead of a flat colored room.
            CreateRoofUnit(
                roof.transform,
                new Vector2(
                    -size.x * 0.20f,
                    size.y * 0.12f));

            if (size.x > 13f)
            {
                CreateRoofUnit(
                    roof.transform,
                    new Vector2(
                        size.x * 0.20f,
                        -size.y * 0.12f));
            }

            GameObject triggerGo =
                new GameObject(
                    name + " Interior Trigger");

            triggerGo.transform.SetParent(
                root.transform,
                false);

            BoxCollider2D trigger =
                triggerGo.AddComponent<BoxCollider2D>();

            trigger.isTrigger = true;

            trigger.size =
                new Vector2(
                    Mathf.Max(1f, size.x - 1.7f),
                    Mathf.Max(1f, size.y - 1.7f));

            BuildingRoofFader2D fader =
                triggerGo.AddComponent<
                    BuildingRoofFader2D>();

            fader.Configure(
                new[]
                {
                    edgeRenderer,
                    roofRenderer
                },
                trigger,
                0.06f,
                6.5f);
        }

        private static void CreateBuildingInteriorFloor(
            Transform parent,
            Vector2 size,
            Color color)
        {
            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            if (square == null)
                return;

            GameObject floor =
                new GameObject(
                    "Interior Floor");

            floor.transform.SetParent(
                parent,
                false);

            SpriteRenderer sr =
                floor.AddComponent<SpriteRenderer>();

            sr.sprite = square;
            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size =
                new Vector2(
                    Mathf.Max(1f, size.x - 1.3f),
                    Mathf.Max(1f, size.y - 1.3f));

            sr.color = color;
            sr.sortingOrder = -1200;
        }

        private static void CreateRoofUnit(
            Transform roof,
            Vector2 localPosition)
        {
            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            if (square == null)
                return;

            GameObject unit =
                new GameObject(
                    "Roof Vent");

            unit.transform.SetParent(
                roof,
                false);

            unit.transform.localPosition =
                localPosition;

            SpriteRenderer sr =
                unit.AddComponent<SpriteRenderer>();

            sr.sprite = square;
            sr.drawMode =
                SpriteDrawMode.Tiled;

            sr.size =
                new Vector2(
                    1.3f,
                    0.8f);

            sr.color =
                new Color(
                    0.20f,
                    0.23f,
                    0.30f,
                    1f);

            sr.sortingOrder = 1;
        }

        private static void CreateHorizontalBuildingSide(
            Transform parent,
            string label,
            float y,
            float totalLength,
            float thickness,
            float doorCenter,
            float doorWidth,
            Sprite wallSprite,
            Material material)
        {
            if (float.IsNaN(
                    doorCenter))
            {
                CreateBuildingWallSegment(
                    parent,
                    label,
                    new Vector2(
                        0f,
                        y),
                    new Vector2(
                        totalLength,
                        thickness),
                    wallSprite,
                    material);

                return;
            }

            float leftEdge =
                -totalLength * 0.5f;

            float rightEdge =
                totalLength * 0.5f;

            float doorLeft =
                Mathf.Clamp(
                    doorCenter -
                    doorWidth * 0.5f,
                    leftEdge + 0.5f,
                    rightEdge - 0.5f);

            float doorRight =
                Mathf.Clamp(
                    doorCenter +
                    doorWidth * 0.5f,
                    leftEdge + 0.5f,
                    rightEdge - 0.5f);

            float leftLength =
                doorLeft -
                leftEdge;

            float rightLength =
                rightEdge -
                doorRight;

            if (leftLength > 0.5f)
            {
                CreateBuildingWallSegment(
                    parent,
                    label + " Left",
                    new Vector2(
                        leftEdge +
                        leftLength * 0.5f,
                        y),
                    new Vector2(
                        leftLength,
                        thickness),
                    wallSprite,
                    material);
            }

            if (rightLength > 0.5f)
            {
                CreateBuildingWallSegment(
                    parent,
                    label + " Right",
                    new Vector2(
                        doorRight +
                        rightLength * 0.5f,
                        y),
                    new Vector2(
                        rightLength,
                        thickness),
                    wallSprite,
                    material);
            }
        }

        private static void CreateVerticalBuildingSide(
            Transform parent,
            string label,
            float x,
            float totalLength,
            float thickness,
            float doorCenter,
            float doorWidth,
            Sprite wallSprite,
            Material material)
        {
            if (float.IsNaN(
                    doorCenter))
            {
                CreateBuildingWallSegment(
                    parent,
                    label,
                    new Vector2(
                        x,
                        0f),
                    new Vector2(
                        thickness,
                        totalLength),
                    wallSprite,
                    material);

                return;
            }

            float bottomEdge =
                -totalLength * 0.5f;

            float topEdge =
                totalLength * 0.5f;

            float doorBottom =
                Mathf.Clamp(
                    doorCenter -
                    doorWidth * 0.5f,
                    bottomEdge + 0.5f,
                    topEdge - 0.5f);

            float doorTop =
                Mathf.Clamp(
                    doorCenter +
                    doorWidth * 0.5f,
                    bottomEdge + 0.5f,
                    topEdge - 0.5f);

            float bottomLength =
                doorBottom -
                bottomEdge;

            float topLength =
                topEdge -
                doorTop;

            if (bottomLength > 0.5f)
            {
                CreateBuildingWallSegment(
                    parent,
                    label + " Lower",
                    new Vector2(
                        x,
                        bottomEdge +
                        bottomLength * 0.5f),
                    new Vector2(
                        thickness,
                        bottomLength),
                    wallSprite,
                    material);
            }

            if (topLength > 0.5f)
            {
                CreateBuildingWallSegment(
                    parent,
                    label + " Upper",
                    new Vector2(
                        x,
                        doorTop +
                        topLength * 0.5f),
                    new Vector2(
                        thickness,
                        topLength),
                    wallSprite,
                    material);
            }
        }

        private static void CreateBuildingWallSegment(
            Transform parent,
            string name,
            Vector2 localPosition,
            Vector2 size,
            Sprite wallSprite,
            Material material)
        {
            Sprite square =
                GeneratedArtFactory.Get(
                    "ui_square");

            if (square == null)
                return;

            GameObject wall =
                new GameObject(
                    name + " Wall");

            wall.transform.SetParent(
                parent,
                false);

            wall.transform.localPosition =
                localPosition;

            int obstacleLayer =
                LayerMask.NameToLayer(
                    "Obstacles");

            if (obstacleLayer >= 0)
                wall.layer = obstacleLayer;

            BoxCollider2D collider =
                wall.AddComponent<BoxCollider2D>();

            collider.size = size;

            SpriteRenderer baseRenderer =
                wall.AddComponent<SpriteRenderer>();

            baseRenderer.sprite = square;
            baseRenderer.drawMode =
                SpriteDrawMode.Tiled;

            baseRenderer.size = size;

            baseRenderer.color =
                new Color(
                    0.070f,
                    0.085f,
                    0.120f,
                    1f);

            DepthSortedSprite2D depth =
                wall.AddComponent<
                    DepthSortedSprite2D>();

            GameObject top =
                new GameObject(
                    "Wall Highlight");

            top.transform.SetParent(
                wall.transform,
                false);

            SpriteRenderer topRenderer =
                top.AddComponent<SpriteRenderer>();

            topRenderer.sprite = square;
            topRenderer.drawMode =
                SpriteDrawMode.Tiled;

            bool horizontal =
                size.x >= size.y;

            topRenderer.size =
                horizontal
                    ? new Vector2(
                        size.x,
                        Mathf.Min(
                            0.16f,
                            size.y * 0.34f))
                    : new Vector2(
                        Mathf.Min(
                            0.16f,
                            size.x * 0.34f),
                        size.y);

            top.transform.localPosition =
                horizontal
                    ? new Vector3(
                        0f,
                        size.y * 0.24f,
                        0f)
                    : new Vector3(
                        -size.x * 0.24f,
                        0f,
                        0f);

            topRenderer.color =
                new Color(
                    0.24f,
                    0.29f,
                    0.38f,
                    1f);

            topRenderer.sortingOrder = 1;

            depth.Configure(
                new[]
                {
                    baseRenderer,
                    topRenderer
                },
                5000,
                0f);
        }

        private static void AddPropPass(
            Transform parent,
            ProductionArtPack pack,
            Material lit)
        {
            Sprite heavy =
                pack.crateHeavy != null
                    ? pack.crateHeavy
                    : pack.crateLight;

            Sprite stack =
                pack.crateStack != null
                    ? pack.crateStack
                    : pack.crateLight;

            Sprite barrels =
                pack.barrelStack != null
                    ? pack.barrelStack
                    : pack.fuelDrum;

            // WEST APPROACH / WORKSHOP
            CreateProp(parent, "West Road Barricade A", pack.barricade, new Vector2(-34f, -23f), 0.58f, lit);
            CreateProp(parent, "West Road Heavy Cover", heavy, new Vector2(-31f, -18f), 0.62f, lit);
            CreateProp(parent, "Workshop Crate Stack", stack, new Vector2(-25f, -17f), 0.54f, lit);
            CreateProp(parent, "Workshop Barrel Stack", barrels, new Vector2(-19f, -20f), 0.50f, lit);
            CreateProp(parent, "Workshop Pipe Rack", pack.pipeCluster, new Vector2(-27f, -13f), 0.52f, lit);
            CreateProp(parent, "Workshop Terminal", pack.terminal, new Vector2(-17f, -14f), 0.56f, lit);
            CreateProp(parent, "Workshop Fence", pack.fence, new Vector2(-14f, -24f), 0.54f, lit);

            // CENTRAL PLAZA
            CreateProp(parent, "Plaza Cover West", heavy, new Vector2(-8f, -4f), 0.56f, lit);
            CreateProp(parent, "Plaza Cover East", heavy, new Vector2(8f, 3f), 0.56f, lit);
            CreateProp(parent, "Plaza Barricade North", pack.barricade, new Vector2(-3f, 5f), 0.54f, lit);
            CreateProp(parent, "Plaza Barricade South", pack.barricade, new Vector2(4f, -8f), 0.54f, lit);
            CreateProp(parent, "Plaza Ammo Crates", stack, new Vector2(10f, -7f), 0.48f, lit);
            CreateProp(parent, "Plaza Debris A", pack.debris, new Vector2(-2f, -1f), 0.22f, lit);
            CreateProp(parent, "Plaza Debris B", pack.debris, new Vector2(5f, 6f), 0.20f, lit);

            // WAREHOUSE INTERIOR / APRON
            CreateProp(parent, "Warehouse Interior Stack A", stack, new Vector2(-28f, 10f), 0.54f, lit);
            CreateProp(parent, "Warehouse Interior Stack B", heavy, new Vector2(-21f, 14f), 0.58f, lit);
            CreateProp(parent, "Warehouse Interior Barrels", barrels, new Vector2(-16f, 9f), 0.48f, lit);
            CreateProp(parent, "Warehouse Loading Barricade", pack.barricade, new Vector2(-14f, 3f), 0.54f, lit);
            CreateProp(parent, "Warehouse Terminal", pack.terminal, new Vector2(-30f, 17f), 0.52f, lit);
            CreateProp(parent, "Warehouse Pipe Cluster", pack.pipeCluster, new Vector2(-17f, 17f), 0.48f, lit);

            // NORTH SERVICE LANE
            CreateProp(parent, "North Fence A", pack.fence, new Vector2(-10f, 25f), 0.54f, lit);
            CreateProp(parent, "North Fence B", pack.fence, new Vector2(10f, 25f), 0.54f, lit);
            CreateProp(parent, "North Ammo Box", pack.ammoBox, new Vector2(4f, 18f), 0.46f, lit);
            CreateProp(parent, "North Cable Bundle", pack.cableBundle, new Vector2(-4f, 22f), 0.44f, lit);

            // ADMINISTRATION INTERIOR / COURTYARD
            CreateProp(parent, "Admin Interior Cover A", heavy, new Vector2(20f, 8f), 0.54f, lit);
            CreateProp(parent, "Admin Interior Cover B", stack, new Vector2(29f, 13f), 0.48f, lit);
            CreateProp(parent, "Admin Archive Terminal", pack.terminal, new Vector2(31f, 17f), 0.56f, lit);
            CreateProp(parent, "Admin Medkit Box", pack.medkitBox, new Vector2(20f, 17f), 0.46f, lit);
            CreateProp(parent, "Admin Courtyard Barricade", pack.barricade, new Vector2(15f, 0f), 0.54f, lit);
            CreateProp(parent, "Admin Exterior Pipe", pack.pipeCluster, new Vector2(36f, 4f), 0.48f, lit);

            // BARRACKS / SOUTH ROAD
            CreateProp(parent, "Barracks Crate Stack", stack, new Vector2(22f, -18f), 0.50f, lit);
            CreateProp(parent, "Barracks Heavy Cover", heavy, new Vector2(29f, -20f), 0.54f, lit);
            CreateProp(parent, "Barracks Barrel Stack", barrels, new Vector2(31f, -13f), 0.46f, lit);
            CreateProp(parent, "South Road Barricade", pack.barricade, new Vector2(8f, -23f), 0.56f, lit);
            CreateProp(parent, "South Road Fuel Drum", pack.fuelDrum, new Vector2(3f, -18f), 0.50f, lit);

            CreateHazardStrip(
                parent,
                "West Gate Marking",
                new Vector2(-39f, -22f),
                new Vector2(4.8f, 0.30f),
                lit);

            CreateHazardStrip(
                parent,
                "Plaza Crossing",
                new Vector2(0f, -10f),
                new Vector2(7.5f, 0.30f),
                lit);

            CreateHazardStrip(
                parent,
                "Admin Entry Marking",
                new Vector2(15f, 2f),
                new Vector2(4.5f, 0.30f),
                lit);

            CreateStreetLight(parent, "West Gate Lamp A", pack.lamp, new Vector2(-37f, -17f), new Color(0.25f, 0.70f, 1f), lit, true);
            CreateStreetLight(parent, "West Gate Lamp B", pack.lamp, new Vector2(-28f, -26f), new Color(1f, 0.20f, 0.25f), lit, false);
            CreateStreetLight(parent, "Plaza Lamp North", pack.lamp, new Vector2(-7f, 7f), new Color(0.34f, 0.72f, 1f), lit, true);
            CreateStreetLight(parent, "Plaza Lamp South", pack.lamp, new Vector2(7f, -10f), new Color(1f, 0.18f, 0.25f), lit, true);
            CreateStreetLight(parent, "Warehouse Lamp", pack.lamp, new Vector2(-12f, 9f), new Color(0.25f, 0.80f, 1f), lit, false);
            CreateStreetLight(parent, "North Alley Lamp", pack.lamp, new Vector2(5f, 27f), new Color(0.34f, 0.66f, 1f), lit, true);
            CreateStreetLight(parent, "Admin Lamp A", pack.lamp, new Vector2(14f, 9f), new Color(1f, 0.20f, 0.30f), lit, true);
            CreateStreetLight(parent, "Admin Lamp B", pack.lamp, new Vector2(36f, 14f), new Color(0.34f, 0.70f, 1f), lit, false);
            CreateStreetLight(parent, "Barracks Lamp", pack.lamp, new Vector2(16f, -20f), new Color(0.28f, 0.65f, 1f), lit, true);
            CreateStreetLight(parent, "Workshop Lamp", pack.lamp, new Vector2(-13f, -16f), new Color(1f, 0.18f, 0.26f), lit, true);
        }

        private static void CreateStreetLight(
            Transform parent,
            string name,
            Sprite lampSprite,
            Vector2 position,
            Color color,
            Material material,
            bool unstable)
        {
            if (lampSprite == null)
                return;

            GameObject root =
                new GameObject(name);

            root.transform.SetParent(
                parent,
                false);

            root.transform.position =
                position;

            SpriteRenderer lamp =
                root.AddComponent<SpriteRenderer>();

            lamp.sprite =
                lampSprite;

            lamp.color =
                Color.white;

            root.transform.localScale =
                Vector3.one *
                0.46f;

            if (material != null)
                lamp.sharedMaterial = material;

            DepthSortedSprite2D depth =
                root.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { lamp },
                5000,
                0f);

            GameObject glowGo =
                new GameObject(
                    "Light Spill");

            glowGo.transform.SetParent(
                root.transform,
                false);

            glowGo.transform.localPosition =
                new Vector3(
                    0f,
                    -0.30f,
                    0f);

            SpriteRenderer glow =
                glowGo.AddComponent<SpriteRenderer>();

            glow.sprite =
                GeneratedArtFactory.Get(
                    "soft_glow");

            glow.color =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.25f);

            glow.sortingOrder =
                -1;

            glowGo.transform.localScale =
                new Vector3(
                    5.8f,
                    4.5f,
                    1f);

            Light2D light =
                glowGo.AddComponent<Light2D>();

            light.lightType =
                Light2D.LightType.Point;

            light.color =
                color;

            light.intensity =
                0.88f;

            light.pointLightOuterRadius =
                5.2f;

            light.pointLightInnerRadius =
                1.0f;

            if (unstable)
            {
                LightFlicker2D flicker =
                    glowGo.AddComponent<
                        LightFlicker2D>();

                flicker.Configure(
                    light,
                    glow,
                    0.88f,
                    0.34f);
            }
        }

        private static void AddFogPass(
            Transform parent,
            ProductionArtPack pack)
        {
            Sprite smoke =
                pack.smoke != null
                    ? pack.smoke
                    : GeneratedArtFactory.Get("smoke");

            if (smoke == null)
                return;

            CreateFogPatch(
                parent,
                "Start Low Fog",
                smoke,
                new Vector2(-15.5f, -6.5f),
                new Vector3(2.4f, 1.2f, 1f),
                new Color(0.34f, 0.28f, 0.58f, 0.16f),
                780,
                new Vector2(0.010f, 0.002f));

            CreateFogPatch(
                parent,
                "Start Neon Fog",
                smoke,
                new Vector2(-10.0f, -3.8f),
                new Vector3(1.8f, 1.0f, 1f),
                new Color(0.68f, 0.18f, 0.60f, 0.12f),
                790,
                new Vector2(-0.008f, 0.003f));

            CreateFogPatch(
                parent,
                "Warehouse Fog",
                smoke,
                new Vector2(0.0f, 1.5f),
                new Vector3(2.8f, 1.35f, 1f),
                new Color(0.28f, 0.46f, 0.70f, 0.13f),
                800,
                new Vector2(0.006f, 0.002f));

            CreateFogPatch(
                parent,
                "Admin Fog",
                smoke,
                new Vector2(14.0f, 2.8f),
                new Vector3(2.5f, 1.3f, 1f),
                new Color(0.62f, 0.18f, 0.48f, 0.12f),
                810,
                new Vector2(-0.006f, 0.003f));

            CreateFogPatch(
                parent,
                "Foreground Rolling Fog",
                smoke,
                new Vector2(-2.0f, -7.0f),
                new Vector3(3.8f, 1.8f, 1f),
                new Color(0.34f, 0.28f, 0.54f, 0.10f),
                7600,
                new Vector2(0.004f, 0.001f));
        }

        private static void CreateFogPatch(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector3 scale,
            Color color,
            int order,
            Vector2 drift)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.localScale =
                scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = order;

            FogDrift2D fog =
                go.AddComponent<FogDrift2D>();

            fog.Configure(
                drift,
                0.12f,
                0.025f);
        }

        private static void AddFloorDetailPass(
            Transform parent,
            ProductionArtPack pack)
        {
            if (pack.bloodDrop != null)
            {
                CreateFloorDecal(
                    parent,
                    "Blood A",
                    pack.bloodDrop,
                    new Vector2(-12.8f, -5.9f),
                    new Vector2(0.85f, 0.46f),
                    -18f,
                    new Color(0.46f, 0.02f, 0.05f, 0.84f));

                CreateFloorDecal(
                    parent,
                    "Blood B",
                    pack.bloodDrop,
                    new Vector2(-4.4f, -1.5f),
                    new Vector2(0.72f, 0.38f),
                    31f,
                    new Color(0.42f, 0.015f, 0.04f, 0.78f));

                CreateFloorDecal(
                    parent,
                    "Blood C",
                    pack.bloodDrop,
                    new Vector2(6.4f, 0.4f),
                    new Vector2(0.95f, 0.48f),
                    -42f,
                    new Color(0.50f, 0.02f, 0.06f, 0.78f));

                CreateFloorDecal(
                    parent,
                    "Blood D",
                    pack.bloodDrop,
                    new Vector2(15.2f, 3.5f),
                    new Vector2(0.70f, 0.36f),
                    12f,
                    new Color(0.44f, 0.018f, 0.05f, 0.78f));
            }

            if (pack.bulletHole != null)
            {
                for (int i = 0; i < 7; i++)
                {
                    CreateFloorDecal(
                        parent,
                        "Impact Mark " + i,
                        pack.bulletHole,
                        new Vector2(
                            -16.5f + i * 5.2f,
                            -7.6f + Mathf.Sin(i * 2.1f) * 2.0f),
                        new Vector2(0.18f, 0.18f),
                        i * 29f,
                        new Color(0.20f, 0.18f, 0.22f, 0.66f));
                }
            }
        }

        private static void CreateFloorDecal(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 scale,
            float rotation,
            Color color)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            go.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    rotation);

            go.transform.localScale =
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 1160;
        }

        private static void AddLightingPass(
            Transform parent,
            Material lit)
        {
            int defaultLayer =
                SortingLayer.NameToID("Default");

            GameObject globalGo =
                new GameObject("Global 2D Light");

            globalGo.transform.SetParent(
                parent,
                false);

            Light2D global =
                globalGo.AddComponent<Light2D>();

            global.lightType =
                Light2D.LightType.Global;

            global.color =
                new Color(
                    0.43f,
                    0.48f,
                    0.72f,
                    1f);

            global.intensity = 0.58f;
            global.targetSortingLayers =
                new[] { defaultLayer };

            CreatePointLight(
                parent,
                "Cyan Start Light",
                new Vector2(-16.2f, -4.0f),
                new Color(0.10f, 0.70f, 1.0f),
                1.05f,
                5.8f,
                false,
                defaultLayer);

            CreatePointLight(
                parent,
                "Red Start Alarm",
                new Vector2(-9.2f, -2.9f),
                new Color(1.0f, 0.08f, 0.15f),
                1.25f,
                4.2f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Warehouse Cyan",
                new Vector2(0.8f, 4.5f),
                new Color(0.15f, 0.82f, 0.95f),
                1.10f,
                6.6f,
                false,
                defaultLayer);

            CreatePointLight(
                parent,
                "Warehouse Magenta",
                new Vector2(5.0f, 2.1f),
                new Color(0.95f, 0.13f, 0.72f),
                0.95f,
                5.2f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Admin Red",
                new Vector2(12.8f, 5.2f),
                new Color(1.0f, 0.08f, 0.12f),
                1.22f,
                5.3f,
                true,
                defaultLayer);

            CreatePointLight(
                parent,
                "Admin Cold",
                new Vector2(18.1f, 0.8f),
                new Color(0.22f, 0.56f, 1.0f),
                0.92f,
                5.8f,
                false,
                defaultLayer);
        }

        private static void CreatePointLight(
            Transform parent,
            string name,
            Vector2 position,
            Color color,
            float intensity,
            float radius,
            bool dropout,
            int sortingLayer)
        {
            GameObject go =
                new GameObject(name);

            go.transform.SetParent(
                parent,
                false);

            go.transform.position =
                position;

            Light2D light =
                go.AddComponent<Light2D>();

            light.lightType =
                Light2D.LightType.Point;

            light.color = color;
            light.intensity = intensity;
            light.pointLightInnerRadius =
                radius * 0.16f;

            light.pointLightOuterRadius =
                radius;

            light.falloffIntensity = 0.62f;
            light.overlapOperation =
                Light2D.OverlapOperation.Additive;

            light.targetSortingLayers =
                new[] { sortingLayer };

            GameObject glow =
                new GameObject("Glow");

            glow.transform.SetParent(
                go.transform,
                false);

            glow.transform.localScale =
                Vector3.one *
                radius *
                0.45f;

            SpriteRenderer sr =
                glow.AddComponent<SpriteRenderer>();

            sr.sprite =
                GeneratedArtFactory.Get(
                    "soft_glow");

            sr.color =
                new Color(
                    color.r,
                    color.g,
                    color.b,
                    0.10f);

            sr.sortingOrder = 620;

            NeonLightFlicker2D flicker =
                go.AddComponent<
                    NeonLightFlicker2D>();

            flicker.Configure(
                light,
                sr,
                intensity,
                dropout ? 0.16f : 0.06f,
                dropout ? 16f : 7f,
                dropout);
        }

        private static void CreateHazardStrip(
            Transform parent,
            string name,
            Vector2 position,
            Vector2 scale,
            Material material)
        {
            Sprite sprite =
                GeneratedArtFactory.Get("hazard");

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
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color =
                new Color(
                    0.86f,
                    0.78f,
                    0.62f,
                    0.88f);

            sr.sortingOrder = 1230;

            if (material != null)
                sr.sharedMaterial = material;
        }

        private static void CreateStructure(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            Vector2 scale,
            float rotation,
            bool flipX,
            int baseOrder,
            Material material)
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
                new Vector3(
                    scale.x,
                    scale.y,
                    1f);

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.flipX = flipX;
            sr.sortingOrder = 0;

            if (material != null)
                sr.sharedMaterial = material;

            DepthSortedSprite2D depth =
                go.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                5000,
                0f);

            AddFootprintCollider(
                go,
                sprite,
                0.96f,
                0.28f,
                0.01f);

            ThreeQuarterOccluder2D occ =
                go.AddComponent<
                    ThreeQuarterOccluder2D>();
        }

        private static void CreateProp(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 position,
            float scale,
            Material material)
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
                Vector3.one * scale;

            SpriteRenderer sr =
                go.AddComponent<SpriteRenderer>();

            sr.sprite = sprite;
            sr.color = Color.white;
            sr.sortingOrder = 0;

            if (material != null)
                sr.sharedMaterial = material;

            DepthSortedSprite2D depth =
                go.AddComponent<
                    DepthSortedSprite2D>();

            depth.Configure(
                new[] { sr },
                5000,
                0f);

            if (IsSolidProp(name))
            {
                AddFootprintCollider(
                    go,
                    sprite,
                    0.88f,
                    0.46f,
                    0.005f);
            }
        }

        private static bool IsSolidProp(
            string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            return
                name.Contains("Cover") ||
                name.Contains("Crate") ||
                name.Contains("Barrel") ||
                name.Contains("Fuel Drum") ||
                name.Contains("Barricade") ||
                name.Contains("Terminal") ||
                name.Contains("Fence") ||
                name.Contains("Pipe") ||
                name.Contains("Ammo Box") ||
                name.Contains("Medkit Box");
        }

        private static void AddFootprintCollider(
            GameObject go,
            Sprite sprite,
            float widthFraction,
            float heightFraction,
            float verticalInsetFraction)
        {
            if (go == null ||
                sprite == null)
            {
                return;
            }

            int obstacleLayer =
                LayerMask.NameToLayer(
                    "Obstacles");

            if (obstacleLayer >= 0)
                go.layer = obstacleLayer;

            Bounds bounds =
                sprite.bounds;

            float width =
                Mathf.Max(
                    0.28f,
                    bounds.size.x *
                    Mathf.Clamp01(
                        widthFraction));

            float height =
                Mathf.Clamp(
                    bounds.size.y *
                    Mathf.Clamp01(
                        heightFraction),
                    0.22f,
                    0.90f);

            float inset =
                bounds.size.y *
                Mathf.Max(
                    0f,
                    verticalInsetFraction);

            BoxCollider2D collider =
                go.AddComponent<BoxCollider2D>();

            collider.size =
                new Vector2(
                    width,
                    height);

            collider.offset =
                new Vector2(
                    bounds.center.x,
                    bounds.min.y +
                    inset +
                    height * 0.5f);
        }

        private static void ApplyMaterialToConceptSprites(
            Material material)
        {
            if (material == null)
                return;

            GameObject polishRoot =
                GameObject.Find(RootName);

            Transform polishTransform =
                polishRoot != null
                    ? polishRoot.transform
                    : null;

            foreach (SpriteRenderer sr in
                     Object.FindObjectsByType<SpriteRenderer>(
                         FindObjectsSortMode.None))
            {
                if (sr == null ||
                    sr.sprite == null)
                {
                    continue;
                }

                string spritePath =
                    AssetDatabase.GetAssetPath(
                        sr.sprite);

                bool generatedConcept =
                    spritePath.Contains(
                        "IntegratedConcept");

                bool sourceConcept =
                    spritePath.Contains(
                        "ConceptAtlases");

                bool conceptRuntime =
                    sr.gameObject.name.Contains(
                        "Concept Atlas Visual");

                bool stableCharacter =
                    sr.GetComponent<ThreeQuarterCharacterVisual2D>() != null ||
                    sr.GetComponentInParent<ThreeQuarterCharacterVisual2D>() != null ||
                    sr.gameObject.name.Contains("3-4 Visual");

                bool stableFloor =
                    sr.gameObject.name == "Concept Floor";

                if (stableCharacter || stableFloor)
                    continue;

                bool conceptPolish =
                    polishTransform != null &&
                    sr.transform.IsChildOf(
                        polishTransform);

                if (generatedConcept ||
                    sourceConcept ||
                    conceptRuntime ||
                    conceptPolish)
                {
                    sr.sharedMaterial =
                        material;
                }
            }
        }
    }
}
#endif
