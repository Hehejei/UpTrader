using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System.Collections.Concurrent;
using UpTrader.Data.DataWallet;

public class WalletService
{
    private readonly DbCntx _dbContext;

    public WalletService(DbCntx dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<WalletViewModel>> GetWalletsAsync(int take, int skip)
    {
        var wallets = await _dbContext.Wallets
            .OrderBy(w => w.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync();

        var web3 = new Web3("https://mainnet.infura.io/v3/49ec41a221e5423a90cf0a4a5c1623e8");

        var walletsView = new ConcurrentBag<WalletViewModel>();

        await Parallel.ForEachAsync(wallets, async (wallet, cancellationToken) =>
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(wallet.Address);
            var walletView = new WalletViewModel
            {
                Id = wallet.Id,
                Address = wallet.Address,
                Balance = Web3.Convert.FromWei(balance, UnitConversion.EthUnit.Ether)
            };
            walletsView.Add(walletView);
        });

        return walletsView.ToList();
    }
}
