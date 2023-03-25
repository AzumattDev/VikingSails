using System;
using System.Collections;
using System.Linq;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;
using VikingSails.Compatibility.WardIsLove;

namespace VikingSails.Patches
{
    public class VikingShipURL : MonoBehaviour, Hoverable, Interactable, TextReceiver
    {
        /* Credit to RoloPogo for the original code this was worked off of. */
        
        private string m_url;
        public string m_name = "VikingSailURL";
        private const int m_characterLimit = int.MaxValue;

        private SkinnedMeshRenderer sailRenderer;
        private Texture origSailTexture;

        private ZNetView m_nview;

        private void Awake()
        {
            TryGetComponent(out ZNetView? zNetView);
            if (zNetView == null) return;
            if (!zNetView.IsValid()) return;
            m_nview = zNetView;


            sailRenderer = GetComponentsInChildren<SkinnedMeshRenderer>()
                .FirstOrDefault(x => x?.name == "sail_full");

            try
            {
                if (sailRenderer?.material != null)
                {
                    origSailTexture = sailRenderer.material.GetTexture("_MainTex");
                }
            }
            catch (Exception e)
            {
                VikingSailsPlugin.VikingSailsLogger.LogError($"Error getting texture: {e}");
            }

            if (m_nview?.GetZDO() != null)
            {
                InvokeRepeating("UpdateText", 2f, 2f);
            }
        }


        public string GetHoverText()
        {
            if (!VikingSailsPlugin.AllowInput()) return string.Empty;

            return Localization.instance.Localize($"{Environment.NewLine}[<color=#FFFF00><b>$KEY_Use</b></color>] $set_url") + "\n" +
                   GetText();
        }

        public string GetHoverName()
        {
            return !VikingSailsPlugin.AllowInput() ? string.Empty : Localization.instance.Localize("$sail_url");
        }

        public bool Interact(Humanoid character, bool hold, bool alt)
        {
            if (VikingSailsPlugin.useServerSailURL.Value == VikingSailsPlugin.Toggle.On)
            {
                character?.Message(MessageHud.MessageType.Center, Localization.instance.Localize($"<color=#FFFF00><b>$piece_noaccess</b></color> {Environment.NewLine} $server_sail_url_deny"));
                return false;
            }

            if (!VikingSailsPlugin.AllowInput()) return false;
            if (hold)
            {
                return false;
            }

            if (!PrivateArea.CheckAccess(transform.position, 0f, true))
            {
                character?.Message(MessageHud.MessageType.Center, "<color=#FFFF00><b>$piece_noaccess</b></color>");
                return false;
            }
            
            if (WardIsLovePlugin.IsLoaded())
            {
                if (WardIsLovePlugin.WardEnabled().Value &&
                    WardMonoscript.CheckInWardMonoscript(transform.position))
                {
                    if (!WardMonoscript.CheckAccess(transform.position, 0f, true))
                    {
                        // private zone
                       return false;
                    }
                }
            }

            TextInput.instance.RequestText(this, "$piece_sign_input", m_characterLimit);
            return true;
        }

        private void UpdateText()
        {
            string text = GetText();
            if (m_url == text) { return; }
            SetText(text);
        }

        public string GetText()
        {
            return VikingSailsPlugin.useServerSailURL.Value == VikingSailsPlugin.Toggle.On
                ? VikingSailsPlugin.serverSailURL.Value
                : m_nview.GetZDO().GetString("VikingSailURL", string.Empty);
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            return false;
        }

        public void SetText(string text)
        {
            if (!PrivateArea.CheckAccess(transform.position, 0f, true)) { return; }
            StartCoroutine(DownloadTexture(text, ApplyTexture));
        }

        private void ApplyTexture(string url, Texture2D obj)
        {
            m_nview.ClaimOwnership();
            m_nview.GetZDO().Set("VikingSailURL", url);
            sailRenderer.material.SetTexture("_MainTex", string.IsNullOrWhiteSpace(url) ? origSailTexture : obj);
        }

        public IEnumerator DownloadTexture(string url, Action<string, Texture2D> callback)
        {
            m_url = url;
            if (m_url.IsNullOrWhiteSpace())
            {
                callback.Invoke(url, null!);
                yield break;
            }

            using UnityWebRequest uwr = UnityWebRequest.Get(url);
            yield return uwr.SendWebRequest();
            if (uwr.isNetworkError || uwr.isHttpError)
            {
                VikingSailsPlugin.VikingSailsLogger.LogError(uwr.error + Environment.NewLine + url);
            }
            else
            {
                var tex = new Texture2D(2, 2);
                tex.LoadImage(uwr.downloadHandler.data);
                callback.Invoke(url, tex);
            }
        }
    }
}