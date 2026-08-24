#if UNITY_EDITOR
using System;
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
    public static class RecommendedThemeSkinBuilder
    {
        const string SpriteRoot = "Assets/_MemberWorkspace/JTH/Graphics/Sprites/Themes";
        const string Root = "Assets/_MemberWorkspace/JTH/RecommendedThemes";
        const string ShaderPath = "Assets/_MemberWorkspace/JTH/Graphics/Shaders/ThemeBurst.shader";

        [InitializeOnLoadMethod]
        static void UpgradeLavaFlowWhenNeeded()
        {
            const string path = Root + "/Lava/Prefabs/LavaBurst_0.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && !prefab.GetComponent<ParticleSystem>().main.startSize3D)
                EditorApplication.delayCall += UpdateLavaLineClearEffects;
        }

        [InitializeOnLoadMethod]
        static void UpgradeCloudEvaporationWhenNeeded()
        {
            const string path = Root + "/Cloud/Prefabs/CloudBurst_0.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null && !prefab.GetComponent<ParticleSystem>().main.startSize3D)
                EditorApplication.delayCall += UpdateCloudEvaporationEffects;
        }

        sealed class Theme
        {
            public string Id, Name;
            public int Mode;
            public bool Crack;
            public Color[] Colors;
        }

        static readonly Theme[] Themes =
        {
            T("Ice","얼음",0,true, "163b73,176bb0,169ddd,30c9ed,66ddf2,91e9f5,bceff7,eafcff"),
            T("Lava","용암",1,false,"4b1712,7a1d12,b82c0d,e9490b,f56d0a,ff9812,ffc326,ffe43b"),
            T("Cloud","구름",2,false,"f3f7fa,75cafa,a9b8c8,374a67,f7b59f,f29a46,a58ddb,28549a"),
            T("Hologram","홀로그램",3,false,"19e8ef,1687ff,6e50ff,d82ded,ff39a6,45e954,63f5d5,ffb522"),
            T("Galaxy","우주",4,false,"0d2b7d,1744c8,16b8d9,7029b6,dc278f,25ad75,e4a725,aab4cb"),
            T("Chocolate","초콜릿",5,false,"321710,512317,854327,c7793c,f4dfad,c94e69,87c99a,e8dfca"),
            T("Candy","사탕",6,true,"ef2638,f47718,f5cf21,73ca2a,2676ed,25cbd0,872bd1,ef5c9b"),
            T("Wood","나무",7,true,"4b2718,a2632e,d8a869,e7dcc0,b65c32,7f2d20,56633d,82909a"),
            T("Fabric","천",8,false,"e5dbc2,315f91,c92737,d49c20,83a66b,18a6c9,a784c5,444447"),
            T("Ink","잉크",9,false,"17171a,183474,1554cc,20bada,198849,d52825,6f24b5,d4a524")
        };

        [MenuItem("Tools/Codex/Build 10 Recommended Theme Skins")]
        public static void Build()
        {
            EnsureFolder(Root);
            Shader shader=AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
            if(shader==null) throw new InvalidOperationException("ThemeBurst shader is missing.");
            var allItems=new List<PoolItemSO>();
            foreach(Theme theme in Themes)
            {
                string themeRoot=$"{Root}/{theme.Id}";
                foreach(string child in new[]{"Animations","Materials","Prefabs","Pool"}) EnsureFolder($"{themeRoot}/{child}");
                string texturePath=$"{SpriteRoot}/{theme.Id}Blocks.png";
                ConfigureSprites(texturePath,theme.Id);
                Sprite[] sprites=LoadSprites(texturePath);
                AnimationClip hint=CreateHint(theme,themeRoot);
                var effects=new PoolItemSO[8];
                for(int i=0;i<8;i++){ effects[i]=CreateBurst(theme,i,themeRoot,shader); allItems.Add(effects[i]); }
                CreateSkin(theme,sprites,hint,effects);
            }
            Register(allItems);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Codex] Built 10 flat recommended theme skins and 80 pooled effects.");
        }

        [MenuItem("Tools/Codex/Update Lava Flow Line Clear Effects")]
        public static void UpdateLavaLineClearEffects()
        {
            for (int i = 0; i < 8; i++)
            {
                string path = $"{Root}/Lava/Prefabs/LavaBurst_{i}.prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                var ps = root.GetComponent<ParticleSystem>();
                ConfigureLavaFlow(ps, root.GetComponent<ParticleSystemRenderer>());
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Codex] Updated Lava line-clear effects to downward melting flow.");
        }

        [MenuItem("Tools/Codex/Update Cloud Evaporation Line Clear Effects")]
        public static void UpdateCloudEvaporationEffects()
        {
            for (int i = 0; i < 8; i++)
            {
                string path = $"{Root}/Cloud/Prefabs/CloudBurst_{i}.prefab";
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                ConfigureCloudEvaporation(root.GetComponent<ParticleSystem>(), root.GetComponent<ParticleSystemRenderer>());
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Codex] Updated Cloud line-clear effects to rising evaporation.");
        }

        public static void UpdateSlimeAndCloudLineClearEffects()
        {
            SlimeSkinBuilder.UpdateSlimeFlowLineClearEffects();
            UpdateCloudEvaporationEffects();
        }

        static Theme T(string id,string name,int mode,bool crack,string csv)
        {
            string[] values=csv.Split(','); var colors=new Color[values.Length];
            for(int i=0;i<values.Length;i++) ColorUtility.TryParseHtmlString("#"+values[i],out colors[i]);
            return new Theme{Id=id,Name=name,Mode=mode,Crack=crack,Colors=colors};
        }

        static void EnsureFolder(string folder)
        {
            if(AssetDatabase.IsValidFolder(folder)) return;
            string parent=System.IO.Path.GetDirectoryName(folder)?.Replace('\\','/');
            if(!AssetDatabase.IsValidFolder(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,System.IO.Path.GetFileName(folder));
        }

        static void ConfigureSprites(string path,string id)
        {
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.mipmapEnabled=false; importer.alphaIsTransparency=true; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.filterMode=FilterMode.Bilinear; importer.spritePixelsPerUnit=460;
            var sheet=new SpriteMetaData[8];
            for(int row=0;row<2;row++) for(int col=0;col<4;col++)
            { int i=row*4+col; sheet[i]=new SpriteMetaData{name=$"{id}Blocks_{i}",rect=new Rect(col*512,(1-row)*512,512,512),alignment=(int)SpriteAlignment.Center,pivot=new Vector2(.5f,.5f)}; }
            importer.spritesheet=sheet; importer.SaveAndReimport();
        }

        static Sprite[] LoadSprites(string path)
        {
            var result=new Sprite[8];
            foreach(UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path)) if(asset is Sprite s && int.TryParse(s.name[(s.name.LastIndexOf('_')+1)..],out int i) && i<8) result[i]=s;
            return result;
        }

        static AnimationClip CreateHint(Theme theme,string root)
        {
            string path=$"{root}/Animations/{theme.Id}Hint.anim";
            var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(path)??new AnimationClip{name=$"{theme.Id}Hint",frameRate=60};
            if(!AssetDatabase.Contains(clip)) AssetDatabase.CreateAsset(clip,path);
            clip.wrapMode=WrapMode.Loop; var settings=AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime=true; AnimationUtility.SetAnimationClipSettings(clip,settings);
            string property=theme.Crack?"shatter":"waterWobble";
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(BlockShatterHint),property),new AnimationCurve(new Keyframe(0,.08f),new Keyframe(.14f,.72f),new Keyframe(.30f,.25f),new Keyframe(.48f,.62f),new Keyframe(.66f,.08f)));
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalScale.x"),new AnimationCurve(new Keyframe(0,1),new Keyframe(.16f,1.035f),new Keyframe(.33f,.98f),new Keyframe(.66f,1)));
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Transform),"m_LocalScale.y"),new AnimationCurve(new Keyframe(0,1),new Keyframe(.16f,.98f),new Keyframe(.33f,1.035f),new Keyframe(.66f,1)));
            EditorUtility.SetDirty(clip); return clip;
        }

        static PoolItemSO CreateBurst(Theme theme,int index,string root,Shader shader)
        {
            string matPath=$"{root}/Materials/{theme.Id}Burst_{index}.mat";
            var mat=AssetDatabase.LoadAssetAtPath<Material>(matPath)??new Material(shader);
            mat.shader=shader; mat.SetColor("_Tint",theme.Colors[index]); mat.SetFloat("_Mode",theme.Mode);
            if(!AssetDatabase.Contains(mat)) AssetDatabase.CreateAsset(mat,matPath);
            var go=new GameObject($"{theme.Id}Burst_{index}"); var ps=go.AddComponent<ParticleSystem>(); var main=ps.main;
            main.duration=.75f; main.loop=false; main.playOnAwake=false; main.startLifetime=new ParticleSystem.MinMaxCurve(.28f,.72f); main.startSpeed=new ParticleSystem.MinMaxCurve(1.1f,3.4f); main.startSize=new ParticleSystem.MinMaxCurve(.07f,.25f); main.gravityModifier=theme.Mode==2?.02f:(theme.Mode==8?.18f:.55f); main.maxParticles=36;
            var emission=ps.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,18,28)});
            var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=.16f;
            var color=ps.colorOverLifetime; color.enabled=true; color.color=new ParticleSystem.MinMaxGradient(new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(theme.Colors[index],1)},alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.8f,.55f),new GradientAlphaKey(0,1)}});
            var renderer=go.GetComponent<ParticleSystemRenderer>(); renderer.sharedMaterial=mat; renderer.sortingOrder=55;
            if(theme.Mode==1) ConfigureLavaFlow(ps,renderer);
            if(theme.Mode==2) ConfigureCloudEvaporation(ps,renderer);
            var pooled=go.AddComponent<PooledParticleEffect>(); var pso=new SerializedObject(pooled); pso.FindProperty("rootParticleSystem").objectReferenceValue=ps; pso.FindProperty("particleRenderer").objectReferenceValue=renderer; pso.ApplyModifiedPropertiesWithoutUndo();
            string prefabPath=$"{root}/Prefabs/{theme.Id}Burst_{index}.prefab"; GameObject prefab=PrefabUtility.SaveAsPrefabAsset(go,prefabPath); UnityEngine.Object.DestroyImmediate(go);
            string itemPath=$"{root}/Pool/{theme.Id}Burst_{index}.asset"; var item=AssetDatabase.LoadAssetAtPath<PoolItemSO>(itemPath)??ScriptableObject.CreateInstance<PoolItemSO>(); item.name=$"{theme.Id}Burst_{index}"; item.itemName=item.name; item.prefab=prefab; item.initCount=12; if(!AssetDatabase.Contains(item)) AssetDatabase.CreateAsset(item,itemPath);
            var prefabPooled=prefab.GetComponent<PooledParticleEffect>(); var iso=new SerializedObject(prefabPooled); iso.FindProperty("<Item>k__BackingField").objectReferenceValue=item; iso.ApplyModifiedPropertiesWithoutUndo(); PrefabUtility.SavePrefabAsset(prefab); EditorUtility.SetDirty(item); return item;
        }

        static void ConfigureLavaFlow(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main=ps.main; main.duration=1.15f; main.startLifetime=new ParticleSystem.MinMaxCurve(.72f,1.18f); main.startSpeed=new ParticleSystem.MinMaxCurve(.02f,.16f); main.startSize3D=true; main.startSizeX=new ParticleSystem.MinMaxCurve(.10f,.18f); main.startSizeY=new ParticleSystem.MinMaxCurve(.28f,.48f); main.startSizeZ=.1f; main.gravityModifier=new ParticleSystem.MinMaxCurve(.42f,.72f); main.maxParticles=28; main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission; emission.rateOverTime=0; emission.SetBursts(new[]{new ParticleSystem.Burst(0,14,22)});
            var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Box; shape.scale=new Vector3(.48f,.05f,.01f);
            var velocity=ps.velocityOverLifetime; velocity.enabled=true; velocity.space=ParticleSystemSimulationSpace.World; velocity.x=new ParticleSystem.MinMaxCurve(-.12f,.12f); velocity.y=new ParticleSystem.MinMaxCurve(-.42f,-1.05f); velocity.z=new ParticleSystem.MinMaxCurve(0f,0f);
            var noise=ps.noise; noise.enabled=true; noise.separateAxes=true; noise.strengthX=new ParticleSystem.MinMaxCurve(.08f,.22f); noise.strengthY=new ParticleSystem.MinMaxCurve(.01f,.06f); noise.frequency=1.35f; noise.scrollSpeed=.35f;
            var size=ps.sizeOverLifetime; size.enabled=true; size.separateAxes=true; size.x=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0,.75f),new Keyframe(.3f,1f),new Keyframe(1,.18f))); size.y=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0,.45f),new Keyframe(.45f,1.15f),new Keyframe(1,.2f))); size.z=1f;
            var trails=ps.trails; trails.enabled=true; trails.ratio=.72f; trails.lifetime=new ParticleSystem.MinMaxCurve(.12f,.28f); trails.dieWithParticles=true; trails.widthOverTrail=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0,1f),new Keyframe(1,0f)));
            var color=ps.colorOverLifetime; color.enabled=true; color.color=new ParticleSystem.MinMaxGradient(new Gradient{colorKeys=new[]{new GradientColorKey(new Color(1.35f,.82f,.25f),0),new GradientColorKey(Color.white,.22f),new GradientColorKey(new Color(.42f,.08f,.02f),1)},alphaKeys=new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.9f,.58f),new GradientAlphaKey(0,1)}});
            renderer.renderMode=ParticleSystemRenderMode.Billboard; renderer.alignment=ParticleSystemRenderSpace.View; renderer.sortingOrder=55; renderer.trailMaterial=renderer.sharedMaterial;
        }

        static void ConfigureCloudEvaporation(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main=ps.main; main.duration=1.15f; main.startLifetime=new ParticleSystem.MinMaxCurve(.72f,1.28f); main.startSpeed=new ParticleSystem.MinMaxCurve(.04f,.22f); main.startSize3D=true; main.startSizeX=new ParticleSystem.MinMaxCurve(.16f,.34f); main.startSizeY=new ParticleSystem.MinMaxCurve(.12f,.28f); main.startSizeZ=.1f; main.startRotation=new ParticleSystem.MinMaxCurve(-.5f,.5f); main.gravityModifier=new ParticleSystem.MinMaxCurve(-.06f,-.16f); main.maxParticles=32; main.simulationSpace=ParticleSystemSimulationSpace.World;
            var emission=ps.emission; emission.rateOverTime=new ParticleSystem.MinMaxCurve(5f,9f); emission.SetBursts(new[]{new ParticleSystem.Burst(0,12,19)});
            var shape=ps.shape; shape.shapeType=ParticleSystemShapeType.Circle; shape.radius=.24f; shape.radiusThickness=1f;
            var velocity=ps.velocityOverLifetime; velocity.enabled=true; velocity.space=ParticleSystemSimulationSpace.World; velocity.x=new ParticleSystem.MinMaxCurve(-.16f,.16f); velocity.y=new ParticleSystem.MinMaxCurve(.18f,.62f); velocity.z=new ParticleSystem.MinMaxCurve(0f,0f);
            var noise=ps.noise; noise.enabled=true; noise.separateAxes=true; noise.strengthX=new ParticleSystem.MinMaxCurve(.12f,.32f); noise.strengthY=new ParticleSystem.MinMaxCurve(.04f,.14f); noise.frequency=.55f; noise.scrollSpeed=.2f;
            var size=ps.sizeOverLifetime; size.enabled=true; size.separateAxes=true; size.x=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0,.18f),new Keyframe(.38f,1f),new Keyframe(1,1.7f))); size.y=new ParticleSystem.MinMaxCurve(1f,new AnimationCurve(new Keyframe(0,.15f),new Keyframe(.42f,.85f),new Keyframe(1,1.45f))); size.z=1f;
            var color=ps.colorOverLifetime; color.enabled=true; color.color=new ParticleSystem.MinMaxGradient(new Gradient{colorKeys=new[]{new GradientColorKey(Color.white,0),new GradientColorKey(new Color(.82f,.92f,1.08f),.45f),new GradientColorKey(new Color(.62f,.76f,.9f),1)},alphaKeys=new[]{new GradientAlphaKey(.82f,0),new GradientAlphaKey(.48f,.48f),new GradientAlphaKey(0,1)}});
            var trails=ps.trails; trails.enabled=false;
            renderer.renderMode=ParticleSystemRenderMode.Billboard; renderer.alignment=ParticleSystemRenderSpace.View; renderer.sortingOrder=55; renderer.trailMaterial=null;
        }

        static void CreateSkin(Theme theme,Sprite[] sprites,AnimationClip hint,PoolItemSO[] effects)
        {
            string path=$"Assets/_Shared/ScriptableObjects/Skins/{theme.Id}.asset"; var skin=AssetDatabase.LoadAssetAtPath<SkinDataSO>(path)??ScriptableObject.CreateInstance<SkinDataSO>(); if(!AssetDatabase.Contains(skin)) AssetDatabase.CreateAsset(skin,path);
            var so=new SerializedObject(skin); so.FindProperty("<SkinName>k__BackingField").stringValue=theme.Name; so.FindProperty("<SkinId>k__BackingField").stringValue=theme.Id; SetArray(so.FindProperty("<Sprites>k__BackingField"),sprites);
            var hints=new AnimationClip[8]; for(int i=0;i<8;i++) hints[i]=hint; SetArray(so.FindProperty("<HintClips>k__BackingField"),hints); SetArray(so.FindProperty("<LineClearEffects>k__BackingField"),effects); so.FindProperty("<FireCenteredLineClear>k__BackingField").boolValue=false; so.FindProperty("icon").objectReferenceValue=sprites[0]; so.FindProperty("unlockType").enumValueIndex=0; so.FindProperty("unlockValue").intValue=1; so.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(skin);
        }

        static void Register(List<PoolItemSO> items)
        { var manager=AssetDatabase.LoadAssetAtPath<PoolManagerSO>("Assets/GameLib/ObjectPool/PoolManager.asset"); foreach(var item in items) if(!manager.itemList.Contains(item)) manager.itemList.Add(item); EditorUtility.SetDirty(manager); }
        static void SetArray<T>(SerializedProperty p,T[] values) where T:UnityEngine.Object { p.arraySize=values.Length; for(int i=0;i<values.Length;i++) p.GetArrayElementAtIndex(i).objectReferenceValue=values[i]; }
    }
}
#endif
