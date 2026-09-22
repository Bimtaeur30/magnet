#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace CodexBridge
{
    /// <summary>
    /// 스킨별 라인클리어 파티클이 서로 겹쳐 보이지 않도록 움직임을 구성하는 공용 헬퍼.
    /// 각 SkinBuilder의 CreateBurst와 "Update All Skin Line Clear Motion" 메뉴가 같은 설정을 쓴다.
    /// </summary>
    public static class SkinBurstMotion
    {
        const string J = "Assets/_MemberWorkspace/JTH";
        const string ThemeShaderPath = J + "/Graphics/Shaders/ThemeBurst.shader";

        [MenuItem("Tools/Codex/Update All Skin Line Clear Motion")]
        public static void UpdateAll()
        {
            Apply(J + "/PaperSkin/Prefabs/PaperBurst_{0}.prefab", 8, (root, i) => ConfigurePaper(root));
            Apply(J + "/StoneSkin/Prefabs/StoneBurst_{0}.prefab", 8, (root, i) => ConfigureStone(root, i));
            Apply(J + "/WaterBalloonSkin/Prefabs/WaterBalloonBurst_{0}.prefab", 8, (root, i) => ConfigureWaterBalloon(root));
            Apply(J + "/WaterDropSkin/Prefabs/WaterDropBurst_{0}.prefab", 3, (root, i) => ConfigureWaterDrop(root));
            RecommendedThemeSkinBuilder.UpdateThemeMotionEffects();
            AssetDatabase.SaveAssets();
            Debug.Log("[Codex] Updated line-clear motion for Paper, Stone, WaterBalloon, WaterDrop and recommended themes.");
        }

        static void Apply(string pathFormat, int count, Action<GameObject, int> configure)
        {
            for (int i = 0; i < count; i++)
            {
                string path = string.Format(pathFormat, i);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null) continue;
                GameObject root = PrefabUtility.LoadPrefabContents(path);
                configure(root, i);
                PrefabUtility.SaveAsPrefabAsset(root, path);
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        #region Helpers

        public static ParticleSystem.MinMaxCurve Curve(params (float t, float v)[] keys)
        {
            var frames = new Keyframe[keys.Length];
            for (int i = 0; i < keys.Length; i++) frames[i] = new Keyframe(keys[i].t, keys[i].v);
            return new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(frames));
        }

        public static ParticleSystem.MinMaxGradient Alpha(params (float t, float a)[] keys)
        {
            var alphas = new GradientAlphaKey[keys.Length];
            for (int i = 0; i < keys.Length; i++) alphas[i] = new GradientAlphaKey(keys[i].a, keys[i].t);
            return new ParticleSystem.MinMaxGradient(new Gradient
            {
                colorKeys = new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
                alphaKeys = alphas
            });
        }

        static ParticleSystem.MinMaxCurve Range(float min, float max) => new(min, max);

        /// <summary>파티클 시스템을 공통 기본값으로 되돌리고 선택 모듈을 모두 끈다.</summary>
        public static void Reset(ParticleSystem ps, ParticleSystemRenderer renderer)
        {
            var main = ps.main;
            main.loop = false; main.playOnAwake = false; main.startDelay = 0f;
            main.startSize3D = false; main.startRotation3D = false; main.startRotation = 0f;
            main.gravityModifier = 0f; main.simulationSpace = ParticleSystemSimulationSpace.Local; main.maxParticles = 36;
            var emission = ps.emission; emission.enabled = true; emission.rateOverTime = 0; emission.SetBursts(Array.Empty<ParticleSystem.Burst>());
            var shape = ps.shape;
            shape.enabled = true; shape.shapeType = ParticleSystemShapeType.Circle; shape.radius = .16f; shape.radiusThickness = 1f;
            shape.arc = 360f; shape.arcMode = ParticleSystemShapeMultiModeValue.Random; shape.angle = 25f; shape.position = Vector3.zero; shape.rotation = Vector3.zero; shape.scale = Vector3.one;
            var velocity = ps.velocityOverLifetime; velocity.enabled = false;
            var limit = ps.limitVelocityOverLifetime; limit.enabled = false;
            var noise = ps.noise; noise.enabled = false;
            var size = ps.sizeOverLifetime; size.enabled = false; size.separateAxes = false;
            var rotation = ps.rotationOverLifetime; rotation.enabled = false; rotation.separateAxes = false;
            var trails = ps.trails; trails.enabled = false;
            var collision = ps.collision; collision.enabled = false;
            var color = ps.colorOverLifetime; color.enabled = true;
            renderer.renderMode = ParticleSystemRenderMode.Billboard; renderer.alignment = ParticleSystemRenderSpace.View;
            renderer.mesh = null; renderer.sortingOrder = 55; renderer.trailMaterial = null;
        }

        /// <summary>이름으로 자식 파티클을 찾거나 만든다. 부모와 같은 머티리얼을 기본으로 쓴다.</summary>
        public static ParticleSystem Child(GameObject root, string name, Material material)
        {
            Transform t = root.transform.Find(name);
            if (t == null)
            {
                t = new GameObject(name).transform;
                t.SetParent(root.transform, false);
            }
            var ps = t.GetComponent<ParticleSystem>();
            if (ps == null) ps = t.gameObject.AddComponent<ParticleSystem>();
            var renderer = t.GetComponent<ParticleSystemRenderer>();
            Reset(ps, renderer);
            renderer.sharedMaterial = material;
            return ps;
        }

        public static void RemoveChild(GameObject root, string name)
        {
            Transform t = root.transform.Find(name);
            if (t != null) UnityEngine.Object.DestroyImmediate(t.gameObject);
        }

        static ParticleSystemRenderer R(ParticleSystem ps) => ps.GetComponent<ParticleSystemRenderer>();

        #endregion

        #region Skins

        /// <summary>종이: 종잇조각이 3D로 뒤집히며 좌우로 흔들려 떨어진다.</summary>
        public static void ConfigurePaper(GameObject root)
        {
            var ps = root.GetComponent<ParticleSystem>(); var renderer = R(ps);
            Reset(ps, renderer);
            var main = ps.main;
            main.duration = 1.3f; main.startLifetime = Range(.9f, 1.35f); main.startSpeed = Range(1.1f, 2.3f);
            main.startSize = Range(.13f, .22f); main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, .85f), Color.white);
            main.startRotation3D = true; main.startRotationX = Range(0f, Mathf.PI * 2f); main.startRotationY = Range(0f, Mathf.PI * 2f); main.startRotationZ = Range(0f, Mathf.PI * 2f);
            main.gravityModifier = Range(.3f, .45f); main.maxParticles = 18;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 9, 13) });
            var shape = ps.shape; shape.radius = .18f;
            var limit = ps.limitVelocityOverLifetime; limit.enabled = true; limit.limit = .55f; limit.dampen = .14f;
            var noise = ps.noise; noise.enabled = true; noise.separateAxes = true; noise.strengthX = Range(.35f, .6f); noise.strengthY = .05f; noise.strengthZ = 0f; noise.frequency = .9f; noise.scrollSpeed = .5f;
            var rotation = ps.rotationOverLifetime; rotation.enabled = true; rotation.separateAxes = true;
            rotation.x = Range(-7f, 7f); rotation.y = Range(-5f, 5f); rotation.z = Range(-2f, 2f);
            var color = ps.colorOverLifetime; color.color = Alpha((0, 1), (.7f, .9f), (1, 0));
            renderer.renderMode = ParticleSystemRenderMode.Mesh;
            renderer.mesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            renderer.alignment = ParticleSystemRenderSpace.Local;
        }

        /// <summary>돌: 무거운 덩어리가 아래로 무너지고 회색 먼지가 피어오른다.</summary>
        public static void ConfigureStone(GameObject root, int index)
        {
            var ps = root.GetComponent<ParticleSystem>(); var renderer = R(ps);
            Reset(ps, renderer);
            var main = ps.main;
            main.duration = .9f; main.startLifetime = Range(.5f, .8f); main.startSpeed = Range(.3f, 1.3f);
            main.startSize = Range(.11f, .24f); main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, .9f), Color.white);
            main.startRotation = Range(0f, Mathf.PI * 2f); main.gravityModifier = 1.7f; main.maxParticles = 14;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 7, 11) });
            var shape = ps.shape; shape.shapeType = ParticleSystemShapeType.Rectangle; shape.scale = new Vector3(.42f, .42f, 1f);
            var rotation = ps.rotationOverLifetime; rotation.enabled = true; rotation.z = Range(-3f, 3f);
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = Curve((0, 1), (.8f, 1), (1, .5f));
            var color = ps.colorOverLifetime; color.color = Alpha((0, 1), (.8f, 1), (1, 0));

            Color tint = renderer.sharedMaterial.GetColor("_Tint");
            var dust = Child(root, "Dust", DustMaterial(J + $"/StoneSkin/Materials/StoneDust_{index}.mat", Color.Lerp(tint, new Color(.78f, .76f, .72f), .6f)));
            var dm = dust.main;
            dm.duration = .9f; dm.startLifetime = Range(.55f, .9f); dm.startSpeed = Range(.15f, .5f); dm.startSize = Range(.2f, .34f);
            dm.startRotation = Range(-.6f, .6f); dm.gravityModifier = -.04f; dm.maxParticles = 10;
            dust.emission.SetBursts(new[] { new ParticleSystem.Burst(.03f, 5, 8) });
            var ds = dust.shape; ds.shapeType = ParticleSystemShapeType.Rectangle; ds.scale = new Vector3(.4f, .2f, 1f); ds.position = new Vector3(0, -.12f, 0);
            var dl = dust.limitVelocityOverLifetime; dl.enabled = true; dl.limit = .2f; dl.dampen = .2f;
            var dsz = dust.sizeOverLifetime; dsz.enabled = true; dsz.size = Curve((0, .35f), (.4f, 1f), (1, 1.5f));
            var dc = dust.colorOverLifetime; dc.color = Alpha((0, 0), (.15f, .5f), (1, 0));
            R(dust).sortingOrder = 54;
        }

        /// <summary>물풍선: 터지는 순간 고리 충격파 + 물방울이 포물선으로 튄다.</summary>
        public static void ConfigureWaterBalloon(GameObject root)
        {
            var ps = root.GetComponent<ParticleSystem>(); var renderer = R(ps);
            Reset(ps, renderer);
            var main = ps.main;
            main.duration = .8f; main.startLifetime = Range(.4f, .7f); main.startSpeed = Range(2.6f, 4.4f);
            main.startSize = Range(.06f, .14f); main.startColor = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, .85f), Color.white);
            main.gravityModifier = 1.5f; main.maxParticles = 26;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 14, 20) });
            var shape = ps.shape; shape.radius = .14f;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = Curve((0, 1), (.7f, .85f), (1, .3f));
            var color = ps.colorOverLifetime; color.color = Alpha((0, 1), (.75f, .9f), (1, 0));

            var pop = Child(root, "PopRing", renderer.sharedMaterial);
            var pm = pop.main;
            pm.duration = .3f; pm.startLifetime = .22f; pm.startSpeed = 0f; pm.startSize = .5f; pm.maxParticles = 1;
            pop.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1) });
            var psh = pop.shape; psh.enabled = false;
            var psz = pop.sizeOverLifetime; psz.enabled = true; psz.size = Curve((0, .5f), (.35f, 1.6f), (1, 2.1f));
            var pc = pop.colorOverLifetime; pc.color = Alpha((0, .9f), (.4f, .5f), (1, 0));
            R(pop).sortingOrder = 56;
        }

        /// <summary>물방울: 동심원 파문이 순차로 퍼지고 작은 물방울이 위로 튀었다 떨어진다.</summary>
        public static void ConfigureWaterDrop(GameObject root)
        {
            var ps = root.GetComponent<ParticleSystem>(); var renderer = R(ps);
            Reset(ps, renderer);
            var main = ps.main;
            main.duration = .8f; main.startLifetime = .6f; main.startSpeed = 0f; main.startSize = .34f;
            main.startColor = Color.white; main.maxParticles = 3;
            ps.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 1), new ParticleSystem.Burst(.1f, 1), new ParticleSystem.Burst(.2f, 1) });
            var shape = ps.shape; shape.enabled = false;
            var size = ps.sizeOverLifetime; size.enabled = true; size.size = Curve((0, .25f), (.35f, 1.3f), (1, 2f));
            var color = ps.colorOverLifetime; color.color = Alpha((0, .95f), (.5f, .45f), (1, 0));

            var crown = Child(root, "Crown", renderer.sharedMaterial);
            var cm = crown.main;
            cm.duration = .6f; cm.startLifetime = Range(.4f, .6f); cm.startSpeed = Range(1.6f, 2.5f); cm.startSize = Range(.04f, .08f);
            cm.gravityModifier = 1.4f; cm.maxParticles = 10;
            crown.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 5, 8) });
            var cs = crown.shape; cs.shapeType = ParticleSystemShapeType.Cone; cs.angle = 22f; cs.radius = .08f; cs.rotation = new Vector3(-90f, 0f, 0f);
            var cc = crown.colorOverLifetime; cc.color = Alpha((0, 1), (.8f, .9f), (1, 0));
            R(crown).sortingOrder = 56;
        }

        /// <summary>용암: 기존 녹아내림 위에 불씨가 깜빡이며 위로 올라간다.</summary>
        public static void AddLavaEmbers(GameObject root)
        {
            var embers = Child(root, "Embers", root.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            var m = embers.main;
            m.duration = .6f; m.startLifetime = Range(.5f, .9f); m.startSpeed = Range(.1f, .3f); m.startSize = Range(.03f, .06f);
            m.gravityModifier = -.12f; m.maxParticles = 16; m.simulationSpace = ParticleSystemSimulationSpace.World;
            var e = embers.emission; e.rateOverTime = 10f; e.SetBursts(new[] { new ParticleSystem.Burst(0, 4, 7) });
            var s = embers.shape; s.shapeType = ParticleSystemShapeType.Rectangle; s.scale = new Vector3(.46f, .2f, 1f);
            var v = embers.velocityOverLifetime; v.enabled = true; v.space = ParticleSystemSimulationSpace.World;
            v.x = Range(-.1f, .1f); v.y = Range(.35f, .8f); v.z = Range(0f, 0f);
            var n = embers.noise; n.enabled = true; n.strength = .15f; n.frequency = 2f; n.scrollSpeed = 1f;
            var c = embers.colorOverLifetime; c.color = Alpha((0, 1), (.2f, .4f), (.35f, 1), (.55f, .3f), (.7f, .9f), (1, 0));
            R(embers).sortingOrder = 56;
        }

        /// <summary>초콜릿: 큰 조각 아래로 가루 부스러기를 흩뿌린다.</summary>
        public static void AddChocolateCrumbs(GameObject root)
        {
            var crumbs = Child(root, "Crumbs", root.GetComponent<ParticleSystemRenderer>().sharedMaterial);
            var m = crumbs.main;
            m.duration = .7f; m.startLifetime = Range(.3f, .55f); m.startSpeed = Range(1.4f, 3f); m.startSize = Range(.025f, .05f);
            m.startRotation = Range(0f, Mathf.PI * 2f); m.gravityModifier = 1.2f; m.maxParticles = 28;
            crumbs.emission.SetBursts(new[] { new ParticleSystem.Burst(0, 16, 24) });
            var s = crumbs.shape; s.radius = .2f;
            var c = crumbs.colorOverLifetime; c.color = Alpha((0, 1), (.7f, .9f), (1, 0));
            R(crumbs).sortingOrder = 54;
        }

        static Material DustMaterial(string path, Color tint)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(AssetDatabase.LoadAssetAtPath<Shader>(ThemeShaderPath));
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_Tint", tint);
            material.SetFloat("_Mode", 2f);
            material.SetFloat("_Softness", .15f);
            EditorUtility.SetDirty(material);
            return material;
        }

        #endregion
    }
}
#endif
