using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace QuickCup
{
    static class Mod
    {
        [Conditional("DEBUG")]
        public static void Log(string text)
        {
#if DEBUG
            File.AppendAllText("QuickCup.txt", $"{text}\r\n");
#endif
        }

        public static void Main(string[] args)
        {
            Log($"Init start on {DateTime.Now:R}! -----");

            PlayerManager.OnPlayerJoinedEvent += PlayerManager_OnPlayerJoinedEvent;
            SceneLoader.OnFadeOutEndEvent += SceneLoader_OnFadeOutEndEvent;
            
            SceneLoader.OnFadeInEndEvent += SceneLoader_OnFadeOutStartEventQuickReset;
            SceneLoader.OnFadeOutStartEvent += SceneLoader_OnFadeInEndEventQuickReset;
        }

        private static void SceneLoader_OnFadeInEndEventQuickReset(float time)
        {
            Log("quickReset SceneLoader_OnFadeOutEndEvent: " + time);
            
            var quickReset = new GameObject {name = "quickReset"};
            quickReset.AddComponent<QuickReset>();
        }

        private static void SceneLoader_OnFadeOutStartEventQuickReset()
        {
            Log("quickReset destroy");
            var gameObject = GameObject.Find("quickReset");
            if(gameObject)
                GameObject.Destroy(gameObject);
        }
        
        private static void SceneLoader_OnFadeOutEndEvent()
        {
            Log("SceneLoader_OnFadeOutEndEvent");
            var p = PlayerManager.GetFirst();
            if (p == null)
            {
                Log("SceneLoader_OnFadeOutEndEvent p null");
                return;
            }

            Log($"SceneLoader_OnFadeOutEndEvent p is {p.id}");
            p.stats.OnPlayerDeathEvent += Stats_OnPlayerDeathEvent;
        }

        private static void PlayerManager_OnPlayerJoinedEvent(PlayerId playerId)
        {
            Log($"PlayerManager_OnPlayerJoinedEvent for {playerId}");
            var p = PlayerManager.GetPlayer(playerId);
            if (p == null)
            {
                Log($"p null for {playerId}");
                return;
            }

            Log($"p not null for {playerId}");
            p.stats.OnPlayerDeathEvent += Stats_OnPlayerDeathEvent;
        }

        private static void Stats_OnPlayerDeathEvent(PlayerId playerId)
        {
            Log($"Stats_OnPlayerDeathEvent for {playerId}");
            var deaths = PlayerData.Data.DeathCount(playerId);

            File.WriteAllText($"QuickCup_{playerId}_Stats_Deaths.txt", $"{deaths}");
        }
    }

    public class QuickReset : MonoBehaviour
    {
        private CupheadInput.AnyPlayerInput anyPlayerInput;

        private void Start()
        {
            Mod.Log("QuickReset created");
            anyPlayerInput = new CupheadInput.AnyPlayerInput();
        }

        private void RestartCheck()
        {
            if (anyPlayerInput.GetButton(CupheadButton.Lock) &&
                anyPlayerInput.GetButton(CupheadButton.Pause)) 
                Restart();
        }
        
        private void Restart()
        {
            Mod.Log("Restarting");
            var levelPauseGUI = FindObjectOfType<LevelPauseGUI>();
            
            var method = typeof(LevelPauseGUI).GetMethod("Restart", BindingFlags.Instance | BindingFlags.NonPublic);
            method.Invoke(levelPauseGUI, new object[] { });
        }
        
        void Update()
        {
            RestartCheck();
        }
    }
}
