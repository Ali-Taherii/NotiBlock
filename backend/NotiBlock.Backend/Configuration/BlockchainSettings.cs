namespace NotiBlock.Backend.Configuration
{
    public class BlockchainSettings
    {
        public string RpcUrl { get; set; } = string.Empty;
        public string PrivateKey { get; set; } = string.Empty;
        public string ContractAddress { get; set; } = string.Empty;
        public int ChainId { get; set; } = 11155111; // Sepolia
        public int ConfirmationBlocks { get; set; } = 1;
    }
}
