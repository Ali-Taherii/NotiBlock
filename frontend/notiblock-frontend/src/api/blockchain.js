import apiClient from './api';

export async function issueRecallToBlockchain(recallId) {
    return await apiClient.post(`/recalls/${recallId}/issue-to-blockchain`);
}

export async function updateRecallStatusOnBlockchain(recallId, status) {
    return await apiClient.post(`/recalls/${recallId}/update-status-blockchain`, { status });
}

export async function getBlockchainProof(recallId) {
    return await apiClient.get(`/recalls/${recallId}/blockchain-proof`);
}

export async function verifyBlockchainTransaction(txHash) {
    return await apiClient.get(`/blockchain/verify/${txHash}`);
}