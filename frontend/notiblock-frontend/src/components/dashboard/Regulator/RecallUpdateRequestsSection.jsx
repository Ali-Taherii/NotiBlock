import { useEffect, useMemo, useState } from "react";
import {
  FiAlertCircle,
  FiCheckCircle,
  FiEdit2,
  FiFileText,
  FiInfo,
  FiRefreshCcw,
  FiShuffle
} from "react-icons/fi";
import { decideRecallUpdate, getPendingRecallUpdates, getRecallById } from "../../../api/recalls";
import { useToast } from "../../../hooks/useToast";
import Toast from "../../shared/Toast";

const statusLabels = {
  0: "Active",
  1: "Resolved",
  2: "Cancelled",
  3: "Pending Approval",
  4: "Rejected"
};

export default function RecallUpdateRequestsSection() {
  const [requests, setRequests] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedRequest, setSelectedRequest] = useState(null);
  const [recallDetails, setRecallDetails] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [approvalNotes, setApprovalNotes] = useState("");
  const [rejectionNotes, setRejectionNotes] = useState("");
  const [submitting, setSubmitting] = useState(null);
  const { toast, success, error, hideToast } = useToast();

  useEffect(() => {
    fetchPendingRequests();
  }, []);

  const insights = useMemo(() => {
    const reasonUpdates = requests.filter((req) => req.proposedReason).length;
    const actionUpdates = requests.filter((req) => req.proposedActionRequired).length;
    const statusUpdates = requests.filter((req) => typeof req.proposedStatus === "number").length;

    return { reasonUpdates, actionUpdates, statusUpdates };
  }, [requests]);

  const fetchPendingRequests = async () => {
    try {
      setLoading(true);
      const response = await getPendingRecallUpdates();
      const data = response?.data?.data || response?.data || response || [];
      setRequests(Array.isArray(data) ? data : []);
    } catch (err) {
      console.error("Failed to load update requests", err);
      error("Unable to load update requests");
      setRequests([]);
    } finally {
      setLoading(false);
    }
  };

  const openModal = async (request) => {
    setSelectedRequest(request);
    setApprovalNotes("");
    setRejectionNotes("");
    setRecallDetails(null);
    setDetailsLoading(true);

    try {
      const response = await getRecallById(request.recallId);
      const data = response?.data?.data || response?.data || response || null;
      setRecallDetails(data);
    } catch (err) {
      console.error("Failed to fetch recall details", err);
      error("Could not load recall details");
      setRecallDetails(null);
    } finally {
      setDetailsLoading(false);
    }
  };

  const closeModal = () => {
    setSelectedRequest(null);
    setRecallDetails(null);
    setApprovalNotes("");
    setRejectionNotes("");
    setSubmitting(null);
  };

  const handleDecision = async (approve) => {
    if (!selectedRequest) return;

    if (!approve && (!rejectionNotes.trim() || rejectionNotes.trim().length < 10)) {
      error("Provide context for the rejection (min 10 characters)");
      return;
    }

    try {
      setSubmitting(approve ? "approve" : "reject");
      await decideRecallUpdate(selectedRequest.id, {
        approve,
        notes: approve ? approvalNotes?.trim() || undefined : rejectionNotes.trim()
      });
      success(approve ? "Update applied to recall" : "Update request rejected");
      closeModal();
      fetchPendingRequests();
    } catch (err) {
      console.error("Failed to decide update request", err);
      error(err.response?.data?.message || "Unable to process decision");
      setSubmitting(null);
    }
  };

  return (
    <div>
      <Toast show={toast.show} message={toast.message} type={toast.type} onClose={hideToast} />

      <div className="flex flex-col gap-3 md:flex-row md:items-center md:justify-between mb-6">
        <div>
          <h2 className="text-xl font-semibold">Recall Update Requests</h2>
          <p className="text-sm text-gray-600">Approve manufacturer-proposed edits to active recalls.</p>
        </div>
        <button
          onClick={fetchPendingRequests}
          className="self-start md:self-auto flex items-center gap-2 px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          disabled={loading}
        >
          <FiRefreshCcw className="text-sm" />
          Refresh
        </button>
      </div>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4 mb-6">
        <div className="border rounded-lg p-4 bg-white shadow-sm">
          <p className="text-sm text-gray-500">Pending requests</p>
          <p className="text-3xl font-semibold text-gray-900 mt-1">{requests.length}</p>
        </div>
        <div className="border rounded-lg p-4 bg-white shadow-sm">
          <p className="text-sm text-gray-500">Reason edits</p>
          <p className="text-3xl font-semibold text-gray-900 mt-1">{insights.reasonUpdates}</p>
        </div>
        <div className="border rounded-lg p-4 bg-white shadow-sm">
          <p className="text-sm text-gray-500">Action edits</p>
          <p className="text-3xl font-semibold text-gray-900 mt-1">{insights.actionUpdates}</p>
        </div>
        <div className="border rounded-lg p-4 bg-white shadow-sm">
          <p className="text-sm text-gray-500">Status changes</p>
          <p className="text-3xl font-semibold text-gray-900 mt-1">{insights.statusUpdates}</p>
        </div>
      </div>

      {loading ? (
        <div className="text-center py-8 text-gray-600">Loading update requests...</div>
      ) : requests.length === 0 ? (
        <div className="text-center py-10 bg-gray-50 border border-dashed border-gray-200 rounded-lg">
          <FiShuffle className="mx-auto text-5xl text-gray-400 mb-3" />
          <p className="text-gray-700 font-medium">No update requests pending</p>
          <p className="text-sm text-gray-500">Manufacturers will submit change proposals here for your review.</p>
        </div>
      ) : (
        <div className="space-y-4">
          {requests.map((request) => (
            <div key={request.id} className="border rounded-lg p-5 bg-white shadow-sm">
              <div className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
                <div>
                  <p className="text-xs uppercase tracking-wide text-gray-500">Manufacturer</p>
                  <p className="text-lg font-semibold text-gray-900">
                    {request.manufacturerName || request.manufacturerId}
                  </p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-wide text-gray-500">Recall ID</p>
                  <p className="text-sm font-mono text-gray-800">{request.recallId}</p>
                </div>
                <div>
                  <p className="text-xs uppercase tracking-wide text-gray-500">Submitted</p>
                  <p className="text-sm text-gray-800">
                    {new Date(request.createdAt).toLocaleString()}
                  </p>
                </div>
              </div>

              <div className="mt-4 grid gap-4 md:grid-cols-3 text-sm">
                <div className="bg-gray-50 p-3 rounded">
                  <p className="text-xs font-semibold text-gray-500 mb-1">Reason</p>
                  <p className="text-gray-700">
                    {request.proposedReason ? request.proposedReason : "No change"}
                  </p>
                </div>
                <div className="bg-gray-50 p-3 rounded">
                  <p className="text-xs font-semibold text-gray-500 mb-1">Action Required</p>
                  <p className="text-gray-700">
                    {request.proposedActionRequired ? request.proposedActionRequired : "No change"}
                  </p>
                </div>
                <div className="bg-gray-50 p-3 rounded">
                  <p className="text-xs font-semibold text-gray-500 mb-1">Status</p>
                  <p className="text-gray-700">
                    {typeof request.proposedStatus === "number"
                      ? statusLabels[request.proposedStatus] || request.proposedStatus
                      : "No change"}
                  </p>
                </div>
              </div>

              {request.manufacturerNotes && (
                <div className="mt-3 border-l-4 border-blue-200 bg-blue-50 p-3 text-sm text-blue-900">
                  <p className="font-semibold mb-1 flex items-center gap-2">
                    <FiInfo /> Manufacturer notes
                  </p>
                  <p>{request.manufacturerNotes}</p>
                </div>
              )}

              <div className="mt-5 flex flex-wrap gap-2 justify-end">
                <button
                  onClick={() => openModal(request)}
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Review Request
                </button>
              </div>
            </div>
          ))}
        </div>
      )}

      {selectedRequest && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50 p-4">
          <div className="bg-white rounded-lg shadow-2xl max-w-4xl w-full p-6">
            <div className="flex justify-between items-start mb-4">
              <div>
                <h3 className="text-2xl font-semibold">Review Update Request</h3>
                <p className="text-sm text-gray-600">Recall {selectedRequest.recallId}</p>
              </div>
              <button className="text-gray-500 hover:text-gray-700" onClick={closeModal}>
                ✕
              </button>
            </div>

            {detailsLoading ? (
              <div className="text-center py-10 text-gray-600">Loading recall details...</div>
            ) : (
              <div className="space-y-6">
                <div className="grid gap-4 md:grid-cols-3">
                  <div className="bg-gray-50 p-4 rounded">
                    <p className="text-xs uppercase tracking-wide text-gray-500">Manufacturer</p>
                    <p className="text-gray-900 font-semibold mt-1">
                      {selectedRequest.manufacturerName || selectedRequest.manufacturerId}
                    </p>
                  </div>
                  <div className="bg-gray-50 p-4 rounded">
                    <p className="text-xs uppercase tracking-wide text-gray-500">Submitted</p>
                    <p className="text-gray-900 font-semibold mt-1">
                      {new Date(selectedRequest.createdAt).toLocaleString()}
                    </p>
                  </div>
                  <div className="bg-gray-50 p-4 rounded">
                    <p className="text-xs uppercase tracking-wide text-gray-500">Current Status</p>
                    <p className="text-gray-900 font-semibold mt-1">
                      {recallDetails ? statusLabels[recallDetails.status] : "—"}
                    </p>
                  </div>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="border rounded-lg p-4">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Current Reason</p>
                    <p className="text-sm text-gray-800 min-h-[80px]">{recallDetails?.reason || "Not available"}</p>
                  </div>
                  <div className="border rounded-lg p-4 bg-gray-50">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Proposed Reason</p>
                    <p className="text-sm text-gray-800 min-h-[80px]">
                      {selectedRequest.proposedReason || "No change"}
                    </p>
                  </div>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="border rounded-lg p-4">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Current Action Required</p>
                    <p className="text-sm text-gray-800 min-h-[80px]">{recallDetails?.actionRequired || "Not available"}</p>
                  </div>
                  <div className="border rounded-lg p-4 bg-gray-50">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Proposed Action Required</p>
                    <p className="text-sm text-gray-800 min-h-[80px]">
                      {selectedRequest.proposedActionRequired || "No change"}
                    </p>
                  </div>
                </div>

                <div className="grid gap-4 md:grid-cols-2">
                  <div className="border rounded-lg p-4">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Current Status</p>
                    <p className="text-sm text-gray-800">
                      {recallDetails ? statusLabels[recallDetails.status] : "Not available"}
                    </p>
                  </div>
                  <div className="border rounded-lg p-4 bg-gray-50">
                    <p className="text-xs font-semibold text-gray-500 mb-1">Proposed Status</p>
                    <p className="text-sm text-gray-800">
                      {typeof selectedRequest.proposedStatus === "number"
                        ? statusLabels[selectedRequest.proposedStatus]
                        : "No change"}
                    </p>
                  </div>
                </div>

                <div className="grid gap-6 md:grid-cols-2">
                  <div className="bg-green-50 border border-green-200 rounded-lg p-4">
                    <h4 className="text-green-900 font-semibold flex items-center gap-2 mb-2">
                      <FiCheckCircle /> Apply update
                    </h4>
                    <textarea
                      value={approvalNotes}
                      onChange={(e) => setApprovalNotes(e.target.value)}
                      className="w-full border border-green-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-green-400"
                      rows="4"
                      placeholder="Optional notes for the manufacturer"
                    />
                    <button
                      onClick={() => handleDecision(true)}
                      disabled={submitting === "approve"}
                      className="mt-3 w-full px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700 disabled:opacity-50"
                    >
                      {submitting === "approve" ? "Approving..." : "Approve Update"}
                    </button>
                  </div>

                  <div className="bg-red-50 border border-red-200 rounded-lg p-4">
                    <h4 className="text-red-900 font-semibold flex items-center gap-2 mb-2">
                      <FiAlertCircle /> Reject update
                    </h4>
                    <textarea
                      value={rejectionNotes}
                      onChange={(e) => setRejectionNotes(e.target.value)}
                      className="w-full border border-red-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-red-400"
                      rows="4"
                      placeholder="Explain why the update is rejected"
                    />
                    <button
                      onClick={() => handleDecision(false)}
                      disabled={submitting === "reject"}
                      className="mt-3 w-full px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50"
                    >
                      {submitting === "reject" ? "Rejecting..." : "Reject Update"}
                    </button>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
