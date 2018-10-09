using System.Collections.Generic;
using System.Linq;
using MSCLoader;
using UnityEngine;
using HutongGames.PlayMaker;

namespace NicotineGum
{
    public class NicotineGum : Mod
    {
        public override string ID => "NicotineGum";
        public override string Name => "Nicotine gum";
        public override string Author => "Budzilla";
        public override string Version => "1.0";
        public override bool UseAssetsFolder => true;

        private GameObject _pack;
        private List<GameObject> _list;

        public static float TimeLeft = 0.0f;
        public static float StressRate = 1.0f;
        public static float OverdoseAmount = 0.0f;
        public static FsmFloat FatigueFsm;
        public static FsmFloat StressFsm;
        public static FsmFloat StressRateFsm;
        public static FsmFloat DrunkFsm;
        public static FsmFloat ThirstFsm;
        private static readonly List<Vector3> _pos = new List<Vector3>
        {
            new Vector3(-1550.640f, 4.646173f, 1183.058f),
            new Vector3(-1550.632f, 4.646173f, 1183.035f),
            new Vector3(-1550.624f, 4.646173f, 1183.011f),
            new Vector3(-1550.617f, 4.646173f, 1182.988f),
            new Vector3(-1550.608f, 4.646173f, 1182.964f)   
            /*new Vector3(-1547.536f, 4.63f, 1180.284f),
            new Vector3(-1547.452f, 4.63f, 1180.273f),
            new Vector3(-1547.502f, 4.63f, 1180.339f)*/
        };

        public NicotineGum()
        {
            GumSaveData.SavePath = System.IO.Path.Combine(Application.persistentDataPath, "nicotine_gum.xml");
        }

        public static bool Swallow(float amount, float rate)
        {
            TimeLeft += amount;
            StressRate = rate;
            OverdoseAmount += amount;
            ModConsole.Print($"[Gum] Ate a piece of gum: {NicotineGum.TimeLeft.ToString("0")}  \noverdose: {NicotineGum.OverdoseAmount.ToString("0")} \nstress rate:{NicotineGum.StressRateFsm.Value.ToString("0.0000")}");
            return true;
  
        }

        public void InitShop()
        {
            if (_list != null && _list.Count <= 0)
            {
                foreach (var o in _list)
                {
                    GameObject.Destroy(o);
                }
            }

            _list = new List<GameObject>();
            for (int i = 0; i < 5; ++i)
            {
                GameObject gum = GameObject.Instantiate<GameObject>(_pack);
                gum.name = "nicotine gum(Clone)";
                gum.tag = "Untagged";
                gum.layer = 0;
                gum.transform.position = _pos[i];
                gum.transform.rotation = Quaternion.Euler(90.0f, 160.0f, 0.0f);
                gum.GetComponent<Rigidbody>().isKinematic = true;
                GumBehaviour comp = gum.AddComponent<GumBehaviour>();
                comp.ShopList = _list;
                _list.Add(gum);
            }
            GameObject.Destroy(_pack);
        }

        public override void OnLoad()
        {
            // Original
            AssetBundle ab = LoadAssets.LoadBundle(this, "pack.unity3d");
            GameObject original = ab.LoadAsset<GameObject>("pack.prefab");
            _pack = GameObject.Instantiate<GameObject>(original);
            _pack.name = "nicotine gum";
            Material m = new Material(Shader.Find("Standard"));
            m.mainTexture = original.GetComponent<Renderer>().material.mainTexture;
            _pack.GetComponent<Renderer>().material = m;

            GameObject.Destroy(original);
            ab.Unload(false);

            // Load save
            GumSaveData data = GumSaveData.Deserialize<GumSaveData>(GumSaveData.SavePath);
            for (int i = 0; i < data.Pos.Count; ++i)
            {
                GameObject gum = GameObject.Instantiate<GameObject>(_pack);
                gum.transform.position = data.Pos[i];
                gum.transform.rotation = data.Rot[i];
                GumBehaviour c = gum.AddComponent<GumBehaviour>();
                c.ShopList = _list;
                c.Count = data.GumCount[i];
                c.Activate();
                c.SetBought();
            }

            // Setup
            FatigueFsm = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerFatigue");
            StressFsm = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerStress");
            StressRateFsm = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerStressRate");
            DrunkFsm = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerDrunk");
            ThirstFsm = PlayMakerGlobals.Instance.Variables.FindFsmFloat("PlayerThirst");
            InitShop();
            ConsoleCommand.Add(new GumCommand(this));

            ModConsole.Print($"[Gum] has loaded without issue");
        }

        public override void OnSave()
        {
            GumSaveData data = new GumSaveData(new List<Vector3>(), new List<Quaternion>(), new List<int>(), OverdoseAmount);
            foreach (GameObject o in GameObject.FindObjectsOfType<GameObject>().Where(o => o.GetComponent<GumBehaviour>() != null && !_list.Contains(o)))
            {
                data.Pos.Add(o.transform.position);
                data.Rot.Add(o.transform.rotation);
                data.GumCount.Add(o.GetComponent<GumBehaviour>().Count);
            }
            GumSaveData.Serialize(data, GumSaveData.SavePath);
        }

        public override void Update()
        {
            if (TimeLeft > 0)
            {
                float d = StressRate * Time.deltaTime;
                StressRateFsm.Value -= d/50.0f;
                TimeLeft -= d*25.0f;
            }
            if (OverdoseAmount > 0.0f)
            {
                OverdoseAmount -= 0.5f * Time.deltaTime;
            }
            if (OverdoseAmount >= 100.0f)
            {
                StressFsm.Value = 150.0f;
                StressRateFsm.Value = 8.4f;
                DrunkFsm.Value = 0.5f;
                ThirstFsm.Value = 100.0f;
                OverdoseAmount = 50.0f;
                ModConsole.Print($"[Gum] Overdosed");
            }
            if (StressRateFsm.Value < 1.2f)
            {
                StressRateFsm = 1.2f;
            }
        }
    }

    public class GumCommand : ConsoleCommand
    {
        public override string Name => "gum";
        public override string Help => "[stats - shows current status] [od - manually overdose]";
        NicotineGum _mod;
        public GumCommand(NicotineGum mod)
        {
            _mod = mod;
        }

        public override void Run(string[] args)
        {
            if (args.Length < 1)
                return;
            try
            {
                if (args[0].ToLowerInvariant() == "stats")
                {
                    ModConsole.Print($"\n-- Nicotine gum --\ntime left: {NicotineGum.TimeLeft.ToString("0")} \noverdose: {NicotineGum.OverdoseAmount.ToString("0")} \nstress rate:{NicotineGum.StressRateFsm.Value.ToString("0.000000000")}");
                }

                if (args[0].ToLowerInvariant() == "od")
                {
                    NicotineGum.StressFsm.Value = 150.0f;
                    NicotineGum.StressRateFsm.Value = 8.4f;
                    NicotineGum.DrunkFsm.Value = 0.5f;
                    NicotineGum.ThirstFsm.Value = 100.0f;
                    ModConsole.Print($"[Gum] Overdosed");
                }
            }
            catch { }
        }
    }
}
