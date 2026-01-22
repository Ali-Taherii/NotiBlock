// SPDX-License-Identifier: MIT
pragma solidity ^0.8.24;

contract RecallRegistry {
    // Keep it simple: backend sends hashes, chain stores immutable log
    event RecallIssued(
        uint256 indexed recallId,
        bytes32 indexed productHash,
        bytes32 indexed metadataHash,
        string actorRole,
        address actor
    );

    event RecallStatusChanged(
        uint256 indexed recallId,
        bytes32 indexed metadataHash,
        string newStatus,
        string actorRole,
        address actor
    );

    function emitRecallIssued(
        uint256 recallId,
        bytes32 productHash,
        bytes32 metadataHash,
        string calldata actorRole
    ) external {
        emit RecallIssued(recallId, productHash, metadataHash, actorRole, msg.sender);
    }

    function emitRecallStatusChanged(
        uint256 recallId,
        bytes32 metadataHash,
        string calldata newStatus,
        string calldata actorRole
    ) external {
        emit RecallStatusChanged(recallId, metadataHash, newStatus, actorRole, msg.sender);
    }
}