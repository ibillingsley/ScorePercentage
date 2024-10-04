using SiraUtil.Affinity;
using SongPlayHistory.SongPlayData;
using System;

namespace ScorePercentage.Patches
{
    class LevelStatsViewPatches : IAffinity
    {
        private readonly IScoringCacheManager _scoringCacheManager;

        public LevelStatsViewPatches(IScoringCacheManager scoringCacheManager)
        {
            _scoringCacheManager = scoringCacheManager;
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
                    ShowPercentage(__instance, beatmapKey, currentScore);
                }
            }
        }

        async void ShowPercentage(LevelStatsView __instance, BeatmapKey beatmapKey, int currentScore)
        {
            var levelScoringCache = await _scoringCacheManager.GetScoringInfo(beatmapKey);
            int maxScore = levelScoringCache.MaxMultipliedScore;
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
