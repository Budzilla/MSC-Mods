using System.Collections.Generic;
using MSCLoader;
using UnityEngine;

namespace CinderBlocks
{
    public class CinderBlocks : Mod
    {
        public override string ID => "CinderBlocks";
        public override string Name => "Cinder Blocks";
        public override string Author => "Budzilla";
        public override string Version => "1.0";
        public override bool UseAssetsFolder => true;

        private static readonly List<Vector3> _pos = new List<Vector3>
        {
            new Vector3(51.563f, -1.349f, -88.717f),
            new Vector3(51.778f, -1.349f, -88.720f),
            new Vector3(51.989f, -1.349f, -88.724f),
            new Vector3(52.204f, -1.349f, -88.728f)
        };

        private List<GameObject> _list;
        private GameObject _block;

        public CinderBlocks()
        {
            CinderBlocksSaveData.SavePath = System.IO.Path.Combine(Application.persistentDataPath, "cinderblocks.xml");
        }

        public void InitSpawn()
        {  
            _list = new List<GameObject>();
            for (int i = 0; i < 4; ++i)
            {
                GameObject cinderblock = GameObject.Instantiate<GameObject>(_block);
                cinderblock.name = "cinder block(Clone)";
                cinderblock.tag = "PART";
                cinderblock.layer = LayerMask.NameToLayer("Parts");
                cinderblock.transform.position = _pos[i];
                cinderblock.transform.rotation = Quaternion.Euler(270.0f, 90.0f, 0.0f);
                cinderblock.GetComponent<Rigidbody>().isKinematic = true;
                _list.Add(cinderblock);
            }
            GameObject.Destroy(_block);
        }

        public override void OnLoad()
        {
            AssetBundle ab = LoadAssets.LoadBundle(this, "cinderblocks.unity3d");
            GameObject original = ab.LoadAsset<GameObject>("cinderblock.prefab");

            _block = GameObject.Instantiate<GameObject>(original);
            _block.name = "cinder block";
            _block.tag = "PART";
            _block.layer = LayerMask.NameToLayer("Parts");
            _block.GetComponent<Rigidbody>().isKinematic = true;

            GameObject.Destroy(original);
            ab.Unload(false);

            // Load save
            if (CinderBlocksSaveData.saveExists == true)
            {
                CinderBlocksSaveData data = CinderBlocksSaveData.Deserialize<CinderBlocksSaveData>(CinderBlocksSaveData.SavePath);
                for (int i = 0; i < data.Pos.Count; ++i)
                {
                    GameObject cinderblock = GameObject.Instantiate<GameObject>(_block);
                    cinderblock.transform.position = data.Pos[i];
                    cinderblock.transform.rotation = data.Rot[i];
                }
            }

            // Initialize
            if (CinderBlocksSaveData.saveExists == false)
            {
                InitSpawn();
            }
        }

        public override void OnSave()
        {
            {
                CinderBlocksSaveData data = new CinderBlocksSaveData(new List<Vector3>(), new List<Quaternion>());
                foreach (GameObject obj in GameObject.FindObjectsOfType<GameObject>())
                    if (obj.name == "cinder block(Clone)")
                    {
                        data.Pos.Add(obj.transform.position);
                        data.Rot.Add(obj.transform.rotation);
                    }
                    CinderBlocksSaveData.Serialize(data, CinderBlocksSaveData.SavePath);
            }
        }
    }
}