import { useEffect, useMemo, useState } from "react";
import {
  FiAlertTriangle,
  FiCheckCircle,
  FiClock,
  FiRefreshCcw,
  FiShield,
  FiTag,
  FiUsers
} from "react-icons/fi";
import { approveRecall, getPendingRecalls, rejectRecall } from "../../../api/recalls";
import { useToast } from "../../../hooks/useToast";
import Toast from "../../shared/Toast";

export default function RecallApprovalsSection() {
  const [recalls, setRecalls] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedRecall, setSelectedRecall] = useState(null);
  const [approvalNotes, setApprovalNotes] = useState("");
  const [rejectionReason, setRejectionReason] = useState("");
  const [submitting, setSubmitting] = useState(null);
  const { toast, success, error, hideToast } = useToast();

  useEffect(() => {
    fetchPendingRecalls();
  }, []);

  const queueStats = useMemo(() => {
    if (recalls.length === 0) {
      return {
        oldest: "—",
        avgAge: "—"
      };
    }

    const now = Date.now();
    const ages = recalls.map((recall) => now - new Date(recall.createdAt || recall.issuedAt || recall.lastUpdatedAt).getTime());
    const oldestMs = Math.max(...ages);
    const avgMs = ages.reduce((acc, age) => acc + age, 0) / ages.length;

    const formatDuration = (ms) => {
      const hours = Math.floor(ms / (1000 * 60 * 60));
      if (hours < 1) {
        const mins = Math.max(1, Math.floor(ms / (1000 * 60)));
        return `${mins} min`;
      }
      if (hours < 24) {
        return `${hours} hr`;
      }
      const days = Math.floor(hours / 24);
      return `${days} day${days === 1 ? "" : "s"}`;
    };

    return {
      oldest: formatDuration(oldestMs),
      avgAge: formatDuration(avgMs)
    };
  }, [recalls]);

  const fetchPendingRecalls = async () => {
    try {
      setLoading(true);
      const response = await getPendingRecalls();
      const data = response?.data?.data || response?.data || response || [];
      setRecalls(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Failed to load pending recalls", err);
      error("Unable to load pending recalls");
      setRecalls([]);
    } finally {
      setLoading(false);
    }
  };

  const openModal = (recall) => {
    setSelectedRecall(recall);
    setApprovalNotes("");
    setRejectionReason("");
  };

  const closeModal = () => {
    setSelectedRecall(null);
    setApprovalNotes("");
    setRejectionReason("");
    setSubmitting(null);
  };

  const handleApprove = async () => {
    if (!selectedRecall) return;

    try {
      setSubmitting("approve");
      await approveRecall(selectedRecall.id, {
        notes: approvalNotes?.trim() || undefined
      });
      success("Recall approved and activated on-chain");
      closeModal();
      fetchPendingRecalls();
    } catch (err) {
      console.error("Failed to approve recall", err);
      error(err.response?.data?.message || "Unable to approve recall");
      setSubmitting(null);
    }
  };

  const handleReject = async () => {
    if (!selectedRecall) return;

    if (!rejectionReason.trim() || rejectionReason.trim().length < 10) {
      error("Provide a rejection reason (min 10 characters)");
      return;
    }

    try {
      setSubmitting("reject");
      await rejectRecall(selectedRecall.id, { reason: rejectionReason.trim() });
      success("Recall rejected");
      closeModal();
      fetchPendingRecalls();
    } catch (err) {
      console.error("Failed to reject recall", err);
      error(err.response?.data?.message || "Unable to reject recall");
      setSubmitting(null);
    }
  };

  const renderQueueCards = () => (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4 mb-6">
      <div className="border rounded-lg p-4 bg-white shadow-sm">
        <p className="text-sm text-gray-500">Pending approvals</p>
        <p className="text-3xl font-semibold text-gray-900 mt-1">{recalls.length}</p>
      </div>
      <div className="border rounded-lg p-4 bg-white shadow-sm">
        <p className="text-sm text-gray-500">Oldest in queue</p>
        <p className="text-2xl font-semibold text-gray-900 mt-1">{queueStats.oldest}</p>
      </div>
      <div className="border rounded-lg p-4 bg-white shadow-sm">
        <p className="text-sm text-gray-500">Avg. waiting time</p>
        <p className="text-2xl font-semibold text-gray-900 mt-1">{queueStats.avgAge}</p>
      </div>
      <div className="border rounded-lg p-4 bg-white shadow-sm">
        <p className="text-sm text-gray-500">With update requests</p>
        <p className="text-3xl font-semibold text-gray-900 mt-1">
          {recalls.reduce((acc, recall) => acc + (recall.pendingUpdateRequestCount ? 1 : 0), 0)}
        </p>
      </div>
    </div>
  );

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />

      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <h2 className="text-xl font-semibold">Recall Approvals</h2>
          <p className="text-sm text-gray-600">Review pending manufacturer proposals and activate them on-chain.</p>
        </div>
        <button
          onClick={fetchPendingRecalls}
          className="self-start md:self-auto flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          disabled={loading}
        >
          <FiRefreshCcw className="text-sm" />
          Refresh
        </button>
      </div>

      {renderQueueCards()}

      {loading ? (
        <div className="text-center py-8 text-gray-600">Loading pending recalls...</div>
      ) : recalls.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 border border-dashed border-gray-200 rounded-lg">
          <FiShield className="mx-auto text-5xl text-gray-400 mb-3" />
          <p className="text-gray-700 font-medium">No recall approvals waiting 🎉</p>
          <p className="text-sm text-gray-500">You'll be notified when manufacturers submit a new recall proposal.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {recalls.map((recall) => (
            <div key={recall.id} className="border rounded-lg p-5 bg-white shadow-sm">
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div>
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <FiUsers className="text-gray-400" />
                    Manufacturer
                  </div>
                  <p className="text-lg font-semibold text-gray-900">
                    {recall.manufacturerName || recall.manufacturerId}
                  </p>
                </div>
                <div>
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <FiTag className="text-gray-400" />
                    Product Serial
                  </div>
                  <p className="font-semibold text-gray-900">{recall.productId}</p>
                </div>
                <div>
                  <div className="flex items-center gap-2 text-sm text-gray-500 mb-1">
                    <FiClock className="text-gray-400" />
                    Submitted
                  </div>
                  <p className="font-semibold text-gray-900">
                    {new Date(recall.createdAt || recall.issuedAt).toLocaleString()}
                  </p>
                </div>
              </div>

              <div className="grid gap-4 md:grid-cols-2 mt-4">
                <div className="bg-gray-50 p-4 rounded">
                  <p className="text-xs uppercase tracking-wide text-gray-500 mb-1">Reason</p>
                  <p className="text-gray-800 leading-relaxed">{recall.reason}</p>
                </div>
                <div className="bg-gray-50 p-4 rounded">
                  <p className="text-xs uppercase tracking-wide text-gray-500 mb-1">Action Required</p>
                  <p className="text-gray-800 leading-relaxed">{recall.actionRequired}</p>
                </div>
              </div>

              <div className="mt-4 flex flex-wrap gap-3 text-sm">
                <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-yellow-100 text-yellow-800">
                  <FiClock /> Pending approval
                </span>
                {recall.pendingUpdateRequestCount > 0 && (
                  <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-purple-100 text-purple-800">
                    {recall.pendingUpdateRequestCount} update request(s)
                  </span>
                )}
              </div>

              <div className="mt-5 flex flex-col gap-2 md:flex-row md:justify-end">
                <button
                  onClick={() => openModal(recall)}
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Review & Decide
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {selectedRecall && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-2xl max-w-3xl w-full p-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-2xl font-semibold">Review Recall Proposal</h3>
                <p className="text-sm text-gray-600 mt-1">Product {selectedRecall.productId}</p>
              </div>
              <button className="text-gray-500 hover:text-gray-700" onClick={closeModal}>
                ✕
              </button>
            </div>

            <div className="grid gap-4 md:grid-cols-3 mb-6">
              <div className="bg-gray-50 p-4 rounded">
                <p className="text-xs uppercase tracking-wide text-gray-500">Manufacturer</p>
                <p className="text-gray-900 font-semibold mt-1">
                  {selectedRecall.manufacturerName || selectedRecall.manufacturerId}
                </p>
              </div>
              <div className="bg-gray-50 p-4 rounded">
                <p className="text-xs uppercase tracking-wide text-gray-500">Product Serial</p>
                <p className="text-gray-900 font-semibold mt-1">{selectedRecall.productId}</p>
              </div>
              <div className="bg-gray-50 p-4 rounded">
                <p className="text-xs uppercase tracking-wide text-gray-500">Submitted</p>
                <p className="text-gray-900 font-semibold mt-1">
                  {new Date(selectedRecall.createdAt || selectedRecall.issuedAt).toLocaleString()}
                </p>
              </div>
            </div>

            <div className="grid gap-4 md:grid-cols-2 mb-6">
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Reason</label>
                <div className="border rounded-lg p-3 text-sm text-gray-800 bg-gray-50 min-h-[120px]">
                  {selectedRecall.reason}
                </div>
              </div>
              <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">Action Required</label>
                <div className="border rounded-lg p-3 text-sm text-gray-800 bg-gray-50 min-h-[120px]">
                  {selectedRecall.actionRequired}
                </div>
              </div>
            </div>

            <div className="grid gap-6 md:grid-cols-2">
              <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                <h4 className="text-green-900 font-semibold flex items-center gap-2 mb-2">
                  <FiCheckCircle /> Approve & activate
                </h4>
                <textarea
                  value={approvalNotes}
                  onChange={(e) => setApprovalNotes(e.target.value)}
                  className="w-full border border-green-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
                  rows="4"
                  placeholder="Optional: add notes that will be shared with the manufacturer"
                />
                <button
                  onClick={handleApprove}
                  disabled={submitting === "approve"}
                  className="mt-3 w-full px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
                >
                  {submitting === "approve" ? "Approving..." : "Approve Recall"}
                </button>
              </div>

              <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                <h4 className="text-red-900 font-semibold flex items-center gap-2 mb-2">
                  <FiAlertTriangle /> Reject recall
                </h4>
                <textarea
                  value={rejectionReason}
                  onChange={(e) => setRejectionReason(e.target.value)}
                  className="w-full border border-red-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
                  rows="4"
                  placeholder="Explain why the recall is being rejected"
                />
                <button
                  onClick={handleReject}
                  disabled={submitting === "reject"}
                  className="mt-3 w-full px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                >
                  {submitting === "reject" ? "Rejecting..." : "Reject Recall"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
