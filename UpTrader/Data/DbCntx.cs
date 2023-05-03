using Microsoft.EntityFrameworkCore;

namespace UpTrader.Data.DataWallet
{
    public class DbCntx : DbContext
    {
        public DbCntx(DbContextOptions<DbCntx> options) : base(options)
        {
        }

        public DbSet<Wallet> Wallets { get; set; }

    }
}

