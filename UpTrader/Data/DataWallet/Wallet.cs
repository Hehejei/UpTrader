using System.ComponentModel.DataAnnotations;

namespace UpTrader.Data.DataWallet
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        public string Address { get; set; }
    }
}
