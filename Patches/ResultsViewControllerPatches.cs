using SiraUtil.Affinity;
using System;


namespace ScorePercentage.Patches
{
    class ResultsViewData
    {
        public static int highScore = 0;

        public ResultsViewData(PlayerDataModel playerDataModel, BeatmapKey beatmapKey)
        {
            var playerLevelStatsData = playerDataModel.playerData.GetOrCreatePlayerLevelStatsData(beatmapKey);
            highScore = playerLevelStatsData.validScore ? playerLevelStatsData.highScore : 0;
        }
    }

    class ResultsViewControllerPatches : IAffinity
    {
        [AffinityPatch(typeof(ResultsViewController), nameof(ResultsViewController.SetDataToUI))]
        [AffinityPostfix]
        private void PostfixSetDataToUI(ResultsViewController __instance)
        {
            int maxScore;
            double resultPercentage;
            int resultScore;
            int modifiedScore;
            int highScore = ResultsViewData.highScore;
            // Default Rank Text
            string rankTextLine1 = __instance._rankText.text;
            string rankTextLine2 = "";
            // Colors
            string colorPositive = "#00B300";
            string colorNegative = "#FF0000";
            //Empty for negatives, "+" for positives
            string positiveIndicator = "";
            LevelCompletionResults levelCompletionResults = __instance._levelCompletionResults;


            //Only calculate percentage, if map was successfully cleared
            if (levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                modifiedScore = levelCompletionResults.modifiedScore;
                //maxScore = ScorePercentageCommon.calculateMaxScore(__instance._difficultyBeatmap.beatmapData.cuttableNotesCount);

                maxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(__instance._transformedBeatmapData);


                //use modifiedScore with negative multipliers
                if (levelCompletionResults.gameplayModifiers.noFailOn0Energy
                    || (levelCompletionResults.gameplayModifiers.enabledObstacleType != GameplayModifiers.EnabledObstacleType.All)
                    || levelCompletionResults.gameplayModifiers.noArrows
                    || levelCompletionResults.gameplayModifiers.noBombs
                    || levelCompletionResults.gameplayModifiers.zenMode
                    || levelCompletionResults.gameplayModifiers.songSpeed == GameplayModifiers.SongSpeed.Slower
                    )
                {
                    resultScore = modifiedScore;
                }
                //use rawScore without and with positive modifiers to avoid going over 100% without recalculating maxScore
                else
                {
                    resultScore = levelCompletionResults.multipliedScore;
                }

                resultPercentage = ScorePercentageCommon.calculatePercentage(maxScore, resultScore);

                //disable wrapping and autosize (unneccessary?)
                __instance._rankText.autoSizeTextContainer = false;
                __instance._rankText.enableWordWrapping = false;

                //Rank Text Changes
                if (PluginConfig.Instance.EnableLevelEndRank)
                {
                    //Set Percentage to first line
                    rankTextLine1 = "<line-height=27.5%><size=60%>" + Math.Round(resultPercentage, 2).ToString() + "<size=45%>%";

                    // Add Percent Difference to 2nd Line if enabled and previous Score exists
                    if (PluginConfig.Instance.EnableScorePercentageDifference && highScore != 0)
                    {
                        double oldPercentage = ScorePercentageCommon.calculatePercentage(maxScore, highScore);
                        double percentageDifference = resultPercentage - oldPercentage;
                        string percentageDifferenceColor;
                        //Better or same Score
                        if (percentageDifference >= 0)
                        {
                            percentageDifferenceColor = colorPositive;
                            positiveIndicator = "+";
                        }
                        //Worse Score
                        else
                        {
                            percentageDifferenceColor = colorNegative;
                            positiveIndicator = "";
                            //Fix negative score rounding to exactly 0% just showing 0% instead of -0%
                            if (Math.Round(percentageDifference, 2) == 0)
                            {
                                positiveIndicator = "-";
                            }
                        }
                        rankTextLine2 = "\n<color=" + percentageDifferenceColor + "><size=40%>" + positiveIndicator + Math.Round(percentageDifference, 2).ToString() + "<size=30%>%";
                    }
                    __instance._newHighScoreText.SetActive(false);
                }//End Preparations for Changes to Rank Text

                __instance._rankText.text = rankTextLine1 + rankTextLine2;


                //Add ScoreDifference Calculation if enabled
                if (PluginConfig.Instance.EnableScoreDifference && highScore != 0)
                {
                    string scoreDifference = "";
                    string scoreDifferenceColor = "";
                    scoreDifference = ScoreFormatter.Format(modifiedScore - highScore);
                    //Better Score
                    if ((modifiedScore - highScore) >= 0)
                    {
                        scoreDifferenceColor = colorPositive;
                        positiveIndicator = "+";
                    }
                    //Worse Score
                    else if ((modifiedScore - highScore) < 0)
                    {
                        scoreDifferenceColor = colorNegative;
                        positiveIndicator = "";
                    }

                    //Build new ScoreText string
                    __instance._scoreText.text = "<line-height=27.5%><size=60%>" + ScoreFormatter.Format(modifiedScore) + "\n"
                            + "<size=40%><color=" + scoreDifferenceColor + "><size=40%>" + positiveIndicator + scoreDifference;

                    __instance._newHighScoreText.SetActive(false);
                }//End ScoreDifference Calculation

            }//End Level Cleared

            // Reset highScore
            ResultsViewData.highScore = 0;

        }//End Postfix Function

    }//End Class
}//End Namespace
