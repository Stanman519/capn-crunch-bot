using System.Collections.Generic;

namespace CapnCrunchGMBot.Models
{
    public class TradeTransactionSingle
    {
        public string version { get; set; }
        public string encoding { get; set; }
        public TransactionsSingle transactions { get; set; }
    }
    public class TransactionsSingle
    {
        public TradeSingle transaction { get; set; }
    }
    public class TradeSingle
    {
        public string franchise1_gave_up { get; set; }
        public string franchise { get; set; }
        public string franchise2_gave_up { get; set; }
        public string franchise2 { get; set; }
        public string timestamp { get; set; }
        public string comments { get; set; }
        public string type { get; set; }
        public string expires { get; set; }
    }
    public class TradeTransactionMulti
    {
        public string version { get; set; }
        public string encoding { get; set; }
        public TransactionsMulti transactions { get; set; }
    }
    public class TransactionsMulti
    {
        public List<TradeSingle> transaction { get; set; }
    }
    
}