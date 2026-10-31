# StockExchangeProjekt (SEP)

A simulated stock exchange for virtual coins. Users create their own coins,
trade them with a wallet balance, and watch prices move in real time.

## Features
- **Live prices:** a background service randomly changes every coin's price every few seconds
- **Price history & charts:** every price change is stored; the web client shows an interactive chart (1 h / 24 h / 7 d / 30 d)
- **Wallets & trading:** buy and sell coins at the current price, atomically (MySQL transactions)
- **Coin ownership:** users create coins; the owner receives a share of each purchase, and can't buy their own coin
- **Protected API:** write endpoints require an API key; actions are logged

## Tech stack
ASP.NET Core Web API · C# · MySQL · layered architecture (BO / DAL / BLL / API) · vanilla JavaScript + SVG frontend

## Getting started
1. Run `stockexchange.sql` to create the database
2. Set the connection string and API key in the API project
3. Start the API, then open `web/index.html`