import { useState } from "react";
import { createRecall } from "../api/recalls";
import { useToast } from "../hooks/useToast";
import Toast from "./shared/Toast";

export default function RecallForm({refetch}) {
  const [productId, setProductId] = useState("");
  const [reason, setReason] = useState("");
  const [actionRequired, setActionRequired] = useState("");
  const { toast, success, error, hideToast } = useToast();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      const data = { productId, reason, actionRequired };
      await createRecall(data);
      setProductId("");
      setReason("");
      setActionRequired("");
      refetch(); // Refetch recalls after creation
      success("Recall created successfully!");
    } catch (err) {
      console.error("Error creating recall:", err);
      error("Failed to create recall. Please try again.");
    }
  };

  return (
    <div className="max-w-md mx-auto p-4 bg-white shadow-md rounded">
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />
      
      <h2 className="text-2xl font-bold mb-4">Create Recall</h2>
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block mb-1 font-medium">Product ID</label>
          <input
            type="text"
            className="w-full border border-gray-300 rounded px-3 py-2"
            value={productId}
            onChange={(e) => setProductId(e.target.value)}
          />
        </div>
        <div>
          <label className="block mb-1 font-medium">Reason</label>
          <input
            type="text"
            className="w-full border border-gray-300 rounded px-3 py-2"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
          />
        </div>
        <div>
          <label className="block mb-1 font-medium">Action Required</label>
          <input
            type="text"
            className="w-full border border-gray-300 rounded px-3 py-2"
            value={actionRequired}
            onChange={(e) => setActionRequired(e.target.value)}
          />
        </div>
        <button
          type="submit"
          className="bg-blue-600 text-white px-4 py-2 rounded hover:bg-blue-700"
        >
          Submit
        </button>
      </form>
    </div>
  );
}
