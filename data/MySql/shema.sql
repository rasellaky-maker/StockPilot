CREATE TABLE IF NOT EXISTS User (
    Id             INT PRIMARY KEY AUTO_INCREMENT,
    Username       VARCHAR(255) NOT NULL UNIQUE,
    PassHash       VARCHAR(255) NOT NULL,
    PassSalt       VARCHAR(255) NOT NULL,
    Email          VARCHAR(255) NOT NULL UNIQUE,
    AvailableCoins TINYINT UNSIGNED NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS Coin (
    Id             INT PRIMARY KEY AUTO_INCREMENT,
    Name           VARCHAR(255) NOT NULL UNIQUE,
    Value          DECIMAL(18, 8) NOT NULL,
    NumberOfBuyers INT NOT NULL DEFAULT 0,
    Image          LONGTEXT,
    UserId         INT NULL,
    CONSTRAINT fk_coin_user FOREIGN KEY (UserId) REFERENCES User(Id) ON DELETE SET NULL
);

CREATE TABLE IF NOT EXISTS Wallet (
    Id      INT PRIMARY KEY AUTO_INCREMENT,
    UserId  INT NOT NULL,
    Balance DECIMAL(18, 8) NOT NULL,
    FOREIGN KEY (UserId) REFERENCES User(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS UsersCoins (
    Id              INT PRIMARY KEY AUTO_INCREMENT,
    UserId          INT NOT NULL,
    CoinId          INT NOT NULL,
    Quantity        DECIMAL(18, 8) NOT NULL DEFAULT 1,
    AcquisitionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (UserId) REFERENCES User(Id) ON DELETE CASCADE,
    FOREIGN KEY (CoinId) REFERENCES Coin(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS CoinPriceHistory (
    Id         INT PRIMARY KEY AUTO_INCREMENT,
    CoinId     INT NOT NULL,
    Price      DECIMAL(18, 8) NOT NULL,
    RecordedAt DATETIME NOT NULL DEFAULT (UTC_TIMESTAMP()),
    FOREIGN KEY (CoinId) REFERENCES Coin(Id) ON DELETE CASCADE,
    INDEX idx_coin_time (CoinId, RecordedAt)
);

CREATE INDEX idx_wallet_userid ON Wallet(UserId);
CREATE INDEX idx_userscoins_userid ON UsersCoins(UserId);
CREATE INDEX idx_userscoins_coinid ON UsersCoins(CoinId);

CREATE INDEX idx_history_recordedat ON CoinPriceHistory(RecordedAt);

INSERT INTO CoinPriceHistory (CoinId, Price, RecordedAt)
SELECT c.Id, c.Value, UTC_TIMESTAMP()
FROM Coin c
WHERE NOT EXISTS (SELECT 1 FROM CoinPriceHistory h WHERE h.CoinId = c.Id);

DROP EVENT IF EXISTS cleanup_coinpricehistory;
CREATE EVENT cleanup_coinpricehistory
ON SCHEDULE EVERY 1 DAY
DO
  DELETE FROM CoinPriceHistory
  WHERE RecordedAt < UTC_TIMESTAMP() - INTERVAL 1 MONTH;