﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    public class ReplayPlayer : Player
    {
        private readonly Score score;

        public ReplayPlayer(Score score, bool allowPause = true, bool showResults = true)
            : base(allowPause, showResults)
        {
            this.score = score;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DrawableRuleset?.SetReplayScore(score);
        }

        protected override ScoreInfo CreateScore() => score.ScoreInfo;
    }
}
