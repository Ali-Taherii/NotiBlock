export default function RecallItem({ recall }) {
    return (
        <div className="p-4 border border-gray-300 rounded shadow-sm bg-white">
        <h3 className="text-lg font-semibold">Product ID: {recall.productId}</h3>
        <p className="text-gray-700">Reason: {recall.reason}</p>
        <p className="text-gray-700">Action Required: {recall.actionRequired}</p>
        </div>
    );
    }