using IPA;
using IPA.Config;
using IPA.Utilities;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using IPA.Logging;
using System;
using SiraUtil.Zenject;
using ScorePercentage.Installers;


namespace ScorePercentage
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public static string PluginName => "ScorePercentage";
        internal static ScorePercentageCommon scorePercentageCommon;

        public static Logger log { get; private set; }

        [Init]
        public void Init(Logger logger, Config cfgProvider, Zenjector zenjector)
        {
            log = logger;
            PluginConfig.Instance = cfgProvider.Generated<PluginConfig>();
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.GameCore);
        }

        [OnStart]
        public void OnApplicationStart()
        {
            log.Debug("Starting ScorePercentage Plugin");
            scorePercentageCommon = new ScorePercentageCommon();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            log.Debug("Stopping ScorePercentage Plugin");
        }

        public void OnFixedUpdate()
        {

        }

        public void OnUpdate()
        {

        }

        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {

        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {

        }

        public void OnSceneUnloaded(Scene scene)
        {

        }
    }
}
