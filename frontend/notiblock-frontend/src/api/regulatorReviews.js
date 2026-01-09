import apiClient from "./api";

export async function createReview(data) {
    return await apiClient.post("/regulator-reviews", data);
}

export async function getReviewById(id) {
    return await apiClient.get(`/regulator-reviews/${id}`);
}

export async function updateReview(id, data) {
    return await apiClient.put(`/regulator-reviews/${id}`, data);
}

export async function deleteReview(id) {
    return await apiClient.delete(`/regulator-reviews/${id}`);
}

export async function getMyReviews() {
    return await apiClient.get(`/regulator-reviews/my-reviews`);
}

export async function getTicketById(ticketId) {
    return await apiClient.get(`/regulator-reviews/ticket/${ticketId}`);
}

export async function getPendingTickets() {
    return await apiClient.get("/regulator-reviews/pending-tickets");
}

export async function escalateTicket(ticketId) {
    return await apiClient.post(`/regulator-reviews/ticket/${ticketId}/escalate`);
}

export async function getReviewStatistics() {
    return await apiClient.get("/regulator-reviews/stats");
}