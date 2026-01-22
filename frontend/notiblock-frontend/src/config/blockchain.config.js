import contractData from '../../../blockchain/exports/RecallRegistry.json';

export const blockchainConfig = {
    rpcUrl: import.meta.env.VITE_SEPOLIA_RPC_URL,
    contractAddress: import.meta.env.VITE_CONTRACT_ADDRESS || contractData.address,
    chainId: parseInt(import.meta.env.VITE_CHAIN_ID || '11155111'),
    etherscanBaseUrl: import.meta.env.VITE_ETHERSCAN_BASE_URL || 'https://sepolia.etherscan.io',
    contractAbi: contractData.abi,
    network: contractData.network || 'sepolia',
    deployedAt: contractData.deployedAt
};

// Validation
if (!blockchainConfig.rpcUrl) {
    console.warn('VITE_SEPOLIA_RPC_URL not set in environment variables');
}

if (!blockchainConfig.contractAddress) {
    console.warn('VITE_CONTRACT_ADDRESS not set, using deployed contract address from exports');
}