import { ethers as hreEthers } from "@nomicfoundation/hardhat-ethers";
import "@nomicfoundation/hardhat-toolbox";
import fs from "fs";
import path from "path";

async function main() {
  console.log("Deploying RecallRegistry contract...");

  const Factory = await ethers.getContractFactory("RecallRegistry");
  const contract = await Factory.deploy();
  await contract.waitForDeployment();

  const address = await contract.getAddress();
  console.log("RecallRegistry deployed to:", address);

  // Export contract address and ABI for frontend integration
  const deploymentInfo = {
    address: address,
    abi: JSON.parse(contract.interface.formatJson()),
    deployedAt: new Date().toISOString(),
    network: (await ethers.provider.getNetwork()).name,
  };

  const exportsDir = path.join(__dirname, "..", "exports");
  if (!fs.existsSync(exportsDir)) {
    fs.mkdirSync(exportsDir);
  }

  const exportPath = path.join(exportsDir, "RecallRegistry.json");
  fs.writeFileSync(exportPath, JSON.stringify(deploymentInfo, null, 2));

  console.log("Contract info exported to:", exportPath);
  console.log("\nDeployment Summary:");
  console.log("-------------------");
  console.log("Address:", address);
  console.log("Network:", deploymentInfo.network);
  console.log("Deployed at:", deploymentInfo.deployedAt);
}

main().catch((e) => {
  console.error(e);
  process.exitCode = 1;
});
