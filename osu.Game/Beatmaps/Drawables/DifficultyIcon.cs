﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class DifficultyIcon : CompositeDrawable, IHasCustomTooltip<DifficultyIconTooltipContent>
    {
        /// <summary>
        /// Size of this difficulty icon.
        /// </summary>
        public new Vector2 Size
        {
            get => iconContainer.Size;
            set => iconContainer.Size = value;
        }

        [NotNull]
        private readonly IBeatmapInfo beatmapInfo;

        [CanBeNull]
        private readonly IRulesetInfo ruleset;

        [CanBeNull]
        private readonly IReadOnlyList<Mod> mods;

        [Resolved]
        private IRulesetStore rulesets { get; set; }

        private readonly bool shouldShowTooltip;

        private readonly bool performBackgroundDifficultyLookup;

        private Drawable background;

        private readonly Container iconContainer;

        private readonly Bindable<StarDifficulty> difficultyBindable = new Bindable<StarDifficulty>();

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/> with a given <see cref="RulesetInfo"/> and <see cref="Mod"/> combination.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to show the difficulty of.</param>
        /// <param name="ruleset">The ruleset to show the difficulty with.</param>
        /// <param name="mods">The mods to show the difficulty with.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        /// <param name="performBackgroundDifficultyLookup">Whether to perform difficulty lookup (including calculation if necessary).</param>
        public DifficultyIcon([NotNull] IBeatmapInfo beatmapInfo, [CanBeNull] IRulesetInfo ruleset, [CanBeNull] IReadOnlyList<Mod> mods, bool shouldShowTooltip = true,
                              bool performBackgroundDifficultyLookup = true)
            : this(beatmapInfo, shouldShowTooltip, performBackgroundDifficultyLookup)
        {
            this.ruleset = ruleset ?? beatmapInfo.Ruleset;
            this.mods = mods ?? Array.Empty<Mod>();
        }

        /// <summary>
        /// Creates a new <see cref="DifficultyIcon"/> that follows the currently-selected ruleset and mods.
        /// </summary>
        /// <param name="beatmapInfo">The beatmap to show the difficulty of.</param>
        /// <param name="shouldShowTooltip">Whether to display a tooltip when hovered.</param>
        /// <param name="performBackgroundDifficultyLookup">Whether to perform difficulty lookup (including calculation if necessary).</param>
        public DifficultyIcon([NotNull] IBeatmapInfo beatmapInfo, bool shouldShowTooltip = true, bool performBackgroundDifficultyLookup = true)
        {
            this.beatmapInfo = beatmapInfo ?? throw new ArgumentNullException(nameof(beatmapInfo));
            this.shouldShowTooltip = shouldShowTooltip;
            this.performBackgroundDifficultyLookup = performBackgroundDifficultyLookup;

            AutoSizeAxes = Axes.Both;

            InternalChild = iconContainer = new Container { Size = new Vector2(20f) };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            iconContainer.Children = new Drawable[]
            {
                new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.06f),

                        Type = EdgeEffectType.Shadow,
                        Radius = 3,
                    },
                    Child = background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.ForStarDifficulty(beatmapInfo.StarRating) // Default value that will be re-populated once difficulty calculation completes
                    },
                },
                new ConstrainedIconContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    // the null coalesce here is only present to make unit tests work (ruleset dlls aren't copied correctly for testing at the moment)
                    Icon = getRulesetIcon()
                },
            };

            if (performBackgroundDifficultyLookup)
                iconContainer.Add(new DelayedLoadUnloadWrapper(createDifficultyRetriever, 0));
            else
                difficultyBindable.Value = new StarDifficulty(beatmapInfo.StarRating, 0);

            difficultyBindable.BindValueChanged(difficulty => background.Colour = colours.ForStarDifficulty(difficulty.NewValue.Stars));
        }

        private Drawable createDifficultyRetriever()
        {
            if (ruleset != null && mods != null)
                return new DifficultyRetriever(beatmapInfo, ruleset, mods) { StarDifficulty = { BindTarget = difficultyBindable } };

            return new DifficultyRetriever(beatmapInfo) { StarDifficulty = { BindTarget = difficultyBindable } };
        }

        private Drawable getRulesetIcon()
        {
            int? onlineID = (ruleset ?? beatmapInfo.Ruleset).OnlineID;

            if (onlineID >= 0 && rulesets.GetRuleset(onlineID.Value)?.CreateInstance() is Ruleset rulesetInstance)
                return rulesetInstance.CreateIcon();

            return new SpriteIcon { Icon = FontAwesome.Regular.QuestionCircle };
        }

        ITooltip<DifficultyIconTooltipContent> IHasCustomTooltip<DifficultyIconTooltipContent>.
            GetCustomTooltip() => new DifficultyIconTooltip();

        DifficultyIconTooltipContent IHasCustomTooltip<DifficultyIconTooltipContent>.
            TooltipContent => shouldShowTooltip ? new DifficultyIconTooltipContent(beatmapInfo, difficultyBindable) : null;
    }
}
