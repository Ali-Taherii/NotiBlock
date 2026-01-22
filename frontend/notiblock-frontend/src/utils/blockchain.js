import { ethers } from 'ethers';
import { blockchainConfig } from '../config/blockchain.config';

export class BlockchainService {
    constructor() {
        if (!blockchainConfig.rpcUrl) {
            throw new Error('RPC URL is not configured');
        }

        this.provider = new ethers.JsonRpcProvider(blockchainConfig.rpcUrl);
        this.contract = new ethers.Contract(
            blockchainConfig.contractAddress,
            blockchainConfig.contractAbi,
            this.provider
        );
    }

    /**
     * Get transaction details from Sepolia
     */
    async getTransaction(txHash) {
        try {
            const tx = await this.provider.getTransaction(txHash);
            return tx;
        } catch (error) {
            console.error('Error fetching transaction:', error);
            return null;
        }
    }

    /**
     * Get transaction receipt (confirmation status)
     */
    async getTransactionReceipt(txHash) {
        try {
            const receipt = await this.provider.getTransactionReceipt(txHash);
            return receipt;
        } catch (error) {
            console.error('Error fetching receipt:', error);
            return null;
        }
    }

    /**
     * Get block number
     */
    async getBlockNumber() {
        try {
            return await this.provider.getBlockNumber();
        } catch (error) {
            console.error('Error fetching block number:', error);
            return null;
        }
    }

    /**
     * Get recall events from blockchain
     */
    async getRecallEvents(recallId) {
        try {
            const filter = this.contract.filters.RecallIssued(recallId);
            const events = await this.contract.queryFilter(filter);
            return events;
        } catch (error) {
            console.error('Error fetching recall events:', error);
            return [];
        }
    }

    /**
     * Get recall status change events
     */
    async getRecallStatusEvents(recallId) {
        try {
            const filter = this.contract.filters.RecallStatusChanged(recallId);
            const events = await this.contract.queryFilter(filter);
            return events;
        } catch (error) {
            console.error('Error fetching status events:', error);
            return [];
        }
    }

    /**
     * Get Etherscan/Sepolia explorer link
     */
    getExplorerLink(txHash) {
        return `${blockchainConfig.etherscanBaseUrl}/tx/${txHash}`;
    }

    /**
     * Get address explorer link
     */
    getAddressLink(address) {
        return `${blockchainConfig.etherscanBaseUrl}/address/${address}`;
    }

    /**
     * Format address for display
     */
    formatAddress(address) {
        if (!address) return '';
        return `${address.substring(0, 6)}...${address.substring(address.length - 4)}`;
    }

    /**
     * Format transaction hash for display
     */
    formatTxHash(txHash) {
        if (!txHash) return '';
        return `${txHash.substring(0, 10)}...${txHash.substring(txHash.length - 8)}`;
    }

    /**
     * Wait for transaction confirmation
     */
    async waitForConfirmation(txHash, confirmations = 1) {
        try {
            const receipt = await this.provider.waitForTransaction(txHash, confirmations);
            return receipt;
        } catch (error) {
            console.error('Error waiting for confirmation:', error);
            return null;
        }
    }

    /**
     * Get contract info
     */
    getContractInfo() {
        return {
            address: blockchainConfig.contractAddress,
            network: blockchainConfig.network,
            chainId: blockchainConfig.chainId,
            deployedAt: blockchainConfig.deployedAt
        };
    }
}

export const blockchainService = new BlockchainService();