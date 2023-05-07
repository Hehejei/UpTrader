using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Util;
using Nethereum.Web3;
using System.Collections.Concurrent;
using UpTrader.Data.DataWallet;

public class WalletService
{
    private readonly IServiceProvider _serviceProvider;

    public WalletService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<List<WalletViewModel>> GetWalletsAsync(int take, int skip, decimal? minBalance, decimal? maxBalance)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbCntx>();

        var query = dbContext.Wallets.AsQueryable();
        var web3 = new Web3("https://mainnet.infura.io/v3/49ec41a221e5423a90cf0a4a5c1623e8");

        var walletsView = new ConcurrentBag<WalletViewModel>();

        await Parallel.ForEachAsync(query.OrderBy(w => w.Id).Skip(skip).Take(take), async (wallet, cancellationToken) =>
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

        if (minBalance.HasValue && maxBalance.HasValue)
        {
            walletsView = new ConcurrentBag<WalletViewModel>(walletsView.Where(w => w.Balance >= minBalance && w.Balance <= maxBalance).Distinct());
        }

        else if (minBalance.HasValue)
        {
            walletsView = new ConcurrentBag<WalletViewModel>(walletsView.Where(w => w.Balance >= minBalance).Distinct());
        }

        else if (maxBalance.HasValue)
        {
            walletsView = new ConcurrentBag<WalletViewModel>(walletsView.Where(w => w.Balance <= maxBalance).Distinct());
        }

        return walletsView.ToList();
    }
}