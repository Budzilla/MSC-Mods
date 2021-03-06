﻿using UnityEngine;
using HutongGames.PlayMaker;
using System.Collections.Generic;

namespace NicotineGum
{
    public class GumBehaviour : MonoBehaviour
    {
        public static float Price => 45.50f;
        public static Vector3 CounterPos => new Vector3(-1551.03f, 4.8f, 1182.74f);
        public static string ItemName => "nicotine gum";

        // While in the shop, the items should be maintained on a List so they can be properly disposed of when the shop is restocked
        public List<GameObject> ShopList;
        public int Count = 10;

        private GameObject _register;
        private bool _bought = false;
        private bool _buying = false;
        private FsmBool _guiBuy;
        private FsmBool _guiUse;
        private FsmString _guiText;
        private const float _strength = 10.0f;
        private const float _rate = 0.05f;

        private void _swallow()
        {
            if (Count > 0)
            {
                if (NicotineGum.Swallow(_strength, _rate))
                    --Count;
            }
            if (Count <= 0)
            {
                GameObject.Destroy(gameObject);
            }
        }

        // Make the object interactive
        public void Activate()
        {
            transform.parent = null;
            gameObject.name = ItemName + "(Clone)";
            gameObject.layer = LayerMask.NameToLayer("Parts");
            gameObject.tag = "PART";
            gameObject.GetComponent<Rigidbody>().isKinematic = false;
        }

        // Public member that sets the "bought" status (e.g. for loading a save)
        public void SetBought()
        {
            _bought = true;
        }

        // Put the item in the basket
        private void _buy()
        {
            PlayMakerFSM registerFsm = _register.GetComponent<PlayMakerFSM>();
            registerFsm.FsmVariables.GetFsmFloat("PriceTotal").Value += Price;
            registerFsm.SendEvent("PURCHASE");
            _buying = true;
            gameObject.SetActive(false);
        }

        // Pay for the items in the basket
        private void _pay()
        {
            if (_buying)
            {
                // Move to counter
                gameObject.SetActive(true);
                transform.position = CounterPos;

                // Make interactive and dynamic
                Activate();
                _buying = false;
                _bought = true;

                ShopList.Remove(gameObject);
            }
        }

        // Find GUI FSMvars and hook into the store action
        void Awake()
        {
            _register = GameObject.Find("STORE/StoreCashRegister/Register");
            _guiBuy = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIbuy");
            _guiUse = PlayMakerGlobals.Instance.Variables.FindFsmBool("GUIuse");
            _guiText = PlayMakerGlobals.Instance.Variables.FindFsmString("GUIinteraction");
            GameHook.InjectStateHook(_register, "Purchase", () => { _pay(); });
        }

        void Update()
        {
            bool interact = Input.GetKeyDown(KeyCode.Mouse0);
            bool use = cInput.GetButtonDown("Use");

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 1.0f) && hit.collider.gameObject == this.gameObject)
            {
                if (_bought && NicotineGum.StressRateFsm.Value > 1.2f)
                {
                    string s = NicotineGum.OverdoseAmount <= 0.0f ? $"nicotine gum \n({Count} pieces left)" : NicotineGum.OverdoseAmount <= 50.0f ? $"nicotine gum \n(you're still chewing on the last piece) \n({Count} pieces left)" : $"nicotine gum \n(you are feeling ill) \n({Count} pieces left)";
                    _guiUse.Value = true;
                    _guiText.Value = s;
                    if (use)
                    {
                        _swallow();
                    }
                }
                else if (_bought && NicotineGum.StressRateFsm.Value <= 1.2f)
                {
                    string s = NicotineGum.OverdoseAmount <= 0.0f ? $"nicotine gum \n({Count} pieces left)" : NicotineGum.OverdoseAmount <= 50.0f ? $"nicotine gum \n(no longer addicted) \n({Count} pieces left)" : $"nicotine gum \n(you are feeling ill) \n({Count} pieces left)";
                    _guiUse.Value = true;
                    _guiText.Value = s;
                    if (use)
                    {
                        _swallow();
                    }
                }
                else
                {
                    _guiBuy.Value = true;
                    _guiText.Value = $"{ItemName} 45.50 mk";
                    if (interact)
                    {
                        _buy();
                    }
                }
            }
        }
    }
}
