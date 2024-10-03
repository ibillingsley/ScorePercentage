using SiraUtil.Affinity;
using System;
using System.Threading;

namespace ScorePercentage.Patches
{
    class LevelStatsViewPatches : IAffinity
    {
        private readonly BeatmapLevelLoader _beatmapLevelLoader;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;
        private readonly BeatmapDataLoader _beatmapDataLoader = new BeatmapDataLoader();

        public LevelStatsViewPatches(BeatmapLevelLoader beatmapLevelLoader, StandardLevelDetailViewController standardLevelDetailViewController, BeatmapLevelsEntitlementModel beatmapLevelsEntitlementModel)
        {
            _beatmapLevelLoader = beatmapLevelLoader;
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _beatmapLevelsEntitlementModel = beatmapLevelsEntitlementModel;
        }

        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPostfix]
        private void PostfixShowStats(LevelStatsView __instance, in BeatmapKey beatmapKey, PlayerData playerData)
        {
            //Update highScoreText, if enabled in Plugin Config
            if (!PluginConfig.Instance.EnableMenuHighscore || playerData == null)
            {
                return;
            }

            PlayerLevelStatsData playerLevelStatsData = playerData.GetOrCreatePlayerLevelStatsData(beatmapKey);

            if (playerLevelStatsData.validScore)
            {
                //Plugin.log.Debug("Valid Score");
                var currentScore = playerLevelStatsData.highScore;
                if (currentScore != 0)
                {
                    ShowPercentage(__instance, beatmapKey, playerData, currentScore);
                }
            }
        }

        private async void ShowPercentage(LevelStatsView __instance, BeatmapKey beatmapKey, PlayerData playerData, int currentScore)
        {
            var beatmapLevel = _standardLevelDetailViewController.beatmapLevel;
            var beatmapLevelDataVersion = await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapLevel.levelID, CancellationToken.None);
            var beatmapLevelData = await _beatmapLevelLoader.LoadBeatmapLevelDataAsync(beatmapLevel, beatmapLevelDataVersion, CancellationToken.None);
            var currentReadonlyBeatmapData = await _beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData.beatmapLevelData, beatmapKey, beatmapLevel.beatsPerMinute, true, null, beatmapLevelDataVersion, playerData.gameplayModifiers, playerData.playerSpecificSettings, false);

            int maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(currentReadonlyBeatmapData);
            //Plugin.log.Debug("Calculated Max Score: " + maxScore.ToString());

            if (maxScore != 0)
            {
                var currentPercentage = ScorePercentageCommon.calculatePercentage(maxScore, currentScore);
                //Plugin.log.Debug("Calculated Percentage");
                //Plugin.log.Debug("Adding Percentage to HighscoreText");

                __instance._highScoreText.text = currentScore.ToString() + " " + "(" + Math.Round(currentPercentage, 2).ToString() + "%)";
            }
        }
    }
}
