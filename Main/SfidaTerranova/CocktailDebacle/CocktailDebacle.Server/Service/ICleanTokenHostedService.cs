using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


// questo è un servizio che si occupa di invalidare i token scaduti e rimuoverli dal database
namespace CocktailDebacle.Server.Service
{
    public interface ICleanTokenHostedService : IHostedService
    {
        Task TrackToken(User user, DateTime expiry);
    }

    public class CleanTokenHostedService : ICleanTokenHostedService, IDisposable
    {
        private readonly IServiceProvider _services; // servizio per accedere al contesto del database
        private readonly PriorityQueue<User, DateTime> _expirationQueue = new(); // coda prioritaria per tenere traccia dei token
        private readonly ILogger<CleanTokenHostedService> _logger; 
        private Timer? _timer;

        public CleanTokenHostedService(IServiceProvider services, ILogger<CleanTokenHostedService> logger)
        {
            _services = services;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitializeExpiredTokensAsync();
            _timer = new Timer(CheckExpiredTokens, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        private async Task InitializeExpiredTokensAsync()
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            //Pulisco eventuali token già scaduti all'avvio 
            var expiredUsers = await dbContext.DbUser
                .Where(u => u.TokenExpiration != null && u.TokenExpiration <= DateTime.UtcNow)
                .ToListAsync();

            if (expiredUsers.Count == 0)
            {
                return;
            }

            foreach (var user in expiredUsers)
            {
                user.Token = string.Empty;
                user.TokenExpiration = null;
            }
            await dbContext.SaveChangesAsync();

            // Carico in coda i token ancora validi
            var validUsers = await dbContext.DbUser
                .Where(u => u.TokenExpiration != null && u.TokenExpiration > DateTime.UtcNow)
                .ToListAsync();
            
            foreach (var user in validUsers)
            {
                _expirationQueue.Enqueue(user, user.TokenExpiration!.Value);
            }
        }

        private async void CheckExpiredTokens(object? state)
        {   
            var now = DateTime.UtcNow;
            try
            {
                while (_expirationQueue.TryPeek(out var user, out var expiry) && expiry <= now)
                {
                    _expirationQueue.Dequeue();
                    await InvalidateTokenAsync(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token invalidation");
            }
        }

        public Task TrackToken(User user, DateTime expiry)
        {
            _expirationQueue.Enqueue(user, expiry);
            return Task.CompletedTask;
        }

        private async Task InvalidateTokenAsync(User user)
        {
            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            var dbUser = await dbContext.DbUser.FindAsync(user.Id);
            if (dbUser != null && dbUser.TokenExpiration == user.TokenExpiration)
            {
                dbUser.Token = string.Empty;
                dbUser.TokenExpiration = null;
                await dbContext.SaveChangesAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}