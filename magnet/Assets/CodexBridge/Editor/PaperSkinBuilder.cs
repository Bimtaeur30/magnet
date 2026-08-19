#if UNITY_EDITOR
using System.Collections.Generic;
using GameLib.ObjectPool.Runtime;
using JTH.Scripts.Presentation;
using Magnet.Core.SO.Skin;
using PTY.Scripts.Vfx;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CodexBridge
{
    [InitializeOnLoad]
    public static class PaperSkinBuilder
    {
        const string TexturePath = "Assets/_MemberWorkspace/JTH/Graphics/Sprites/PaperBlocks.png";
        const string Root = "Assets/_MemberWorkspace/JTH/PaperSkin";
        static readonly Color[] Colors =
        {
            new(.92f, .86f, .72f), new(.58f, .37f, .19f), new(.68f, .68f, .66f), new(.40f, .65f, .82f),
            new(.86f, .43f, .38f), new(.78f, .57f, .20f), new(.46f, .62f, .38f), new(.62f, .45f, .68f)
        };

        static PaperSkinBuilder()
        {
            if (AssetDatabase.LoadAssetAtPath<SkinDataSO>("Assets/_Shared/ScriptableObjects/Skins/Paper.asset") == null)
                EditorApplication.delayCall += Build;
        }

        [MenuItem("Tools/Codex/Build Paper Skin")]
        public static void Build()
        {
            EnsureFolders();
            AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureSprites();
            AnimationClip hint = CreateHintClip();
            Shader burstShader = AssetDatabase.LoadAssetAtPath<Shader>(
                "Assets/_MemberWorkspace/JTH/Graphics/Shaders/PaperBurst.shader");
            if (burstShader == null) throw new System.InvalidOperationException("PaperBurst shader import failed.");

            Sprite[] sprites = LoadSprites();
            var effects = new PoolItemSO[8];
            for (int i = 0; i < 8; i++) effects[i] = CreateBurst(i, burstShader);
            RegisterPoolItems(effects);
            CreateSkin(sprites, hint, effects);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Codex] Paper SkinDataSO, sprites, flutter hint, and confetti effects created.");
        }

        static void EnsureFolders()
        {
            string[] folders = { Root, Root + "/Animations", Root + "/Materials", Root + "/Prefabs", Root + "/Pool" };
            foreach (string folder in folders)
            {
                string parent = System.IO.Path.GetDirectoryName(folder)?.Replace('\\', '/');
                string name = System.IO.Path.GetFileName(folder);
                if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder(parent, name);
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
            importer.spritePixelsPerUnit = 390f;

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
            float cellW = texture.width / 4f;
            float cellH = texture.height / 2f;
            var sheet = new SpriteMetaData[8];
            for (int row = 0; row < 2; row++)
            for (int col = 0; col < 4; col++)
            {
                int index = row * 4 + col;
                sheet[index] = new SpriteMetaData
                {
                    name = $"PaperBlocks_{index}",
                    rect = new Rect(col * cellW, (1 - row) * cellH, cellW, cellH),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(.5f, .5f)
                };
            }
            importer.spritesheet = sheet;
            importer.SaveAndReimport();
        }

        static Sprite[] LoadSprites()
        {
            var result = new Sprite[8];
            foreach (Object asset in AssetDatabase.LoadAllAssetsAtPath(TexturePath))
                if (asset is Sprite sprite && int.TryParse(sprite.name[(sprite.name.LastIndexOf('_') + 1)..], out int i) && i < 8)
                    result[i] = sprite;
            return result;
        }

        static AnimationClip CreateHintClip()
        {
            string path = Root + "/Animations/PaperHintFlutter.anim";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path) ?? new AnimationClip { name = "PaperHintFlutter", frameRate = 60f };
            if (!AssetDatabase.Contains(clip)) AssetDatabase.CreateAsset(clip, path);
            clip.wrapMode = WrapMode.Loop;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(BlockShatterHint), "waterWobble"),
                new AnimationCurve(new Keyframe(0, .12f), new Keyframe(.12f, .58f), new Keyframe(.25f, .22f), new Keyframe(.39f, .66f), new Keyframe(.56f, .12f)));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.x"),
                new AnimationCurve(new Keyframe(0, 1), new Keyframe(.12f, 1.035f), new Keyframe(.25f, .975f), new Keyframe(.39f, 1.025f), new Keyframe(.56f, 1)));
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Transform), "m_LocalScale.y"),
                new AnimationCurve(new Keyframe(0, 1), new Keyframe(.12f, .98f), new Keyframe(.25f, 1.025f), new Keyframe(.39f, .985f), new Keyframe(.56f, 1)));
            EditorUtility.SetDirty(clip);
            return clip;
        }

        static PoolItemSO CreateBurst(int index, Shader shader)
        {
            string matPath = $"{Root}/Materials/PaperBurst_{index}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(matPath) ?? new Material(shader);
            material.shader = shader;
            material.SetColor("_Tint", Colors[index]);
            if (!AssetDatabase.Contains(material)) AssetDatabase.CreateAsset(material, matPath);

            var go = new GameObject($"PaperBurst_{index}");
            var particle = go.AddComponent<ParticleSystem>();
            var main = particle.main;
            main.duration = .62f;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(.48f, .92f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(.8f, 2.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(.08f, .22f);
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1,1,1,.72f), Color.white);
            main.gravityModifier = .16f;
            main.maxParticles = 32;
            var emission = particle.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0, 18, 26) });
            var shape = particle.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = .18f;
            var size = particle.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0, .2f), new Keyframe(.12f, 1f), new Keyframe(1, .1f)));
            var color = particle.colorOverLifetime;
            color.enabled = true;
            color.color = new ParticleSystem.MinMaxGradient(new Gradient
            {
                colorKeys = new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Colors[index], 1) },
                alphaKeys = new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(.8f, .55f), new GradientAlphaKey(0, 1) }
            });
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingOrder = 55;
            var pooled = go.AddComponent<PooledParticleEffect>();
            var serialized = new SerializedObject(pooled);
            serialized.FindProperty("rootParticleSystem").objectReferenceValue = particle;
            serialized.FindProperty("particleRenderer").objectReferenceValue = renderer;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            string prefabPath = $"{Root}/Prefabs/PaperBurst_{index}.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);

            string itemPath = $"{Root}/Pool/PaperBurst_{index}.asset";
            var item = AssetDatabase.LoadAssetAtPath<PoolItemSO>(itemPath) ?? ScriptableObject.CreateInstance<PoolItemSO>();
            item.name = $"PaperBurst_{index}";
            item.itemName = item.name;
            item.prefab = prefab;
            item.initCount = 12;
            if (!AssetDatabase.Contains(item)) AssetDatabase.CreateAsset(item, itemPath);
            var prefabPooled = prefab.GetComponent<PooledParticleEffect>();
            var itemProperty = new SerializedObject(prefabPooled);
            itemProperty.FindProperty("<Item>k__BackingField").objectReferenceValue = item;
            itemProperty.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SavePrefabAsset(prefab);
            EditorUtility.SetDirty(item);
            return item;
        }

        static void RegisterPoolItems(PoolItemSO[] effects)
        {
            var manager = AssetDatabase.LoadAssetAtPath<PoolManagerSO>("Assets/GameLib/ObjectPool/PoolManager.asset");
            foreach (PoolItemSO item in effects) if (!manager.itemList.Contains(item)) manager.itemList.Add(item);
            EditorUtility.SetDirty(manager);
        }

        static void CreateSkin(Sprite[] sprites, AnimationClip hint, PoolItemSO[] effects)
        {
            string path = "Assets/_Shared/ScriptableObjects/Skins/Paper.asset";
            var skin = AssetDatabase.LoadAssetAtPath<SkinDataSO>(path) ?? ScriptableObject.CreateInstance<SkinDataSO>();
            if (!AssetDatabase.Contains(skin)) AssetDatabase.CreateAsset(skin, path);
            var so = new SerializedObject(skin);
            so.FindProperty("<SkinName>k__BackingField").stringValue = "종이";
            so.FindProperty("<SkinId>k__BackingField").stringValue = "Paper";
            SetArray(so.FindProperty("<Sprites>k__BackingField"), sprites);
            SetArray(so.FindProperty("<HintClips>k__BackingField"), new[] { hint, hint, hint, hint, hint, hint, hint, hint });
            SetArray(so.FindProperty("<LineClearEffects>k__BackingField"), effects);
            so.FindProperty("<FireCenteredLineClear>k__BackingField").boolValue = false;
            so.FindProperty("icon").objectReferenceValue = sprites[0];
            so.FindProperty("unlockType").enumValueIndex = 0;
            so.FindProperty("unlockValue").intValue = 1;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
        }

        static void SetArray<T>(SerializedProperty property, T[] values) where T : Object
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
