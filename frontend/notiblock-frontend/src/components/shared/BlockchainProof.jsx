import { useState, useEffect } from 'react';
import { FiCheckCircle, FiClock, FiExternalLink, FiAlertCircle } from 'react-icons/fi';
import { blockchainService } from '../../utils/blockchain';

export default function BlockchainProof({ transactionHash, recallId }) {
  const [receipt, setReceipt] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [blockNumber, setBlockNumber] = useState(null);

  useEffect(() => {
    if (transactionHash) {
      fetchProof();
    }
  }, [transactionHash]);

  const fetchProof = async () => {
    try {
      setLoading(true);
      setError(null);

      const [txReceipt, currentBlock] = await Promise.all([
        blockchainService.getTransactionReceipt(transactionHash),
        blockchainService.getBlockNumber()
      ]);

      setReceipt(txReceipt);
      setBlockNumber(currentBlock);
    } catch (err) {
      console.error('Error fetching blockchain proof:', err);
      setError('Failed to verify blockchain proof');
    } finally {
      setLoading(false);
    }
  };

  if (!transactionHash) {
    return (
      <div className="bg-yellow-50 border border-yellow-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-yellow-700">
          <FiClock className="text-xl" />
          <span className="font-medium">Not yet recorded on blockchain</span>
        </div>
      </div>
    );
  }

  if (loading) {
    return (
      <div className="bg-gray-50 border border-gray-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-gray-600">
          <FiClock className="text-xl animate-spin" />
          <span>Verifying blockchain proof...</span>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center gap-2 text-red-700">
          <FiAlertCircle className="text-xl" />
          <span>{error}</span>
        </div>
      </div>
    );
  }

  const confirmations = receipt && blockNumber 
    ? blockNumber - receipt.blockNumber + 1 
    : 0;

  return (
    <div className="bg-green-50 border border-green-200 rounded-lg p-4">
      <div className="flex items-start gap-3">
        <FiCheckCircle className="text-green-600 text-2xl flex-shrink-0 mt-1" />
        
        <div className="flex-1">
          <h4 className="font-semibold text-green-800 mb-2">
            Verified on Blockchain
          </h4>
          
          <div className="space-y-2 text-sm text-gray-700">
            <div className="flex justify-between">
              <span className="text-gray-600">Transaction:</span>
              <a
                href={blockchainService.getExplorerLink(transactionHash)}
                target="_blank"
                rel="noopener noreferrer"
                className="text-blue-600 hover:text-blue-800 flex items-center gap-1"
              >
                {blockchainService.formatTxHash(transactionHash)}
                <FiExternalLink />
              </a>
            </div>
            
            {receipt && (
              <>
                <div className="flex justify-between">
                  <span className="text-gray-600">Block:</span>
                  <span className="font-mono">{receipt.blockNumber}</span>
                </div>
                
                <div className="flex justify-between">
                  <span className="text-gray-600">Confirmations:</span>
                  <span className="font-semibold text-green-600">{confirmations}</span>
                </div>
                
                <div className="flex justify-between">
                  <span className="text-gray-600">Status:</span>
                  <span className="text-green-600 font-medium">
                    {receipt.status === 1 ? 'Success' : 'Failed'}
                  </span>
                </div>
              </>
            )}
          </div>
          
          <button
            onClick={fetchProof}
            className="mt-3 text-sm text-blue-600 hover:text-blue-800"
          >
            Refresh Status
          </button>
        </div>
      </div>
    </div>
  );
}