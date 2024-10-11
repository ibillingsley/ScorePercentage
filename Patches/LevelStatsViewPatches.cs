using SiraUtil.Affinity;
using SongPlayHistory.Model;
using SongPlayHistory.SongPlayData;
using System;
using System.Threading.Tasks;

namespace ScorePercentage.Patches
{
    class LevelStatsViewPatches : IAffinity
    {
        private LevelStatsView? _levelStatsView;
        private BeatmapKey? _beatmapKey;
        private BeatmapKey? _maxScoreKey;
        private int _highScore = 0;
        private int _maxScore = 0;

        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPostfix]
        private void PostfixShowStats(LevelStatsView __instance, in BeatmapKey beatmapKey, PlayerData playerData)
        {
            if (!PluginConfig.Instance.EnableMenuHighscore)
            {
                return;
            }

            _levelStatsView = __instance;
            _beatmapKey = beatmapKey;

            PlayerLevelStatsData playerLevelStatsData = playerData.GetOrCreatePlayerLevelStatsData(beatmapKey);
            _highScore = playerLevelStatsData.validScore ? playerLevelStatsData.highScore : 0;
            UpdateHighScoreText();
        }

        [AffinityPatch(typeof(ScoringCacheManager), nameof(ScoringCacheManager.GetScoringInfo))]
        [AffinityPostfix]
        private async void PostfixGetScoringInfo(BeatmapKey beatmapKey, Task<LevelScoringCache> __result)
        {
            if (!PluginConfig.Instance.EnableMenuHighscore)
            {
                return;
            }

            var cache = await __result;
            _maxScoreKey = beatmapKey;
            _maxScore = cache.MaxMultipliedScore;
            UpdateHighScoreText();
        }

        private void UpdateHighScoreText()
        {
            if (_highScore == 0 || _maxScore == 0 || _beatmapKey != _maxScoreKey || _levelStatsView == null)
            {
                return;
            }
            var percentage = ScorePercentageCommon.calculatePercentage(_maxScore, _highScore);
            _levelStatsView._highScoreText.text = _highScore.ToString() + " " + "(" + Math.Round(percentage, 2).ToString() + "%)";
        }
    }
}
