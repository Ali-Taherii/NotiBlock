import { useState } from "react";
import { submitRecallUpdateRequest } from "../../../api/recalls";

export default function RecallUpdateRequestModal({ recall, onClose, onSuccess }) {
  const [form, setForm] = useState({
    reason: "",
    actionRequired: "",
    status: recall.status,
    notes: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [errorMessage, setErrorMessage] = useState(null);

  const handleChange = (field, value) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!form.reason && !form.actionRequired && form.status === recall.status && !form.notes.trim()) {
      setErrorMessage("Provide at least one change before submitting.");
      return;
    }

    const payload = {
      reason: form.reason?.trim() || undefined,
      actionRequired: form.actionRequired?.trim() || undefined,
      status: form.status,
      notes: form.notes?.trim() || undefined,
    };

    setSubmitting(true);
    setErrorMessage(null);

    try {
      await submitRecallUpdateRequest(recall.id, payload);
      if (onSuccess) {
        onSuccess();
      }
      onClose();
    } catch (err) {
      console.error("Failed to submit update request", err);
      setErrorMessage(err.response?.data?.message || "Unable to submit update request. Please try again.");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-lg max-w-xl w-full p-6">
        <div className="flex justify-between items-center mb-4">
          <h3 className="text-xl font-semibold">Propose Recall Update</h3>
          <button onClick={onClose} className="text-gray-500 hover:text-gray-700">
            ✕
          </button>
        </div>

        <p className="text-sm text-gray-600 mb-4">
          Any change you submit will be reviewed by regulators. Provide clear details so they can approve the update quickly.
        </p>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Reason</label>
            <textarea
              value={form.reason}
              onChange={(e) => handleChange("reason", e.target.value)}
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="3"
              placeholder="Describe the updated reason (optional)"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Action Required</label>
            <textarea
              value={form.actionRequired}
              onChange={(e) => handleChange("actionRequired", e.target.value)}
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="3"
              placeholder="Explain new instructions for consumers (optional)"
            />
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Status</label>
            <select
              value={form.status}
              onChange={(e) => handleChange("status", Number(e.target.value))}
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={0}>Active</option>
              <option value={1}>Resolved</option>
              <option value={2}>Cancelled</option>
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Notes for Regulator</label>
            <textarea
              value={form.notes}
              onChange={(e) => handleChange("notes", e.target.value)}
              className="w-full border border-gray-300 rounded px-3 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
              rows="3"
              placeholder="Add context to help regulators understand the change"
            />
          </div>

          {errorMessage && (
            <div className="p-3 bg-red-50 border border-red-200 rounded text-sm text-red-700">
              {errorMessage}
            </div>
          )}

          <div className="flex gap-2 justify-end">
            <button
              type="button"
              onClick={onClose}
              className="px-4 py-2 bg-gray-200 text-gray-700 rounded hover:bg-gray-300"
              disabled={submitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              disabled={submitting}
              className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? "Submitting..." : "Submit Update Request"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
