using HIUServices.DbContexts;
using HIUServices.Entities.Requests.Masters;

using HIUServices.Repostitories.Masters.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HIUServices.Repositories.Masters
{
    public class MasterService : IMasterService, IDisposable
    {
        private bool _disposed;
        private readonly RequestDbContext _dbContext;

        public MasterService(RequestDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<List<PurposeOfUse>> GetPurposeOfUse()
        {
            var purposeOfUses = await _dbContext.PurposeOfUses
                .FromSqlRaw("SELECT * FROM sp_hiu_select_purposeofuse()")
                .ToListAsync();

            return purposeOfUses;
        }



        public async Task<List<HiTypes>> GetHiTypes()
        {
            var hiTypes = await _dbContext.HiTypes
                .FromSqlRaw("SELECT * FROM sp_hiu_select_hi_types()")
                .ToListAsync();

            return hiTypes;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _dbContext.Dispose();
                }
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
