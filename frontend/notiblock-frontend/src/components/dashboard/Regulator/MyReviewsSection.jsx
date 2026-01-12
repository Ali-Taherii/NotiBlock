import { useState, useEffect } from 'react';
import { getMyReviews, updateReview, deleteReview } from '../../../api/regulatorReviews';
import { useToast } from '../../../hooks/useToast';
import { FiEdit2, FiTrash2, FiCheckCircle, FiXCircle, FiAlertCircle } from 'react-icons/fi';

export default function MyReviewsSection() {
  const [reviews, setReviews] = useState([]);
  const [loading, setLoading] = useState(true);
  const [selectedReview, setSelectedReview] = useState(null);
  const [showEditModal, setShowEditModal] = useState(false);
  const [decision, setDecision] = useState('');
  const [notes, setNotes] = useState('');
  const toast = useToast();

  const decisionLabels = {
    0: { label: 'Approved', icon: FiCheckCircle, color: 'green' },
    1: { label: 'Rejected', icon: FiXCircle, color: 'red' },
    2: { label: 'Needs More Info', icon: FiAlertCircle, color: 'yellow' },
    3: { label: 'Reopened', icon: FiAlertCircle, color: 'blue' }
  };

  useEffect(() => {
    fetchReviews();
  }, []);

  async function fetchReviews() {
    try {
      setLoading(true);
      const response = await getMyReviews();
      // Handle different response structures
      const reviewsData = response.data?.items || response.data?.data || response.data || [];
      setReviews(Array.isArray(reviewsData) ? reviewsData : []);
    } catch (error) {
      console.error('Error fetching reviews:', error);
      toast.error('Failed to load reviews');
      setReviews([]);
    } finally {
      setLoading(false);
    }
  }

  function openEditModal(review) {
    setSelectedReview(review);
    setDecision(review.decision.toString());
    setNotes(review.notes);
    setShowEditModal(true);
  }

  async function handleUpdateReview(e) {
    e.preventDefault();
    
    if (!decision || !notes.trim()) {
      toast.error('Please fill in all fields');
      return;
    }

    if (notes.trim().length < 10) {
      toast.error('Notes must be at least 10 characters');
      return;
    }

    try {
      await updateReview(selectedReview.id, {
        ticketId: selectedReview.ticketId,
        decision: parseInt(decision),
        notes: notes.trim()
      });
      
      toast.success('Review updated successfully');
      setShowEditModal(false);
      setSelectedReview(null);
      setDecision('');
      setNotes('');
      fetchReviews();
    } catch (error) {
      console.error('Error updating review:', error);
      toast.error(error.response?.data?.message || 'Failed to update review');
    }
  }

  async function handleDeleteReview(reviewId) {
    if (!confirm('Are you sure you want to delete this review?')) {
      return;
    }

    try {
      await deleteReview(reviewId);
      toast.success('Review deleted successfully');
      fetchReviews();
    } catch (error) {
      console.error('Error deleting review:', error);
      toast.error(error.response?.data?.message || 'Failed to delete review');
    }
  }

  if (loading) {
    return <div className="p-4">Loading reviews...</div>;
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-semibold">My Reviews</h2>
        <button
          onClick={fetchReviews}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          Refresh
        </button>
      </div>

      {reviews.length === 0 ? (
        <p className="text-gray-600">No reviews found.</p>
      ) : (
        <div className="space-y-4">
          {reviews.map((review) => {
            const decisionInfo = decisionLabels[review.decision] || decisionLabels[0];
            const DecisionIcon = decisionInfo.icon;

            return (
              <div key={review.id} className="border rounded-lg p-4 bg-white shadow-sm">
                <div className="flex justify-between items-start mb-3">
                  <div className="flex-1">
                    <div className="mb-2">
                      <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Decision</span>
                      <div className="flex items-center gap-2 mt-1">
                        <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-sm font-medium bg-${decisionInfo.color}-100 text-${decisionInfo.color}-700`}>
                          <DecisionIcon className="w-4 h-4" />
                          {decisionInfo.label}
                        </span>
                      </div>
                    </div>
                    {review.ticketSubject && (
                      <div className="mb-2">
                        <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Ticket</span>
                        <p className="text-sm text-gray-700 mt-0.5">{review.ticketSubject}</p>
                      </div>
                    )}
                  </div>
                  <div className="flex gap-2">
                    <button
                      onClick={() => openEditModal(review)}
                      className="p-2 text-blue-600 hover:bg-blue-50 rounded"
                      title="Edit Review"
                    >
                      <FiEdit2 />
                    </button>
                    <button
                      onClick={() => handleDeleteReview(review.id)}
                      className="p-2 text-red-600 hover:bg-red-50 rounded"
                      title="Delete Review"
                    >
                      <FiTrash2 />
                    </button>
                  </div>
                </div>

                <div className="mb-2">
                  <span className="text-xs font-semibold text-gray-500 uppercase tracking-wide">Review Notes</span>
                  <div className="bg-gray-50 rounded p-3 mt-1">
                    <p className="text-gray-700 whitespace-pre-wrap">{review.notes}</p>
                  </div>
                </div>

                <div>
                  <span className="text-xs font-semibold text-gray-500">Created</span>
                  <p className="text-sm text-gray-700">
                    {new Date(review.createdAt).toLocaleString()}
                  </p>
                </div>
              </div>
            );
          })}
        </div>
      )}

      {/* Edit Modal */}
      {showEditModal && selectedReview && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-white rounded-lg p-6 max-w-lg w-full mx-4">
            <h3 className="text-xl font-semibold mb-4">Edit Review</h3>
            
            {selectedReview.ticketSubject && (
              <div className="mb-4 p-3 bg-gray-50 rounded">
                <p className="font-semibold">{selectedReview.ticketSubject}</p>
              </div>
            )}

            <form onSubmit={handleUpdateReview}>
              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">
                  Decision <span className="text-red-500">*</span>
                </label>
                <select
                  value={decision}
                  onChange={(e) => setDecision(e.target.value)}
                  className="w-full border rounded px-3 py-2"
                  required
                >
                  <option value="">Select Decision</option>
                  <option value="0">Approve</option>
                  <option value="1">Reject</option>
                  <option value="2">Needs More Info</option>
                  <option value="3">Reopen</option>
                </select>
              </div>

              <div className="mb-4">
                <label className="block text-sm font-medium mb-2">
                  Review Notes <span className="text-red-500">*</span>
                </label>
                <textarea
                  value={notes}
                  onChange={(e) => setNotes(e.target.value)}
                  className="w-full border rounded px-3 py-2"
                  rows="4"
                  placeholder="Enter your review notes (minimum 10 characters)..."
                  required
                  minLength={10}
                  maxLength={2000}
                />
                <p className="text-sm text-gray-500 mt-1">
                  {notes.length}/2000 characters (minimum 10)
                </p>
              </div>

              <div className="flex justify-end gap-3">
                <button
                  type="button"
                  onClick={() => {
                    setShowEditModal(false);
                    setSelectedReview(null);
                    setDecision('');
                    setNotes('');
                  }}
                  className="px-4 py-2 border rounded hover:bg-gray-50"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
                >
                  Update Review
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
