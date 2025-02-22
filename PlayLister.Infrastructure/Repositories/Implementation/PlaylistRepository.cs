﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PlayLister.Infrastructure.Context;
using PlayLister.Infrastructure.Models;
using PlayLister.Infrastructure.Repositories.Interfaces;

namespace PlayLister.Infrastructure.Repositories.Implementation
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly PlayListerContext _context;
        private static readonly Encoding Utf8Encoder = Encoding.GetEncoding(
            "UTF-8",
            new EncoderReplacementFallback(string.Empty),
            new DecoderExceptionFallback()
        );

        public PlaylistRepository(PlayListerContext context)
        {
            _context = context;
        }

        public async Task<PlaylistRepoModel> GetPlaylistItemsAsync(string id, int page)
        {
            var data = await _context.Playlists.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            var maxPage = data.Items.Count / 15;

             if (page < maxPage)
                 data.Items = data.Items.Take(new Range(15 * page, 15 * (page + 1))).ToList();
             else
                 data.Items = null;

            return data;
        }

        public async Task<PlaylistRepoModel> GetFullPlaylist(string id)
        {
            return await _context.Playlists.Include(x => x.Items).AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task AddPlaylist(PlaylistRepoModel playlist)
        {
            playlist.PreviousPageToken ??= string.Empty;
            playlist.NextPageToken ??= string.Empty;
            playlist = CheckTextLength(playlist);
            
            try
            {
                var check = _context.Playlists.FirstOrDefault(x => x.Id == playlist.Id);
                if (check == null)
                {
                    _context.Playlists.Add(playlist);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddOnlyItem(PlaylistRepoModel playlist)
        {
            playlist.PreviousPageToken ??= string.Empty;
            playlist.NextPageToken ??= string.Empty;
            playlist = CheckTextLength(playlist);

            try
            {
                foreach (var item in playlist.Items)
                {
                    var check = await _context.Items.FirstOrDefaultAsync(x => x.Title == item.Title);
                    if (check == null)
                    {
                        item.PlaylistId = playlist.Id;
                        _context.Items.Add(item);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task RemoveItem(int id, string playlistId)
        {
            var check = await _context.Items.FirstOrDefaultAsync(x => x.Id == id && x.PlaylistId == playlistId);
            if (check != null)
            {
                _context.Items.Remove(check);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> CheckIfExists(string id)
        {
            var check = await _context.Playlists.FirstOrDefaultAsync(x => x.Id == id);
            if (check != null)
                return true;
            else
                return false;
        }

        private  PlaylistRepoModel CheckTextLength(PlaylistRepoModel model)
        {
            foreach (var item in model.Items)
            {
                if (item.Description.Length > 81)
                {
                    item.Description = item.Description.Substring(0, 80);
                    item.Description = Utf8Encoder.GetString(Utf8Encoder.GetBytes(item.Description));
                }

                if (item.Title.Length > 150)
                {
                    item.Title = item.Title.Substring(0, 149);
                    item.Title = Utf8Encoder.GetString(Utf8Encoder.GetBytes(item.Title));
                }
            }
            return model;
        }
    }
}
