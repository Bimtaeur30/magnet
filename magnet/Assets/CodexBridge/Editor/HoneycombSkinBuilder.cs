#if UNITY_EDITOR
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Presentation;
using Magnet.Core.SO.Skin;
using PTY.Scripts.Vfx;
using UnityEditor;
using UnityEngine;

namespace CodexBridge
{
    [InitializeOnLoad]
    public static class HoneycombSkinBuilder
    {
        const string TexturePath = "Assets/_MemberWorkspace/JTH/Graphics/Sprites/HoneycombBlocks.png";
        const string Root = "Assets/_MemberWorkspace/JTH/HoneycombSkin";
        const string SkinPath = "Assets/_Shared/ScriptableObjects/Skins/Honeycomb.asset";
        const string SkinListPath = "Assets/_MemberWorkspace/JTH/ScriptableObjects/test/Skin data list.asset";

        static readonly Color[] Colors =
        {
            new(0.72f, 0.28f, 0.04f),
            new(0.95f, 0.58f, 0.08f),
            new(1.00f, 0.82f, 0.28f)
        };

        static HoneycombSkinBuilder()
        {
            if (AssetDatabase.LoadAssetAtPath<SkinDataSO>(SkinPath) == null)
                EditorApplication.delayCall += Build;
        }

        [MenuItem("Tools/Codex/Build Honeycomb Skin")]
        public static void Build()
        {
            EnsureFolders();
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureSprites();
            AnimationClip hint = CreateHintClip();
            Shader burstShader = AssetDatabase.LoadAssetAtPath<Shader>(
                "Assets/_MemberWorkspace/JTH/Graphics/Shaders/HoneyBurst.shader");
            if (burstShader == null)
                throw new System.InvalidOperationException("HoneyBurst shader import failed.");

            Sprite[] sprites = LoadSprites();
            var effects = new PoolItemSO[3];
            for (int i = 0; i < 3; i++)
                effects[i] = CreateBurst(i, burstShader);
            RegisterPoolItems(effects);
            SkinDataSO skin = CreateSkin(sprites, hint, effects);
            RegisterSkin(skin);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Codex] Honeycomb SkinDataSO, squash hint, and honey splat effects created.");
        }

        static void EnsureFolders()
        {
            string[] folders = { Root, Root + "/Animations", Root + "/Materials", Root + "/Prefabs", Root + "/Pool" };
            foreach (string folder in folders)
            {
                string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
                string name = System.IO.Path.GetFileName(folder);
                if (!AssetDatabase.IsValidFolder(folder))
                    AssetDatabase.CreateFolder(parent, name);
            }
        }

        static void ConfigureSprites()
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.filterMode = FilterMode.Bilinear;
            importer.spritePixelsPerUnit = 310f;

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            float cellW = texture.width / 3f;
            float cellH = texture.height;
            var sheet = new SpriteMetaData[3];
            for (int i = 0; i < 3; i++)
            {
                sheet[i] = new SpriteMetaData
                {
                    name = $"HoneycombBlocks_{i}",
                    rect = new Rect(i * cellW, 0, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                };
            }

            importer.spritesheet = sheet;
            importer.SaveAndReimport();
        }

        static Sprite[] LoadSprites()
        {
            var result = new Sprite[3];
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(TexturePath))
            {
                if (asset is Sprite sprite &&
                    int.TryParse(sprite.name[(sprite.name.LastIndexOf('_') + 1)..], out int i) &&
                    i < 3)
                    result[i] = sprite;
            }

            return result;
        }

        static AnimationClip CreateHintClip()
        {
            string path = Root + "/Animations/HoneycombHintSquash.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path) ??
                       new AnimationClip { name = "HoneycombHintSquash", frameRate = 60f };
            if (!AssetDatabase.Contains(clip))
                AssetDatabase.CreateAsset(clip, path);
            clip.wrapMode = WrapMode.Loop;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve("", typeof(BlockShatterHint), "squash"),
                new AnimationCurve(
                    new Keyframe(0f, 0.34f),
                    new Keyframe(0.32f, 1f),
                    new Keyframe(0.72f, 0.42f),
                    new Keyframe(1.08f, 0.86f),
                    new Keyframe(1.45f, 0.34f)));
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"),
                new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1.45f, 1f)));
            AnimationUtility.SetEditorCurve(
                clip,
                EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"),
                new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1.45f, 1f)));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        static PoolItemSO CreateBurst(int index, Shader shader)
        {
            string matPath = $"{Root}/Materials/HoneyBurst_{index}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(matPath) ?? new Material(shader);
            material.shader = shader;
            material.SetColor("_Tint", Colors[index]);
            if (!AssetDatabase.Contains(material))
                AssetDatabase.CreateAsset(material, matPath);

            var go = new GameObject($"HoneyBurst_{index}");
            var particle = go.AddComponent<ParticleSystem>();
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            ConfigureHoneySpread(particle, renderer);
            var pooled = go.AddComponent<PooledParticleEffect>();
            var serialized = new SerializedObject(pooled);
            serialized.FindProperty("rootParticleSystem").objectReferenceValue = particle;
            serialized.FindProperty("particleRenderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{Root}/Prefabs/HoneyBurst_{index}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            string itemPath = $"{Root}/Pool/HoneyBurst_{index}.asset";
            var item = AssetDatabase.LoadAssetAtPath<PoolItemSO>(itemPath) ??
                       ScriptableObject.CreateInstance<PoolItemSO>();
            item.name = $"HoneyBurst_{index}";
            item.itemName = item.name;
            item.prefab = prefab;
            item.initCount = 12;
            if (!AssetDatabase.Contains(item))
                AssetDatabase.CreateAsset(item, itemPath);
            var prefabPooled = prefab.GetComponent<PooledParticleEffect>();
            var itemProperty = new SerializedObject(prefabPooled);
            itemProperty.FindProperty("<Item>k__BackingField").objectReferenceValue = item;
            itemProperty.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SavePrefabAsset(prefab);
            EditorUtility.SetDirty(item);
            return item;
        }

        static void ConfigureHoneySpread(ParticleSystem particle, ParticleSystemRenderer renderer)
        {
            var main = particle.main;
            main.duration = 0.95f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.42f, 0.82f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(1.35f, 2.85f);
            main.startSize3D = true;
            main.startSizeX = new ParticleSystem.MinMaxCurve(0.11f, 0.22f);
            main.startSizeY = new ParticleSystem.MinMaxCurve(0.16f, 0.34f);
            main.startSizeZ = 0.1f;
            main.gravityModifier = new ParticleSystem.MinMaxCurve(0.38f, 0.62f);
            main.maxParticles = 36;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.283185f);

            var emission = particle.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 20, 30) });

            var shape = particle.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.1f;
            shape.radiusThickness = 1f;

            var velocity = particle.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.y = new ParticleSystem.MinMaxCurve(-0.12f, -0.38f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f, 0f);
            velocity.radial = new ParticleSystem.MinMaxCurve(0.35f, 0.85f);

            var limit = particle.limitVelocityOverLifetime;
            limit.enabled = true;
            limit.dampen = 0.42f;
            limit.limit = 1.6f;

            var noise = particle.noise;
            noise.enabled = true;
            noise.strength = 0.22f;
            noise.frequency = 0.9f;
            noise.scrollSpeed = 0.25f;

            var size = particle.sizeOverLifetime;
            size.enabled = true;
            size.separateAxes = true;
            size.x = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.55f), new Keyframe(0.22f, 1.2f), new Keyframe(1f, 1.35f)));
            size.y = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.7f), new Keyframe(0.18f, 1.05f), new Keyframe(1f, 0.28f)));
            size.z = 1f;

            var color = particle.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(new Gradient
            {
                colorKeys = new[]
                {
                    new GradientColorKey(Color.white, 0f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.25f), 0.28f),
                    new GradientColorKey(new Color(0.45f, 0.16f, 0.02f), 1f)
                },
                alphaKeys = new[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.9f, 0.55f),
                    new GradientAlphaKey(0f, 1f)
                }
            });

            var trails = particle.trails;
            trails.enabled = true;
            trails.ratio = 0.72f;
            trails.lifetime = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            trails.dieWithParticles = true;
            trails.widthOverTrail = new ParticleSystem.MinMaxCurve(
                1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0f)));

            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.sortingOrder = 55;
            renderer.trailMaterial = renderer.sharedMaterial;
        }

        static void RegisterPoolItems(PoolItemSO[] effects)
        {
            var manager = AssetDatabase.LoadAssetAtPath<PoolManagerSO>("Assets/GameLib/ObjectPool/PoolManager.asset");
            foreach (PoolItemSO item in effects)
            {
                if (!manager.itemList.Contains(item))
                    manager.itemList.Add(item);
            }

            EditorUtility.SetDirty(manager);
        }

        static SkinDataSO CreateSkin(Sprite[] sprites, AnimationClip hint, PoolItemSO[] effects)
        {
            var skin = AssetDatabase.LoadAssetAtPath<SkinDataSO>(SkinPath) ??
                       ScriptableObject.CreateInstance<SkinDataSO>();
            if (!AssetDatabase.Contains(skin))
                AssetDatabase.CreateAsset(skin, SkinPath);
            var so = new SerializedObject(skin);
            so.FindProperty("<SkinName>k__BackingField").stringValue = "꿀벌집";
            so.FindProperty("<SkinId>k__BackingField").stringValue = "Honeycomb";
            so.FindProperty("<RandomizeSprites>k__BackingField").boolValue = true;
            SetArray(so.FindProperty("<Sprites>k__BackingField"), sprites);
            SetArray(so.FindProperty("<HintClips>k__BackingField"), new[] { hint, hint, hint });
            SetArray(so.FindProperty("<LineClearEffects>k__BackingField"), effects);
            so.FindProperty("<FireCenteredLineClear>k__BackingField").boolValue = false;
            so.FindProperty("icon").objectReferenceValue = sprites[0];
            so.FindProperty("unlockType").enumValueIndex = 0;
            so.FindProperty("unlockValue").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            return skin;
        }

        static void RegisterSkin(SkinDataSO skin)
        {
            var list = AssetDatabase.LoadAssetAtPath<SkinDataListSO>(SkinListPath);
            var so = new SerializedObject(list);
            SerializedProperty skins = so.FindProperty("<Skins>k__BackingField");
            for (int i = 0; i < skins.arraySize; i++)
            {
                if (skins.GetArrayElementAtIndex(i).objectReferenceValue == skin)
                    return;
            }

            skins.arraySize++;
            skins.GetArrayElementAtIndex(skins.arraySize - 1).objectReferenceValue = skin;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(list);
        }

        static void SetArray<T>(SerializedProperty property, T[] values) where T : Object
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
