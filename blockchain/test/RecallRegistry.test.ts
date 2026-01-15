import { expect } from "chai";
import { ethers } from "hardhat";
import { RecallRegistry } from "../typechain-types";
import { HardhatEthersSigner } from "@nomicfoundation/hardhat-ethers/signers";

describe("RecallRegistry", function () {
  let recallRegistry: RecallRegistry;
  let owner: HardhatEthersSigner;
  let manufacturer: HardhatEthersSigner;
  let regulator: HardhatEthersSigner;

  beforeEach(async function () {
    [owner, manufacturer, regulator] = await ethers.getSigners();
    
    const RecallRegistryFactory = await ethers.getContractFactory("RecallRegistry");
    recallRegistry = await RecallRegistryFactory.deploy();
    await recallRegistry.waitForDeployment();
  });

  describe("RecallIssued Event", function () {
    it("Should emit RecallIssued event with correct parameters", async function () {
      const recallId = 1;
      const productHash = ethers.keccak256(ethers.toUtf8Bytes("Product123"));
      const metadataHash = ethers.keccak256(ethers.toUtf8Bytes("Metadata123"));
      const actorRole = "Manufacturer";

      await expect(
        recallRegistry.connect(manufacturer).emitRecallIssued(
          recallId,
          productHash,
          metadataHash,
          actorRole
        )
      )
        .to.emit(recallRegistry, "RecallIssued")
        .withArgs(recallId, productHash, metadataHash, actorRole, manufacturer.address);
    });

    it("Should allow multiple recalls to be issued", async function () {
      const recallId1 = 1;
      const recallId2 = 2;
      const productHash1 = ethers.keccak256(ethers.toUtf8Bytes("Product1"));
      const productHash2 = ethers.keccak256(ethers.toUtf8Bytes("Product2"));
      const metadataHash1 = ethers.keccak256(ethers.toUtf8Bytes("Metadata1"));
      const metadataHash2 = ethers.keccak256(ethers.toUtf8Bytes("Metadata2"));

      await recallRegistry.emitRecallIssued(recallId1, productHash1, metadataHash1, "Manufacturer");
      await recallRegistry.emitRecallIssued(recallId2, productHash2, metadataHash2, "Regulator");

      // Both should succeed without reverting
    });
  });

  describe("RecallStatusChanged Event", function () {
    it("Should emit RecallStatusChanged event with correct parameters", async function () {
      const recallId = 1;
      const metadataHash = ethers.keccak256(ethers.toUtf8Bytes("UpdatedMetadata"));
      const newStatus = "Resolved";
      const actorRole = "Regulator";

      await expect(
        recallRegistry.connect(regulator).emitRecallStatusChanged(
          recallId,
          metadataHash,
          newStatus,
          actorRole
        )
      )
        .to.emit(recallRegistry, "RecallStatusChanged")
        .withArgs(recallId, metadataHash, newStatus, actorRole, regulator.address);
    });

    it("Should allow status changes from different actors", async function () {
      const recallId = 1;
      const metadataHash = ethers.keccak256(ethers.toUtf8Bytes("Metadata"));

      await recallRegistry.connect(manufacturer).emitRecallStatusChanged(
        recallId,
        metadataHash,
        "In Progress",
        "Manufacturer"
      );

      await recallRegistry.connect(regulator).emitRecallStatusChanged(
        recallId,
        metadataHash,
        "Approved",
        "Regulator"
      );

      // Both should succeed
    });
  });

  describe("Contract Deployment", function () {
    it("Should deploy successfully", async function () {
      const address = await recallRegistry.getAddress();
      expect(address).to.be.properAddress;
    });
  });
});