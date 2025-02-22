using BepInEx;
using UnboundLib;
using LightHTTP;
using System.IO;
using System.Reflection;
using WebSocketSharp.Server;
using UnityEngine;
using BepInExExternalViewTester.Schemas;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BepInExExternalViewTester
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, ModVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public static Plugin Instance { get; private set; }

        public const string ModId = "com.willis.rounds.externaldataviewtest";
        public const string ModName = "External Data View Test";
        public const string ModVersion = "0.0.1";


        void Start()
        {
            Instance = this;

            Unbound.RegisterClientSideMod(ModId);

            var wwwrootPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "wwwroot");
            var homepagePath = Path.Combine(wwwrootPath, "index.html");

            var server = new LightHttpServer();
            var webAppUrl = server.AddAvailableLocalPrefix();

            server.HandlesStaticFile("/", Path.Combine(wwwrootPath, "index.html"));
            server.Start();

            Application.OpenURL(webAppUrl);

            var wsServer = new WebSocketServer("ws://0.0.0.0:8080");
            wsServer.AddWebSocketService<StatMessageHandler>("/");
            wsServer.Start();

            new GameObject("Websocket Service", typeof(WebsocketSingleton));
        }


        private class WebsocketSingleton : MonoBehaviour
        {
            private static List<Player>? Players { get => PlayerManager.instance?.players; }


            void Awake()
            {
                DontDestroyOnLoad(gameObject);
                gameObject.hideFlags = HideFlags.HideAndDontSave;
            }

            void Update()
            {
                if (Players == null || Players.Count == 0) return;

                var stats = Players.Select(p => new PlayerStat(
                        $"[{p.playerID}] {p.data.view.Owner.NickName}", 
                        Mathf.Max(p.data.health, 0), 
                        p.data.maxHealth,
                        p.data.weaponHandler.gun.damage * p.data.weaponHandler.gun.bulletDamageMultiplier * p.data.weaponHandler.gun.projectiles[0].objectToSpawn.GetComponent<ProjectileHit>().damage,
                        p.data.block.Cooldown(),
                        Mathf.Min(p.data.block.counter, p.data.block.Cooldown()),
                        p.data.weaponHandler.gun.gunAmmo.ReloadTime(),
                        p.data.weaponHandler.gun.gunAmmo.ReloadTime() - Mathf.Max(0, p.data.weaponHandler.gun.gunAmmo.reloadCounter),
                        p.data.weaponHandler.gun.usedCooldown,
                        Mathf.Min(p.data.weaponHandler.gun.sinceAttack, p.data.weaponHandler.gun.usedCooldown)
                    )
                );
                StatMessageHandler.BroadcastPlayerStats(stats.ToList());
            }
        }


        private class StatMessageHandler : WebSocketBehavior
        {
            public static StatMessageHandler Instance { get; private set; }

            protected override void OnOpen()
            {
                Instance = this;
                Send("Welcome to the server!");
            }

            public static void Broadcast(string message)
            {
                Instance.Sessions.Broadcast(message);
            }

            [Serializable]
            public class PlayerStats
            {
                public List<PlayerStat> stats;

                public PlayerStats(List<PlayerStat> stats)
                {
                    this.stats = stats;
                }
            }

            public static void BroadcastPlayerStats(List<PlayerStat> stats)
            {
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(stats);
                Broadcast(json);
            }
        }
    }
}
