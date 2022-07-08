// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using Humanizer;
using osu.Framework.IO.Network;
using osu.Game.Online.API;
using osu.Game.Screens.OnlinePlay.Lounge.Components;

namespace osu.Game.Online.Rooms
{
    public class GetRoomsRequest : APIRequest<List<Room>>
    {
        private readonly RoomStatusFilter status;
        private readonly string category;

        public GetRoomsRequest(RoomStatusFilter status, string category)
        {
            this.status = status;
            this.category = category;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (status != RoomStatusFilter.Open)
                req.AddParameter("mode", status.ToString().Underscore().ToLowerInvariant());

            if (!string.IsNullOrEmpty(category))
                req.AddParameter("category", category);

            return req;
        }

        protected override string Target => "rooms";
    }
}
