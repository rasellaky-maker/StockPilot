namespace BLL.CoreServisecs
{
    /// <summary>
    /// Thrown when a user tries to buy a coin that he owns himself.
    /// </summary>
    public class OwnCoinPurchaseException : Exception
    {
        public OwnCoinPurchaseException()
            : base("You cannot buy your own coin.")
        {
        }
    }
}