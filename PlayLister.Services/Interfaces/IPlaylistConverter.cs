﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayLister.Services.Models;
using PlayLister.Services.Models.ServiceModels;

namespace PlayLister.Services.Interfaces
{
    public interface IPlaylistConverter
    {
        Task<PlaylistServiceModel> GetPlaylistItems(string platListId);
        Task<PlaylistServiceModel> GetPlaylistDataPerPage(string id, int page);
        Task RemoveItemFromPlaylist(int id, string playlistId);
        Task MakeSpotifyPlaylist(string playlistId, string accessToken);
    }
}
